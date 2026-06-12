namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal;

internal class PendingZonedDateTimeExpression : SqlExpression
{
    internal PendingZonedDateTimeExpression(SqlExpression operand, SqlExpression timeZoneId)
        : base(typeof(ZonedDateTime), typeMapping: null)
    {
        (Operand, TimeZoneId) = (operand, timeZoneId);
    }

    internal SqlExpression Operand { get; }

    internal SqlExpression TimeZoneId { get; }

    public override Expression Quote()
        => throw new UnreachableException("PendingDateTimeZoneProviderExpression is a temporary tree representation and should never be quoted");

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Operand);
        expressionPrinter.Append(" AT TIME ZONE ");
        expressionPrinter.Visit(TimeZoneId);
    }
}
