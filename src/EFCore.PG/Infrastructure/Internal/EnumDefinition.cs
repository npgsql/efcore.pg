using System.Diagnostics.CodeAnalysis;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class EnumDefinition : IEquatable<EnumDefinition>
{
    /// <summary>
    ///     Maps the CLR member values to the PostgreSQL value labels.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public IReadOnlyDictionary<object, string> Labels { get; }

    /// <summary>
    ///     The name of the PostgreSQL enum type to be mapped.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public string StoreTypeName { get; }

    /// <summary>
    ///     The PostgreSQL schema in which the enum is defined. If null, the default schema is used
    ///     (which is public unless changed on the model).
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public string? StoreTypeSchema { get; }

    /// <summary>
    ///     The CLR type of the enum.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public Type ClrType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public INpgsqlNameTranslator NameTranslator { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EnumDefinition(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type clrType,
        string? name,
        string? schema,
        INpgsqlNameTranslator nameTranslator)
    {
        if (clrType is not { IsEnum: true, IsClass: false })
        {
            throw new ArgumentException($"Enum type mappings require a CLR enum. {clrType.FullName} is not an enum.");
        }

        StoreTypeName = name ?? nameTranslator.TranslateTypeName(clrType.Name);
        StoreTypeSchema = schema;
        ClrType = clrType;

        NameTranslator = nameTranslator;
        Labels = clrType.GetFields(BindingFlags.Static | BindingFlags.Public)
            .ToDictionary(
                x => x.GetValue(null)!,
                x => x.GetCustomAttribute<PgNameAttribute>()?.PgName ?? nameTranslator.TranslateMemberName(x.Name));
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is EnumDefinition other && Equals(other);

    /// <inheritdoc />
    public bool Equals(EnumDefinition? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(other, this))
        {
            return true;
        }

        if (StoreTypeName != other.StoreTypeName
            || StoreTypeSchema != other.StoreTypeSchema
            || ClrType != other.ClrType
            || !ReferenceEquals(NameTranslator, other.NameTranslator)
            || Labels.Count != other.Labels.Count)
        {
            return false;
        }

        foreach (var (key, value) in Labels)
        {
            if (!other.Labels.TryGetValue(key, out var otherValue)
                || value != otherValue)
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(StoreTypeName);
        hashCode.Add(StoreTypeSchema);
        hashCode.Add(ClrType);
        hashCode.Add(NameTranslator);
        foreach (var (key, value) in Labels)
        {
            hashCode.Add(key);
            hashCode.Add(value);
        }

        return hashCode.ToHashCode();
    }
}
