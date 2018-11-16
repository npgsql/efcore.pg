using System;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL UUID functions.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/datatype-uuid.html
    /// </remarks>
    public class NpgsqlGuidTranslator : SingleOverloadStaticMethodCallTranslator
    {
        /// <inheritdoc />
        public NpgsqlGuidTranslator() : base(typeof(Guid), nameof(Guid.NewGuid), "uuid_generate_v4") {}
    }
}
