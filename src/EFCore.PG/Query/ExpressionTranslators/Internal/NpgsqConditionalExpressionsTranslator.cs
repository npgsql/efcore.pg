using System.Runtime.CompilerServices;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
/// Translates <see cref="T:DbFunctionsExtensions.Greatest"/> methods into PostgreSQL GREATEST expressions.
/// </summary>
public class NpgsqConditionalExpressionsTranslator : IMethodCallTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="NpgsqConditionalExpressionsTranslator"/> class.
    /// </summary>
    public NpgsqConditionalExpressionsTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType == typeof(NpgsqlDbFunctionsExtensions))
        {
            switch (method.Name)
            {
                case nameof(NpgsqlDbFunctionsExtensions.NullIf):
                    return _sqlExpressionFactory.Function(
                        "nullif",
                        arguments.Skip(1),
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[arguments.Count - 1],
                        method.ReturnType);
                case nameof(NpgsqlDbFunctionsExtensions.Greatest):
                    return _sqlExpressionFactory.Function(
                        "greatest",
                        arguments.Skip(1),
                        nullable: true,
                        argumentsPropagateNullability: GetArgumentsPropagateNullability(arguments.Count - 1),
                        method.ReturnType);
                case nameof(NpgsqlDbFunctionsExtensions.Least):
                    return _sqlExpressionFactory.Function(
                        "least",
                        arguments.Skip(1),
                        nullable: true,
                        argumentsPropagateNullability: GetArgumentsPropagateNullability(arguments.Count - 1),
                        method.ReturnType);
            }
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<bool> GetArgumentsPropagateNullability(int length)
        => length < TrueArrays.Length
         ? TrueArrays[length]
         : (IEnumerable<bool>)Enumerable.Repeat(true, length).ToArray();
}
