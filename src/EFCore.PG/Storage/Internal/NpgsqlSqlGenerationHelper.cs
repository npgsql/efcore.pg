using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlSqlGenerationHelper : RelationalSqlGenerationHelper
    {
        static readonly HashSet<string> ReservedWords;

        static NpgsqlSqlGenerationHelper()
        {
            // https://www.postgresql.org/docs/current/static/sql-keywords-appendix.html
            using (var conn = new NpgsqlConnection())
                ReservedWords = new HashSet<string>(conn.GetSchema("ReservedWords").Rows.Cast<DataRow>().Select(r => (string)r["ReservedWord"]));
        }

        public NpgsqlSqlGenerationHelper([NotNull] RelationalSqlGenerationHelperDependencies dependencies)
            : base(dependencies) {}

        public override string DelimitIdentifier(string identifier)
            => RequiresQuoting(identifier) ? base.DelimitIdentifier(identifier) : identifier;

        public override void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            if (RequiresQuoting(identifier))
                base.DelimitIdentifier(builder, identifier);
            else
                builder.Append(identifier);
        }

        /// <summary>
        /// Returns whether the given string can be used as an unquoted identifier in PostgreSQL, without quotes.
        /// </summary>
        static bool RequiresQuoting(string identifier)
        {
            var first = identifier[0];
            if (!char.IsLower(first) && first != '_')
                return true;

            for (var i = 1; i < identifier.Length; i++)
            {
                var c = identifier[i];

                if (char.IsLower(c))
                    continue;

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
                return true;

            return false;
        }
    }
}
