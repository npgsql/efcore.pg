// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlRowValueTranslator : IMethodCallTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

    private static readonly MethodInfo GreaterThan =
        typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
            nameof(NpgsqlDbFunctionsExtensions.GreaterThan),
            new[] { typeof(DbFunctions), typeof(ITuple), typeof(ITuple) })!;

    private static readonly MethodInfo LessThan =
        typeof(NpgsqlDbFunctionsExtensions).GetMethods()
            .Single(m => m.Name == nameof(NpgsqlDbFunctionsExtensions.LessThan));

    private static readonly MethodInfo GreaterThanOrEqual =
        typeof(NpgsqlDbFunctionsExtensions).GetMethods()
            .Single(m => m.Name == nameof(NpgsqlDbFunctionsExtensions.GreaterThanOrEqual));

    private static readonly MethodInfo LessThanOrEqual =
        typeof(NpgsqlDbFunctionsExtensions).GetMethods()
            .Single(m => m.Name == nameof(NpgsqlDbFunctionsExtensions.LessThanOrEqual));

    private static readonly Dictionary<MethodInfo, ExpressionType> ComparisonMethods = new()
    {
        { GreaterThan, ExpressionType.GreaterThan },
        { LessThan, ExpressionType.LessThan },
        { GreaterThanOrEqual, ExpressionType.GreaterThanOrEqual },
        { LessThanOrEqual, ExpressionType.LessThanOrEqual }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="NpgsqlRowValueTranslator"/> class.
    /// </summary>
    public NpgsqlRowValueTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    /// <inheritdoc />
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(ValueType))] // For ValueTuple.Create
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Translate ValueTuple.Create
        if (method.DeclaringType == typeof(ValueTuple) && method.IsStatic && method.Name == nameof(ValueTuple.Create))
        {
            return new PostgresRowValueExpression(arguments, method.ReturnType);
        }

        // Translate EF.Functions.GreaterThan and other comparisons
        if (method.DeclaringType != typeof(NpgsqlDbFunctionsExtensions) || !ComparisonMethods.TryGetValue(method, out var expressionType))
        {
            return null;
        }

        var leftCount = arguments[1] is PostgresRowValueExpression leftRowValue
            ? leftRowValue.Values.Count
            : arguments[1] is SqlConstantExpression { Value : ITuple leftTuple }
                ? (int?)leftTuple.Length
                : null;

        var rightCount = arguments[2] is PostgresRowValueExpression rightRowValue
            ? rightRowValue.Values.Count
            : arguments[2] is SqlConstantExpression { Value : ITuple rightTuple }
                ? (int?)rightTuple.Length
                : null;

        if (leftCount is null || rightCount is null)
        {
            return null;
        }

        if (leftCount != rightCount)
        {
            throw new ArgumentException(NpgsqlStrings.RowValueComparisonRequiresTuplesOfSameLength);
        }

        return _sqlExpressionFactory.MakeBinary(expressionType, arguments[1], arguments[2], typeMapping: null);
    }
}
