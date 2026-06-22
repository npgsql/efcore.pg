namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal;

internal class PendingDateTimeZoneProviderExpression : SqlExpression
{
    internal static PendingDateTimeZoneProviderExpression Instance = new();

    private PendingDateTimeZoneProviderExpression()
        : base(typeof(IDateTimeZoneProvider), typeMapping: null)
    {
    }

    public override Expression Quote()
        => throw new UnreachableException("PendingDateTimeZoneProviderExpression is a temporary tree representation and should never be quoted");

    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append("TZDB");
}
