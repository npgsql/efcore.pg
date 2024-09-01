using System.Diagnostics.CodeAnalysis;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     Locates instances of <see cref="PgUnnestExpression" /> in the tree and prunes the WITH ORDINALITY clause from them if the
///     ordinality column isn't referenced anywhere.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class NpgsqlUnnestPostprocessor : ExpressionVisitor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("expression")]
    public override Expression? Visit(Expression? expression)
    {
        switch (expression)
        {
            case ShapedQueryExpression shapedQueryExpression:
                return shapedQueryExpression
                    .UpdateQueryExpression(Visit(shapedQueryExpression.QueryExpression))
                    .UpdateShaperExpression(Visit(shapedQueryExpression.ShaperExpression));

            case SelectExpression selectExpression:
            {
                TableExpressionBase[]? newTables = null;

                var orderings = selectExpression.Orderings;

                for (var i = 0; i < selectExpression.Tables.Count; i++)
                {
                    var table = selectExpression.Tables[i];
                    var unwrappedTable = table.UnwrapJoin();

                    // Find any unnest table which does not have any references to its ordinality column in the projection or orderings
                    // (this is where they may appear); if found, remove the ordinality column from the unnest call.
                    // Note that if the ordinality column is the first ordering, we can still remove it, since unnest already returns
                    // ordered results.
                    if (unwrappedTable is PgUnnestExpression unnest
                        && !selectExpression.Orderings.Skip(1).Select(o => o.Expression)
                            .Concat(selectExpression.Projection.Select(p => p.Expression))
                            .Any(IsOrdinalityColumn))
                    {
                        if (newTables is null)
                        {
                            newTables = new TableExpressionBase[selectExpression.Tables.Count];

                            for (var j = 0; j < i; j++)
                            {
                                newTables[j] = selectExpression.Tables[j];
                            }
                        }

                        var newUnnest = new PgUnnestExpression(unnest.Alias, unnest.Array, unnest.ColumnName, withOrdinality: false);

                        newTables[i] = table switch
                        {
                            JoinExpressionBase j => j.Update(newUnnest),
                            PgUnnestExpression => newUnnest,
                            _ => throw new UnreachableException()
                        };

                        if (orderings.Count > 0 && IsOrdinalityColumn(orderings[0].Expression))
                        {
                            orderings = orderings.Skip(1).ToList();
                        }
                    }

                    bool IsOrdinalityColumn(SqlExpression expression)
                        => expression is ColumnExpression { Name: "ordinality" } ordinalityColumn
                            && ordinalityColumn.TableAlias == unwrappedTable.Alias;
                }

                return base.Visit(
                    newTables is null
                        ? selectExpression
                        : selectExpression.Update(
                            newTables,
                            selectExpression.Predicate,
                            selectExpression.GroupBy,
                            selectExpression.Having,
                            selectExpression.Projection,
                            orderings,
                            selectExpression.Offset,
                            selectExpression.Limit));
            }

            default:
                return base.Visit(expression);
        }
    }
}
