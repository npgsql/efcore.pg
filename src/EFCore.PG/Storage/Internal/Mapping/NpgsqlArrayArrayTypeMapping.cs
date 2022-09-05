using System.Diagnostics.CodeAnalysis;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

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
    public NpgsqlArrayArrayTypeMapping(string storeType, RelationalTypeMapping elementMapping)
        : this(storeType, elementMapping, elementMapping.ClrType.MakeArrayType()) {}

    /// <summary>
    /// Creates the default array mapping (i.e. for the single-dimensional CLR array type)
    /// </summary>
    /// <param name="arrayType">The array type to map.</param>
    /// <param name="elementMapping">The element type mapping.</param>
    public NpgsqlArrayArrayTypeMapping(Type arrayType, RelationalTypeMapping elementMapping)
        : this(elementMapping.StoreType + "[]", elementMapping, arrayType) {}

    private NpgsqlArrayArrayTypeMapping(string storeType, RelationalTypeMapping elementMapping, Type arrayType)
        : this(CreateParameters(storeType, elementMapping, arrayType), elementMapping)
    {
    }

    private static RelationalTypeMappingParameters CreateParameters(
        string storeType,
        RelationalTypeMapping elementMapping,
        Type arrayType)
    {
        ValueConverter? converter = null;

        if (elementMapping.Converter is { } elementConverter)
        {
            var isNullable = arrayType.GetElementType()!.IsNullableValueType();

            // We construct the array's ProviderClrType and ModelClrType from the element's, but nullability has been unwrapped on the
            // element mapping. So we look at the given arrayType for that.
            var providerClrType = isNullable
                ? elementConverter.ProviderClrType.MakeNullable().MakeArrayType()
                : elementConverter.ProviderClrType.MakeArrayType();

            var modelClrType = isNullable
                ? elementConverter.ModelClrType.MakeNullable().MakeArrayType()
                : elementConverter.ModelClrType.MakeArrayType();

            converter = (ValueConverter)Activator.CreateInstance(
                typeof(NpgsqlArrayConverter<,>).MakeGenericType(modelClrType, providerClrType),
                elementConverter)!;
        }

        return new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(arrayType, converter, CreateComparer(elementMapping, arrayType)),
            storeType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlArrayArrayTypeMapping(
        RelationalTypeMappingParameters parameters,
        RelationalTypeMapping elementMapping,
        bool? isElementNullable = null)
        : base(
            parameters,
            elementMapping,
            CalculateElementNullability(
                // Note that the ClrType on elementMapping has been unwrapped for nullability, so we consult the array's CLR type instead
                parameters.CoreParameters.ClrType.GetElementType()
                ?? throw new ArgumentException($"CLR type {parameters.CoreParameters.ClrType} isn't an array"),
                isElementNullable))
    {
        if (!parameters.CoreParameters.ClrType.IsArray)
        {
            throw new ArgumentException("ClrType must be an array", nameof(parameters));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override NpgsqlArrayTypeMapping MakeNonNullable()
        => new NpgsqlArrayArrayTypeMapping(Parameters, ElementMapping, isElementNullable: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping)
        => new NpgsqlArrayArrayTypeMapping(parameters, elementMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override NpgsqlArrayTypeMapping FlipArrayListClrType(Type newType)
    {
        var elementType = ClrType.GetElementType()!;
        if (newType.IsArray)
        {
            var newTypeElement = newType.GetElementType()!;

            return newTypeElement == elementType
                ? this
                : throw new ArgumentException(
                    $"Mismatch in array element CLR types when converting a type mapping: {newTypeElement.Name} and {elementType.Name}");
        }

        if (newType.IsGenericList())
        {
            var listElementType = newType.GetGenericArguments()[0];

            return listElementType == elementType
                ? new NpgsqlArrayListTypeMapping(newType, ElementMapping)
                : throw new ArgumentException(
                    "Mismatch in array element CLR types when converting a type mapping: " +
                    $"{listElementType} and {elementType.Name}");
        }

        throw new ArgumentException($"Non-array/list type: {newType.Name}");
    }

    #region Value comparer

    private static ValueComparer? CreateComparer(RelationalTypeMapping elementMapping, Type arrayType)
    {
        Debug.Assert(arrayType.IsArray);
        var elementType = arrayType.GetElementType()!;
        var unwrappedType = elementType.UnwrapNullableType();

        // We currently don't support mapping multi-dimensional arrays.
        if (arrayType.GetArrayRank() != 1)
        {
            return null;
        }

        return (ValueComparer)Activator.CreateInstance(
            elementType == unwrappedType
                ? typeof(SingleDimensionalArrayComparer<>).MakeGenericType(elementType)
                : typeof(NullableSingleDimensionalArrayComparer<>).MakeGenericType(unwrappedType),
            elementMapping)!;
    }

    private sealed class SingleDimensionalArrayComparer<TElem> : ValueComparer<TElem[]>
    {
        public SingleDimensionalArrayComparer(RelationalTypeMapping elementMapping) : base(
            (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
            o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
            source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

        public override Type Type => typeof(TElem[]);

        private static bool Compare(TElem[]? a, TElem[]? b, ValueComparer<TElem> elementComparer)
        {
            if (a is null)
            {
                return b is null;
            }

            if (b is null || a.Length != b.Length)
            {
                return false;
            }

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // Note: the following currently boxes every element access because ValueComparer isn't really
            // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
            for (var i = 0; i < a.Length; i++)
            {
                if (!elementComparer.Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetHashCode(TElem[] source, ValueComparer<TElem> elementComparer)
        {
            var hash = new HashCode();

            foreach (var el in source)
            {
                hash.Add(el, elementComparer);
            }

            return hash.ToHashCode();
        }

        [return: NotNullIfNotNull("source")]
        private static TElem[] Snapshot(TElem[] source, ValueComparer<TElem> elementComparer)
        {
            var snapshot = new TElem[source.Length];
            // Note: the following currently boxes every element access because ValueComparer isn't really
            // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
            for (var i = 0; i < source.Length; i++)
            {
                var element = source[i];
                snapshot[i] = element is null ? default! : elementComparer.Snapshot(element);
            }

            return snapshot;
        }
    }

    private sealed class NullableSingleDimensionalArrayComparer<TElem> : ValueComparer<TElem?[]>
        where TElem : struct
    {
        public NullableSingleDimensionalArrayComparer(RelationalTypeMapping elementMapping) : base(
            (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
            o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
            source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

        public override Type Type => typeof(TElem?[]);

        private static bool Compare(TElem?[]? a, TElem?[]? b, ValueComparer<TElem> elementComparer)
        {
            if (a is null)
            {
                return b is null;
            }

            if (b is null || a.Length != b.Length)
            {
                return false;
            }

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // Note: the following currently boxes every element access because ValueComparer isn't really
            // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
            for (var i = 0; i < a.Length; i++)
            {
                var (el1, el2) = (a[i], b[i]);
                if (el1 is null)
                {
                    if (el2 is null)
                    {
                        continue;
                    }

                    return false;
                }

                if (el2 is null || !elementComparer.Equals(el1, el2))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetHashCode(TElem?[] source, ValueComparer<TElem> elementComparer)
        {
            var nullableEqualityComparer = new NullableEqualityComparer<TElem>(elementComparer);
            var hash = new HashCode();

            foreach (var el in source)
            {
                hash.Add(el, nullableEqualityComparer);
            }

            return hash.ToHashCode();
        }

        [return: NotNullIfNotNull("source")]
        private static TElem?[]? Snapshot(TElem?[]? source, ValueComparer<TElem> elementComparer)
        {
            if (source is null)
            {
                return null;
            }

            var snapshot = new TElem?[source.Length];
            // Note: the following currently boxes every element access because ValueComparer isn't really
            // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
            for (var i = 0; i < source.Length; i++)
            {
                snapshot[i] = source[i] is { } value ? elementComparer.Snapshot(value) : null;
            }

            return snapshot;
        }
    }

    #endregion Value comparer
}
