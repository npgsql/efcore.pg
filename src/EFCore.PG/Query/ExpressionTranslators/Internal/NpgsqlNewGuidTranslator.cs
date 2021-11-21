using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
/// Provides translation services for PostgreSQL UUID functions.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/datatype-uuid.html
/// </remarks>
public class NpgsqlNewGuidTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo MethodInfo = typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), Array.Empty<Type>())!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly string _uuidGenerationFunction;

    public NpgsqlNewGuidTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        Version? postgresVersion)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _uuidGenerationFunction = postgresVersion?.AtLeast(13) == true ? "gen_random_uuid" : "uuid_generate_v4";
    }

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => MethodInfo.Equals(method)
            ? _sqlExpressionFactory.Function(
                _uuidGenerationFunction,
                Array.Empty<SqlExpression>(),
                nullable: false,
                argumentsPropagateNullability: FalseArrays[0],
                method.ReturnType)
            : null;
}