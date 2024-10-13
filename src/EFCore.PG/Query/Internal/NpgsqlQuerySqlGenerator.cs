using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     The default query SQL generator for Npgsql.
/// </summary>
public class NpgsqlQuerySqlGenerator : QuerySqlGenerator
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private RelationalTypeMapping? _textTypeMapping;

    /// <summary>
    ///     True if null ordering is reversed; otherwise false.
    /// </summary>
    private readonly bool _reverseNullOrderingEnabled;

    /// <summary>
    ///     The backend version to target. If null, it means the user hasn't set a compatibility version, and the
    ///     latest should be targeted.
    /// </summary>
    private readonly Version _postgresVersion;

    /// <inheritdoc />
    public NpgsqlQuerySqlGenerator(
        QuerySqlGeneratorDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource,
        bool reverseNullOrderingEnabled,
        Version postgresVersion)
        : base(dependencies)
    {
        _sqlGenerationHelper = dependencies.SqlGenerationHelper;
        _typeMappingSource = typeMappingSource;
        _reverseNullOrderingEnabled = reverseNullOrderingEnabled;
        _postgresVersion = postgresVersion;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            PgAllExpression e => VisitArrayAll(e),
            PgAnyExpression e => VisitArrayAny(e),
            PgArrayIndexExpression e => VisitArrayIndex(e),
            PgArraySliceExpression e => VisitArraySlice(e),
            PgBinaryExpression e => VisitPgBinary(e),
            PgDeleteExpression e => VisitPgDelete(e),
            PgFunctionExpression e => VisitPgFunction(e),
            PgILikeExpression e => VisitILike(e),
            PgJsonTraversalExpression e => VisitJsonPathTraversal(e),
            PgNewArrayExpression e => VisitNewArray(e),
            PgRegexMatchExpression e => VisitRegexMatch(e),
            PgRowValueExpression e => VisitRowValue(e),
            PgUnknownBinaryExpression e => VisitUnknownBinary(e),
            PgTableValuedFunctionExpression e => VisitPgTableValuedFunctionExpression(e),

            _ => base.VisitExtension(extensionExpression)
        };

    /// <inheritdoc />
    protected override void GenerateRootCommand(Expression queryExpression)
    {
        switch (queryExpression)
        {
            case PgDeleteExpression postgresDeleteExpression:
                GenerateTagsHeaderComment(postgresDeleteExpression.Tags);
                VisitPgDelete(postgresDeleteExpression);
                break;

            default:
                base.GenerateRootCommand(queryExpression);
                break;
        }
    }

    /// <inheritdoc />
    protected override void GenerateLimitOffset(SelectExpression selectExpression)
    {
        Check.NotNull(selectExpression, nameof(selectExpression));

        if (selectExpression.Limit is not null)
        {
            Sql.AppendLine().Append("LIMIT ");
            Visit(selectExpression.Limit);
        }

        if (selectExpression.Offset is not null)
        {
            if (selectExpression.Limit is null)
            {
                Sql.AppendLine();
            }
            else
            {
                Sql.Append(" ");
            }

            Sql.Append("OFFSET ");
            Visit(selectExpression.Offset);
        }
    }

    /// <inheritdoc />
    protected override string GetOperator(SqlBinaryExpression e)
        => e.OperatorType switch
        {
            // PostgreSQL has a special string concatenation operator: ||
            // We switch to it if the expression itself has type string, or if one of the sides has a string type mapping.
            // Same for full-text search's TsVector, arrays.
            ExpressionType.Add when
                e.Type == typeof(string)
                || e.Left.TypeMapping?.ClrType == typeof(string)
                || e.Right.TypeMapping?.ClrType == typeof(string)
                || e.Type == typeof(NpgsqlTsVector)
                || e.Left.TypeMapping?.ClrType == typeof(NpgsqlTsVector)
                || e.Right.TypeMapping?.ClrType == typeof(NpgsqlTsVector)
                || e.Left.TypeMapping is NpgsqlArrayTypeMapping && e.Right.TypeMapping is NpgsqlArrayTypeMapping
                => " || ",

            ExpressionType.And when e.Type == typeof(bool) => " AND ",
            ExpressionType.Or when e.Type == typeof(bool) => " OR ",

            // In most databases/languages, the caret (^) is the bitwise XOR operator. But in PostgreSQL the caret is the exponentiation
            // operator, and hash (#) is used instead.
            ExpressionType.ExclusiveOr when e.Type == typeof(bool) => " <> ",
            ExpressionType.ExclusiveOr => " # ",

            _ => base.GetOperator(e)
        };

    /// <inheritdoc />
    protected override Expression VisitOrdering(OrderingExpression ordering)
    {
        var result = base.VisitOrdering(ordering);

        if (_reverseNullOrderingEnabled)
        {
            Sql.Append(ordering.IsAscending ? " NULLS FIRST" : " NULLS LAST");
        }

        return result;
    }

    /// <inheritdoc />
    protected override void GenerateTop(SelectExpression selectExpression)
    {
        // No TOP() in PostgreSQL, see GenerateLimitOffset
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
    {
        Sql.Append("JOIN LATERAL ");

        if (crossApplyExpression.Table is TableExpression table)
        {
            // PostgreSQL doesn't support LATERAL JOIN over table, and it doesn't really make sense to do it - but EF Core
            // will sometimes generate that. #1560
            Sql
                .Append("(SELECT * FROM ")
                .Append(_sqlGenerationHelper.DelimitIdentifier(table.Name, table.Schema))
                .Append(")")
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(table.Alias));
        }
        else
        {
            Visit(crossApplyExpression.Table);
        }

        Sql.Append(" ON TRUE");
        return crossApplyExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
    {
        Sql.Append("LEFT JOIN LATERAL ");

        if (outerApplyExpression.Table is TableExpression table)
        {
            // PostgreSQL doesn't support LATERAL JOIN over table, and it doesn't really make sense to do it - but EF Core
            // will sometimes generate that. #1560
            Sql
                .Append("(SELECT * FROM ")
                .Append(_sqlGenerationHelper.DelimitIdentifier(table.Name, table.Schema))
                .Append(")")
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(table.Alias));
        }
        else
        {
            Visit(outerApplyExpression.Table);
        }

        Sql.Append(" ON TRUE");
        return outerApplyExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitSqlBinary(SqlBinaryExpression binary)
    {
        switch (binary.OperatorType)
        {
            case ExpressionType.Add:
            {
                if (_postgresVersion >= new Version(9, 5))
                {
                    return base.VisitSqlBinary(binary);
                }

                // PostgreSQL 9.4 and below has some weird operator precedence fixed in 9.5 and described here:
                // http://git.postgresql.org/gitweb/?p=postgresql.git&a=commitdiff&h=c6b3c939b7e0f1d35f4ed4996e71420a993810d2
                // As a result we must surround string concatenation with parentheses
                if (binary.Left.Type == typeof(string) && binary.Right.Type == typeof(string))
                {
                    Sql.Append("(");
                    var exp = base.VisitSqlBinary(binary);
                    Sql.Append(")");
                    return exp;
                }

                return base.VisitSqlBinary(binary);
            }

            case ExpressionType.ArrayIndex:
                return VisitArrayIndex(binary);

            default:
                return base.VisitSqlBinary(binary);
        }
    }

    // NonQueryConvertingExpressionVisitor converts the relational DeleteExpression to PostgresDeleteExpression, so we should never
    // get here
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitDelete(DeleteExpression deleteExpression)
        => throw new InvalidOperationException("Inconceivable!");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitPgDelete(PgDeleteExpression pgDeleteExpression)
    {
        Sql.Append("DELETE FROM ");
        Visit(pgDeleteExpression.Table);

        if (pgDeleteExpression.FromItems.Count > 0)
        {
            Sql.AppendLine().Append("USING ");
            GenerateList(pgDeleteExpression.FromItems, t => Visit(t), sql => sql.Append(", "));
        }

        if (pgDeleteExpression.Predicate != null)
        {
            Sql.AppendLine().Append("WHERE ");
            Visit(pgDeleteExpression.Predicate);
        }

        return pgDeleteExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUpdate(UpdateExpression updateExpression)
    {
        var selectExpression = updateExpression.SelectExpression;

        if (selectExpression.Offset == null
            && selectExpression.Limit == null
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Projection.Count == 0
            && (selectExpression.Tables.Count == 1
                || !ReferenceEquals(selectExpression.Tables[0], updateExpression.Table)
                || selectExpression.Tables[1] is InnerJoinExpression
                || selectExpression.Tables[1] is CrossJoinExpression))
        {
            Sql.Append("UPDATE ");
            Visit(updateExpression.Table);
            Sql.AppendLine();
            Sql.Append("SET ");
            Sql.Append(
                $"{_sqlGenerationHelper.DelimitIdentifier(updateExpression.ColumnValueSetters[0].Column.Name)} = ");
            Visit(updateExpression.ColumnValueSetters[0].Value);
            using (Sql.Indent())
            {
                foreach (var columnValueSetter in updateExpression.ColumnValueSetters.Skip(1))
                {
                    Sql.AppendLine(",");
                    Sql.Append($"{_sqlGenerationHelper.DelimitIdentifier(columnValueSetter.Column.Name)} = ");
                    Visit(columnValueSetter.Value);
                }
            }

            var predicate = selectExpression.Predicate;
            var firstTable = true;
            OuterReferenceFindingExpressionVisitor? visitor = null;

            if (selectExpression.Tables.Count > 1)
            {
                Sql.AppendLine().Append("FROM ");

                for (var i = 0; i < selectExpression.Tables.Count; i++)
                {
                    var table = selectExpression.Tables[i];
                    var joinExpression = table as JoinExpressionBase;

                    if (updateExpression.Table.Alias == (joinExpression?.Table.Alias ?? table.Alias))
                    {
                        LiftPredicate(table);
                        continue;
                    }

                    visitor ??= new OuterReferenceFindingExpressionVisitor(updateExpression.Table);

                    // PostgreSQL doesn't support referencing the main update table from anywhere except for the UPDATE WHERE clause.
                    // This specifically makes it impossible to have joins which reference the main table in their predicate (ON ...).
                    // Because of this, we detect all such inner joins and lift their predicates to the main WHERE clause (where a reference to the
                    // main table is allowed), producing UPDATE ... FROM x, y WHERE y.foreign_key = x.id instead of INNER JOIN ... ON.
                    if (firstTable)
                    {
                        LiftPredicate(table);
                        table = joinExpression?.Table ?? table;
                    }
                    else if (joinExpression is InnerJoinExpression innerJoinExpression
                             && visitor.ContainsReferenceToMainTable(innerJoinExpression.JoinPredicate))
                    {
                        LiftPredicate(innerJoinExpression);

                        Sql.AppendLine(",");
                        using (Sql.Indent())
                        {
                            Visit(innerJoinExpression.Table);
                        }

                        continue;
                    }

                    if (firstTable)
                    {
                        firstTable = false;
                    }
                    else
                    {
                        Sql.AppendLine();
                    }

                    Visit(table);

                    void LiftPredicate(TableExpressionBase joinTable)
                    {
                        if (joinTable is PredicateJoinExpressionBase predicateJoinExpression)
                        {
                            Check.DebugAssert(joinExpression is not LeftJoinExpression, "Cannot lift predicate for left join");

                            predicate = predicate == null
                                ? predicateJoinExpression.JoinPredicate
                                : new SqlBinaryExpression(
                                    ExpressionType.AndAlso,
                                    predicateJoinExpression.JoinPredicate,
                                    predicate,
                                    typeof(bool),
                                    predicate.TypeMapping);
                        }
                    }
                }
            }

            if (predicate != null)
            {
                Sql.AppendLine().Append("WHERE ");
                Visit(predicate);
            }

            return updateExpression;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate)));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitNewArray(PgNewArrayExpression pgNewArrayExpression)
    {
        Debug.Assert(pgNewArrayExpression.TypeMapping is not null);

        Sql.Append("ARRAY[");
        var first = true;
        foreach (var initializer in pgNewArrayExpression.Expressions)
        {
            if (!first)
            {
                Sql.Append(",");
            }

            first = false;
            Visit(initializer);
        }

        // Not sure if the explicit store type is necessary, but just to be sure.
        Sql
            .Append("]::")
            .Append(pgNewArrayExpression.TypeMapping.StoreType);

        return pgNewArrayExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitPgBinary(PgBinaryExpression binaryExpression)
    {
        Check.NotNull(binaryExpression, nameof(binaryExpression));

        var requiresParentheses = RequiresParentheses(binaryExpression, binaryExpression.Left);

        if (requiresParentheses)
        {
            Sql.Append("(");
        }

        Visit(binaryExpression.Left);

        if (requiresParentheses)
        {
            Sql.Append(")");
        }

        Debug.Assert(binaryExpression.Left.TypeMapping is not null);
        Debug.Assert(binaryExpression.Right.TypeMapping is not null);

        Sql
            .Append(" ")
            .Append(
                binaryExpression.OperatorType switch
                {
                    PgExpressionType.Contains
                        when binaryExpression.Left.TypeMapping is NpgsqlInetTypeMapping or NpgsqlCidrTypeMapping
                        => ">>",

                    PgExpressionType.ContainedBy
                        when binaryExpression.Left.TypeMapping is NpgsqlInetTypeMapping or NpgsqlCidrTypeMapping
                        => "<<",

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
                        when binaryExpression.Right.TypeMapping.StoreType == "lquery"
                        || binaryExpression.Right.TypeMapping is NpgsqlArrayTypeMapping { ElementTypeMapping.StoreType: "lquery" } => "~",
                    PgExpressionType.LTreeMatches
                        when binaryExpression.Right.TypeMapping.StoreType == "ltxtquery"
                        => "@",
                    PgExpressionType.LTreeMatchesAny => "?",
                    PgExpressionType.LTreeFirstAncestor => "?@>",
                    PgExpressionType.LTreeFirstDescendent => "?<@",
                    PgExpressionType.LTreeFirstMatches
                        when binaryExpression.Right.TypeMapping.StoreType == "lquery" => "?~",
                    PgExpressionType.LTreeFirstMatches
                        when binaryExpression.Right.TypeMapping.StoreType == "ltxtquery" => "?@",

                    PgExpressionType.Distance => "<->",

                    PgExpressionType.DictionaryContainsAnyKey => "?|",
                    PgExpressionType.DictionaryContainsAllKeys => "?&",

                    PgExpressionType.DictionaryContainsKey => "?",
                    PgExpressionType.DictionaryValueForKey => "->",
                    PgExpressionType.DictionaryConcat => "||",
                    PgExpressionType.DictionarySubtract => "-",

                    _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {binaryExpression.OperatorType}")
                })
            .Append(" ");

        requiresParentheses = RequiresParentheses(binaryExpression, binaryExpression.Right);

        if (requiresParentheses)
        {
            Sql.Append("(");
        }

        Visit(binaryExpression.Right);

        if (requiresParentheses)
        {
            Sql.Append(")");
        }

        return binaryExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitArrayIndex(SqlBinaryExpression expression)
    {
        Visit(expression.Left);
        Sql.Append("[");
        Visit(expression.Right);
        Sql.Append("]");
        return expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
    {
        Debug.Assert(sqlUnaryExpression.TypeMapping is not null);
        Debug.Assert(sqlUnaryExpression.Operand.TypeMapping is not null);

        switch (sqlUnaryExpression.OperatorType)
        {
            case ExpressionType.Convert:
            {
                // PostgreSQL supports the standard CAST(x AS y), but also a lighter x::y which we use
                // where there's no precedence issues
                switch (sqlUnaryExpression.Operand)
                {
                    case SqlConstantExpression:
                    case SqlParameterExpression:
                    case SqlUnaryExpression { OperatorType: ExpressionType.Convert }:
                    case ColumnExpression:
                    case SqlFunctionExpression:
                    case ScalarSubqueryExpression:
                        var storeType = sqlUnaryExpression.TypeMapping.StoreType switch
                        {
                            "integer" => "int",
                            "timestamp with time zone" => "timestamptz",
                            "timestamp without time zone" => "timestamp",
                            var s => s
                        };

                        Visit(sqlUnaryExpression.Operand);
                        Sql.Append("::");
                        Sql.Append(storeType);
                        return sqlUnaryExpression;
                }

                break;
            }

            // Bitwise complement on networking types
            case ExpressionType.Not when
                sqlUnaryExpression.Operand.TypeMapping.ClrType == typeof(IPAddress)
                || sqlUnaryExpression.Operand.TypeMapping.ClrType == typeof((IPAddress, int))
                || sqlUnaryExpression.Operand.TypeMapping.ClrType == typeof(PhysicalAddress):
                Sql.Append("~");
                Visit(sqlUnaryExpression.Operand);
                return sqlUnaryExpression;

            // NOT operation on full-text queries
            case ExpressionType.Not when sqlUnaryExpression.Operand.TypeMapping.ClrType == typeof(NpgsqlTsQuery):
                Sql.Append("!!");
                Visit(sqlUnaryExpression.Operand);
                return sqlUnaryExpression;

            // NOT over expression types which have fancy embedded negation
            case ExpressionType.Not
                when sqlUnaryExpression.Type == typeof(bool):
            {
                switch (sqlUnaryExpression.Operand)
                {
                    case PgRegexMatchExpression regexMatch:
                        VisitRegexMatch(regexMatch, negated: true);
                        return sqlUnaryExpression;

                    case PgILikeExpression iLike:
                        VisitILike(iLike, negated: true);
                        return sqlUnaryExpression;
                }

                break;
            }
        }

        return base.VisitSqlUnary(sqlUnaryExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateSetOperationOperand(SetOperationBase setOperation, SelectExpression operand)
    {
        // PostgreSQL allows ORDER BY and LIMIT in set operation operands, but requires parentheses
        if (operand.Orderings.Count > 0 || operand.Limit is not null)
        {
            Sql.AppendLine("(");
            using (Sql.Indent())
            {
                Visit(operand);
            }

            Sql.AppendLine().Append(")");
            return;
        }

        base.GenerateSetOperationOperand(setOperation, operand);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitCollate(CollateExpression collateExpression)
    {
        Check.NotNull(collateExpression, nameof(collateExpression));

        Visit(collateExpression.Operand);

        // In PG, collation names are regular identifiers which need to be quoted for case-sensitivity.
        Sql
            .Append(" COLLATE ")
            .Append(_sqlGenerationHelper.DelimitIdentifier(collateExpression.Collation));

        return collateExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool TryGenerateWithoutWrappingSelect(SelectExpression selectExpression)
        // PostgreSQL supports VALUES as a top-level statement - and directly under set operations.
        // However, when on the left side of a set operation, we need the column coming out of VALUES to be named, so we need the wrapping
        // SELECT for that.
        => selectExpression.Tables is not [ValuesExpression]
            && base.TryGenerateWithoutWrappingSelect(selectExpression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateSetOperation(SetOperationBase setOperation)
    {
        GenerateSetOperationOperand(setOperation, setOperation.Source1);

        Sql
            .AppendLine()
            .Append(
                setOperation switch
                {
                    ExceptExpression => "EXCEPT",
                    IntersectExpression => "INTERSECT",
                    UnionExpression => "UNION",
                    _ => throw new InvalidOperationException(CoreStrings.UnknownEntity("SetOperationType"))
                })
            .AppendLine(setOperation.IsDistinct ? string.Empty : " ALL");

        // For ValuesExpression, we can remove its wrapping SelectExpression but only if on the right side of a set operation, since on
        // the left side we need the column name to be specified.
        if (setOperation.Source2 is
            {
                Tables: [ValuesExpression valuesExpression],
                Offset: null,
                Limit: null,
                IsDistinct: false,
                Predicate: null,
                Having: null,
                Orderings.Count: 0,
                GroupBy.Count: 0,
            } rightSelectExpression
            && rightSelectExpression.Projection.Count == valuesExpression.ColumnNames.Count
            && rightSelectExpression.Projection.Select(
                    (pe, index) => pe.Expression is ColumnExpression column
                        && column.Name == valuesExpression.ColumnNames[index])
                .All(e => e))
        {
            GenerateValues(valuesExpression);
        }
        else
        {
            GenerateSetOperationOperand(setOperation, setOperation.Source2);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitValues(ValuesExpression valuesExpression)
    {
        base.VisitValues(valuesExpression);

        // PostgreSQL VALUES supports setting the projects column names: FROM (VALUES (1), (2)) AS v(foo)
        Sql.Append("(");

        for (var i = 0; i < valuesExpression.ColumnNames.Count; i++)
        {
            if (i > 0)
            {
                Sql.Append(", ");
            }

            Sql.Append(_sqlGenerationHelper.DelimitIdentifier(valuesExpression.ColumnNames[i]));
        }

        Sql.Append(")");

        return valuesExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void GenerateValues(ValuesExpression valuesExpression)
    {
        if (valuesExpression.RowValues is null)
        {
            throw new UnreachableException();
        }

        if (valuesExpression.RowValues.Count == 0)
        {
            throw new InvalidOperationException(RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot);
        }

        // PostgreSQL supports providing the names of columns projected out of VALUES: (VALUES (1, 3), (2, 4)) AS x(a, b).
        // But since other databases sometimes don't, the default relational implementation is complex, involving a SELECT for the first row
        // and a UNION All on the rest. Override to do the nice simple thing.
        var rowValues = valuesExpression.RowValues;

        Sql.Append("VALUES ");

        for (var i = 0; i < rowValues.Count; i++)
        {
            // TODO: Do we want newlines here?
            if (i > 0)
            {
                Sql.Append(", ");
            }

            Visit(valuesExpression.RowValues[i]);
        }
    }

    #region PostgreSQL-specific expression types

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitArrayAll(PgAllExpression expression)
    {
        Visit(expression.Item);

        Sql
            .Append(" ")
            .Append(
                expression.OperatorType switch
                {
                    PgAllOperatorType.Like => "LIKE",
                    PgAllOperatorType.ILike => "ILIKE",
                    _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {expression.OperatorType}")
                })
            .Append(" ALL (");

        Visit(expression.Array);

        Sql.Append(")");

        return expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitArrayAny(PgAnyExpression expression)
    {
        Visit(expression.Item);

        Sql
            .Append(" ")
            .Append(
                expression.OperatorType switch
                {
                    PgAnyOperatorType.Equal => "=",
                    PgAnyOperatorType.Like => "LIKE",
                    PgAnyOperatorType.ILike => "ILIKE",
                    _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {expression.OperatorType}")
                })
            .Append(" ANY (");

        Visit(expression.Array);

        Sql.Append(")");

        return expression;
    }

    /// <summary>
    ///     Produces SQL array index expression (e.g. arr[1]).
    /// </summary>
    protected virtual Expression VisitArrayIndex(PgArrayIndexExpression expression)
    {
        var requiresParentheses = RequiresParentheses(expression, expression.Array);

        if (requiresParentheses)
        {
            Sql.Append("(");
        }

        Visit(expression.Array);

        if (requiresParentheses)
        {
            Sql.Append(")");
        }

        Sql.Append("[");
        Visit(expression.Index);
        Sql.Append("]");
        return expression;
    }

    /// <summary>
    ///     Produces SQL array slice expression (e.g. arr[1:2]).
    /// </summary>
    protected virtual Expression VisitArraySlice(PgArraySliceExpression expression)
    {
        var requiresParentheses = RequiresParentheses(expression, expression.Array);

        if (requiresParentheses)
        {
            Sql.Append("(");
        }

        Visit(expression.Array);

        if (requiresParentheses)
        {
            Sql.Append(")");
        }

        Sql.Append("[");
        Visit(expression.LowerBound);
        Sql.Append(":");
        Visit(expression.UpperBound);
        Sql.Append("]");
        return expression;
    }

    /// <summary>
    ///     Produces SQL for PostgreSQL regex matching.
    /// </summary>
    /// <remarks>
    ///     See: http://www.postgresql.org/docs/current/static/functions-matching.html
    /// </remarks>
    protected virtual Expression VisitRegexMatch(PgRegexMatchExpression expression, bool negated = false)
    {
        var options = expression.Options;

        Visit(expression.Match);

        if (options.HasFlag(RegexOptions.IgnoreCase))
        {
            Sql.Append(negated ? " !~* " : " ~* ");
            options &= ~RegexOptions.IgnoreCase;
        }
        else
        {
            Sql.Append(negated ? " !~ " : " ~ ");
        }

        // PG regexps are single-line by default
        if (options == RegexOptions.Singleline)
        {
            Visit(expression.Pattern);
            return expression;
        }

        var constantPattern = (expression.Pattern as SqlConstantExpression)?.Value as string;

        if (constantPattern is null)
        {
            Sql.Append("(");
        }

        Sql.Append("'(?");

        if (options.HasFlag(RegexOptions.Multiline))
        {
            Sql.Append("n");
        }
        else if (!options.HasFlag(RegexOptions.Singleline))
        {
            // In .NET's default mode, . doesn't match newlines but in PostgreSQL it does.
            Sql.Append("p");
        }

        if (options.HasFlag(RegexOptions.IgnorePatternWhitespace))
        {
            Sql.Append("x");
        }

        Sql.Append(")");

        if (constantPattern is null)
        {
            Sql.Append("' || ");
            Visit(expression.Pattern);
            Sql.Append(")");
        }
        else
        {
            Sql.Append(constantPattern.Replace("'", "''"));
            Sql.Append("'");
        }

        // Sql.Append(")' || ");
        // Visit(expression.Pattern);
        // Sql.Append(")");

        return expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitRowValue(PgRowValueExpression rowValueExpression)
    {
        Sql.Append("(");

        var values = rowValueExpression.Values;
        var count = values.Count;
        for (var i = 0; i < count; i++)
        {
            Visit(values[i]);

            if (i < count - 1)
            {
                Sql.Append(", ");
            }
        }

        Sql.Append(")");

        return rowValueExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitILike(PgILikeExpression likeExpression, bool negated = false)
    {
        Visit(likeExpression.Match);

        if (negated)
        {
            Sql.Append(" NOT");
        }

        Sql.Append(" ILIKE ");

        Visit(likeExpression.Pattern);

        if (likeExpression.EscapeChar is not null)
        {
            Sql.Append(" ESCAPE ");
            Visit(likeExpression.EscapeChar);
        }

        return likeExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitJsonScalar(JsonScalarExpression jsonScalarExpression)
    {
        // TODO: Stop producing empty JsonScalarExpressions, #30768
        var path = jsonScalarExpression.Path;
        if (path.Count == 0)
        {
            Visit(jsonScalarExpression.Json);
            return jsonScalarExpression;
        }

        switch (jsonScalarExpression.TypeMapping)
        {
            // This case is for when a nested JSON entity is being accessed. We want the json/jsonb fragment in this case (not text),
            // so we can perform further JSON operations on it.
            case NpgsqlOwnedJsonTypeMapping:
                GenerateJsonPath(returnsText: false);
                break;

            // No need to cast the output when we expect a string anyway
            case StringTypeMapping:
                GenerateJsonPath(returnsText: true);
                break;

            // bytea requires special handling, since we encode the binary data as base64 inside the JSON, but that requires a special
            // conversion function to be extracted out to a PG bytea.
            case NpgsqlByteArrayTypeMapping:
                Sql.Append("decode(");
                GenerateJsonPath(returnsText: true);
                Sql.Append(", 'base64')");
                break;

            // Arrays require special handling; we cannot simply cast a JSON array (as text) to a PG array ([1,2,3] isn't a valid PG array
            // representation). We use jsonb_array_elements_text to extract the array elements as a set, cast them to their PG element type
            // and then build an array from that.
            case NpgsqlArrayTypeMapping arrayMapping:
                Sql.Append("(ARRAY(SELECT CAST(element AS ").Append(arrayMapping.ElementTypeMapping.StoreType)
                    .Append(") FROM jsonb_array_elements_text(");
                GenerateJsonPath(returnsText: false);
                Sql.Append(") WITH ORDINALITY AS t(element) ORDER BY ordinality))");
                break;

            default:
                Sql.Append("CAST(");
                GenerateJsonPath(returnsText: true);
                Sql.Append(" AS ");
                Sql.Append(jsonScalarExpression.TypeMapping!.StoreType);
                Sql.Append(")");
                break;
        }

        return jsonScalarExpression;

        void GenerateJsonPath(bool returnsText)
            => this.GenerateJsonPath(
                jsonScalarExpression.Json,
                returnsText: returnsText,
                jsonScalarExpression.Path.Select(
                    s => s switch
                    {
                        { PropertyName: string propertyName }
                            => new SqlConstantExpression(propertyName, _textTypeMapping ??= _typeMappingSource.FindMapping(typeof(string))),
                        { ArrayIndex: SqlExpression arrayIndex } => arrayIndex,
                        _ => throw new UnreachableException()
                    }).ToList());
    }

    /// <summary>
    ///     Visits the children of an <see cref="PgJsonTraversalExpression" />.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>
    ///     An <see cref="Expression" />.
    /// </returns>
    protected virtual Expression VisitJsonPathTraversal(PgJsonTraversalExpression expression)
    {
        GenerateJsonPath(expression.Expression, expression.ReturnsText, expression.Path);
        return expression;
    }

    private void GenerateJsonPath(SqlExpression expression, bool returnsText, IReadOnlyList<SqlExpression> path)
    {
        Visit(expression);

        if (path.Count == 1)
        {
            Sql.Append(returnsText ? " ->> " : " -> ");
            Visit(path[0]);
            return;
        }

        // Multiple path components
        Sql.Append(returnsText ? " #>> " : " #> ");

        // Use simplified array literal syntax if all path components are constants for cleaner SQL
        if (path.All(p => p is SqlConstantExpression { Value: var pathSegment }
                && (pathSegment is not string s || s.All(char.IsAsciiLetterOrDigit))))
        {
            Sql
                .Append("'{")
                .Append(string.Join(",", path.Select(p => ((SqlConstantExpression)p).Value)))
                .Append("}'");
        }
        else
        {
            Sql.Append("ARRAY[");
            for (var i = 0; i < path.Count; i++)
            {
                Visit(path[i]);
                if (i < path.Count - 1)
                {
                    Sql.Append(",");
                }
            }

            Sql.Append("]::text[]");
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitPgTableValuedFunctionExpression(PgTableValuedFunctionExpression tableValuedFunctionExpression)
    {
        // PostgresTableValuedFunctionExpression extends the standard TableValuedFunctionExpression, adding the possibility to specify
        // column names and types at the end, as well as an optional WITH ORDINALITY to project an index out.

        // Note that PostgreSQL doesn't support specifying both a column definition (with type) *and* WITH ORDINALITY; but it does allow
        // wrapping the function invocation and column definition inside ROWS FROM, and placing the table alias and WITH ORDINALITY outside.
        // We take care of that here.
        if (tableValuedFunctionExpression is
            {
                WithOrdinality: true,
                ColumnInfos: { } columnInfos
            }
            && columnInfos.Any(ci => ci.TypeMapping is not null))
        {
            Sql.Append("ROWS FROM (");

            Sql.Append(tableValuedFunctionExpression.Name).Append("(");
            GenerateList(tableValuedFunctionExpression.Arguments, e => Visit(e));
            Sql.Append(") AS ");

            GenerateColumnDefinition();

            Sql.Append(") WITH ORDINALITY AS ").Append(_sqlGenerationHelper.DelimitIdentifier(tableValuedFunctionExpression.Alias));
        }
        else
        {
            Sql.Append(tableValuedFunctionExpression.Name).Append("(");
            GenerateList(tableValuedFunctionExpression.Arguments, e => Visit(e));
            Sql.Append(")");

            if (tableValuedFunctionExpression.WithOrdinality)
            {
                Sql.Append(" WITH ORDINALITY");
            }

            Sql.Append(AliasSeparator).Append(_sqlGenerationHelper.DelimitIdentifier(tableValuedFunctionExpression.Alias));

            if (tableValuedFunctionExpression.ColumnInfos is not null)
            {
                GenerateColumnDefinition();
            }
        }

        return tableValuedFunctionExpression;

        void GenerateColumnDefinition()
        {
            Sql.Append("(");

            if (tableValuedFunctionExpression.ColumnInfos is [var singleColumnInfo])
            {
                GenerateColumnInfo(singleColumnInfo);
            }
            else
            {
                Sql.AppendLine();
                using var _ = Sql.Indent();

                for (var i = 0; i < tableValuedFunctionExpression.ColumnInfos.Count; i++)
                {
                    var columnInfo = tableValuedFunctionExpression.ColumnInfos[i];

                    if (i > 0)
                    {
                        Sql.AppendLine(",");
                    }

                    GenerateColumnInfo(columnInfo);
                }

                Sql.AppendLine();
            }

            Sql.Append(")");

            void GenerateColumnInfo(PgTableValuedFunctionExpression.ColumnInfo columnInfo)
            {
                Sql.Append(_sqlGenerationHelper.DelimitIdentifier(columnInfo.Name));

                if (columnInfo.TypeMapping is not null)
                {
                    Sql.Append(" ").Append(columnInfo.TypeMapping.StoreType);
                }
            }
        }
    }

    /// <summary>
    ///     Visits the children of a <see cref="PgUnknownBinaryExpression" />.
    /// </summary>
    /// <param name="unknownBinaryExpression">The expression.</param>
    /// <returns>
    ///     An <see cref="Expression" />.
    /// </returns>
    protected virtual Expression VisitUnknownBinary(PgUnknownBinaryExpression unknownBinaryExpression)
    {
        Check.NotNull(unknownBinaryExpression, nameof(unknownBinaryExpression));

        var requiresParentheses = RequiresParentheses(unknownBinaryExpression, unknownBinaryExpression.Left);

        if (requiresParentheses)
        {
            Sql.Append("(");
        }

        Visit(unknownBinaryExpression.Left);

        if (requiresParentheses)
        {
            Sql.Append(")");
        }

        Sql
            .Append(" ")
            .Append(unknownBinaryExpression.Operator)
            .Append(" ");

        requiresParentheses = RequiresParentheses(unknownBinaryExpression, unknownBinaryExpression.Right);

        if (requiresParentheses)
        {
            Sql.Append("(");
        }

        Visit(unknownBinaryExpression.Right);

        if (requiresParentheses)
        {
            Sql.Append(")");
        }

        return unknownBinaryExpression;
    }

    /// <summary>
    ///     Visits the children of a <see cref="PgFunctionExpression" />.
    /// </summary>
    /// <param name="e">The expression.</param>
    /// <returns>
    ///     An <see cref="Expression" />.
    /// </returns>
    protected virtual Expression VisitPgFunction(PgFunctionExpression e)
    {
        Check.NotNull(e, nameof(e));

        if (e.IsBuiltIn)
        {
            Sql.Append(e.Name);
        }
        else
        {
            if (!string.IsNullOrEmpty(e.Schema))
            {
                Sql
                    .Append(_sqlGenerationHelper.DelimitIdentifier(e.Schema))
                    .Append(".");
            }

            // TODO: Quote user-defined function names with upper-case (also for regular SqlFunctionExpression)

            Sql.Append(_sqlGenerationHelper.DelimitIdentifier(e.Name));
        }

        Sql.Append("(");

        if (e.IsAggregateDistinct)
        {
            Sql.Append("DISTINCT ");
        }

        for (var i = 0; i < e.Arguments.Count; i++)
        {
            if (i < e.ArgumentNames.Count && e.ArgumentNames[i] is { } argumentName)
            {
                Sql
                    .Append(argumentName)
                    .Append(" => ");
            }

            Visit(e.Arguments[i]);

            if (i < e.Arguments.Count - 1)
            {
                Sql.Append(
                    i < e.ArgumentSeparators.Count && e.ArgumentSeparators[i] is not null
                        ? $" {e.ArgumentSeparators[i]} "
                        : ", ");
            }
        }

        if (e.AggregateOrderings.Count > 0)
        {
            Sql.Append(" ORDER BY ");

            for (var i = 0; i < e.AggregateOrderings.Count; i++)
            {
                if (i > 0)
                {
                    Sql.Append(", ");
                }

                Visit(e.AggregateOrderings[i]);
            }
        }

        Sql.Append(")");

        if (e.AggregatePredicate is not null)
        {
            Sql.Append(" FILTER (WHERE ");

            Visit(e.AggregatePredicate);

            Sql.Append(")");
        }

        return e;
    }

    #endregion PostgreSQL-specific expression types

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool RequiresParentheses(SqlExpression outerExpression, SqlExpression innerExpression)
    {
        switch (innerExpression)
        {
            // PG doesn't support ~-, -~, ~~, -- so we add parentheses
            case SqlUnaryExpression innerUnary when outerExpression is SqlUnaryExpression outerUnary
                && (innerUnary.OperatorType is ExpressionType.Negate
                    || innerUnary.OperatorType is ExpressionType.Not && innerUnary.Type != typeof(bool))
                && (outerUnary.OperatorType is ExpressionType.Negate
                    || outerUnary.OperatorType is ExpressionType.Not && outerUnary.Type != typeof(bool)):
                return true;

            // Copy paste of QuerySqlGenerator.RequiresParentheses for SqlBinaryExpression
            case PgBinaryExpression innerBinary:
            {
                // If the provider defined precedence for the two expression, use that
                if (TryGetOperatorInfo(outerExpression, out var outerPrecedence, out var isOuterAssociative)
                    && TryGetOperatorInfo(innerExpression, out var innerPrecedence, out _))
                {
                    return outerPrecedence.CompareTo(innerPrecedence) switch
                    {
                        > 0 => true,
                        < 0 => false,

                        // If both operators have the same precedence, add parentheses unless they're the same operator, and
                        // that operator is associative (e.g. a + b + c)
                        _ => outerExpression is not PgBinaryExpression outerBinary
                            || outerBinary.OperatorType != innerBinary.OperatorType
                            || !isOuterAssociative
                            // Arithmetic operators on floating points aren't associative, because of rounding errors.
                            || outerExpression.Type == typeof(float)
                            || outerExpression.Type == typeof(double)
                            || innerExpression.Type == typeof(float)
                            || innerExpression.Type == typeof(double)
                    };
                }

                // Otherwise always parenthesize for safety
                return true;
            }

            case PgUnknownBinaryExpression:
                return true;

            default:
                return base.RequiresParentheses(outerExpression, innerExpression);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool TryGetOperatorInfo(SqlExpression expression, out int precedence, out bool isAssociative)
    {
        // See https://www.postgresql.org/docs/current/sql-syntax-lexical.html#SQL-PRECEDENCE
        (precedence, isAssociative) = expression switch
        {
            // TODO: Exponent => 1300

            SqlBinaryExpression sqlBinaryExpression => sqlBinaryExpression.OperatorType switch
            {
                // Multiplication, division, modulo
                ExpressionType.Multiply => (1200, true),
                ExpressionType.Divide => (1200, false),
                ExpressionType.Modulo => (1200, false),

                // Addition, subtraction (binary)
                ExpressionType.Add => (1100, true),
                ExpressionType.Subtract => (1100, false),

                // All other native and user-defined operators => 1000
                ExpressionType.LeftShift => (1000, true),
                ExpressionType.RightShift => (1000, true),
                ExpressionType.And when sqlBinaryExpression.Type != typeof(bool) => (1000, true),
                ExpressionType.Or when sqlBinaryExpression.Type != typeof(bool) => (1000, true),

                // Comparison operators
                ExpressionType.Equal => (800, false),
                ExpressionType.NotEqual => (800, false),
                ExpressionType.LessThan => (800, false),
                ExpressionType.LessThanOrEqual => (800, false),
                ExpressionType.GreaterThan => (800, false),
                ExpressionType.GreaterThanOrEqual => (800, false),

                // Logical operators
                ExpressionType.AndAlso => (500, true),
                ExpressionType.OrElse => (500, true),
                ExpressionType.And when sqlBinaryExpression.Type == typeof(bool) => (500, true),
                ExpressionType.Or when sqlBinaryExpression.Type == typeof(bool) => (500, true),

                _ => default,
            },

            SqlUnaryExpression sqlUnaryExpression => sqlUnaryExpression.OperatorType switch
            {
                ExpressionType.Convert => (1600, false),
                ExpressionType.Negate => (1400, false),
                ExpressionType.Not when sqlUnaryExpression.Type != typeof(bool) => (1000, false),
                ExpressionType.Equal => (700, false), // IS NULL
                ExpressionType.NotEqual => (700, false), // IS NOT NULL
                ExpressionType.Not when sqlUnaryExpression.Type == typeof(bool) => (600, false),

                _ => default,
            },

            // There's an "any other operator" category in the PG operator precedence table, we assign that a numeric value of 1000.
            // TODO: Some operators here may be associative
            PgBinaryExpression => (1000, false),

            CollateExpression => (1000, false),
            AtTimeZoneExpression => (1100, false),
            InExpression => (900, false),
            PgJsonTraversalExpression => (1000, false),
            PgArrayIndexExpression => (1500, false),
            PgAllExpression or PgAnyExpression => (800, false),
            LikeExpression or PgILikeExpression or PgRegexMatchExpression => (900, false),

            _ => default,
        };

        return precedence != default;
    }

    private void GenerateList<T>(
        IReadOnlyList<T> items,
        Action<T> generationAction,
        Action<IRelationalCommandBuilder>? joinAction = null)
    {
        joinAction ??= (isb => isb.Append(", "));

        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                joinAction(Sql);
            }

            generationAction(items[i]);
        }
    }

    private sealed class OuterReferenceFindingExpressionVisitor(TableExpression mainTable) : ExpressionVisitor
    {
        private bool _containsReference;

        public bool ContainsReferenceToMainTable(SqlExpression sqlExpression)
        {
            _containsReference = false;

            Visit(sqlExpression);

            return _containsReference;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (_containsReference)
            {
                return expression;
            }

            if (expression is ColumnExpression { TableAlias: var tableAlias }
                && tableAlias == mainTable.Alias)
            {
                _containsReference = true;

                return expression;
            }

            return base.Visit(expression);
        }
    }
}
