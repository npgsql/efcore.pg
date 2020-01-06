using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
    {
        [NotNull]
        static readonly MethodInfo Like2MethodInfo =
            typeof(DbFunctionsExtensions)
                .GetRuntimeMethod(nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        // ReSharper disable once InconsistentNaming
        [NotNull]
        static readonly MethodInfo ILike2MethodInfo =
            typeof(NpgsqlDbFunctionsExtensions)
                .GetRuntimeMethod(nameof(NpgsqlDbFunctionsExtensions.ILike), new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        [NotNull]
        static readonly MethodInfo EnumerableAnyWithPredicate =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 2);

        [NotNull]
        static readonly MethodInfo EnumerableAll =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(mi => mi.Name == nameof(Enumerable.All) && mi.GetParameters().Length == 2);

        [NotNull]
        static readonly MethodInfo Contains =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        readonly NpgsqlJsonPocoTranslator _jsonPocoTranslator;

        [NotNull]
        readonly RelationalTypeMapping _boolMapping;

        public NpgsqlSqlTranslatingExpressionVisitor(
            RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            IModel model,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            : base(dependencies, model, queryableMethodTranslatingExpressionVisitor)
        {
            _sqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
            _jsonPocoTranslator = ((NpgsqlMemberTranslatorProvider)Dependencies.MemberTranslatorProvider).JsonPocoTranslator;
            _boolMapping = _sqlExpressionFactory.FindMapping(typeof(bool));
        }

        // PostgreSQL COUNT() always returns bigint, so we need to downcast to int
        // TODO: Translate Count with predicate for GroupBy (see base implementation)
        public override SqlExpression TranslateCount(Expression expression = null)
        {
            if (expression != null)
            {
                // TODO: Translate Count with predicate for GroupBy
                return null;
            }

            return _sqlExpressionFactory.Convert(
                _sqlExpressionFactory.ApplyDefaultTypeMapping(
                    _sqlExpressionFactory.Function("COUNT", new[] { _sqlExpressionFactory.Fragment("*") }, typeof(long))),
                typeof(int), _sqlExpressionFactory.FindMapping(typeof(int)));
        }

        // In PostgreSQL SUM() doesn't return the same type as its argument for smallint, int and bigint.
        // Cast to get the same type.
        // http://www.postgresql.org/docs/current/static/functions-aggregate.html
        public override SqlExpression TranslateSum(Expression expression)
        {
            var sqlExpression = expression as SqlExpression ??
                                Translate(expression) ??
                                throw new InvalidOperationException(CoreStrings.TranslationFailed(expression.Print()));

            var inputType = sqlExpression.Type.UnwrapNullableType();

            // Note that there is no Sum over short in LINQ
            if (inputType == typeof(int))
                return _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function("SUM", new[] { sqlExpression }, typeof(long)),
                    inputType,
                    sqlExpression.TypeMapping);

            if (inputType == typeof(long))
                return _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function("SUM", new[] { sqlExpression }, typeof(decimal)),
                    inputType,
                    sqlExpression.TypeMapping);

            return _sqlExpressionFactory.Function("SUM", new[] { sqlExpression }, inputType, sqlExpression.TypeMapping);
        }

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType == ExpressionType.ArrayLength)
            {
                if (TranslationFailed(unaryExpression.Operand, Visit(unaryExpression.Operand), out var sqlOperand))
                    return null;

                return _jsonPocoTranslator.TranslateArrayLength(sqlOperand) ??
                       _sqlExpressionFactory.Function("cardinality", new[] { sqlOperand }, typeof(int?));
            }

            return base.VisitUnary(unaryExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            var visited = base.VisitMethodCall(methodCall);
            if (visited != null)
                return visited;

            if (methodCall.Arguments.Count > 0 && (
                    methodCall.Arguments[0].Type.IsArray || methodCall.Arguments[0].Type.IsGenericList()))
            {
                return VisitArrayMethodCall(methodCall.Method, methodCall.Arguments);
            }

            return null;
        }

        /// <summary>
        /// Identifies complex array-related constructs which cannot be translated in regular method translators, since
        /// they require accessing lambdas.
        /// </summary>
        protected virtual Expression VisitArrayMethodCall(MethodInfo method, ReadOnlyCollection<Expression> arguments)
        {
            {
                // Pattern match for .Where(e => new[] { "a", "b", "c" }.Any(p => EF.Functions.Like(e.SomeText, p))),
                // which we translate to WHERE s.""SomeText"" LIKE ANY (ARRAY['a','b','c']) (see test Any_like)
                // Note: NavigationExpander normalized Any(x) to Where(x).Any()
                if (method.IsClosedFormOf(EnumerableAnyWithPredicate) &&
                    arguments[1] is LambdaExpression wherePredicate &&
                    wherePredicate.Body is MethodCallExpression wherePredicateMethodCall && (
                        wherePredicateMethodCall.Method == Like2MethodInfo ||
                        wherePredicateMethodCall.Method == ILike2MethodInfo))
                {
                    var array = (SqlExpression)Visit(arguments[0]);
                    var match = (SqlExpression)Visit(wherePredicateMethodCall.Arguments[1]);

                    return _sqlExpressionFactory.ArrayAnyAll(match, array, ArrayComparisonType.Any,
                        wherePredicateMethodCall.Method == Like2MethodInfo ? "LIKE" : "ILIKE");
                }

                // Note: we also handle the above with equality instead of Like, see NpgsqlArrayMethodTranslator
            }

            {
                // Same for All (but without the normalization)
                if (method.IsClosedFormOf(EnumerableAll) &&
                    arguments[1] is LambdaExpression wherePredicate &&
                    wherePredicate.Body is MethodCallExpression wherePredicateMethodCall && (
                        wherePredicateMethodCall.Method == Like2MethodInfo ||
                        wherePredicateMethodCall.Method == ILike2MethodInfo))
                {
                    var array = (SqlExpression)Visit(arguments[0]);
                    var match = (SqlExpression)Visit(wherePredicateMethodCall.Arguments[1]);

                    return _sqlExpressionFactory.ArrayAnyAll(match, array, ArrayComparisonType.All,
                        wherePredicateMethodCall.Method == Like2MethodInfo ? "LIKE" : "ILIKE");
                }
            }

            {
                // Translate e => new[] { 4, 5 }.Any(p => e.SomeArray.Contains(p)),
                // using array overlap (&&)
                if (method.IsClosedFormOf(EnumerableAnyWithPredicate) &&
                    arguments[1] is LambdaExpression wherePredicate &&
                    wherePredicate.Body is MethodCallExpression wherePredicateMethodCall &&
                    wherePredicateMethodCall.Method.IsClosedFormOf(Contains) &&
                    wherePredicateMethodCall.Arguments[0].Type.IsArrayOrGenericList() &&
                    wherePredicateMethodCall.Arguments[1] is ParameterExpression parameterExpression &&
                    parameterExpression == wherePredicate.Parameters[0])
                {
                    var array1 = (SqlExpression)Visit(arguments[0]);
                    var array2 = (SqlExpression)Visit(wherePredicateMethodCall.Arguments[0]);
                    var inferredMapping = ExpressionExtensions.InferTypeMapping(array1, array2);

                    return new SqlCustomBinaryExpression(
                        _sqlExpressionFactory.ApplyTypeMapping(array1, inferredMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(array2, inferredMapping),
                        "&&",
                        typeof(bool),
                        _boolMapping);
                }
            }

            {
                // Translate e => new[] { 4, 5 }.All(p => e.SomeArray.Contains(p)),
                // using array containment (<@)
                if (method.IsClosedFormOf(EnumerableAll) &&
                    arguments[1] is LambdaExpression wherePredicate &&
                    wherePredicate.Body is MethodCallExpression wherePredicateMethodCall &&
                    wherePredicateMethodCall.Method.IsClosedFormOf(Contains) &&
                    wherePredicateMethodCall.Arguments[0].Type.IsArrayOrGenericList() &&
                    wherePredicateMethodCall.Arguments[1] is ParameterExpression parameterExpression &&
                    parameterExpression == wherePredicate.Parameters[0])
                {
                    var array1 = (SqlExpression)Visit(arguments[0]);
                    var array2 = (SqlExpression)Visit(wherePredicateMethodCall.Arguments[0]);
                    var inferredMapping = ExpressionExtensions.InferTypeMapping(array1, array2);

                    return new SqlCustomBinaryExpression(
                        _sqlExpressionFactory.ApplyTypeMapping(array1, inferredMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(array2, inferredMapping),
                        "<@",
                        typeof(bool),
                        _boolMapping);
                }
            }

            return null;
        }

        /// <inheritdoc />
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            // Support DateTime subtraction, which returns a TimeSpan.
            if (binaryExpression.NodeType == ExpressionType.Subtract
                && binaryExpression.Left.Type.UnwrapNullableType() == typeof(DateTime)
                && binaryExpression.Left.Type.UnwrapNullableType() == typeof(DateTime))
            {
                if (TranslationFailed(binaryExpression.Left, Visit(TryRemoveImplicitConvert(binaryExpression.Left)), out var sqlLeft)
                    || TranslationFailed(binaryExpression.Right, Visit(TryRemoveImplicitConvert(binaryExpression.Right)), out var sqlRight))
                {
                    return null;
                }

                var inferredDateTimeTypeMapping = ExpressionExtensions.InferTypeMapping(sqlLeft, sqlRight);
                return new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    _sqlExpressionFactory.ApplyTypeMapping(sqlLeft, inferredDateTimeTypeMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(sqlRight, inferredDateTimeTypeMapping),
                    typeof(TimeSpan),
                    null);
            }

            if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
            {
                if (TranslationFailed(binaryExpression.Left, Visit(TryRemoveImplicitConvert(binaryExpression.Left)), out var sqlLeft)
                    || TranslationFailed(binaryExpression.Right, Visit(TryRemoveImplicitConvert(binaryExpression.Right)), out var sqlRight))
                {
                    return null;
                }

                // ArrayIndex over bytea is special, we have to use function rather than subscript
                if (binaryExpression.Left.Type == typeof(byte[]))
                {
                    return _sqlExpressionFactory.Function(
                        "get_byte",
                        new[]
                        {
                            _sqlExpressionFactory.ApplyDefaultTypeMapping(sqlLeft),
                            _sqlExpressionFactory.ApplyDefaultTypeMapping(sqlRight)
                        },
                        typeof(byte),
                        _sqlExpressionFactory.FindMapping(typeof(byte))
                    );
                }

                return
                    // Try translating ArrayIndex inside json column
                    _jsonPocoTranslator.TranslateMemberAccess(sqlLeft, sqlRight, binaryExpression.Type) ??
                    // Other types should be subscriptable - but PostgreSQL arrays are 1-based, so adjust the index.
                    _sqlExpressionFactory.ArrayIndex(sqlLeft, GenerateOneBasedIndexExpression(sqlRight));
            }

            return base.VisitBinary(binaryExpression);
        }

        /// <summary>
        /// PostgreSQL array indexing is 1-based. If the index happens to be a constant,
        /// just increment it. Otherwise, append a +1 in the SQL.
        /// </summary>
        SqlExpression GenerateOneBasedIndexExpression([NotNull] SqlExpression expression)
            => expression is SqlConstantExpression constant
                ? _sqlExpressionFactory.Constant(Convert.ToInt32(constant.Value) + 1, constant.TypeMapping)
                : (SqlExpression)_sqlExpressionFactory.Add(expression, _sqlExpressionFactory.Constant(1));

        #region Copied from RelationalSqlTranslatingExpressionVisitor

        static Expression TryRemoveImplicitConvert(Expression expression)
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
        bool TranslationFailed(Expression original, Expression translation, out SqlExpression castTranslation)
        {
            if (original != null && !(translation is SqlExpression))
            {
                castTranslation = null;
                return true;
            }

            castTranslation = translation as SqlExpression;
            return false;
        }

        #endregion Copied from RelationalSqlTranslatingExpressionVisitor
    }
}
