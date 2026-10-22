using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlSqlGenerationHelper : RelationalSqlGenerationHelper
{
    private static readonly HashSet<string> ReservedWords;

    static NpgsqlSqlGenerationHelper()
    {
        // https://www.postgresql.org/docs/current/static/sql-keywords-appendix.html
        using (var conn = new NpgsqlConnection())
        {
            ReservedWords =
                [..conn.GetSchema("ReservedWords").Rows.Cast<DataRow>().Select(r => (string)r["ReservedWord"])];
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string DelimitIdentifier(string identifier)
        => RequiresQuoting(identifier) ? base.DelimitIdentifier(identifier) : identifier;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void DelimitIdentifier(StringBuilder builder, string identifier)
    {
        if (RequiresQuoting(identifier))
        {
            base.DelimitIdentifier(builder, identifier);
        }
        else
        {
            builder.Append(identifier);
        }
    }

    // Annotation keys only need quoting to distinguish literal dots and quotes from schema separators.
    internal static string QuoteIfNeeded(string name)
        => name.Contains('.') || name.Contains('"') ? Quote(name) : name;

    private static string Quote(string name)
        => $"\"{name.Replace("\"", "\"\"")}\"";

    internal static string GetSchemaQualifiedName(string? schema, string name)
        => schema is null ? QuoteIfNeeded(name) : $"{Quote(schema)}.{Quote(name)}";

    internal static (string? Schema, string Name) ParseSchemaQualifiedName(string name)
    {
        var separator = -1;
        var inQuotes = false;
        for (var i = 0; i < name.Length; i++)
        {
            switch (name[i])
            {
                case '"':
                    inQuotes = !inQuotes;
                    break;
                case '.' when !inQuotes:
                    if (separator != -1)
                    {
                        throw new ArgumentException($"Cannot parse identifier: {name}");
                    }

                    separator = i;
                    break;
            }
        }

        if (inQuotes)
        {
            throw new ArgumentException($"Cannot parse identifier: {name}");
        }

        return separator == -1
            ? (null, Unquote(name))
            : (Unquote(name[..separator]), Unquote(name[(separator + 1)..]));

        string Unquote(string identifier)
        {
            var unquoted = identifier.Length >= 2 && identifier.StartsWith('"') && identifier.EndsWith('"')
                ? identifier[1..^1].Replace("\"\"", "\"")
                : identifier;

            if (identifier.Length == 0 || (identifier.Contains('"') && Quote(unquoted) != identifier))
            {
                throw new ArgumentException($"Cannot parse identifier: {name}");
            }

            return unquoted;
        }
    }

    internal string DelimitCollation(string collation, IModel? model = null)
    {
        var (schema, name) = ResolveCollation(collation, model);
        return DelimitIdentifier(name, schema);
    }

    internal static (string? Schema, string Name) ResolveCollation(string collation, IModel? model)
    {
        var (schema, name) = collation.StartsWith('"')
            ? ParseSchemaQualifiedName(collation)
            : (null, collation);

        // Only model-defined collations follow the model's default schema; built-in collations remain on the search path.
        if (schema is null
            && model?.GetDefaultSchema() is { } defaultSchema
            && model.GetCollations().Any(c => c.Name == name && (c.Schema is null || c.Schema == defaultSchema)))
        {
            schema = defaultSchema;
        }

        return (schema, name);
    }

    /// <summary>
    ///     Returns whether the given string can be used as an unquoted identifier in PostgreSQL, without quotes.
    /// </summary>
    private static bool RequiresQuoting(string identifier)
    {
        var first = identifier[0];
        if (!char.IsLower(first) && first != '_')
        {
            return true;
        }

        for (var i = 1; i < identifier.Length; i++)
        {
            var c = identifier[i];

            if (char.IsLower(c))
            {
                continue;
            }

            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '_':
                case '$': // yes it's true
                    continue;
            }

            return true;
        }

        if (ReservedWords.Contains(identifier.ToUpperInvariant()))
        {
            return true;
        }

        return false;
    }
}
