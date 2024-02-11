using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using ColumnInfo = Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal.PgTableValuedFunctionExpression.ColumnInfo;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlSqlTreePruner  : SqlTreePruner
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression node)
    {
        switch (node)
        {
            case PgTableValuedFunctionExpression { ColumnInfos: IReadOnlyList<ColumnInfo> columnInfos } tvf:
                var arguments = this.Visit(tvf.Arguments);

                List<ColumnInfo>? newColumnInfos = null;

                if (ReferencedColumnMap.TryGetValue(tvf.Alias, out var referencedAliases))
                {
                    for (var i = 0; i < columnInfos.Count; i++)
                    {
                        if (referencedAliases.Contains(columnInfos[i].Name))
                        {
                            newColumnInfos?.Add(columnInfos[i]);
                        }
                        else if (newColumnInfos is null)
                        {
                            newColumnInfos = [];
                            for (var j = 0; j < i; j++)
                            {
                                newColumnInfos.Add(columnInfos[j]);
                            }
                        }
                    }
                }

                return tvf
                    .Update(arguments)
                    .WithColumnInfos(newColumnInfos ?? columnInfos);

            default:
                return base.VisitExtension(node);
        }
    }
}
