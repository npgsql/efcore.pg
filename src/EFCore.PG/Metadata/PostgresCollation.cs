using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class PostgresCollation
{
    private readonly IReadOnlyAnnotatable _annotatable;
    private readonly string _annotationName;

    internal PostgresCollation(IReadOnlyAnnotatable annotatable, string annotationName)
    {
        _annotatable = Check.NotNull(annotatable, nameof(annotatable));
        _annotationName = Check.NotNull(annotationName, nameof(annotationName));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static PostgresCollation GetOrAddCollation(
        IMutableAnnotatable annotatable,
        string? schema,
        string name,
        string lcCollate,
        string lcCtype,
        string? provider = null,
        bool? deterministic = null)
    {
        Check.NotNull(annotatable, nameof(annotatable));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotEmpty(name, nameof(name));

        if (FindCollation(annotatable, schema, name) is { } collation)
        {
            return collation;
        }

        var annotationName = BuildAnnotationName(schema, name);

        return new PostgresCollation(annotatable, annotationName)
        {
            LcCollate = lcCollate,
            LcCtype = lcCtype,
            Provider = provider,
            IsDeterministic = deterministic
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static PostgresCollation? FindCollation(
        IReadOnlyAnnotatable annotatable,
        string? schema,
        string name)
    {
        Check.NotNull(annotatable, nameof(annotatable));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotEmpty(name, nameof(name));

        var annotationName = BuildAnnotationName(schema, name);

        return annotatable[annotationName] is null ? null : new PostgresCollation(annotatable, annotationName);
    }

    internal static string BuildAnnotationName(string? schema, string name)
        => schema is not null
            ? $"{NpgsqlAnnotationNames.CollationDefinitionPrefix}{NpgsqlSqlGenerationHelper.QuoteIfNeeded(schema)}.{NpgsqlSqlGenerationHelper.QuoteIfNeeded(name)}"
            : $"{NpgsqlAnnotationNames.CollationDefinitionPrefix}{NpgsqlSqlGenerationHelper.QuoteIfNeeded(name)}";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<PostgresCollation> GetCollations(IReadOnlyAnnotatable annotatable)
        => Check.NotNull(annotatable, nameof(annotatable))
            .GetAnnotations()
            .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.CollationDefinitionPrefix, StringComparison.Ordinal))
            .Select(a => new PostgresCollation(annotatable, a.Name));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Annotatable Annotatable
        => (Annotatable)_annotatable;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? Schema
        => GetData().Schema;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name
        => GetData().Name!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string LcCollate
    {
        get => GetData().LcCollate!;
        set => SetData(lcCollate: value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string LcCtype
    {
        get => GetData().LcCtype!;
        set => SetData(lcCtype: value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? Provider
    {
        get => GetData().Provider;
        set => SetData(provider: value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? IsDeterministic
    {
        get => GetData().IsDeterministic;
        set => SetData(deterministic: value);
    }

    private (string? Schema, string? Name, string? LcCollate, string? LcCtype, string? Provider, bool? IsDeterministic) GetData()
        => Deserialize(_annotatable.FindAnnotation(_annotationName));

    private void SetData(string? lcCollate = null, string? lcCtype = null, string? provider = null, bool? deterministic = null)
        => Annotatable[_annotationName] =
            $"{lcCollate ?? LcCollate},{lcCtype ?? LcCtype},{provider ?? Provider},{deterministic ?? IsDeterministic}";

    private static (string? Schema, string? Name, string? LcCollate, string? LcCtype, string? Provider, bool? IsDeterministic)
        Deserialize(IAnnotation? annotation)
    {
        if (annotation is null || !(annotation.Value is string value) || string.IsNullOrEmpty(value))
        {
            return (null, null!, null!, null!, null, null);
        }

        string?[] elements = value.Split(',');
        if (elements.Length != 4)
        {
            throw new ArgumentException($"Cannot parse collation annotation value: {value}");
        }

        for (var i = 0; i < 4; i++)
        {
            if (elements[i] == "")
            {
                elements[i] = null;
            }
        }

        var isDeterministic = elements[3] is { } isDeterminsticString
            ? bool.Parse(isDeterminsticString)
            : (bool?)null;

        var (schema, name) = NpgsqlSqlGenerationHelper.ParseSchemaQualifiedName(annotation.Name[NpgsqlAnnotationNames.CollationDefinitionPrefix.Length..]);
        return (schema, name, elements[0], elements[1], elements[2], isDeterministic);
    }
}
