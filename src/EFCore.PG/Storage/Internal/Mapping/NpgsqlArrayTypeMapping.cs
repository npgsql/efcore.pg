using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
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
            Check.DebugAssert(
                elementTypeMapping is not null,
                "NpgsqlArrayTypeMapping without an element type mapping");
            Check.DebugAssert(
                elementTypeMapping is RelationalTypeMapping,
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
public class NpgsqlArrayTypeMapping<TCollection, TConcreteCollection, TElement> : NpgsqlArrayTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static NpgsqlArrayTypeMapping<TCollection, TConcreteCollection, TElement> Default { get; }
        = new();

    /// <summary>
    ///     The database type used by Npgsql.
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
        : this(elementTypeMapping.StoreType + "[]", elementTypeMapping)
    {
    }

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

    private static RelationalTypeMappingParameters CreateParameters(string storeType, RelationalTypeMapping elementMapping)
    {
        ValueConverter? converter = null;

        // We do GetElementType for multidimensional arrays - these don't implement generic IEnumerable<>
        var elementType = typeof(TCollection).TryGetElementType(typeof(IEnumerable<>)) ?? typeof(TCollection).GetElementType();

        Check.DebugAssert(elementType is not null, "modelElementType cannot be null");

        if (elementMapping.Converter is { } elementConverter)
        {
            var providerElementType = elementConverter.ProviderClrType;

            // Nullability has been unwrapped on the element converter's provider CLR type, so add it back here if needed
            if (elementType.IsNullableValueType())
            {
                providerElementType = providerElementType.MakeNullable();
            }

            converter = (ValueConverter)Activator.CreateInstance(
                typeof(NpgsqlArrayConverter<,,>).MakeGenericType(
                    typeof(TCollection), typeof(TConcreteCollection), providerElementType.MakeArrayType()),
                elementConverter)!;
        }
        else if (typeof(TCollection) != typeof(TConcreteCollection))
        {
            converter = (ValueConverter)Activator.CreateInstance(
                typeof(NpgsqlArrayConverter<,,>).MakeGenericType(
                    typeof(TCollection), typeof(TConcreteCollection), elementType.MakeArrayType()))!;
        }

#pragma warning disable EF1001
        var comparer = typeof(TCollection).IsArray && typeof(TCollection).GetArrayRank() > 1
            ? null // TODO: Value comparer for multidimensional arrays
            : (ValueComparer?)Activator.CreateInstance(
                elementType.IsNullableValueType() || elementMapping.Comparer.Type.IsNullableValueType()
                    ? typeof(ListOfNullableValueTypesComparer<,>)
                        .MakeGenericType(typeof(TConcreteCollection), elementType.UnwrapNullableType())
                    : elementType.IsValueType
                        ? typeof(ListOfValueTypesComparer<,>).MakeGenericType(typeof(TConcreteCollection), elementType)
                        : typeof(ListOfReferenceTypesComparer<,>).MakeGenericType(typeof(TConcreteCollection), elementType),
                elementMapping.Comparer.ToNullableComparer(elementType)!);
#pragma warning restore EF1001

        var elementJsonReaderWriter = elementMapping.JsonValueReaderWriter;
        if (elementJsonReaderWriter is not null && !typeof(TElement).UnwrapNullableType().IsAssignableTo(elementJsonReaderWriter.ValueType))
        {
            throw new InvalidOperationException(
                $"When building an array mapping over '{typeof(TElement).Name}', the JsonValueReaderWriter for element mapping '{elementMapping.GetType().Name}' is incorrect ('{elementJsonReaderWriter.ValueType.GetType().Name}' instead of '{typeof(TElement).UnwrapNullableType()}', the JsonValueReaderWriter is '{elementJsonReaderWriter.GetType().Name}').");
        }

        // If there's no JsonValueReaderWriter on the element, we also don't set one on its array (this is for rare edge cases such as
        // NpgsqlRowValueTypeMapping).
        // TODO: Also, we don't (yet) support JSON serialization of multidimensional arrays.
        var collectionJsonReaderWriter =
            elementJsonReaderWriter is null || typeof(TCollection).IsArray && typeof(TCollection).GetArrayRank() > 1
                ? null
                : (JsonValueReaderWriter?)Activator.CreateInstance(
                    (elementType.IsNullableValueType()
                        ? typeof(JsonCollectionOfNullableStructsReaderWriter<,>)
                        : elementType.IsValueType
                            ? typeof(JsonCollectionOfStructsReaderWriter<,>)
                            : typeof(JsonCollectionOfReferencesReaderWriter<,>))
                    .MakeGenericType(typeof(TConcreteCollection), elementType.UnwrapNullableType()),
                    elementJsonReaderWriter);

        return new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(
                typeof(TCollection), converter, comparer, elementMapping: elementMapping,
                jsonValueReaderWriter: collectionJsonReaderWriter),
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
        NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Array
            | (ElementTypeMapping is INpgsqlTypeMapping { NpgsqlDbType: not NpgsqlTypes.NpgsqlDbType.Unknown } elementNpgsqlTypeMapping
                ? elementNpgsqlTypeMapping.NpgsqlDbType
                : ElementTypeMapping.DbType.HasValue
                    ? new NpgsqlParameter { DbType = ElementTypeMapping.DbType.Value }.NpgsqlDbType
                    : default(NpgsqlDbType?));
    }

    // This constructor exists only to support the static Default property above, which is necessary to allow code generation for compiled
    // models. The constructor creates a completely blank type mapping, which will get cloned with all the correct details.
    private NpgsqlArrayTypeMapping()
        : base(new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(typeof(TCollection), elementMapping: NullMapping),
            "int[]"))
    {
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
        // Note that when we have a value converter (e.g. for HashSet), we don't want to convert it to a List, since the value converter
        // expects the original type.
        // TODO: Make Npgsql support IList<> instead of only arrays and List<>
        if (value is not null && Converter is null && !value.GetType().IsArrayOrGenericList())
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

        var param = base.CreateParameter(command, name, value, nullable, direction);
        if (param is not NpgsqlParameter npgsqlParameter)
        {
            throw new InvalidOperationException(
                $"Npgsql-specific type mapping {GetType().Name} being used with non-Npgsql parameter type {param.GetType().Name}");
        }

        // Enums and user-defined ranges require setting NpgsqlParameter.DataTypeName to specify the PostgreSQL type name.
        // Make this work for arrays over these types as well.
        switch (ElementTypeMapping)
        {
            case NpgsqlEnumTypeMapping enumTypeMapping:
                npgsqlParameter.DataTypeName = enumTypeMapping.UnquotedStoreType + "[]";
                break;
            case NpgsqlRangeTypeMapping { UnquotedStoreType: string unquotedStoreType }:
                npgsqlParameter.DataTypeName = unquotedStoreType + "[]";
                break;
        }
        return param;
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
            parameters.CoreParameters.ElementTypeMapping.ClrType == typeof(TElement).UnwrapNullableType(),
            "NpgsqlArrayTypeMapping.Clone attempting to change element ClrType");

        return new NpgsqlArrayTypeMapping<TCollection, TConcreteCollection, TElement>(parameters);
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
