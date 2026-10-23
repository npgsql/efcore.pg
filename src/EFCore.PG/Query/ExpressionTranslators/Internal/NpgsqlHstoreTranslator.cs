using System.Collections.Immutable;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlHstoreTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator, IMemberTranslator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => instance?.TypeMapping is NpgsqlHstoreTypeMapping
            && (method.DeclaringType == typeof(Dictionary<string, string>)
                || method.DeclaringType == typeof(ImmutableDictionary<string, string>))
                ? method.Name switch
                {
                    "get_Item" => sqlExpressionFactory.MakePostgresBinary(PgExpressionType.HstoreGetValue, instance, arguments[0]),
                    nameof(Dictionary<string, string>.ContainsKey)
                        => sqlExpressionFactory.MakePostgresBinary(PgExpressionType.HstoreContainsKey, instance, arguments[0]),
                    _ => null
                }
                : null;

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance?.TypeMapping is not NpgsqlHstoreTypeMapping
            || member.Name is not (nameof(Dictionary<string, string>.Count) or nameof(ImmutableDictionary<string, string>.IsEmpty))
            || (member.DeclaringType != typeof(Dictionary<string, string>)
                && member.DeclaringType != typeof(ImmutableDictionary<string, string>)))
        {
            return null;
        }

        var count = sqlExpressionFactory.Function(
            "cardinality",
            [
                sqlExpressionFactory.Function(
                    "akeys",
                    [instance],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(string[]))
            ],
            nullable: true,
            argumentsPropagateNullability: TrueArrays[1],
            typeof(int));

        return member.Name switch
        {
            nameof(ImmutableDictionary<string, string>.IsEmpty) => sqlExpressionFactory.Equal(count, sqlExpressionFactory.Constant(0)),
            _ => count
        };
    }
}
