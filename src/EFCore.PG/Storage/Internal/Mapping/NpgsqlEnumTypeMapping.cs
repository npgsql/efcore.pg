namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlEnumTypeMapping : RelationalTypeMapping
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private readonly INpgsqlNameTranslator _nameTranslator;

    /// <summary>
    /// Translates the CLR member value to the PostgreSQL value label.
    /// </summary>
    private readonly Dictionary<object, string> _members;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlEnumTypeMapping(
        string storeType,
        string? storeTypeSchema,
        Type enumType,
        ISqlGenerationHelper sqlGenerationHelper,
        INpgsqlNameTranslator? nameTranslator = null)
        : base(sqlGenerationHelper.DelimitIdentifier(storeType, storeTypeSchema), enumType)
    {
        if (!enumType.IsEnum || !enumType.IsValueType)
        {
            throw new ArgumentException($"Enum type mappings require a CLR enum. {enumType.FullName} is not an enum.");
        }

        nameTranslator ??= NpgsqlConnection.GlobalTypeMapper.DefaultNameTranslator;

        _nameTranslator = nameTranslator;
        _sqlGenerationHelper = sqlGenerationHelper;
        _members = CreateValueMapping(enumType, nameTranslator);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlEnumTypeMapping(
        RelationalTypeMappingParameters parameters,
        ISqlGenerationHelper sqlGenerationHelper,
        INpgsqlNameTranslator nameTranslator)
        : base(parameters)
    {
        _nameTranslator = nameTranslator;
        _sqlGenerationHelper = sqlGenerationHelper;
        _members = CreateValueMapping(parameters.CoreParameters.ClrType, nameTranslator);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlEnumTypeMapping(parameters, _sqlGenerationHelper, _nameTranslator);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value) => $"'{_members[value]}'::{StoreType}";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private static Dictionary<object, string> CreateValueMapping(Type enumType, INpgsqlNameTranslator nameTranslator)
        => enumType.GetFields(BindingFlags.Static | BindingFlags.Public)
            .ToDictionary(
                x => x.GetValue(null)!,
                x => x.GetCustomAttribute<PgNameAttribute>()?.PgName ?? nameTranslator.TranslateMemberName(x.Name));
}
