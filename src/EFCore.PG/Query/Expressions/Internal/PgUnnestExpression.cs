namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
///     An expression that represents a PostgreSQL <c>unnest</c> function call in a SQL tree.
/// </summary>
/// <remarks>
///     <para>
///         This expression is just a <see cref="PgTableValuedFunctionExpression" />, adding the ability to provide an explicit column name
///         for its output (<c>SELECT * FROM unnest(array) AS f(foo)</c>). This is necessary since when the column name isn't explicitly
///         specified, it is automatically identical to the table alias (<c>f</c> above); since the table alias may get uniquified by
///         EF, this would break queries.
///     </para>
///     <para>
///         See <see href="https://www.postgresql.org/docs/current/functions-array.html#ARRAY-FUNCTIONS-TABLE">unnest</see> for more
///         information and examples.
///     </para>
///     <para>
///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///         the same compatibility standards as public APIs. It may be changed or removed without notice in
///         any release. You should only use it directly in your code with extreme caution and knowing that
///         doing so can result in application failures when updating to a new Entity Framework Core release.
///     </para>
/// </remarks>
public class PgUnnestExpression : PgTableValuedFunctionExpression
{
    /// <summary>
    ///     The array to be un-nested into a table.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual SqlExpression Array
        => Arguments[0];

    /// <summary>
    ///     The name of the column to be projected out from the <c>unnest</c> call.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual string ColumnName
        => ColumnInfos![0].Name;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public PgUnnestExpression(string alias, SqlExpression array, string columnName, bool withOrdinality = true)
        : this(alias, array, new ColumnInfo(columnName), withOrdinality)
    {
    }

    private PgUnnestExpression(string alias, SqlExpression array, ColumnInfo? columnInfo, bool withOrdinality = true)
        : base(alias, "unnest", [array], columnInfo is null ? null : [columnInfo.Value], withOrdinality)
    {
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => visitor.Visit(Array) is var visitedArray && visitedArray == Array
            ? this
            : new PgUnnestExpression(Alias, (SqlExpression)visitedArray, ColumnName, WithOrdinality);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override PgUnnestExpression Update(IReadOnlyList<SqlExpression> arguments)
        => arguments is [var singleArgument]
            ? Update(singleArgument)
            : throw new ArgumentException();

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="array">The <see cref="Array" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual PgUnnestExpression Update(SqlExpression array)
        => array == Array
            ? this
            : new PgUnnestExpression(Alias, array, ColumnName, WithOrdinality);

    /// <inheritdoc />
    public override TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor)
        => new PgUnnestExpression(alias!, (SqlExpression)cloningExpressionVisitor.Visit(Array), ColumnName, WithOrdinality);

    /// <inheritdoc />
    public override PgUnnestExpression WithAlias(string newAlias)
        => new(newAlias, Array, ColumnName, WithOrdinality);

    /// <inheritdoc />
    public override PgUnnestExpression WithColumnInfos(IReadOnlyList<ColumnInfo> columnInfos)
        => new(
            Alias,
            Array,
            columnInfos switch
            {
                [] => null,
                [var columnInfo] => columnInfo,
                _ => throw new ArgumentException()
            },
            WithOrdinality);
}
