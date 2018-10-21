using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL AT TIME ZONE expression.
    /// </summary>
    public class AtTimeZoneExpression : Expression
    {
        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>
        /// The timestamp.
        /// </summary>
        [NotNull]
        public Expression Timestamp { get; }

        /// <summary>
        /// The time zone.
        /// </summary>
        [NotNull]
        public string TimeZone { get; }

        /// <summary>
        /// Constructs an <see cref="AtTimeZoneExpression"/>.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="type">The type of the expression.</param>
        /// <exception cref="ArgumentNullException" />
        public AtTimeZoneExpression([NotNull] Expression timestamp, [NotNull] string timeZone, [NotNull] Type type)
        {
            Timestamp = Check.NotNull(timestamp, nameof(timestamp));
            TimeZone = Check.NotNull(timeZone, nameof(timeZone));
            Type = Check.NotNull(type, nameof(type));
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitAtTimeZone(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var timestamp = visitor.Visit(Timestamp) ?? Timestamp;

            return
                timestamp != Timestamp
                    ? new AtTimeZoneExpression(timestamp, TimeZone, Type)
                    : this;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Timestamp} AT TIME ZONE {TimeZone}";
    }
}
