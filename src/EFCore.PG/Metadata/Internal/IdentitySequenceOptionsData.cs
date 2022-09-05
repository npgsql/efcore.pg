using System.Globalization;
using System.Text;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class IdentitySequenceOptionsData : IEquatable<IdentitySequenceOptionsData>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly IdentitySequenceOptionsData Empty = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual long? StartValue { get; set; }
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual long IncrementBy { get; set; } = 1;
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual long? MinValue { get; set; }
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual long? MaxValue { get; set; }
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsCyclic { get; set; }
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual long NumbersToCache { get; set; } = 1;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Serialize()
    {
        var builder = new StringBuilder();

        EscapeAndQuote(builder, StartValue);
        builder.Append(", ");
        EscapeAndQuote(builder, IncrementBy);
        builder.Append(", ");
        EscapeAndQuote(builder, MinValue);
        builder.Append(", ");
        EscapeAndQuote(builder, MaxValue);
        builder.Append(", ");
        EscapeAndQuote(builder, IsCyclic);
        builder.Append(", ");
        EscapeAndQuote(builder, NumbersToCache);

        return builder.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IdentitySequenceOptionsData Get(IReadOnlyAnnotatable annotatable)
        => Deserialize((string?)annotatable[NpgsqlAnnotationNames.IdentityOptions]);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IdentitySequenceOptionsData Deserialize(string? value)
    {
        var data = new IdentitySequenceOptionsData();

        if (value is null)
        {
            return data;
        }

        try
        {
            // ReSharper disable PossibleInvalidOperationException
            var position = 0;
            data.StartValue = AsLong(ExtractValue(value, ref position));
            data.IncrementBy = (int)AsLong(ExtractValue(value, ref position))!;
            data.MinValue = AsLong(ExtractValue(value, ref position));
            data.MaxValue = AsLong(ExtractValue(value, ref position));
            data.IsCyclic = AsBool(ExtractValue(value, ref position));
            data.NumbersToCache = (int)AsLong(ExtractValue(value, ref position))!;
            // ReSharper restore PossibleInvalidOperationException

            return data;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Couldn't deserialize {nameof(IdentitySequenceOptionsData)} from annotation", ex);
        }
    }

    private static string? ExtractValue(string value, ref int position)
    {
        position = value.IndexOf('\'', position) + 1;

        var end = value.IndexOf('\'', position);

        while (end + 1 < value.Length
               && value[end + 1] == '\'')
        {
            end = value.IndexOf('\'', end + 2);
        }

        var extracted = value.Substring(position, end - position).Replace("''", "'");
        position = end + 1;

        return extracted.Length == 0 ? null : extracted;
    }

    private static long? AsLong(string? value)
        => value is null ? null : long.Parse(value, CultureInfo.InvariantCulture);

    private static bool AsBool(string? value)
        => value is not null && bool.Parse(value);

    private static void EscapeAndQuote(StringBuilder builder, object? value)
    {
        builder.Append("'");

        if (value is not null)
        {
            builder.Append(value.ToString()!.Replace("'", "''"));
        }

        builder.Append("'");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool Equals(IdentitySequenceOptionsData? other)
        => !(other is null) && (
            ReferenceEquals(this, other) ||
            StartValue == other.StartValue &&
            IncrementBy == other.IncrementBy &&
            MinValue == other.MinValue &&
            MaxValue == other.MaxValue &&
            IsCyclic == other.IsCyclic &&
            NumbersToCache == other.NumbersToCache
        );

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is IdentitySequenceOptionsData other && Equals(other);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetHashCode()
        => HashCode.Combine(StartValue, IncrementBy, MinValue, MaxValue, IsCyclic, NumbersToCache);
}
