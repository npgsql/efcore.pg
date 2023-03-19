using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
{
    private static readonly ConstructorInfo DateTimeCtor1 =
        typeof(DateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) })!;

    private static readonly ConstructorInfo DateTimeCtor2 =
        typeof(DateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) })!;

    private static readonly ConstructorInfo DateTimeCtor3 =
        typeof(DateTime).GetConstructor(
            new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(DateTimeKind) })!;

    private static readonly ConstructorInfo DateOnlyCtor =
        typeof(DateOnly).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) })!;

    private static readonly MethodInfo Like2MethodInfo =
        typeof(DbFunctionsExtensions).GetRuntimeMethod(
            nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;

    // ReSharper disable once InconsistentNaming
    private static readonly MethodInfo ILike2MethodInfo
        = typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
            nameof(NpgsqlDbFunctionsExtensions.ILike), new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;

    private static readonly MethodInfo ObjectEquals
        = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) })!;

    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly NpgsqlJsonPocoTranslator _jsonPocoTranslator;
    private readonly NpgsqlLTreeTranslator _ltreeTranslator;

    private readonly RelationalTypeMapping _timestampMapping;
    private readonly RelationalTypeMapping _timestampTzMapping;

    private static Type? _nodaTimePeriodType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlSqlTranslatingExpressionVisitor(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
    {
        _sqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
        _jsonPocoTranslator = ((NpgsqlMemberTranslatorProvider)Dependencies.MemberTranslatorProvider).JsonPocoTranslator;
        _ltreeTranslator = ((NpgsqlMethodCallTranslatorProvider)Dependencies.MethodCallTranslatorProvider).LTreeTranslator;
        _typeMappingSource = dependencies.TypeMappingSource;
        _timestampMapping = _typeMappingSource.FindMapping("timestamp without time zone")!;
        _timestampTzMapping = _typeMappingSource.FindMapping("timestamp with time zone")!;
    }

    /// <inheritdoc />
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        switch (unaryExpression.NodeType)
        {
            case ExpressionType.ArrayLength:
                if (TranslationFailed(unaryExpression.Operand, Visit(unaryExpression.Operand), out var sqlOperand))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                // Translate Length on byte[], but only if the type mapping is for bytea.
                // For byte[] mapped to an actual PG array (smallint[]), that's a primitive collection, and ArrayLength gets transformed to
                // Count() which gets translated to cardinality() as usual in NpgsqlQueryableMethodTranslatingExpressionVisitor.
                if (sqlOperand!.Type == typeof(byte[]) && sqlOperand.TypeMapping is NpgsqlByteArrayTypeMapping or null)
                {
                    return _sqlExpressionFactory.Function(
                        "length",
                        new[] { sqlOperand },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        typeof(int));
                }

                // Attempt to translate Length on a JSON POCO array
                if (_jsonPocoTranslator.TranslateArrayLength(sqlOperand) is SqlExpression translation)
                {
                    return translation;
                }

                // Note that Length over PG arrays (not within JSON) gets translated by QueryableMethodTranslatingEV, since arrays are
                // primitive collections
                break;

            // We have row value comparison methods such as EF.Functions.GreaterThan, which accept two ValueTuples/Tuples.
            // Since they accept ITuple parameters, the arguments have a Convert node casting up from the concrete argument to ITuple;
            // this node causes translation failure in RelationalSqlTranslatingExpressionVisitor, so unwrap here.
            case ExpressionType.Convert
                when unaryExpression.Type == typeof(ITuple) && unaryExpression.Operand.Type.IsAssignableTo(typeof(ITuple)):
                return Visit(unaryExpression.Operand);
        }

        return base.VisitUnary(unaryExpression);
    }

    /// <inheritdoc />
    protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
    {
        if (base.VisitNewArray(newArrayExpression) is SqlExpression visitedNewArrayExpression)
        {
            return visitedNewArrayExpression;
        }

        if (newArrayExpression.NodeType == ExpressionType.NewArrayInit)
        {
            var visitedExpressions = new SqlExpression[newArrayExpression.Expressions.Count];
            for (var i = 0; i < newArrayExpression.Expressions.Count; i++)
            {
                if (Visit(newArrayExpression.Expressions[i]) is SqlExpression visited)
                {
                    visitedExpressions[i] = visited;
                }
                else
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
            }

            return _sqlExpressionFactory.NewArray(visitedExpressions, newArrayExpression.Type);
        }

        return QueryCompilationContext.NotTranslatedExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        if (binaryExpression.NodeType == ExpressionType.Subtract)
        {
            if (binaryExpression.Left.Type.UnwrapNullableType().FullName == "NodaTime.LocalDate" &&
                binaryExpression.Right.Type.UnwrapNullableType().FullName == "NodaTime.LocalDate")
            {
                if (TranslationFailed(binaryExpression.Left, Visit(TryRemoveImplicitConvert(binaryExpression.Left)), out var sqlLeft)
                    || TranslationFailed(binaryExpression.Right, Visit(TryRemoveImplicitConvert(binaryExpression.Right)), out var sqlRight))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                var subtraction = _sqlExpressionFactory.MakeBinary(
                    ExpressionType.Subtract, sqlLeft!, sqlRight!, _typeMappingSource.FindMapping(typeof(int)))!;

                return PostgresFunctionExpression.CreateWithNamedArguments(
                    "make_interval",
                    new[] {  subtraction },
                    new[] { "days" },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    builtIn: true,
                    _nodaTimePeriodType ??= binaryExpression.Left.Type.Assembly.GetType("NodaTime.Period")!,
                    typeMapping: null);
            }

            // Note: many other date/time arithmetic operators are fully supported as-is by PostgreSQL - see NpgsqlSqlExpressionFactory
        }

        if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
        {
            // During preprocessing, ArrayIndex and List[] get normalized to ElementAt; see NpgsqlArrayTranslator
            Check.DebugFail(
                "During preprocessing, ArrayIndex and List[] get normalized to ElementAt; see NpgsqlArrayTranslator. " +
                "Should never see ArrayIndex.");
        }

        return base.VisitBinary(binaryExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNew(NewExpression newExpression)
    {
        var visitedNewExpression = base.VisitNew(newExpression);

        if (visitedNewExpression != QueryCompilationContext.NotTranslatedExpression)
        {
            return visitedNewExpression;
        }

        // We translate new ValueTuple<T1, T2...>(x, y...) to a SQL row value expression: (x, y).
        // This is notably done to support row value comparisons: WHERE (x, y) > (3, 4) (see e.g. NpgsqlDbFunctionsExtensions.GreaterThan)
        if (newExpression.Type.IsAssignableTo(typeof(ITuple)))
        {
            return TryTranslateArguments(out var sqlArguments)
                ? new PostgresRowValueExpression(sqlArguments, newExpression.Type)
                : QueryCompilationContext.NotTranslatedExpression;
        }

        // Translate new DateTime(...) -> make_timestamp/make_date
        if (newExpression.Constructor?.DeclaringType == typeof(DateTime))
        {
            if (newExpression.Constructor == DateTimeCtor1)
            {
                return TryTranslateArguments(out var sqlArguments)
                    ? _sqlExpressionFactory.Function(
                        "make_date", sqlArguments, nullable: true, TrueArrays[3], typeof(DateTime), _timestampMapping)
                    : QueryCompilationContext.NotTranslatedExpression;
            }

            if (newExpression.Constructor == DateTimeCtor2)
            {
                if (!TryTranslateArguments(out var sqlArguments))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                // DateTime's second component is an int, but PostgreSQL's MAKE_TIMESTAMP accepts a double precision
                sqlArguments[5] = _sqlExpressionFactory.Convert(sqlArguments[5], typeof(double));

                return _sqlExpressionFactory.Function(
                    "make_timestamp", sqlArguments, nullable: true, TrueArrays[6], typeof(DateTime), _timestampMapping);
            }

            if (newExpression.Constructor == DateTimeCtor3 && newExpression.Arguments[6] is ConstantExpression { Value : DateTimeKind kind })
            {
                if (!TryTranslateArguments(out var sqlArguments))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                // DateTime's second component is an int, but PostgreSQL's make_timestamp/make_timestamptz accepts a double precision.
                // Also chop off the last Kind argument which does not get sent to PostgreSQL
                var rewrittenArguments = new List<SqlExpression>
                {
                    sqlArguments[0], sqlArguments[1], sqlArguments[2], sqlArguments[3], sqlArguments[4],
                    _sqlExpressionFactory.Convert(sqlArguments[5], typeof(double))
                };

                if (kind == DateTimeKind.Utc)
                {
                    rewrittenArguments.Add(_sqlExpressionFactory.Constant("UTC"));
                }

                return kind == DateTimeKind.Utc
                    ? _sqlExpressionFactory.Function(
                        "make_timestamptz", rewrittenArguments, nullable: true, TrueArrays[8], typeof(DateTime), _timestampTzMapping)
                    : _sqlExpressionFactory.Function(
                        "make_timestamp", rewrittenArguments, nullable: true, TrueArrays[7], typeof(DateTime), _timestampMapping);
            }
        }

        // Translate new DateOnly(...) -> make_date
        if (newExpression.Constructor == DateOnlyCtor)
        {
            return TryTranslateArguments(out var sqlArguments)
                ? _sqlExpressionFactory.Function(
                    "make_date", sqlArguments, nullable: true, TrueArrays[3], typeof(DateOnly))
                : QueryCompilationContext.NotTranslatedExpression;
        }

        return QueryCompilationContext.NotTranslatedExpression;

        bool TryTranslateArguments(out SqlExpression[] sqlArguments)
        {
            sqlArguments = new SqlExpression[newExpression.Arguments.Count];
            for (var i = 0; i < sqlArguments.Length; i++)
            {
                var argument = newExpression.Arguments[i];
                if (TranslationFailed(argument, Visit(argument), out var sqlArgument))
                {
                    return false;
                }

                sqlArguments[i] = sqlArgument!;
            }

            return true;
        }
    }

    #region Copied from RelationalSqlTranslatingExpressionVisitor

    private static Expression TryRemoveImplicitConvert(Expression expression)
    {
        if (expression is UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType == ExpressionType.Convert
                || unaryExpression.NodeType == ExpressionType.ConvertChecked)
            {
                var innerType = unaryExpression.Operand.Type.UnwrapNullableType();
                if (innerType.IsEnum)
                {
                    innerType = Enum.GetUnderlyingType(innerType);
                }
                var convertedType = unaryExpression.Type.UnwrapNullableType();

                if (innerType == convertedType
                    || (convertedType == typeof(int)
                        && (innerType == typeof(byte)
                            || innerType == typeof(sbyte)
                            || innerType == typeof(char)
                            || innerType == typeof(short)
                            || innerType == typeof(ushort))))
                {
                    return TryRemoveImplicitConvert(unaryExpression.Operand);
                }
            }
        }

        return expression;
    }


    [DebuggerStepThrough]
    private static bool TranslationFailed(Expression? original, Expression? translation, out SqlExpression? castTranslation)
    {
        if (original is not null && !(translation is SqlExpression))
        {
            castTranslation = null;
            return true;
        }

        castTranslation = translation as SqlExpression;
        return false;
    }

    #endregion Copied from RelationalSqlTranslatingExpressionVisitor
}
