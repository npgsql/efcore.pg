using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

/// <summary>
/// Represents the metadata for a PostgreSQL enum.
/// </summary>
public class PostgresEnum
{
    private readonly IReadOnlyAnnotatable _annotatable;
    private readonly string _annotationName;

    /// <summary>
    /// Creates a <see cref="PostgresEnum"/>.
    /// </summary>
    /// <param name="annotatable">The annotatable to search for the annotation.</param>
    /// <param name="annotationName">The annotation name to search for in the annotatable.</param>
    /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="annotationName"/></exception>
    internal PostgresEnum(IReadOnlyAnnotatable annotatable, string annotationName)
    {
        _annotatable = Check.NotNull(annotatable, nameof(annotatable));
        _annotationName = Check.NotNull(annotationName, nameof(annotationName));
    }

    /// <summary>
    /// Gets or adds a <see cref="PostgresEnum"/> from or to the <see cref="IMutableAnnotatable"/>.
    /// </summary>
    /// <param name="annotatable">The annotatable from which to get or add the enum.</param>
    /// <param name="schema">The enum schema or null to use the model's default schema.</param>
    /// <param name="name">The enum name.</param>
    /// <param name="labels">The enum labels.</param>
    /// <returns>
    /// The <see cref="PostgresEnum"/> from the <see cref="IMutableAnnotatable"/>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="schema"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="labels"/></exception>
    public static PostgresEnum GetOrAddPostgresEnum(
        IMutableAnnotatable annotatable,
        string? schema,
        string name,
        string[] labels)
    {
        Check.NotNull(annotatable, nameof(annotatable));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(labels, nameof(labels));

        if (FindPostgresEnum(annotatable, schema, name) is { } enumType)
        {
            return enumType;
        }

        var annotationName = BuildAnnotationName(schema, name);

        return new PostgresEnum(annotatable, annotationName) { Labels = labels };
    }

    /// <summary>
    /// Gets or adds a <see cref="PostgresEnum"/> from or to the <see cref="IMutableAnnotatable"/>.
    /// </summary>
    /// <param name="annotatable">The annotatable from which to get or add the enum.</param>
    /// <param name="name">The enum name.</param>
    /// <param name="labels">The enum labels.</param>
    /// <returns>
    /// The <see cref="PostgresEnum"/> from the <see cref="IMutableAnnotatable"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="labels"/></exception>
    public static PostgresEnum GetOrAddPostgresEnum(
        IMutableAnnotatable annotatable,
        string name,
        string[] labels)
        => GetOrAddPostgresEnum(annotatable, null, name, labels);

    /// <summary>
    /// Finds a <see cref="PostgresEnum"/> in the <see cref="IAnnotatable"/>, or returns null if not found.
    /// </summary>
    /// <param name="annotatable">The annotatable to search for the enum.</param>
    /// <param name="schema">The enum schema or null to use the model's default schema.</param>
    /// <param name="name">The enum name.</param>
    /// <returns>
    /// The <see cref="PostgresEnum"/> from the <see cref="IAnnotatable"/>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="schema"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
    public static PostgresEnum? FindPostgresEnum(
        IReadOnlyAnnotatable annotatable,
        string? schema,
        string name)
    {
        Check.NotNull(annotatable, nameof(annotatable));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotEmpty(name, nameof(name));

        var annotationName = BuildAnnotationName(schema, name);

        return annotatable[annotationName] is null ? null : new PostgresEnum(annotatable, annotationName);
    }

    private static string BuildAnnotationName(string? schema, string name)
        => schema is not null
            ? $"{NpgsqlAnnotationNames.EnumPrefix}{schema}.{name}"
            : $"{NpgsqlAnnotationNames.EnumPrefix}{name}";

    /// <summary>
    /// Gets the collection of <see cref="PostgresEnum"/> stored in the <see cref="IAnnotatable"/>.
    /// </summary>
    /// <param name="annotatable">The annotatable to search for <see cref="PostgresEnum"/> annotations.</param>
    /// <returns>
    /// The collection of <see cref="PostgresEnum"/> stored in the <see cref="IAnnotatable"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
    public static IEnumerable<PostgresEnum> GetPostgresEnums(IReadOnlyAnnotatable annotatable)
        => Check.NotNull(annotatable, nameof(annotatable))
            .GetAnnotations()
            .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal))
            .Select(a => new PostgresEnum(annotatable, a.Name));

    /// <summary>
    /// The <see cref="Annotatable"/> that stores the enum.
    /// </summary>
    public virtual Annotatable Annotatable => (Annotatable)_annotatable;

    /// <summary>
    /// The enum schema or null to represent the default schema.
    /// </summary>
    public virtual string? Schema => GetData().Schema;

    /// <summary>
    /// The enum name.
    /// </summary>
    public virtual string Name => GetData().Name!;

    /// <summary>
    /// The enum labels.
    /// </summary>
    public virtual IReadOnlyList<string> Labels
    {
        get => GetData().Labels!;
        set => SetData(value);
    }

    private (string? Schema, string? Name, string[]? Labels) GetData()
        => Deserialize(Annotatable.FindAnnotation(_annotationName));

    private void SetData(IEnumerable<string> labels)
        => Annotatable[_annotationName] = string.Join(",", labels);

    private static (string? Schema, string? Name, string[]? Labels) Deserialize(IAnnotation? annotation)
    {
        if (annotation is null || !(annotation.Value is string value) || string.IsNullOrEmpty(value))
        {
            return (null, null, null);
        }

        var labels = value.Split(',');

        // TODO: This would be a safer operation if we stored schema and name in the annotation value (see Sequence.cs).
        // Yes, this doesn't support dots in the schema/enum name, let somebody complain first.
        var schemaAndName = annotation.Name.Substring(NpgsqlAnnotationNames.EnumPrefix.Length).Split('.');
        switch (schemaAndName.Length)
        {
            case 1:
                return (null, schemaAndName[0], labels);
            case 2:
                return (schemaAndName[0], schemaAndName[1], labels);
            default:
                throw new ArgumentException($"Cannot parse enum name from annotation: {annotation.Name}");
        }
    }
}