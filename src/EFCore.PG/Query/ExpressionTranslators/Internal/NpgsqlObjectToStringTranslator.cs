using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlObjectToStringTranslator : IMethodCallTranslator
{
    private static readonly HashSet<Type> _typeMapping =
    [
        typeof(int),
        typeof(long),
        typeof(DateTime),
        typeof(Guid),
        typeof(bool),
        typeof(byte),
        //typeof(byte[])
        typeof(double),
        typeof(DateTimeOffset),
        typeof(char),
        typeof(short),
        typeof(float),
        typeof(decimal),
        typeof(TimeSpan),
        typeof(uint),
        typeof(ushort),
        typeof(ulong),
        typeof(sbyte)
    ];

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _textTypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlObjectToStringTranslator(IRelationalTypeMappingSource typeMappingSource, ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;

        _textTypeMapping = typeMappingSource.FindMapping("text")!;
    }

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
        if (instance is null || method.Name != nameof(ToString) || arguments.Count != 0)
        {
            return null;
        }

        if (instance.Type == typeof(bool))
        {
            if (instance.Type == typeof(bool))
            {
                if (instance is not ColumnExpression { IsNullable: false })
                {
                    return _sqlExpressionFactory.Case(
                        instance,
                        [
                            new CaseWhenClause(
                                _sqlExpressionFactory.Constant(false),
                                _sqlExpressionFactory.Constant(false.ToString())),
                            new CaseWhenClause(
                                _sqlExpressionFactory.Constant(true),
                                _sqlExpressionFactory.Constant(true.ToString()))
                        ],
                        _sqlExpressionFactory.Constant(string.Empty));
                }

                return _sqlExpressionFactory.Case(
                    [
                        new CaseWhenClause(
                            instance,
                            _sqlExpressionFactory.Constant(true.ToString()))
                    ],
                    _sqlExpressionFactory.Constant(false.ToString()));
            }
        }

        return _typeMapping.Contains(instance.Type)
            || instance.Type.UnwrapNullableType().IsEnum && instance.TypeMapping is NpgsqlEnumTypeMapping
                ? _sqlExpressionFactory.Coalesce(
                    _sqlExpressionFactory.Convert(instance, typeof(string), _textTypeMapping),
                    _sqlExpressionFactory.Constant(string.Empty))
                : null;
    }
}
