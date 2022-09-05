using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// Maps PostgreSQL arrays to <see cref="List{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Note that mapping PostgreSQL arrays to .NET arrays is also supported via <see cref="NpgsqlArrayArrayTypeMapping"/>.
/// </para>
///
/// <para>See: https://www.postgresql.org/docs/current/static/arrays.html</para>
/// </remarks>
public class NpgsqlArrayListTypeMapping : NpgsqlArrayTypeMapping
{
    /// <summary>
    /// Creates the default list mapping.
    /// </summary>
    /// <param name="storeType">The database type to map.</param>
    /// <param name="elementMapping">The element type mapping.</param>
    public NpgsqlArrayListTypeMapping(string storeType, RelationalTypeMapping elementMapping)
        : this(storeType, elementMapping, typeof(List<>).MakeGenericType(elementMapping.ClrType)) {}

    /// <summary>
    /// Creates the default list mapping.
    /// </summary>
    /// <param name="listType">The database type to map.</param>
    /// <param name="elementMapping">The element type mapping.</param>
    public NpgsqlArrayListTypeMapping(Type listType, RelationalTypeMapping elementMapping)
        : this(elementMapping.StoreType + "[]", elementMapping, listType) {}

    private NpgsqlArrayListTypeMapping(string storeType, RelationalTypeMapping elementMapping, Type listType)
        : this(CreateParameters(storeType, elementMapping, listType), elementMapping)
    {
    }

    private static RelationalTypeMappingParameters CreateParameters(
        string storeType,
        RelationalTypeMapping elementMapping,
        Type listType)
    {
        ValueConverter? converter = null;

        if (elementMapping.Converter is { } elementConverter)
        {
            var isNullable = listType.TryGetElementType(out var elementType) && elementType.IsNullableValueType();

            // We construct the list's ProviderClrType and ModelClrType from the element's, but nullability has been unwrapped on the
            // element mapping. So we look at the given listType for that.
            // Note that if there's a value converter, the ProviderClrType is an array rather than a list (because why not)
            var providerClrType = isNullable
                ? elementConverter.ProviderClrType.MakeNullable().MakeArrayType()
                : elementConverter.ProviderClrType.MakeArrayType();

            var modelClrType = typeof(List<>).MakeGenericType(
                isNullable ? elementConverter.ModelClrType.MakeNullable() : elementConverter.ModelClrType);

            converter = (ValueConverter)Activator.CreateInstance(
                typeof(NpgsqlArrayConverter<,>).MakeGenericType(modelClrType, providerClrType),
                elementConverter)!;
        }

        return new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(listType, converter, CreateComparer(elementMapping, listType)),
            storeType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlArrayListTypeMapping(
        RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping, bool? isElementNullable = null)
        : base(
            parameters,
            elementMapping,
            CalculateElementNullability(
                // Note that the ClrType on elementMapping has been unwrapped for nullability, so we consult the List's CLR type instead
                parameters.CoreParameters.ClrType.GetGenericArguments()[0],
                isElementNullable))
    {
        if (!parameters.CoreParameters.ClrType.IsGenericList())
        {
            throw new ArgumentException("ClrType must be a generic List", nameof(parameters));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override NpgsqlArrayTypeMapping MakeNonNullable()
        => new NpgsqlArrayListTypeMapping(Parameters, ElementMapping, isElementNullable: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping)
        => new NpgsqlArrayListTypeMapping(parameters, elementMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override NpgsqlArrayTypeMapping FlipArrayListClrType(Type newType)
    {
        var elementType = ClrType.GetGenericArguments()[0];
        if (newType.IsGenericList())
        {
            var newTypeElement = newType.GetGenericArguments()[0];

            return newTypeElement == elementType
                ? this
                : throw new ArgumentException(
                    $"Mismatch in list element CLR types when converting a type mapping: {newTypeElement.Name} and {elementType.Name}");
        }

        if (newType.IsArray)
        {
            var arrayElementType = newType.GetElementType()!;

            return arrayElementType == elementType
                ? new NpgsqlArrayArrayTypeMapping(newType, ElementMapping)
                : throw new ArgumentException(
                    "Mismatch in list element CLR types when converting a type mapping: " +
                    $"{arrayElementType} and {elementType.Name}");
        }

        throw new ArgumentException($"Non-array/list type: {newType.Name}");
    }

    #region Value Comparison

    // Note that the value comparison code is largely duplicated from NpgsqlArrayTypeMapping.
    // However, a limitation in EF Core prevents us from merging the code together, see
    // https://github.com/aspnet/EntityFrameworkCore/issues/11077

    private static ValueComparer CreateComparer(RelationalTypeMapping elementMapping, Type listType)
    {
        Debug.Assert(listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>));

        var elementType = listType.GetGenericArguments()[0];
        var unwrappedType = elementType.UnwrapNullableType();

        return (ValueComparer)Activator.CreateInstance(
            elementType == unwrappedType
                ? typeof(ListComparer<>).MakeGenericType(elementType)
                : typeof(NullableListComparer<>).MakeGenericType(unwrappedType),
            elementMapping)!;
    }

    private sealed class ListComparer<TElem> : ValueComparer<List<TElem>>
    {
        public ListComparer(RelationalTypeMapping elementMapping)
            : base(
                (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
                source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

        public override Type Type => typeof(List<TElem>);

        private static bool Compare(List<TElem>? a, List<TElem>? b, ValueComparer<TElem> elementComparer)
        {
            if (a is null)
            {
                return b is null;
            }

            if (b is null || a.Count != b.Count)
            {
                return false;
            }

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // Note: the following currently boxes every element access because ValueComparer isn't really
            // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
            for (var i = 0; i < a.Count; i++)
            {
                if (!elementComparer.Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetHashCode(List<TElem> source, ValueComparer<TElem> elementComparer)
        {
            var hash = new HashCode();

            foreach (var el in source)
            {
                hash.Add(el, elementComparer);
            }

            return hash.ToHashCode();
        }

        private static List<TElem> Snapshot(List<TElem> source, ValueComparer<TElem> elementComparer)
        {
            var snapshot = new List<TElem>(source.Count);

            // Note: the following currently boxes every element access because ValueComparer isn't really
            // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
            foreach (var e in source)
            {
                snapshot.Add(e is null ? default! : elementComparer.Snapshot(e));
            }

            return snapshot;
        }
    }

    private sealed class NullableListComparer<TElem> : ValueComparer<List<TElem?>>
        where TElem : struct
    {
        public NullableListComparer(RelationalTypeMapping elementMapping)
            : base(
                (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
                source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

        public override Type Type => typeof(List<TElem?>);

        private static bool Compare(List<TElem?>? a, List<TElem?>? b, ValueComparer<TElem> elementComparer)
        {
            if (a is null)
            {
                return b is null;
            }

            if (b is null || a.Count != b.Count)
            {
                return false;
            }

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // Note: the following currently boxes every element access because ValueComparer isn't really
            // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
            for (var i = 0; i < a.Count; i++)
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

                if (el2 is null || !elementComparer.Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetHashCode(List<TElem?> source, ValueComparer<TElem> elementComparer)
        {
            var nullableEqualityComparer = new NullableEqualityComparer<TElem>(elementComparer);
            var hash = new HashCode();

            foreach (var el in source)
            {
                hash.Add(el, nullableEqualityComparer);
            }

            return hash.ToHashCode();
        }

        private static List<TElem?> Snapshot(List<TElem?> source, ValueComparer<TElem> elementComparer)
        {
            var snapshot = new List<TElem?>(source.Count);

            // Note: the following currently boxes every element access because ValueComparer isn't really
            // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
            foreach (var e in source)
            {
                snapshot.Add(e is { } value ? elementComparer.Snapshot(value) : null);
            }

            return snapshot;
        }
    }

    #endregion Value Comparison
}
