using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlEnumTypeMapping : RelationalTypeMapping
{
    /// <summary>
    ///     Translates the CLR member value to the PostgreSQL value label.
    /// </summary>
    private readonly Dictionary<object, string> _members;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static NpgsqlEnumTypeMapping Default { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual INpgsqlNameTranslator NameTranslator { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlEnumTypeMapping(string storeType, Type enumType, INpgsqlNameTranslator? nameTranslator = null)
        : base(
            storeType,
            enumType,
            jsonValueReaderWriter: (JsonValueReaderWriter?)Activator.CreateInstance(
                typeof(JsonPgEnumReaderWriter<>).MakeGenericType(enumType)))
    {
        if (!enumType.IsEnum || !enumType.IsValueType)
        {
            throw new ArgumentException($"Enum type mappings require a CLR enum. {enumType.FullName} is not an enum.");
        }

#pragma warning disable CS0618 // NpgsqlConnection.GlobalTypeMapper is obsolete
        nameTranslator ??= NpgsqlConnection.GlobalTypeMapper.DefaultNameTranslator;
#pragma warning restore CS0618

        NameTranslator = nameTranslator;
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
        INpgsqlNameTranslator nameTranslator)
        : base(parameters)
    {
        NameTranslator = nameTranslator;
        _members = CreateValueMapping(parameters.CoreParameters.ClrType, nameTranslator);
    }

    // This constructor exists only to support the static Default property above, which is necessary to allow code generation for compiled
    // models. The constructor creates a completely blank type mapping, which will get cloned with all the correct details.
    private NpgsqlEnumTypeMapping()
        : base("some_enum", typeof(int))
    {
#pragma warning disable CS0618 // NpgsqlConnection.GlobalTypeMapper is obsolete
        NameTranslator = NpgsqlConnection.GlobalTypeMapper.DefaultNameTranslator;
#pragma warning restore CS0618
        _members = null!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlEnumTypeMapping(parameters, NameTranslator);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{_members[value]}'::{StoreType}";

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

    // This is public for the compiled model
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class JsonPgEnumReaderWriter<T> : JsonValueReaderWriter<T>
        where T : struct, Enum
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override T FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => Enum.Parse<T>(manager.CurrentReader.GetString()!);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void ToJsonTyped(Utf8JsonWriter writer, T value)
            => writer.WriteStringValue(value.ToString());
    }
}
