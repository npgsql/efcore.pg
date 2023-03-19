using System.Collections;
using System.Data.Common;
using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     Type mapping for PostgreSQL arrays.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/static/arrays.html
/// </remarks>
public class NpgsqlArrayTypeMapping : RelationalTypeMapping
{
    /// <summary>
    /// The database type used by Npgsql.
    /// </summary>
    public virtual NpgsqlDbType? NpgsqlDbType { get; }

    /// <summary>
    ///     Whether the array's element is nullable. This is required since <see cref="Type"/> and <see cref="ElementTypeMapping"/> do not
    ///     contain nullable reference type information.
    /// </summary>
    public virtual bool IsElementNullable { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlArrayTypeMapping(Type arrayType, RelationalTypeMapping elementTypeMapping)
        : this(elementTypeMapping.StoreType + "[]", arrayType, elementTypeMapping) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlArrayTypeMapping(string storeType, Type arrayType, RelationalTypeMapping elementTypeMapping)
        : this(CreateParameters(storeType, arrayType, elementTypeMapping))
    {
        Check.DebugAssert(storeType.EndsWith("[]", StringComparison.Ordinal), "storeType.EndsWith('[]', StringComparison.Ordinal)");
    }

    private static RelationalTypeMappingParameters CreateParameters(
        string storeType,
        Type collectionType,
        RelationalTypeMapping elementTypeMapping)
    {
        ValueConverter? converter = null;

        if (elementTypeMapping.Converter is { } elementConverter)
        {
            var collectionTypeDefinition = collectionType;
            if (!collectionType.IsArray
                && (!collectionType.IsGenericType
                    || ((collectionTypeDefinition = collectionType.GetGenericTypeDefinition()).GetGenericArguments().Length != 1)))
            {
                throw new ArgumentException($"Collection CLR type '{collectionType}' isn't an a generic type with a single type argument");
            }

            if (collectionType.TryGetElementType(typeof(IEnumerable<>)) is not Type arrayElementClrType)
            {
                throw new ArgumentException($"Collection CLR type '{collectionType}' isn't an IEnumerable");
            }

            var isNullable = arrayElementClrType.IsNullableValueType();

            // We construct the array's ProviderClrType and ModelClrType from the element's, but nullability has been unwrapped on the
            // element mapping. So we look at the given arrayType for that.
            var providerClrType = MakeCollectionType(collectionTypeDefinition, elementConverter.ProviderClrType, isNullable);
            var modelClrType = MakeCollectionType(collectionTypeDefinition, elementConverter.ModelClrType, isNullable);

            converter = (ValueConverter)Activator.CreateInstance(
                typeof(NpgsqlArrayConverter<,>).MakeGenericType(modelClrType, providerClrType),
                elementConverter)!;

            static Type MakeCollectionType(Type collectionType, Type elementType, bool isElementNullable)
            {
                if (isElementNullable)
                {
                    elementType = elementType.MakeNullable();
                }

                return collectionType.IsArray
                    ? elementType.MakeArrayType()
                    : collectionType.MakeGenericType(elementType);
            }
        }

        return new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(
                collectionType,
                converter,
                CreateComparer(elementTypeMapping, collectionType),
                elementTypeMapping: elementTypeMapping),
            storeType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlArrayTypeMapping(RelationalTypeMappingParameters parameters, bool? isElementNullable = null)
        : base(parameters)
    {
        var clrType = parameters.CoreParameters.ClrType;
        Type? arrayElementClrType;

        if ((arrayElementClrType = clrType.TryGetElementType(typeof(IEnumerable<>))) == null
            && (arrayElementClrType = clrType.GetElementType()) == null)
        {
            throw new ArgumentException($"CLR type '{parameters.CoreParameters.ClrType}' isn't an IEnumerable");
        }

        // isElementNullable is provided for reference-type properties by decoding NRT information from the property, since that's not
        // available on the CLR type. Note, however, that because of value conversion we may get a discrepancy between the model property's
        // nullability and the provider types' (e.g. array of nullable reference property value-converted to array of non-nullable value
        // type).
        // Note that the ClrType on elementMapping has been unwrapped for nullability, so we consult the array's CLR type instead
        IsElementNullable = arrayElementClrType.IsValueType
            ? arrayElementClrType.IsNullableType()
            : isElementNullable ?? true;

        // If the element mapping has an NpgsqlDbType or DbType, set our own NpgsqlDbType as an array of that.
        // Otherwise let the ADO.NET layer infer the PostgreSQL type. We can't always let it infer, otherwise
        // when given a byte[] it will infer byte (but we want smallint[])
        NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Array |
            (ElementTypeMapping is INpgsqlTypeMapping elementNpgsqlTypeMapping
                ? elementNpgsqlTypeMapping.NpgsqlDbType
                : ElementTypeMapping.DbType.HasValue
                    ? new NpgsqlParameter { DbType = ElementTypeMapping.DbType.Value }.NpgsqlDbType
                    : default(NpgsqlDbType?));
    }

    /// <summary>
    ///     The element's type mapping.
    /// </summary>
    public override RelationalTypeMapping ElementTypeMapping
    {
        get
        {
            var elementTypeMapping = base.ElementTypeMapping;
            Check.DebugAssert(elementTypeMapping is not null,
                "NpgsqlArrayTypeMapping without an element type mapping");
            Check.DebugAssert(elementTypeMapping is RelationalTypeMapping,
                "NpgsqlArrayTypeMapping with a non-relational element type mapping");
            return (RelationalTypeMapping)elementTypeMapping;
        }
    }

    /// <summary>
    ///     Returns a copy of this type mapping with <see cref="IsElementNullable" /> set to <see langword="false" />.
    /// </summary>
    public virtual NpgsqlArrayTypeMapping MakeNonNullable()
        => new(Parameters, isElementNullable: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override CoreTypeMapping Clone(ValueConverter? converter)
    {
        // When the mapping is cloned to apply a value converter, we need to also apply that value converter to the element, otherwise
        // we end up with an array mapping over a converter-less element mapping. This is important in some inference situations.
        // If the array converter was properly set up, it's a INpgsqlArrayConverter with a reference to its element's converter.
        // Just clone the element's mapping with that (same with the null converter case).
        if (converter is INpgsqlArrayConverter or null)
        {
            return Clone(
                Parameters.WithComposedConverter(converter),
                (RelationalTypeMapping)ElementTypeMapping.Clone(converter is INpgsqlArrayConverter arrayConverter
                    ? arrayConverter.ElementConverter
                    : null));
        }

        throw new NotSupportedException(
            $"Value converters for array or List properties must be configured via {nameof(NpgsqlPropertyBuilderExtensions.HasPostgresArrayConversion)}.");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters, RelationalTypeMapping elementMapping)
        => new NpgsqlArrayTypeMapping(parameters.WithCoreParameters(parameters.CoreParameters.WithElementTypeMapping(elementMapping)));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
    {
        var elementMapping = ElementTypeMapping;

        // Apply precision, scale and size to the element mapping, not to the array
        if (parameters.Size is not null)
        {
            elementMapping = elementMapping.Clone(elementMapping.StoreType, parameters.Size);
            parameters = Parameters.WithStoreTypeAndSize(elementMapping.StoreType, size: null);
        }

        if (parameters.Precision is not null || parameters.Scale is not null)
        {
            elementMapping = elementMapping.Clone(parameters.Precision, parameters.Scale);
            parameters = Parameters.WithPrecision(null).WithScale(null);
        }

        parameters = parameters.WithStoreTypeAndSize(elementMapping.StoreType + "[]", size: null);

        return Clone(parameters, elementMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        if (value is not IEnumerable enumerable)
        {
            throw new ArgumentException($"'{value.GetType().Name}' must be an IEnumerable", nameof(value));
        }

        if (value is Array array && array.Rank != 1)
        {
            throw new NotSupportedException("Multidimensional array literals aren't supported");
        }

        var sb = new StringBuilder();
        sb.Append("ARRAY[");

        var isFirst = true;
        foreach (var element in enumerable)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                sb.Append(",");
            }

            sb.Append(ElementTypeMapping.GenerateProviderValueSqlLiteral(element));
        }

        sb.Append("]::");
        sb.Append(ElementTypeMapping.StoreType);
        sb.Append("[]");
        return sb.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter is not NpgsqlParameter npgsqlParameter)
        {
            throw new ArgumentException(
                $"Npgsql-specific type mapping {GetType()} being used with non-Npgsql parameter type {parameter.GetType().Name}");
        }

        if (NpgsqlDbType.HasValue)
        {
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Value;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private sealed class NullableEqualityComparer<T> : IEqualityComparer<T?>
        where T : struct
    {
        private readonly IEqualityComparer<T> _underlyingComparer;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NullableEqualityComparer(IEqualityComparer<T> underlyingComparer)
            => _underlyingComparer = underlyingComparer;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool Equals(T? x, T? y)
            => x is null
                ? y is null
                : y.HasValue && _underlyingComparer.Equals(x.Value, y.Value);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public int GetHashCode(T? obj)
            => obj is null ? 0 : _underlyingComparer.GetHashCode(obj.Value);
    }

    #region Value comparers

    private static ValueComparer? CreateComparer(RelationalTypeMapping elementMapping, Type collectionType)
    {
        // We currently don't support mapping multi-dimensional arrays.
        if (collectionType.IsArray && collectionType.GetArrayRank() != 1)
        {
            return null;
        }

        var elementType = collectionType.TryGetElementType(typeof(IEnumerable<>));
        Check.DebugAssert(elementType is not null, "elementType is not null");
        var unwrappedType = elementType.UnwrapNullableType();

        // TODO: Special comparers for immutable collections

        return (ValueComparer)Activator.CreateInstance(
            elementType == unwrappedType
                ? typeof(CollectionComparer<>).MakeGenericType(elementType)
                : typeof(NullableCollectionComparer<>).MakeGenericType(unwrappedType),
            elementMapping)!;
    }

    private sealed class CollectionComparer<TElem> : ValueComparer<IList<TElem>>
    {
        public CollectionComparer(RelationalTypeMapping elementMapping)
            : base(
                (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
                source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

        public override Type Type => typeof(IList<TElem>);

        private static bool Compare(IList<TElem>? a, IList<TElem>? b, ValueComparer<TElem> elementComparer)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null)
            {
                return b is null;
            }

            if (b is null || a.Count != b.Count)
            {
                return false;
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

        private static int GetHashCode(IList<TElem> source, ValueComparer<TElem> elementComparer)
        {
            var hash = new HashCode();

            foreach (var el in source)
            {
                hash.Add(el, elementComparer);
            }

            return hash.ToHashCode();
        }

        private static IList<TElem> Snapshot(IList<TElem> source, ValueComparer<TElem> elementComparer)
        {
            // Note: the following currently boxes every element access because ValueComparer isn't really
            // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
            if (source.GetType().IsArray)
            {
                var snapshot = new TElem[source.Count];

                for (var i = 0; i < source.Count; i++)
                {
                    snapshot[i] = elementComparer.Snapshot(source[i]);
                }

                return snapshot;
            }
            else
            {
                var snapshot = source is List<TElem>
                    ? new List<TElem>(source.Count)
                    : (IList<TElem>)Activator.CreateInstance(source.GetType())!;

                foreach (var e in source)
                {
                    snapshot.Add(elementComparer.Snapshot(e));
                }

                return snapshot;
            }
        }
    }

    private sealed class NullableCollectionComparer<TElem> : ValueComparer<IList<TElem?>>
        where TElem : struct
    {
        public NullableCollectionComparer(RelationalTypeMapping elementMapping)
            : base(
                (a, b) => Compare(a, b, (ValueComparer<TElem>)elementMapping.Comparer),
                o => GetHashCode(o, (ValueComparer<TElem>)elementMapping.Comparer),
                source => Snapshot(source, (ValueComparer<TElem>)elementMapping.Comparer)) {}

        public override Type Type => typeof(IList<TElem?>);

        private static bool Compare(IList<TElem?>? a, IList<TElem?>? b, ValueComparer<TElem> elementComparer)
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

                if (!elementComparer.Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetHashCode(IList<TElem?> source, ValueComparer<TElem> elementComparer)
        {
            var nullableEqualityComparer = new NullableEqualityComparer<TElem>(elementComparer);
            var hash = new HashCode();

            foreach (var el in source)
            {
                hash.Add(el, nullableEqualityComparer);
            }

            return hash.ToHashCode();
        }

        private static IList<TElem?> Snapshot(IList<TElem?> source, ValueComparer<TElem> elementComparer)
        {
            // Note: the following currently boxes every element access because ValueComparer isn't really
            // generic (see https://github.com/aspnet/EntityFrameworkCore/issues/11072)
            if (source.GetType().IsArray)
            {
                var snapshot = new TElem?[source.Count];

                for (var i = 0; i < source.Count; i++)
                {
                    snapshot[i] = source[i] is { } value ? elementComparer.Snapshot(value) : null;
                }

                return snapshot;
            }
            else
            {
                var snapshot = source is List<TElem?>
                    ? new List<TElem?>(source.Count)
                    : (IList<TElem?>)Activator.CreateInstance(source.GetType())!;

                foreach (var e in source)
                {
                    snapshot.Add(e is { } value ? elementComparer.Snapshot(value) : null);
                }

                return snapshot;
            }
        }
    }

    #endregion Value comparer
}
