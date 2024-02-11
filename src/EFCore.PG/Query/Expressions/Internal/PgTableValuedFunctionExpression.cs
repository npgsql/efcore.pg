namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
///     An expression that represents a PostgreSQL <c>unnest</c> function call in a SQL tree.
/// </summary>
/// <remarks>
///     <para>
///         This expression is just a <see cref="TableValuedFunctionExpression" />, adding the ability to provide an explicit column name
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
public class PgTableValuedFunctionExpression : TableValuedFunctionExpression, IEquatable<PgTableValuedFunctionExpression>
{
    /// <summary>
    ///     The name of the column to be projected out from the <c>unnest</c> call.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual IReadOnlyList<ColumnInfo>? ColumnInfos { get; }

    /// <summary>
    ///     Whether to project an additional ordinality column containing the index of each element in the array.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual bool WithOrdinality { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public PgTableValuedFunctionExpression(
        string alias,
        string name,
        IReadOnlyList<SqlExpression> arguments,
        IReadOnlyList<ColumnInfo>? columnInfos,
        bool withOrdinality = true)
        : base(alias, name, schema: null, builtIn: true, arguments)
    {
        ColumnInfos = columnInfos;
        WithOrdinality = withOrdinality;
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => visitor.VisitAndConvert(Arguments) is var visitedArguments && visitedArguments == Arguments
            ? this
            : new PgTableValuedFunctionExpression(Alias, Name, visitedArguments, ColumnInfos, WithOrdinality);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override PgTableValuedFunctionExpression Update(IReadOnlyList<SqlExpression> arguments)
        => arguments.SequenceEqual(Arguments, ReferenceEqualityComparer.Instance)
            ? this
            : new PgTableValuedFunctionExpression(Alias, Name, arguments, ColumnInfos, WithOrdinality);

    /// <inheritdoc />
    public override TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor)
    {
        var arguments = new SqlExpression[Arguments.Count];
        for (var i = 0; i < arguments.Length; i++)
        {
            arguments[i] = (SqlExpression)cloningExpressionVisitor.Visit(Arguments[i]);
        }

        return new PgTableValuedFunctionExpression(Alias, Name, arguments, ColumnInfos, WithOrdinality);
    }

    /// <inheritdoc />
    public override PgTableValuedFunctionExpression WithAlias(string newAlias)
        => new(newAlias, Name, Arguments, ColumnInfos, WithOrdinality);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PgTableValuedFunctionExpression WithColumnInfos(IReadOnlyList<ColumnInfo> columnInfos)
        => new(Alias, Name, Arguments, columnInfos, WithOrdinality);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(Name);
        expressionPrinter.Append("(");
        expressionPrinter.VisitCollection(Arguments);
        expressionPrinter.Append(")");

        if (WithOrdinality)
        {
            expressionPrinter.Append(" WITH ORDINALITY");
        }

        PrintAnnotations(expressionPrinter);

        expressionPrinter.Append(" AS ").Append(Alias);

        if (ColumnInfos is not null)
        {
            expressionPrinter.Append("(");

            var isFirst = true;

            foreach (var column in ColumnInfos)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    expressionPrinter.Append(", ");
                }

                expressionPrinter.Append(column.Name);

                if (column.TypeMapping is not null)
                {
                    expressionPrinter.Append(" ").Append(column.TypeMapping.StoreType);
                }
            }

            expressionPrinter.Append(")");
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => ReferenceEquals(obj, this) || obj is PgTableValuedFunctionExpression e && Equals(e);

    /// <inheritdoc />
    public bool Equals(PgTableValuedFunctionExpression? expression)
        => base.Equals(expression)
            && (
                expression.ColumnInfos is null && ColumnInfos is null
                || expression.ColumnInfos is not null && ColumnInfos is not null && expression.ColumnInfos.SequenceEqual(ColumnInfos))
            && WithOrdinality == expression.WithOrdinality;

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();

    /// <summary>
    ///     Defines the name of a column coming out of a <see cref="PgTableValuedFunctionExpression" /> and optionally its type.
    /// </summary>
    public readonly record struct ColumnInfo(string Name, RelationalTypeMapping? TypeMapping = null);
}
