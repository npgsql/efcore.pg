using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// Maps PostgreSQL arrays to .NET arrays. Only single-dimensional arrays are supported.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that mapping PostgreSQL arrays to .NET <see cref="List{T}"/> is also supported via
    /// <see cref="NpgsqlArrayListTypeMapping"/>.
    /// </para>
    ///
    /// <para>See: https://www.postgresql.org/docs/current/static/arrays.html</para>
    /// </remarks>
    public class NpgsqlArrayArrayTypeMapping : NpgsqlArrayTypeMapping
    {
        /// <summary>
        /// Creates the default array mapping (i.e. for the single-dimensional CLR array type)
        /// </summary>
        /// <param name="storeType">The database type to map.</param>
        /// <param name="elementMapping">The element type mapping.</param>
        public NpgsqlArrayArrayTypeMapping([NotNull] string storeType, [NotNull] RelationalTypeMapping elementMapping)
            : this(storeType, elementMapping, elementMapping.ClrType.MakeArrayType()) {}

        /// <summary>
        /// Creates the default array mapping (i.e. for the single-dimensional CLR array type)
        /// </summary>
        /// <param name="elementMapping">The element type mapping.</param>
        /// <param name="arrayType">The array type to map.</param>
        public NpgsqlArrayArrayTypeMapping([NotNull] RelationalTypeMapping elementMapping, [NotNull] Type arrayType)
            : this(elementMapping.StoreType + "[]", elementMapping, arrayType) {}

        NpgsqlArrayArrayTypeMapping(string storeType, RelationalTypeMapping elementMapping, Type arrayType)
            : this(new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(arrayType, null, CreateComparer(elementMapping, arrayType)), storeType
            ), elementMapping) {}

        protected NpgsqlArrayArrayTypeMapping(
            RelationalTypeMappingParameters parameters, [NotNull] RelationalTypeMapping elementMapping)
            : base(parameters, elementMapping)
        {
            if (!parameters.CoreParameters.ClrType.IsArray)
                throw new ArgumentException("ClrType must be an array", nameof(parameters));
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlArrayArrayTypeMapping(parameters, ElementMapping);

        #region Value Comparison

        static ValueComparer CreateComparer(RelationalTypeMapping elementMapping, Type arrayType)
        {
            Debug.Assert(arrayType.IsArray);
            var elementType = arrayType.GetElementType();

            // We currently don't support mapping multi-dimensional arrays.
            if (arrayType.GetArrayRank() != 1)
                return null;

            // We use different comparer implementations based on whether we have a non-null element comparer,
            // and if not, whether the element is IEquatable<TElem>

            if (elementMapping.Comparer != null)
                return (ValueComparer)Activator.CreateInstance(
                    typeof(SingleDimComparerWithComparer<>).MakeGenericType(elementType), elementMapping);

            if (typeof(IEquatable<>).MakeGenericType(elementType).IsAssignableFrom(elementType))
                return (ValueComparer)Activator.CreateInstance(typeof(SingleDimComparerWithIEquatable<>).MakeGenericType(elementType));

            // There's no custom comparer, and the element type doesn't implement IEquatable<TElem>. We have
            // no choice but to use the non-generic Equals method.
            return (ValueComparer)Activator.CreateInstance(typeof(SingleDimComparerWithEquals<>).MakeGenericType(elementType));
        }

        sealed class SingleDimComparerWithComparer<TElem> : ValueComparer<TElem[]>
        {
            public SingleDimComparerWithComparer(RelationalTypeMapping elementMapping) : base(
                (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                o => o.GetHashCode(), // TODO: Need to get hash code of elements...
                source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

            public override Type Type => typeof(TElem[]);

            static bool Compare(TElem[] a, TElem[] b, ValueComparer<TElem> elementComparer)
            {
                if (a.Length != b.Length)
                    return false;

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < a.Length; i++)
                    if (!elementComparer.Equals(a[i], b[i]))
                        return false;

                return true;
            }

            static TElem[] Snapshot(TElem[] source, ValueComparer<TElem> elementComparer)
            {
                if (source == null)
                    return null;

                var snapshot = new TElem[source.Length];
                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < source.Length; i++)
                    snapshot[i] = elementComparer.Snapshot(source[i]);
                return snapshot;
            }
        }

        sealed class SingleDimComparerWithIEquatable<TElem> : ValueComparer<TElem[]>
            where TElem : IEquatable<TElem>
        {
            public SingleDimComparerWithIEquatable() : base(
                (a, b) => Compare(a, b),
                o => o.GetHashCode(), // TODO: Need to get hash code of elements...
                source => DoSnapshot(source)) {}

            public override Type Type => typeof(TElem[]);

            static bool Compare(TElem[] a, TElem[] b)
            {
                if (a.Length != b.Length)
                    return false;

                for (var i = 0; i < a.Length; i++)
                {
                    var elem1 = a[i];
                    var elem2 = b[i];
                    // Note: the following null checks are elided if TElem is a value type
                    if (elem1 == null)
                    {
                        if (elem2 == null)
                            continue;
                        return false;
                    }

                    if (!elem1.Equals(elem2))
                        return false;
                }

                return true;
            }

            static TElem[] DoSnapshot(TElem[] source)
            {
                if (source == null)
                    return null;
                var snapshot = new TElem[source.Length];
                source.CopyTo(snapshot, 0);
                return snapshot;
            }
        }

        sealed class SingleDimComparerWithEquals<TElem> : ValueComparer<TElem[]>
        {
            public SingleDimComparerWithEquals() : base(
                (a, b) => Compare(a, b),
                o => o.GetHashCode(), // TODO: Need to get hash code of elements...
                source => DoSnapshot(source)) {}

            public override Type Type => typeof(TElem[]);

            static bool Compare(TElem[] a, TElem[] b)
            {
                if (a.Length != b.Length)
                    return false;

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < a.Length; i++)
                {
                    var elem1 = a[i];
                    var elem2 = b[i];
                    if (elem1 == null)
                    {
                        if (elem2 == null)
                            continue;
                        return false;
                    }

                    if (!elem1.Equals(elem2))
                        return false;
                }

                return true;
            }

            static TElem[] DoSnapshot(TElem[] source)
            {
                if (source == null)
                    return null;

                var snapshot = new TElem[source.Length];
                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < source.Length; i++)
                    snapshot[i] = source[i];
                return snapshot;
            }
        }

        #endregion Value Comparison
    }
}
