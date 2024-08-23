using System.Runtime.InteropServices;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

/// <summary>
///     Generates sequential <see cref="Guid" /> values according to the UUID version 7 specification.
///     Will be updated to use Guid.CreateVersion7 when available.
/// </summary>
public class UUid7ValueGenerator : ValueGenerator<Guid>
{
    private const byte Variant10xxValue = 0x80;
    private const ushort Version7Value = 0x7000;
    private const ushort VersionMask = 0xF000;
    private const byte Variant10xxMask = 0xC0;

    /// <summary>
    ///     Gets a value to be assigned to a property.
    /// </summary>
    /// <param name="entry">The change tracking entry of the entity for which the value is being generated.</param>
    /// <returns>The value to be assigned to a property.</returns>
    public override Guid Next(EntityEntry entry)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        var succeeded = Guid.NewGuid().TryWriteBytes(guidBytes);
        var unixms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Span<byte> counterBytes = stackalloc byte[sizeof(long)];
        MemoryMarshal.Write(counterBytes, in unixms);

        if (!BitConverter.IsLittleEndian)
        {
            counterBytes.Reverse();
        }

        //unix ts ms - 48 bits (6 bytes)
        guidBytes[00] = counterBytes[2];
        guidBytes[01] = counterBytes[3];
        guidBytes[02] = counterBytes[4];
        guidBytes[03] = counterBytes[5];
        guidBytes[04] = counterBytes[0];
        guidBytes[05] = counterBytes[1];

        //UIDv7 version - first 4 bits (1/2 byte) of the next 16 bits (2 bytes)
        var _c = BitConverter.ToInt16(guidBytes.Slice(6, 2));
        _c = (short)((_c & ~VersionMask) | Version7Value);
        BitConverter.TryWriteBytes(guidBytes.Slice(6, 2), _c);

        //2 bit variant
        //first 2 bits of the next 64 bits (8 bytes)
        guidBytes[8] = (byte)((guidBytes[8] & ~Variant10xxMask) | Variant10xxValue);
        return new Guid(guidBytes);
    }

    /// <summary>
    ///     Gets a value indicating whether the values generated are temporary or permanent. This implementation
    ///     always returns false, meaning the generated values will be saved to the database.
    /// </summary>
    public override bool GeneratesTemporaryValues
        => false;
}
