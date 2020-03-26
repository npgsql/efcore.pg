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
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

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

        readonly IModel _model;
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        readonly NpgsqlJsonPocoTranslator _jsonPocoTranslator;
        readonly ISqlGenerationHelper _sqlGenerationHelper;

        [NotNull]
        readonly RelationalTypeMapping _boolMapping;

        public NpgsqlSqlTranslatingExpressionVisitor(
            [NotNull] RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
        {
            _sqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
            _jsonPocoTranslator = ((NpgsqlMemberTranslatorProvider)dependencies.MemberTranslatorProvider).JsonPocoTranslator;
            _model = queryCompilationContext.Model;
            _boolMapping = _sqlExpressionFactory.FindMapping(typeof(bool));
            _sqlGenerationHelper = sqlGenerationHelper;
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
                    _sqlExpressionFactory.Function("COUNT",
                        new[] { _sqlExpressionFactory.Fragment("*") },
                        nullable: false,
                        argumentsPropagateNullability: FalseArrays[1],
                        typeof(long))),
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
                    _sqlExpressionFactory.Function(
                        "SUM",
                        new[] { sqlExpression },
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[1],
                        typeof(long)),
                    inputType,
                    sqlExpression.TypeMapping);

            if (inputType == typeof(long))
                return _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function(
                        "SUM",
                        new[] { sqlExpression },
                        nullable: true,
                        argumentsPropagateNullability: FalseArrays[1],
                        typeof(decimal)),
                    inputType,
                    sqlExpression.TypeMapping);

            return _sqlExpressionFactory.Function(
                "SUM",
                new[] { sqlExpression },
                nullable: true,
                argumentsPropagateNullability: FalseArrays[1],
                inputType,
                sqlExpression.TypeMapping);
        }

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType == ExpressionType.ArrayLength)
            {
                if (TranslationFailed(unaryExpression.Operand, Visit(unaryExpression.Operand), out var sqlOperand))
                    return null;

                // Translate Length on byte[], but only if the type mapping is for bytea. There's also array of bytes
                // (mapped to smallint[]), which is handled below with CARDINALITY.
                if (sqlOperand.Type == typeof(byte[]) &&
                    (sqlOperand.TypeMapping == null || sqlOperand.TypeMapping is NpgsqlByteArrayTypeMapping))
                {
                    return _sqlExpressionFactory.Function(
                        "LENGTH",
                        new[] { sqlOperand },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        typeof(int));
                }

                return _jsonPocoTranslator.TranslateArrayLength(sqlOperand) ??
                       _sqlExpressionFactory.Function(
                           "cardinality",
                           new[] { sqlOperand },
                           nullable: true,
                           argumentsPropagateNullability: TrueArrays[1],
                           typeof(int?));
            }

            return base.VisitUnary(unaryExpression);
        }

        [NotNull] static readonly MethodInfo EqualsWithStringComparison = typeof(string).GetRuntimeMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) });

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            var visited = base.VisitMethodCall(methodCall);
            if (visited != null)
                return visited;

            if (methodCall.Method == EqualsWithStringComparison)
                return VisitEqualsWithStringComparison(methodCall);

            if (methodCall.Arguments.Count > 0 && (
                    methodCall.Arguments[0].Type.IsArray || methodCall.Arguments[0].Type.IsGenericList()))
            {
                return VisitArrayMethodCall(methodCall.Method, methodCall.Arguments);
            }

            return null;
        }

        protected virtual Expression VisitEqualsWithStringComparison(MethodCallExpression methodCall)
        {
            if (!(methodCall.Arguments[1] is ConstantExpression stringComparisonExpression &&
                  stringComparisonExpression.Value is StringComparison stringComparison))
            {
                throw new InvalidOperationException("string.Equals(string, StringComparison) is only supported with a constant StringComparison pararmeter.");
            }

            if (stringComparison != StringComparison.Ordinal &&
                stringComparison != StringComparison.OrdinalIgnoreCase)
            {
                throw new InvalidOperationException($"string.Equals(string, StringComparison) only supports {nameof(StringComparison.Ordinal)} and {nameof(StringComparison.OrdinalIgnoreCase)}.");
            }

            var isCaseSensitive = stringComparison == StringComparison.Ordinal;

            if (TranslationFailed(methodCall.Object, Visit(methodCall.Object), out var left) ||
                TranslationFailed(methodCall.Arguments[0], Visit(methodCall.Arguments[0]), out var right))
            {
                return null;
            }

            var leftCollation = ExtractCollation(left, stringComparison);
            var rightCollation = ExtractCollation(right, stringComparison);

            if (leftCollation is null && rightCollation is null)
            {
                throw new InvalidOperationException(
                    $"Could not find a column configured with a collation for explicitly case-{(isCaseSensitive ? "sensitive" : "insensitive")} operations in the " +
                    "arguments to string.Equals(string, StringComparison). Please consult the docs.");
            }

            // Note: PostgreSQL requires the same collation on both sides of the comparison, but we translate anyway
            // and let it throw.

            return Visit(
                _sqlExpressionFactory.Equal(
                    leftCollation == null ? left : new CollateExpression(left, leftCollation),
                    rightCollation == null ? right : new CollateExpression(right, rightCollation)));

            string ExtractCollation(SqlExpression e, StringComparison stringComparison)
            {
                var columns = new ExpressionTypeExtractingExpressionVisitor<ColumnExpression>().Extract(e);
                if (columns.Count > 1)
                    throw new InvalidOperationException("Multiple columns on the same side of an string.Equals(string, StringComparison) expression aren't supported.");

                if (columns.Count == 0)
                    return null;

                var column = columns[0];

                var table = (TableExpression)column.Table;
                var properties = _model.GetEntityTypes()
                    .Where(e => e.GetTableName() == table.Name)
                    .SelectMany(e => e.GetProperties())
                    .Where(p => p.GetColumnName() == column.Name)
                    .ToList();

                if (properties.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Could not match a property to the column {_sqlGenerationHelper.DelimitIdentifier(column.Name)} on " +
                        $"{_sqlGenerationHelper.DelimitIdentifier(table.Name, table.Schema)}.");
                }

                if (properties.Count > 1)
                {
                    throw new InvalidOperationException(
                        $"Multiple properties are mapped to the column {_sqlGenerationHelper.DelimitIdentifier(column.Name)} on " +
                        $"{_sqlGenerationHelper.DelimitIdentifier(table.Name, table.Schema)}. This is not supported for string.Equals(string, StringComparison).");
                }

                return stringComparison == StringComparison.OrdinalIgnoreCase
                    ? properties.Single().GetCaseInsensitiveCollation()
                    : stringComparison == StringComparison.Ordinal
                        ? properties.Single().GetCaseSensitiveCollation()
                        : throw new ArgumentException($"Invalid value: {stringComparison}", nameof(stringComparison));
            }
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
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
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

        public class ExpressionTypeExtractingExpressionVisitor<T> : ExpressionVisitor
        {
            List<T> _found;

            public List<T> Extract(Expression expression)
            {
                _found = new List<T>();
                Visit(expression);
                return _found;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is T t && !_found.Contains(t))
                    _found.Add(t);
                return base.Visit(expression);
            }
        }
    }
}
