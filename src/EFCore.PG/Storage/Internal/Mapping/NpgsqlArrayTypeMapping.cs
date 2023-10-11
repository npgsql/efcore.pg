using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     Type mapping for PostgreSQL arrays.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/static/arrays.html
/// </remarks>
public abstract class NpgsqlArrayTypeMapping : RelationalTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlArrayTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
}

/// <summary>
///     Type mapping for PostgreSQL arrays.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/static/arrays.html
/// </remarks>
public class NpgsqlArrayTypeMapping<TCollection, TElement> : NpgsqlArrayTypeMapping
{
    /// <summary>
    /// The database type used by Npgsql.
    /// </summary>
    public virtual NpgsqlDbType? NpgsqlDbType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [UsedImplicitly]
    public NpgsqlArrayTypeMapping(RelationalTypeMapping elementTypeMapping)
        : this(elementTypeMapping.StoreType + "[]", elementTypeMapping) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [UsedImplicitly]
    public NpgsqlArrayTypeMapping(string storeType, RelationalTypeMapping elementTypeMapping)
        : this(CreateParameters(storeType, elementTypeMapping))
    {
        Check.DebugAssert(storeType.EndsWith("[]", StringComparison.Ordinal), "NpgsqlArrayTypeMapping created for a non-array store type");
    }

    private static RelationalTypeMappingParameters CreateParameters(
        string storeType,
        RelationalTypeMapping elementMapping)
    {
        ValueConverter? converter = null;

        if (elementMapping.Converter is { } elementConverter)
        {
            var collectionTypeDefinition = typeof(TCollection) switch
            {
                { IsArray: true } => typeof(TCollection),

                { IsGenericType: true }
                    when typeof(TCollection).GetGenericTypeDefinition() is Type genericTypeDefinition
                    && genericTypeDefinition.GetGenericArguments().Length == 1
                    => genericTypeDefinition,

                _ => throw new ArgumentException(
                    $"Collection CLR type '{typeof(TCollection)}' isn't an a generic type with a single type argument")
            };

            if (typeof(TCollection).TryGetElementType(typeof(IEnumerable<>)) is not Type arrayElementClrType)
            {
                throw new ArgumentException($"Collection CLR type '{typeof(TCollection)}' isn't an IEnumerable");
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

        // We do GetElementType for multidimensional arrays - these don't implement generic IEnumerable<>
        var elementType = typeof(TCollection).TryGetElementType(typeof(IEnumerable<>)) ?? typeof(TCollection).GetElementType();

        Check.DebugAssert(elementType is not null, "modelElementType cannot be null");

#pragma warning disable EF1001
        var comparer = typeof(TCollection).IsArray && typeof(TCollection).GetArrayRank() > 1
            ? null // TODO: Value comparer for multidimensional arrays
            : (ValueComparer?)Activator.CreateInstance(
            elementType.IsNullableValueType()
                ? typeof(NullableValueTypeListComparer<>).MakeGenericType(elementType.UnwrapNullableType())
                : elementMapping.Comparer.Type.IsAssignableFrom(elementType)
                    ? typeof(ListComparer<>).MakeGenericType(elementType)
                    : typeof(ObjectListComparer<>).MakeGenericType(elementType),
            elementMapping.Comparer.ToNullableComparer(elementType)!);
#pragma warning restore EF1001

        return new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(typeof(TCollection), converter, comparer, elementMapping: elementMapping),
            storeType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlArrayTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
        var clrType = parameters.CoreParameters.ClrType;

        if (clrType.TryGetElementType(typeof(IEnumerable<>)) == null && clrType.GetElementType() == null)
        {
            throw new ArgumentException($"CLR type '{parameters.CoreParameters.ClrType}' isn't an IEnumerable");
        }

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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override DbParameter CreateParameter(
        DbCommand command,
        string name,
        object? value,
        bool? nullable = null,
        ParameterDirection direction = ParameterDirection.Input)
    {
        // In queries which compose non-server-correlated LINQ operators over an array parameter (e.g. Where(b => ids.Skip(1)...) we
        // get an enumerable parameter value that isn't an array/list - but those aren't supported at the Npgsql ADO level.
        // Detect this here and evaluate the enumerable to get a fully materialized List.
        // TODO: Make Npgsql support IList<> instead of only arrays and List<>
        if (value is not null && !value.GetType().IsArrayOrGenericList())
        {
            switch (value)
            {
                case IEnumerable<TElement> elements:
                    value = elements.ToList();
                    break;

                case IEnumerable elements:
                    value = elements.Cast<TElement>().ToList();
                    break;
            }
        }

        return base.CreateParameter(command, name, value, nullable, direction);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
    {
        Check.DebugAssert(
            parameters.CoreParameters.ClrType == typeof(TCollection), "NpgsqlArrayTypeMapping.Clone attempting to change ClrType");
        Check.DebugAssert(
            parameters.CoreParameters.ElementTypeMapping is not null, "NpgsqlArrayTypeMapping.Clone without an element type mapping");
        Check.DebugAssert(
            parameters.CoreParameters.ElementTypeMapping.ClrType == typeof(TElement),
            "NpgsqlArrayTypeMapping.Clone attempting to change element ClrType");

        return new NpgsqlArrayTypeMapping<TCollection, TElement>(parameters);
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
}
