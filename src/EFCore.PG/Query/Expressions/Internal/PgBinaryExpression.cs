using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
///     An expression that represents a PostgreSQL-specific binary operation in a SQL tree.
/// </summary>
public class PgBinaryExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="PgBinaryExpression" /> class.
    /// </summary>
    /// <param name="operatorType">The operator to apply.</param>
    /// <param name="left">An expression which is left operand.</param>
    /// <param name="right">An expression which is right operand.</param>
    /// <param name="type">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public PgBinaryExpression(
        PgExpressionType operatorType,
        SqlExpression left,
        SqlExpression right,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        Check.NotNull(left, nameof(left));
        Check.NotNull(right, nameof(right));

        OperatorType = operatorType;
        Left = left;
        Right = right;
    }

    /// <summary>
    ///     The operator of this PostgreSQL binary operation.
    /// </summary>
    public virtual PgExpressionType OperatorType { get; }

    /// <summary>
    ///     The left operand.
    /// </summary>
    public virtual SqlExpression Left { get; }

    /// <summary>
    ///     The right operand.
    /// </summary>
    public virtual SqlExpression Right { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Check.NotNull(visitor, nameof(visitor));

        var left = (SqlExpression)visitor.Visit(Left);
        var right = (SqlExpression)visitor.Visit(Right);

        return Update(left, right);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="left">The <see cref="Left" /> property of the result.</param>
    /// <param name="right">The <see cref="Right" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual PgBinaryExpression Update(SqlExpression left, SqlExpression right)
    {
        Check.NotNull(left, nameof(left));
        Check.NotNull(right, nameof(right));

        return left != Left || right != Right
            ? new PgBinaryExpression(OperatorType, left, right, Type, TypeMapping)
            : this;
    }

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(PgBinaryExpression).GetConstructor(
                [typeof(PgExpressionType), typeof(SqlExpression), typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)])!,
            Constant(OperatorType),
            Left.Quote(),
            Right.Quote(),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        Check.NotNull(expressionPrinter, nameof(expressionPrinter));

        var requiresBrackets = RequiresBrackets(Left);

        if (requiresBrackets)
        {
            expressionPrinter.Append("(");
        }

        expressionPrinter.Visit(Left);

        if (requiresBrackets)
        {
            expressionPrinter.Append(")");
        }

        expressionPrinter
            .Append(" ")
            .Append(
                OperatorType switch
                {
                    PgExpressionType.Contains => "@>",
                    PgExpressionType.ContainedBy => "<@",
                    PgExpressionType.Overlaps => "&&",

                    PgExpressionType.NetworkContainedByOrEqual => "<<=",
                    PgExpressionType.NetworkContainsOrEqual => ">>=",
                    PgExpressionType.NetworkContainsOrContainedBy => "&&",

                    PgExpressionType.RangeIsStrictlyLeftOf => "<<",
                    PgExpressionType.RangeIsStrictlyRightOf => ">>",
                    PgExpressionType.RangeDoesNotExtendRightOf => "&<",
                    PgExpressionType.RangeDoesNotExtendLeftOf => "&>",
                    PgExpressionType.RangeIsAdjacentTo => "-|-",
                    PgExpressionType.RangeUnion => "+",
                    PgExpressionType.RangeIntersect => "*",
                    PgExpressionType.RangeExcept => "-",

                    PgExpressionType.TextSearchMatch => "@@",
                    PgExpressionType.TextSearchAnd => "&&",
                    PgExpressionType.TextSearchOr => "||",

                    PgExpressionType.JsonExists => "?",
                    PgExpressionType.JsonExistsAny => "?|",
                    PgExpressionType.JsonExistsAll => "?&",
                    PgExpressionType.JsonValueForKeyAsText => "->>",

                    PgExpressionType.LTreeMatches
                        when Right.TypeMapping is { StoreType: "lquery" } or NpgsqlArrayTypeMapping
                        {
                            ElementTypeMapping.StoreType: "lquery"
                        }
                        => "~",
                    PgExpressionType.LTreeMatches when Right.TypeMapping?.StoreType == "ltxtquery" => "@",
                    PgExpressionType.LTreeMatchesAny => "?",
                    PgExpressionType.LTreeFirstAncestor => "?@>",
                    PgExpressionType.LTreeFirstDescendent => "?<@",
                    PgExpressionType.LTreeFirstMatches when Right.TypeMapping?.StoreType == "lquery" => "?~",
                    PgExpressionType.LTreeFirstMatches when Right.TypeMapping?.StoreType == "ltxtquery" => "?@",

                    PgExpressionType.Distance => "<->",

                    PgExpressionType.DictionaryContainsAnyKey => "?|",
                    PgExpressionType.DictionaryContainsAllKeys => "?&",

                    PgExpressionType.DictionaryContainsKey => "?",
                    PgExpressionType.DictionaryValueForKey => "->",
                    PgExpressionType.DictionaryConcat => "||",
                    PgExpressionType.DictionarySubtract => "-",

                    _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {OperatorType}")
                })
            .Append(" ");

        requiresBrackets = RequiresBrackets(Right);

        if (requiresBrackets)
        {
            expressionPrinter.Append("(");
        }

        expressionPrinter.Visit(Right);

        if (requiresBrackets)
        {
            expressionPrinter.Append(")");
        }

        static bool RequiresBrackets(SqlExpression expression)
            => expression is PgBinaryExpression or LikeExpression;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is not null
            && (ReferenceEquals(this, obj)
                || obj is PgBinaryExpression sqlBinaryExpression
                && Equals(sqlBinaryExpression));

    private bool Equals(PgBinaryExpression sqlBinaryExpression)
        => base.Equals(sqlBinaryExpression)
            && OperatorType == sqlBinaryExpression.OperatorType
            && Left.Equals(sqlBinaryExpression.Left)
            && Right.Equals(sqlBinaryExpression.Right);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), OperatorType, Left, Right);
}
