using System.Diagnostics.CodeAnalysis;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     A postprocessor that rewrites join predicates for PERIOD foreign keys. For PERIOD FKs, the range column join
///     condition must use PostgreSQL range containment (<c>@&gt;</c>) rather than equality (<c>=</c>), since the principal's
///     range must contain the dependent's range.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class NpgsqlPeriodForeignKeyPostprocessor(QueryTrackingBehavior queryTrackingBehavior) : ExpressionVisitor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            ShapedQueryExpression shapedQueryExpression
                => shapedQueryExpression.Update(
                    Visit(shapedQueryExpression.QueryExpression),
                    Visit(shapedQueryExpression.ShaperExpression)),

            // For equality predicates between columns, check if they correspond to a PERIOD FK range column
            // and replace with range containment (@>).
            // Note that EF's change tracker assumes equality, but the temporal foreign key (PERIOD) uses containment;
            // the change tracker is therefore currently incompatible with it. We check and throw, directing the user to use
            // a no-tracking query instead.
            SqlBinaryExpression { OperatorType: ExpressionType.Equal } eqExpression
                when eqExpression.Left is ColumnExpression leftCol
                    && eqExpression.Right is ColumnExpression rightCol
                    && TryGetPeriodFkInfo(leftCol, rightCol, out var principalColumn, out var dependentColumn)
                => queryTrackingBehavior is QueryTrackingBehavior.TrackAll
                    ? throw new InvalidOperationException(NpgsqlStrings.PeriodForeignKeyTrackingNotSupported)
                    : new PgBinaryExpression(
                        PgExpressionType.Contains,
                        principalColumn,
                        dependentColumn,
                        typeof(bool),
                        eqExpression.TypeMapping),

            _ => base.VisitExtension(extensionExpression)
        };

    /// <summary>
    ///     Determines whether two columns in an equality predicate correspond to the range column of a PERIOD FK,
    ///     and if so, identifies which is the principal column and which is the dependent column.
    /// </summary>
    private static bool TryGetPeriodFkInfo(
        ColumnExpression leftCol,
        ColumnExpression rightCol,
        [NotNullWhen(true)] out ColumnExpression? principalColumn,
        [NotNullWhen(true)] out ColumnExpression? dependentColumn)
    {
        principalColumn = null;
        dependentColumn = null;

        // We need column metadata to identify the FK
        if (leftCol.Column is not { } leftColumnBase || rightCol.Column is not { } rightColumnBase)
        {
            return false;
        }

        // Check all properties mapped to the left column for PERIOD FK participation.
        // We check both GetContainingForeignKeys() (property is on the dependent/FK side) and
        // GetContainingKeys() -> GetReferencingForeignKeys() (property is on the principal/PK side).
        foreach (var leftMapping in leftColumnBase.PropertyMappings)
        {
            var leftProperty = leftMapping.Property;

            foreach (var fk in GetPeriodForeignKeys(leftProperty))
            {
                // The range property is the last one in the FK
                var fkRangeProperty = fk.Properties[^1];
                var principalRangeProperty = fk.PrincipalKey.Properties[^1];

                // Determine if the left column is the dependent or principal range property,
                // and look for the counterpart on the right column.
                IProperty expectedRight;
                ColumnExpression candidatePrincipal, candidateDependent;

                if (leftProperty == fkRangeProperty)
                {
                    expectedRight = principalRangeProperty;
                    candidatePrincipal = rightCol;
                    candidateDependent = leftCol;
                }
                else if (leftProperty == principalRangeProperty)
                {
                    expectedRight = fkRangeProperty;
                    candidatePrincipal = leftCol;
                    candidateDependent = rightCol;
                }
                else
                {
                    continue;
                }

                foreach (var rightMapping in rightColumnBase.PropertyMappings)
                {
                    if (rightMapping.Property == expectedRight)
                    {
                        principalColumn = candidatePrincipal;
                        dependentColumn = candidateDependent;
                        return true;
                    }
                }
            }
        }

        return false;

        static IEnumerable<IForeignKey> GetPeriodForeignKeys(IProperty property)
        {
            foreach (var fk in property.GetContainingForeignKeys())
            {
                if (fk.GetPeriod() == true)
                {
                    yield return fk;
                }
            }

            foreach (var key in property.GetContainingKeys())
            {
                foreach (var fk in key.GetReferencingForeignKeys())
                {
                    if (fk.GetPeriod() == true)
                    {
                        yield return fk;
                    }
                }
            }
        }
    }
}
