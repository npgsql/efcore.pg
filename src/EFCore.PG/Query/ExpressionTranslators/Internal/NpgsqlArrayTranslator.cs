using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates method and property calls on arrays/lists into their corresponding PostgreSQL operations.
    /// </summary>
    /// <remarks>
    /// https://www.postgresql.org/docs/current/static/functions-array.html
    /// </remarks>
    public class NpgsqlArrayTranslator : IMethodCallTranslator, IMemberTranslator
    {
        [NotNull] static readonly MethodInfo SequenceEqual =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(m => m.Name == nameof(Enumerable.SequenceEqual) && m.GetParameters().Length == 2);

        [NotNull] static readonly MethodInfo Contains =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

        [NotNull] static readonly MethodInfo EnumerableAnyWithoutPredicate =
            typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 1);

        [NotNull] static readonly MethodInfo StringArrayJoin =
            typeof(EnumerableExtensions).GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Single(mi =>
                    mi.Name == nameof(EnumerableExtensions.Join) &&
                    mi.GetParameters()[1].ParameterType == typeof(string));

        [NotNull]
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        [NotNull]
        readonly NpgsqlJsonPocoTranslator _jsonPocoTranslator;

        public NpgsqlArrayTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory, NpgsqlJsonPocoTranslator jsonPocoTranslator)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _jsonPocoTranslator = jsonPocoTranslator;
        }

        [CanBeNull]
        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (instance != null && instance.Type.IsGenericList() && method.Name == "get_Item" && arguments.Count == 1)
            {
                return
                    // Try translating indexing inside json column
                    _jsonPocoTranslator.TranslateMemberAccess(instance, arguments[0], method.ReturnType) ??
                    // Other types should be subscriptable - but PostgreSQL arrays are 1-based, so adjust the index.
                    _sqlExpressionFactory.ArrayIndex(instance, GenerateOneBasedIndexExpression(arguments[0]));
            }

            if (arguments.Count == 0)
                return null;

            var operand = arguments[0];
            if (!operand.Type.TryGetElementType(out var operandElementType))
                return null; // Not an array/list

            // The array/list CLR type may be mapped to a non-array database type (e.g. byte[] to bytea, or just
            // value converters). Make sure we're dealing with an array
            // Regardless of CLR type, we may be dealing with a non-array database type (e.g. via value converters).
            if (operand.TypeMapping is RelationalTypeMapping typeMapping &&
                !(typeMapping is NpgsqlArrayTypeMapping) && !(typeMapping is NpgsqlJsonTypeMapping))
            {
                return null;
            }

            if (method == StringArrayJoin)
            {
                return _sqlExpressionFactory.Function("array_to_string", new[] { arguments[0], arguments[1] },
                    nullable: true,
                    FalseArrays[2],
                    typeof(string));
            }

            if (method.IsClosedFormOf(SequenceEqual) && arguments[1].Type.IsArray)
                return _sqlExpressionFactory.Equal(operand, arguments[1]);

            // Predicate-less Any - translate to a simple length check.
            if (method.IsClosedFormOf(EnumerableAnyWithoutPredicate))
            {
                return _sqlExpressionFactory.GreaterThan(
                    _jsonPocoTranslator.TranslateArrayLength(operand) ??
                    _sqlExpressionFactory.Function(
                        "cardinality",
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        typeof(int?)),
                    _sqlExpressionFactory.Constant(0));
            }

            // Note that .Where(e => new[] { "a", "b", "c" }.Any(p => e.SomeText == p)))
            // is pattern-matched in AllAnyToContainsRewritingExpressionVisitor, which transforms it to
            // new[] { "a", "b", "c" }.Contains(e.SomeText).
            // Here we go further, and translate that to the PostgreSQL-specific construct e.SomeText = ANY (@p) -
            // this is superior to the general solution which will expand parameters to constants,
            // since non-PG SQL does not support arrays. If the list is a constant we leave it for regular IN
            // (functionality the same but more familiar).

            if (method.IsClosedFormOf(Contains) &&
                // Exclude constant array expressions from this PG-specific optimization since the general
                // EF Core mechanism is fine for that case. After https://github.com/aspnet/EntityFrameworkCore/issues/16375
                // is done we may not need the check any more.
                !(operand is SqlConstantExpression) &&
                (
                    // Handle either parameters (no mapping but supported CLR type), or array columns. We specifically
                    // don't want to translate if the type mapping is bytea (CLR type is array, but not an array in
                    // the database).
                    operand.TypeMapping == null && _sqlExpressionFactory.FindMapping(operand.Type) != null ||
                    operand.TypeMapping is NpgsqlArrayTypeMapping
                 ) &&
                // Exclude arrays/lists over Nullable<T> since the ADO layer doesn't handle them (but will in 5.0)
                Nullable.GetUnderlyingType(operandElementType) == null)
            {
                var item = arguments[1];
                var anyAll = _sqlExpressionFactory.ArrayAnyAll(item, operand, ArrayComparisonType.Any, "=");

                // TODO: no null semantics is implemented here (see https://github.com/npgsql/efcore.pg/issues/1142)
                // We require a null semantics check in case the item is null and the array contains a null.
                // Advanced parameter sniffing would help here: https://github.com/aspnet/EntityFrameworkCore/issues/17598
                // We need to coalesce to false since 'x' = ANY ({'y', NULL}) returns null, not false
                // (and so will be null when negated too)
                return _sqlExpressionFactory.OrElse(
                    anyAll,
                    _sqlExpressionFactory.AndAlso(
                        _sqlExpressionFactory.IsNull(item),
                        _sqlExpressionFactory.IsNotNull(
                            _sqlExpressionFactory.Function(
                                "array_position",
                                new[] { anyAll.Array, _sqlExpressionFactory.Fragment("NULL") },
                                nullable: true,
                                argumentsPropagateNullability: FalseArrays[2],
                                typeof(int)))));
            }

            // Note: we also translate .Where(e => new[] { "a", "b", "c" }.Any(p => EF.Functions.Like(e.SomeText, p)))
            // to LIKE ANY (...). See NpgsqlSqlTranslatingExpressionVisitor.VisitMethodCall.

            return null;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (instance?.Type.IsGenericList() == true &&
                member.Name == nameof(List<object>.Count) &&
                (instance.TypeMapping is NpgsqlArrayTypeMapping || instance.TypeMapping is null))
            {
                return _jsonPocoTranslator.TranslateArrayLength(instance) ??
                       _sqlExpressionFactory.Function(
                           "cardinality",
                           new[] { instance },
                           nullable: true,
                           argumentsPropagateNullability: TrueArrays[1],
                           typeof(int?));
            }

            return null;
        }

        /// <summary>
        /// PostgreSQL array indexing is 1-based. If the index happens to be a constant,
        /// just increment it. Otherwise, append a +1 in the SQL.
        /// </summary>
        SqlExpression GenerateOneBasedIndexExpression([NotNull] SqlExpression expression)
            => expression is SqlConstantExpression constant
                ? _sqlExpressionFactory.Constant(Convert.ToInt32(constant.Value) + 1, constant.TypeMapping)
                : (SqlExpression)_sqlExpressionFactory.Add(expression, _sqlExpressionFactory.Constant(1));
    }
}
