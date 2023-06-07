namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

/// <summary>
/// Represents the metadata for a PostgreSQL extension.
/// </summary>
public class PostgresExtension : IPostgresExtension, IMutablePostgresExtension, IConventionPostgresExtension
{
    private readonly string? _schema;

    private ConfigurationSource _configurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    internal PostgresExtension(
        string name,
        string? schema,
        string? version,
        IReadOnlyModel? model,
        ConfigurationSource configurationSource)
    {
        Model = model;
        Name = name;
        Version = version;
        _schema = schema;
        _configurationSource = configurationSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyModel? Model { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? Schema
        => _schema ?? Model?.GetDefaultSchema();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? Version { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource GetConfigurationSource()
        => _configurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        => _configurationSource = _configurationSource.Max(configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IPostgresExtension? FindPostgresExtension(IReadOnlyModel model, string name, string? schema)
        => model[NpgsqlAnnotationNames.PostgresExtensions] is SortedDictionary<(string, string?), IPostgresExtension> postgresExtensions
            && postgresExtensions.TryGetValue((name, schema), out var postgresExtension)
                ? postgresExtension
                : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static PostgresExtension AddPostgresExtension(
        IMutableAnnotatable annotatable,
        string name,
        string? schema,
        string? version,
        ConfigurationSource configurationSource)
    {
        var postgresExtension = new PostgresExtension(name, schema, version, annotatable as IReadOnlyModel, configurationSource);
        var postgresExtensions = (SortedDictionary<(string, string?), IPostgresExtension>?)annotatable[NpgsqlAnnotationNames.PostgresExtensions];
        if (postgresExtensions == null)
        {
            postgresExtensions = new SortedDictionary<(string, string?), IPostgresExtension>();
            annotatable[NpgsqlAnnotationNames.PostgresExtensions] = postgresExtensions;
        }

        postgresExtensions.Add((name, schema), postgresExtension);

        return postgresExtension;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IPostgresExtension> GetPostgresExtensions(IReadOnlyAnnotatable annotatable)
    {
        switch (annotatable[NpgsqlAnnotationNames.PostgresExtensions])
        {
            // This is the standard representation of the PostgresExtensions annotation in the model
            case SortedDictionary<(string, string?), IPostgresExtension> postgresExtensions:
                foreach (var postgresExtension in postgresExtensions.Values)
                {
                    yield return postgresExtension;
                }
                break;

            // In migration Up/Down code, we can't have complex types, so we have a simplified serialized format - a simple array,
            // whose elements are either a string (for a name-only extension) or a name-schema-version array.
            case object[] extensionsArray:
                foreach (var extensionData in extensionsArray)
                {
                    yield return extensionData switch
                    {
                        string name => new PostgresExtension(name, schema: null, version: null, model: null, ConfigurationSource.Explicit),
                        string[] array => new PostgresExtension(name: array[0], schema: array[1], version: array[2], model: null, ConfigurationSource.Explicit),
                        _ => throw new InvalidOperationException("Invalid PostgreSQL extensions annotation value")
                    };
                }
                break;

            case null:
                yield break;

            default:
                throw new InvalidOperationException("Invalid PostgreSQL extensions annotation value");
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IModel? IPostgresExtension.Model
    {
        [DebuggerStepThrough]
        get => (IModel?)Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableModel? IMutablePostgresExtension.Model
    {
        [DebuggerStepThrough]
        get => (IMutableModel?)Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionModel? IConventionPostgresExtension.Model
    {
        [DebuggerStepThrough]
        get => (IConventionModel?)Model;
    }
}