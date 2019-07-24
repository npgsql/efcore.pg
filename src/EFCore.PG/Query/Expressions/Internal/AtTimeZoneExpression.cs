using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Pipeline;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL AT TIME ZONE expression.
    /// </summary>
    public class AtTimeZoneExpression : SqlExpression, IEquatable<AtTimeZoneExpression>
    {
        /// <summary>
        /// The timestamp.
        /// </summary>
        [NotNull]
        public SqlExpression Timestamp { get; }

        /// <summary>
        /// The time zone.
        /// </summary>
        [NotNull]
        public SqlExpression TimeZone { get; }

        /// <summary>
        /// Constructs an <see cref="AtTimeZoneExpression"/>.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="type">The type of the expression.</param>
        /// <exception cref="ArgumentNullException" />
        public AtTimeZoneExpression([NotNull] SqlExpression timestamp, [NotNull] SqlExpression timeZone, [NotNull] Type type, RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Timestamp = Check.NotNull(timestamp, nameof(timestamp));
            TimeZone = Check.NotNull(timeZone, nameof(timeZone));
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitAtTimeZone(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Timestamp), (SqlExpression)visitor.Visit(TimeZone));

        public AtTimeZoneExpression Update(SqlExpression timestamp, SqlExpression timeZone)
            => timestamp == Timestamp && timeZone == TimeZone
                ? this
                : new AtTimeZoneExpression(timestamp, TimeZone, Type, TypeMapping);

        public bool Equals(AtTimeZoneExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               base.Equals(other) &&
               Timestamp.Equals(other.Timestamp) &&
               TimeZone.Equals(other.TimeZone);

        public override bool Equals(object obj) => obj is AtTimeZoneExpression e && Equals(e);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Timestamp, TimeZone);

        public override void Print(ExpressionPrinter expressionPrinter)
        {
#pragma warning disable EF1001
            expressionPrinter.Visit(Timestamp);
            expressionPrinter.StringBuilder.Append(" AT TIME ZONE ");
            expressionPrinter.Visit(TimeZone);
#pragma warning restore EF1001
        }

        /// <inheritdoc />
        public override string ToString() => $"{Timestamp} AT TIME ZONE {TimeZone}";
    }
}
