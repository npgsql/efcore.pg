using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using System.Linq;
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
            var unwrappedType = elementType.UnwrapNullableType();

            // We currently don't support mapping multi-dimensional arrays.
            if (arrayType.GetArrayRank() != 1)
                return null;

            return (ValueComparer)Activator.CreateInstance(
                elementType == unwrappedType
                    ? typeof(SingleDimensionalArrayComparer<>).MakeGenericType(elementType)
                    : typeof(NullableSingleDimensionalArrayComparer<>).MakeGenericType(unwrappedType),
                elementMapping);
        }

        sealed class SingleDimensionalArrayComparer<TElem> : ValueComparer<TElem[]>
        {
            public SingleDimensionalArrayComparer(RelationalTypeMapping elementMapping) : base(
                (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
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

            static int GetHashCode(TElem[] source, ValueComparer<TElem> elementComparer)
            {
                var hash = new HashCode();
                foreach (var el in source)
                    hash.Add(el, elementComparer);
                return hash.ToHashCode();
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

        sealed class NullableSingleDimensionalArrayComparer<TElem> : ValueComparer<TElem?[]>
            where TElem : struct
        {
            public NullableSingleDimensionalArrayComparer(RelationalTypeMapping elementMapping) : base(
                (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
                source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

            public override Type Type => typeof(TElem?[]);

            static bool Compare(TElem?[] a, TElem?[] b, ValueComparer<TElem> elementComparer)
            {
                if (a.Length != b.Length)
                    return false;

                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < a.Length; i++)
                {
                    var (el1, el2) = (a[i], b[i]);
                    if (el1 is null)
                    {
                        if (el2 is null)
                            continue;
                        return false;
                    }
                    if (el2 is null || !elementComparer.Equals(el1, el2))
                        return false;
                }

                return true;
            }

            static int GetHashCode(TElem?[] source, ValueComparer<TElem> elementComparer)
            {
                var nullableEqualityComparer = new NullableEqualityComparer<TElem>(elementComparer);
                var hash = new HashCode();
                foreach (var el in source)
                    hash.Add(el, nullableEqualityComparer);
                return hash.ToHashCode();
            }

            static TElem?[] Snapshot(TElem?[] source, ValueComparer<TElem> elementComparer)
            {
                if (source == null)
                    return null;

                var snapshot = new TElem?[source.Length];
                // Note: the following currently boxes every element access because ValueComparer isn't really
                // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
                for (var i = 0; i < source.Length; i++)
                    snapshot[i] = source[i] is { } value ? elementComparer.Snapshot(value) : (TElem?)null;
                return snapshot;
            }
        }

        #endregion Value Comparison
    }
}
