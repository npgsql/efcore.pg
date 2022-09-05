using System.Data;
using System.Text;

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
            ReservedWords = new HashSet<string>(conn.GetSchema("ReservedWords").Rows.Cast<DataRow>().Select(r => (string)r["ReservedWord"]));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
        : base(dependencies) {}

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

    /// <summary>
    /// Returns whether the given string can be used as an unquoted identifier in PostgreSQL, without quotes.
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
                case '$':  // yes it's true
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
