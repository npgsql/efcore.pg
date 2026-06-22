using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Provides translation services for PostgreSQL UUID functions.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/datatype-uuid.html
/// </remarks>
public class NpgsqlGuidTranslator(ISqlExpressionFactory sqlExpressionFactory, Version? postgresVersion) : IMethodCallTranslator
{
    private readonly string _uuidGenerationFunction = postgresVersion.AtLeast(13) ? "gen_random_uuid" : "uuid_generate_v4";

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
    {
        if (method.DeclaringType == typeof(Guid))
        {
            return method.Name switch
            {
                nameof(Guid.NewGuid)
                    => sqlExpressionFactory.Function(
                        _uuidGenerationFunction,
                        [],
                        nullable: false,
                        argumentsPropagateNullability: FalseArrays[0],
                        method.ReturnType),

                // Note: uuidv7() was introduce in PostgreSQL 18.
                // In NpgsqlEvaluatableExpressionFilter we only prevent local evaluation when targeting PG18 or later;
                // that means that for lower version, the call gets evaluated locally and the result sent as a parameter
                // (and we never see the method call here).
                nameof(Guid.CreateVersion7)
                    => sqlExpressionFactory.Function(
                        "uuidv7",
                        [],
                        nullable: false,
                        argumentsPropagateNullability: FalseArrays[0],
                        method.ReturnType),

                _ => null
            };
        }

        return null;
    }
}
