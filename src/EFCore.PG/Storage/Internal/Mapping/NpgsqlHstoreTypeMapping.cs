#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The type mapping for the PostgreSQL hstore type.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/hstore.html
    /// </remarks>
    public class NpgsqlHstoreTypeMapping : NpgsqlTypeMapping
    {
        static readonly HstoreComparer ComparerInstance = new HstoreComparer();

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlHstoreTypeMapping"/> class.
        /// </summary>
        public NpgsqlHstoreTypeMapping()
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(Dictionary<string, string>), null, ComparerInstance),
                    "hstore"),
                NpgsqlDbType.Hstore) {}

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlHstoreTypeMapping"/> class.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        protected NpgsqlHstoreTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Hstore) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlHstoreTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var sb = new StringBuilder("HSTORE '");
            foreach (var kv in (Dictionary<string, string>)value)
            {
                sb.Append('"');
                sb.Append(kv.Key); // TODO: Escape
                sb.Append("\"=>");
                if (kv.Value == null)
                    sb.Append("NULL");
                else
                {
                    sb.Append('"');
                    sb.Append(kv.Value); // TODO: Escape
                    sb.Append("\",");
                }
            }

            sb.Remove(sb.Length - 1, 1);

            sb.Append('\'');
            return sb.ToString();
        }

        /// <inheritdoc />
        class HstoreComparer : ValueComparer<Dictionary<string, string>>
        {
            public HstoreComparer() : base(
                (a, b) => Compare(a, b),
                o => o.GetHashCode(),
                o => o == null ? null : new Dictionary<string, string>(o)) {}

            static bool Compare(Dictionary<string, string> a, Dictionary<string, string> b)
            {
                if (a == null)
                    return b == null;
                if (b == null)
                    return false;
                if (a.Count != b.Count)
                    return false;
                foreach (var kv in a)
                {
                    if (!b.TryGetValue(kv.Key, out var bValue) || kv.Value != bValue)
                        return false;
                }

                return true;
            }
        }
    }
}
