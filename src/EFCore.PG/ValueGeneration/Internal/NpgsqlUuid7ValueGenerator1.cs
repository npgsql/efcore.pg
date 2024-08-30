using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;

/// <summary>
///     Generates sequential <see cref="Guid" /> values according to the UUID version 7 specification.
///     Will be updated to use Guid.CreateVersion7 when available.
/// </summary>
public class NpgsqlUuid7ValueGenerator : ValueGenerator<Guid>
{
    /// <summary>
    ///     Gets a value indicating whether the values generated are temporary or permanent. This implementation
    ///     always returns false, meaning the generated values will be saved to the database.
    /// </summary>
    public override bool GeneratesTemporaryValues => false;

    /// <summary>
    ///     Gets a value to be assigned to a property.
    /// </summary>
    /// <param name="entry">The change tracking entry of the entity for which the value is being generated.</param>
    /// <returns>The value to be assigned to a property.</returns>
    public override Guid Next(EntityEntry entry) => BorrowedFromNet9.CreateVersion7(timestamp: DateTimeOffset.UtcNow);

    // Code borrowed from .NET 9 should be removed as soon as the target framework includes such code
    #region Borrowed from .NET 9

#pragma warning disable IDE0007 // Use implicit type -- Avoid changes to code borrowed from BCL

    // https://github.com/dotnet/runtime/blob/f402418aaed508c1d77e41b942e3978675183bfc/src/libraries/System.Private.CoreLib/src/System/Guid.cs
    internal static class BorrowedFromNet9
    {
        private const byte Variant10xxMask = 0xC0;
        private const byte Variant10xxValue = 0x80;

        private const ushort VersionMask = 0xF000;
        private const ushort Version7Value = 0x7000;

        /// <summary>Creates a new <see cref="Guid" /> according to RFC 9562, following the Version 7 format.</summary>
        /// <returns>A new <see cref="Guid" /> according to RFC 9562, following the Version 7 format.</returns>
        /// <remarks>
        ///     <para>This uses <see cref="DateTimeOffset.UtcNow" /> to determine the Unix Epoch timestamp source.</para>
        ///     <para>This seeds the rand_a and rand_b sub-fields with random data.</para>
        /// </remarks>
        public static Guid CreateVersion7() => CreateVersion7(DateTimeOffset.UtcNow);

        /// <summary>Creates a new <see cref="Guid" /> according to RFC 9562, following the Version 7 format.</summary>
        /// <param name="timestamp">The date time offset used to determine the Unix Epoch timestamp.</param>
        /// <returns>A new <see cref="Guid" /> according to RFC 9562, following the Version 7 format.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timestamp" /> represents an offset prior to <see cref="DateTimeOffset.UnixEpoch" />.</exception>
        /// <remarks>
        ///     <para>This seeds the rand_a and rand_b sub-fields with random data.</para>
        /// </remarks>
        public static Guid CreateVersion7(DateTimeOffset timestamp)
        {
            // NewGuid uses CoCreateGuid on Windows and Interop.GetCryptographicallySecureRandomBytes on Unix to get
            // cryptographically-secure random bytes. We could use Interop.BCrypt.BCryptGenRandom to generate the random
            // bytes on Windows, as is done in RandomNumberGenerator, but that's measurably slower than using CoCreateGuid.
            // And while CoCreateGuid only generates 122 bits of randomness, the other 6 bits being for the version / variant
            // fields, this method also needs those bits to be non-random, so we can just use NewGuid for efficiency.
            var result = Guid.NewGuid();

            // 2^48 is roughly 8925.5 years, which from the Unix Epoch means we won't
            // overflow until around July of 10,895. So there isn't any need to handle
            // it given that DateTimeOffset.MaxValue is December 31, 9999. However, we
            // can't represent timestamps prior to the Unix Epoch since UUIDv7 explicitly
            // stores a 48-bit unsigned value, so we do need to throw if one is passed in.

            var unix_ts_ms = timestamp.ToUnixTimeMilliseconds();
            ArgumentOutOfRangeException.ThrowIfNegative(unix_ts_ms, nameof(timestamp));

            ref var resultClone = ref Unsafe.As<Guid, GuidDoppleganger>(ref result); // Deviation from BLC: Reinterpret Guid as our own type so that we can manipulate its private fields

            Unsafe.AsRef(in resultClone._a) = (int)(unix_ts_ms >> 16);
            Unsafe.AsRef(in resultClone._b) = (short)unix_ts_ms;

            Unsafe.AsRef(in resultClone._c) = (short)(resultClone._c & ~VersionMask | Version7Value);
            Unsafe.AsRef(in resultClone._d) = (byte)(resultClone._d & ~Variant10xxMask | Variant10xxValue);

            return result;
        }
    }

    /// <summary>
    /// Used to manipulate the private fields of a <see cref="Guid"/> like its internal methods do, by treating a <see cref="Guid"/> as a <see cref="GuidDoppleganger"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct GuidDoppleganger
    {
#pragma warning disable IDE1006 // Naming Styles -- Avoid further changes to code borrowed from BCL when working with the current type
        internal readonly int _a;   // Do not rename (binary serialization)
        internal readonly short _b; // Do not rename (binary serialization)
        internal readonly short _c; // Do not rename (binary serialization)
        internal readonly byte _d;  // Do not rename (binary serialization)
        internal readonly byte _e;  // Do not rename (binary serialization)
        internal readonly byte _f;  // Do not rename (binary serialization)
        internal readonly byte _g;  // Do not rename (binary serialization)
        internal readonly byte _h;  // Do not rename (binary serialization)
        internal readonly byte _i;  // Do not rename (binary serialization)
        internal readonly byte _j;  // Do not rename (binary serialization)
        internal readonly byte _k;  // Do not rename (binary serialization)
#pragma warning restore IDE1006 // Naming Styles
    }

#pragma warning restore IDE0007 // Use implicit type

    #endregion
}
