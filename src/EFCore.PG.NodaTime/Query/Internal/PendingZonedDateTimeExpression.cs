using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NodaTime;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal;

internal class PendingZonedDateTimeExpression : SqlExpression
{
    internal PendingZonedDateTimeExpression(SqlExpression operand, string timeZoneId)
        : base(typeof(ZonedDateTime), typeMapping: null)
        => (Operand, TimeZoneId) = (operand, timeZoneId);

    internal SqlExpression Operand { get; }

    internal string TimeZoneId { get; }

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Operand);

        expressionPrinter
            .Append(" AT TIME ZONE '")
            .Append(TimeZoneId)
            .Append("'");
    }
}