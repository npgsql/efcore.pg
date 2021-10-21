using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NodaTime;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal
{
    internal class PendingDateTimeZoneProviderExpression : SqlExpression
    {
        internal static PendingDateTimeZoneProviderExpression Instance = new();

        private PendingDateTimeZoneProviderExpression()
            : base(typeof(IDateTimeZoneProvider), typeMapping: null)
        {
        }

        protected override void Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.Append("TZDB");
    }
}
