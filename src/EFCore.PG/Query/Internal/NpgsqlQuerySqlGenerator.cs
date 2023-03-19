using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
/// The default query SQL generator for Npgsql.
/// </summary>
public class NpgsqlQuerySqlGenerator : QuerySqlGenerator
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    /// <summary>
    /// True if null ordering is reversed; otherwise false.
    /// </summary>
    private readonly bool _reverseNullOrderingEnabled;

    /// <summary>
    /// The backend version to target. If null, it means the user hasn't set a compatibility version, and the
    /// latest should be targeted.
    /// </summary>
    private readonly Version _postgresVersion;

    /// <inheritdoc />
    public NpgsqlQuerySqlGenerator(
        QuerySqlGeneratorDependencies dependencies,
        bool reverseNullOrderingEnabled,
        Version postgresVersion)
        : base(dependencies)
    {
        _sqlGenerationHelper = dependencies.SqlGenerationHelper;
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
            PostgresAllExpression e => VisitArrayAll(e),
            PostgresAnyExpression e => VisitArrayAny(e),
            PostgresArrayIndexExpression e => VisitArrayIndex(e),
            PostgresArraySliceExpression e => VisitArraySlice(e),
            PostgresBinaryExpression e => VisitPostgresBinary(e),
            PostgresDeleteExpression e => VisitPostgresDelete(e),
            PostgresFunctionExpression e => VisitPostgresFunction(e),
            PostgresILikeExpression e => VisitILike(e),
            PostgresJsonTraversalExpression e => VisitJsonPathTraversal(e),
            PostgresNewArrayExpression e => VisitPostgresNewArray(e),
            PostgresRegexMatchExpression e => VisitRegexMatch(e),
            PostgresRowValueExpression e => VisitRowValue(e),
            PostgresUnknownBinaryExpression e => VisitUnknownBinary(e),
            PostgresUnnestExpression e => VisitUnnestExpression(e),
            _ => base.VisitExtension(extensionExpression)
        };

    /// <inheritdoc />
    protected override void GenerateRootCommand(Expression queryExpression)
    {
        switch (queryExpression)
        {
            case PostgresDeleteExpression postgresDeleteExpression:
                GenerateTagsHeaderComment(postgresDeleteExpression.Tags);
                VisitPostgresDelete(postgresDeleteExpression);
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
                e.Type == typeof(string) || e.Left.TypeMapping?.ClrType == typeof(string) || e.Right.TypeMapping?.ClrType == typeof(string) ||
                e.Type == typeof(NpgsqlTsVector) || e.Left.TypeMapping?.ClrType == typeof(NpgsqlTsVector) || e.Right.TypeMapping?.ClrType == typeof(NpgsqlTsVector) ||
                e.Left.TypeMapping is NpgsqlArrayTypeMapping && e.Right.TypeMapping is NpgsqlArrayTypeMapping
                => " || ",

            ExpressionType.And when e.Type == typeof(bool)   => " AND ",
            ExpressionType.Or  when e.Type == typeof(bool)   => " OR ",
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
                if (binary.Left.Type == typeof(string) &&
                    binary.Right.Type == typeof(string))
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
    protected virtual Expression VisitPostgresDelete(PostgresDeleteExpression pgDeleteExpression)
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

                    if (ReferenceEquals(updateExpression.Table, joinExpression?.Table ?? table))
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
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(RelationalQueryableExtensions.ExecuteUpdate)));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitPostgresNewArray(PostgresNewArrayExpression postgresNewArrayExpression)
    {
        Debug.Assert(postgresNewArrayExpression.TypeMapping is not null);

        Sql.Append("ARRAY[");
        var first = true;
        foreach (var initializer in postgresNewArrayExpression.Expressions)
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
            .Append(postgresNewArrayExpression.TypeMapping.StoreType);

        return postgresNewArrayExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitPostgresBinary(PostgresBinaryExpression binaryExpression)
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
            .Append(binaryExpression.OperatorType switch
            {
                PostgresExpressionType.Contains
                    when binaryExpression.Left.TypeMapping is NpgsqlInetTypeMapping ||
                    binaryExpression.Left.TypeMapping is NpgsqlCidrTypeMapping
                    => ">>",

                PostgresExpressionType.ContainedBy
                    when binaryExpression.Left.TypeMapping is NpgsqlInetTypeMapping ||
                    binaryExpression.Left.TypeMapping is NpgsqlCidrTypeMapping
                    => "<<",

                PostgresExpressionType.Contains    => "@>",
                PostgresExpressionType.ContainedBy => "<@",
                PostgresExpressionType.Overlaps    => "&&",

                PostgresExpressionType.NetworkContainedByOrEqual    => "<<=",
                PostgresExpressionType.NetworkContainsOrEqual       => ">>=",
                PostgresExpressionType.NetworkContainsOrContainedBy => "&&",

                PostgresExpressionType.RangeIsStrictlyLeftOf     => "<<",
                PostgresExpressionType.RangeIsStrictlyRightOf    => ">>",
                PostgresExpressionType.RangeDoesNotExtendRightOf => "&<",
                PostgresExpressionType.RangeDoesNotExtendLeftOf  => "&>",
                PostgresExpressionType.RangeIsAdjacentTo         => "-|-",
                PostgresExpressionType.RangeUnion                => "+",
                PostgresExpressionType.RangeIntersect            => "*",
                PostgresExpressionType.RangeExcept               => "-",

                PostgresExpressionType.TextSearchMatch => "@@",
                PostgresExpressionType.TextSearchAnd   => "&&",
                PostgresExpressionType.TextSearchOr    => "||",

                PostgresExpressionType.JsonExists    => "?",
                PostgresExpressionType.JsonExistsAny => "?|",
                PostgresExpressionType.JsonExistsAll => "?&",

                PostgresExpressionType.LTreeMatches
                    when binaryExpression.Right.TypeMapping.StoreType == "lquery" ||
                    binaryExpression.Right.TypeMapping is NpgsqlArrayTypeMapping arrayMapping &&
                    arrayMapping.ElementTypeMapping.StoreType == "lquery"
                    => "~",
                PostgresExpressionType.LTreeMatches
                    when binaryExpression.Right.TypeMapping.StoreType == "ltxtquery"
                    => "@",
                PostgresExpressionType.LTreeMatchesAny      => "?",
                PostgresExpressionType.LTreeFirstAncestor   => "?@>",
                PostgresExpressionType.LTreeFirstDescendent => "?<@",
                PostgresExpressionType.LTreeFirstMatches
                    when binaryExpression.Right.TypeMapping.StoreType == "lquery" => "?~",
                PostgresExpressionType.LTreeFirstMatches
                    when binaryExpression.Right.TypeMapping.StoreType == "ltxtquery" => "?@",

                PostgresExpressionType.Distance => "<->",

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

            // Not operation on full-text queries
            case ExpressionType.Not when sqlUnaryExpression.Operand.TypeMapping.ClrType == typeof(NpgsqlTsQuery):
                Sql.Append("!!");
                Visit(sqlUnaryExpression.Operand);
                return sqlUnaryExpression;
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
    public virtual Expression VisitArrayAll(PostgresAllExpression expression)
    {
        Visit(expression.Item);

        Sql
            .Append(" ")
            .Append(expression.OperatorType switch
            {
                PostgresAllOperatorType.Like => "LIKE",
                PostgresAllOperatorType.ILike => "ILIKE",
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
    public virtual Expression VisitArrayAny(PostgresAnyExpression expression)
    {
        Visit(expression.Item);

        Sql
            .Append(" ")
            .Append(expression.OperatorType switch
            {
                PostgresAnyOperatorType.Equal => "=",
                PostgresAnyOperatorType.Like => "LIKE",
                PostgresAnyOperatorType.ILike => "ILIKE",
                _ => throw new ArgumentOutOfRangeException($"Unhandled operator type: {expression.OperatorType}")
            })
            .Append(" ANY (");

        Visit(expression.Array);

        Sql.Append(")");

        return expression;
    }

    /// <summary>
    /// Produces SQL array index expression (e.g. arr[1]).
    /// </summary>
    public virtual Expression VisitArrayIndex(PostgresArrayIndexExpression expression)
    {
        Visit(expression.Array);
        Sql.Append("[");
        Visit(expression.Index);
        Sql.Append("]");
        return expression;
    }

    /// <summary>
    /// Produces SQL array slice expression (e.g. arr[1:2]).
    /// </summary>
    public virtual Expression VisitArraySlice(PostgresArraySliceExpression expression)
    {
        Visit(expression.Array);
        Sql.Append("[");
        Visit(expression.LowerBound);
        Sql.Append(":");
        Visit(expression.UpperBound);
        Sql.Append("]");
        return expression;
    }

    /// <summary>
    /// Visits the children of a <see cref="PostgresRegexMatchExpression"/>.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>
    /// An <see cref="Expression"/>.
    /// </returns>
    /// <remarks>
    /// See: http://www.postgresql.org/docs/current/static/functions-matching.html
    /// </remarks>
    public virtual Expression VisitRegexMatch(PostgresRegexMatchExpression expression)
    {
        var options = expression.Options;

        Visit(expression.Match);

        if (options.HasFlag(RegexOptions.IgnoreCase))
        {
            Sql.Append(" ~* ");
            options &= ~RegexOptions.IgnoreCase;
        }
        else
        {
            Sql.Append(" ~ ");
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
            Sql.Append(constantPattern);
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
    public virtual Expression VisitRowValue(PostgresRowValueExpression rowValueExpression)
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
    /// Visits the children of an <see cref="PostgresILikeExpression"/>.
    /// </summary>
    /// <param name="likeExpression">The expression.</param>
    /// <returns>
    /// An <see cref="Expression"/>.
    /// </returns>
    public virtual Expression VisitILike(PostgresILikeExpression likeExpression)
    {
        Visit(likeExpression.Match);
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
    /// Visits the children of an <see cref="PostgresJsonTraversalExpression"/>.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>
    /// An <see cref="Expression"/>.
    /// </returns>
    public virtual Expression VisitJsonPathTraversal(PostgresJsonTraversalExpression expression)
    {
        Visit(expression.Expression);

        if (expression.Path.Count == 1)
        {
            Sql.Append(expression.ReturnsText ? "->>" : "->");
            Visit(expression.Path[0]);
            return expression;
        }

        // Multiple path components
        Sql.Append(expression.ReturnsText ? "#>>" : "#>");

        // Use simplified array literal syntax if all path components are constants for cleaner SQL
        if (expression.Path.All(p => p is SqlConstantExpression))
        {
            Sql
                .Append("'{")
                .Append(string.Join(",", expression.Path.Select(p => ((SqlConstantExpression)p).Value)))
                .Append("}'");
        }
        else
        {
            Sql.Append("ARRAY[");
            for (var i = 0; i < expression.Path.Count; i++)
            {
                Visit(expression.Path[i]);
                if (i < expression.Path.Count - 1)
                {
                    Sql.Append(",");
                }
            }
            Sql.Append("]::text[]");
        }

        return expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitUnnestExpression(PostgresUnnestExpression unnestExpression)
    {
        // unnest docs: https://www.postgresql.org/docs/current/functions-array.html#ARRAY-FUNCTIONS-TABLE

        // unnest is a regular table-valued function with a special AS foo(bar) at the end
        base.VisitTableValuedFunction(unnestExpression);

        Sql
            .Append("(")
            .Append(unnestExpression.ColumnName)
            .Append(")");

        return unnestExpression;
    }

    /// <summary>
    /// Visits the children of a <see cref="PostgresUnknownBinaryExpression"/>.
    /// </summary>
    /// <param name="unknownBinaryExpression">The expression.</param>
    /// <returns>
    /// An <see cref="Expression"/>.
    /// </returns>
    public virtual Expression VisitUnknownBinary(PostgresUnknownBinaryExpression unknownBinaryExpression)
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
    /// Visits the children of a <see cref="PostgresFunctionExpression"/>.
    /// </summary>
    /// <param name="e">The expression.</param>
    /// <returns>
    /// An <see cref="Expression"/>.
    /// </returns>
    public virtual Expression VisitPostgresFunction(PostgresFunctionExpression e)
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
                Sql.Append(i < e.ArgumentSeparators.Count && e.ArgumentSeparators[i] is not null
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
                && (innerUnary.OperatorType is ExpressionType.Negate || innerUnary.OperatorType is ExpressionType.Not && innerUnary.Type != typeof(bool))
                && (outerUnary.OperatorType is ExpressionType.Negate || outerUnary.OperatorType is ExpressionType.Not && outerUnary.Type != typeof(bool)):
                return true;

            // Copy paste of QuerySqlGenerator.RequiresParentheses for SqlBinaryExpression
            case PostgresBinaryExpression innerBinary:
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
                        0 => outerExpression is not PostgresBinaryExpression outerBinary
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
            PostgresBinaryExpression => (1000, false),

            CollateExpression => (1000, false),
            AtTimeZoneExpression => (1000, false),
            InExpression => (900, false),
            PostgresJsonTraversalExpression => (1000, false),
            PostgresArrayIndexExpression => (1500, false),
            PostgresAllExpression or PostgresAnyExpression => (800, false),
            LikeExpression or PostgresILikeExpression or PostgresRegexMatchExpression => (900, false),

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

    private sealed class OuterReferenceFindingExpressionVisitor : ExpressionVisitor
    {
        private readonly TableExpression _mainTable;
        private bool _containsReference;

        public OuterReferenceFindingExpressionVisitor(TableExpression mainTable)
            => _mainTable = mainTable;

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

            if (expression is ColumnExpression columnExpression
                && columnExpression.Table == _mainTable)
            {
                _containsReference = true;

                return expression;
            }

            return base.Visit(expression);
        }
    }
}
