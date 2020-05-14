namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// PostgreSQL-specific expression node types.
    /// </summary>
    public enum PostgresExpressionType
    {
        Contains,
        ContainedBy,
        Overlaps,

        AtTimeZone,

        NetworkContainedByOrEqual,
        NetworkContainsOrEqual,
        NetworkContainsOrContainedBy,

        RangeIsStrictlyLeftOf,
        RangeIsStrictlyRightOf,
        RangeDoesNotExtendRightOf,
        RangeDoesNotExtendLeftOf,
        RangeIsAdjacentTo,
        RangeUnion,
        RangeIntersect,
        RangeExcept,

        TextSearchMatch,
        TextSearchAnd,
        TextSearchOr,

        JsonExists,
        JsonExistsAny,
        JsonExistsAll
    }
}
