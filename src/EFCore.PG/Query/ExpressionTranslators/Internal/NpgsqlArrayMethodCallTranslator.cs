using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL array operators mapped to methods declared on
    /// <see cref="Array"/>, <see cref="Enumerable"/>, <see cref="List{T}"/>, and <see cref="NpgsqlArrayExtensions"/>.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-array.html
    /// </remarks>
    public class NpgsqlArrayMethodCallTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull] readonly Version _postgresVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlArrayMethodCallTranslator"/> class.
        /// </summary>
        /// <param name="postgresVersion">The backend version to target.</param>
        public NpgsqlArrayMethodCallTranslator([CanBeNull] Version postgresVersion) => _postgresVersion = postgresVersion;

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression expression)
            => EnumerableHandler(expression) ??
               NpgsqlArrayExtensionsHandler(expression) ??
               ArrayStaticHandler(expression) ??
               ArrayInstanceHandler(expression) ??
               StringInstanceHandler(expression) ??
               ListInstanceHandler(expression);

        #region Handlers

        /// <summary>
        /// Translates methods defined on <see cref="Enumerable"/>.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <returns>
        /// A translated expression or null if no translation is supported.
        /// </returns>
        [CanBeNull]
        Expression EnumerableHandler([NotNull] MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType != typeof(Enumerable))
                return null;

            var type = expression.Arguments[0].Type;

            if (!type.IsArray && type != typeof(string) && (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(List<>)))
                return null;

            if (!VersionAtLeast(7, 4))
                return null;

            switch (expression.Method.Name)
            {
            case nameof(Enumerable.Count) when type == typeof(string):
                return
                    Expression.Coalesce(
                        new SqlFunctionExpression(
                            "character_length",
                            typeof(int?),
                            new[] { expression.Arguments[0] }),
                        Expression.Constant(0));

            case nameof(Enumerable.Count):
                return VersionAtLeast(8, 4)
                    ? Expression.Coalesce(
                        new SqlFunctionExpression(
                            "array_length",
                            typeof(int?),
                            new[] { expression.Arguments[0], Expression.Constant(1) }),
                        Expression.Constant(0))
                    : null;

            case nameof(Enumerable.ElementAt):
                return MakeIndex(expression.Arguments[0], expression.Arguments[1]);

            case nameof(Enumerable.Append):
                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "||", expression.Arguments[0].Type);

            case nameof(Enumerable.Prepend):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[0], "||", expression.Arguments[0].Type);

            case nameof(Enumerable.SequenceEqual):
                return Expression.MakeBinary(ExpressionType.Equal, expression.Arguments[0], expression.Arguments[1]);

            default:
                return null;
            }
        }

        /// <summary>
        /// Translates methods defined on <see cref="NpgsqlArrayExtensions"/>.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <returns>
        /// A translated expression or null if no translation is supported.
        /// </returns>
        /// <remarks>
        /// This handler throws on unsupported versions since <see cref="NpgsqlArrayExtensions"/>
        /// does not support client-side evaluation.
        /// </remarks>
        [CanBeNull]
        Expression NpgsqlArrayExtensionsHandler([NotNull] MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType != typeof(NpgsqlArrayExtensions))
                return null;

            switch (expression.Method.Name)
            {
            case nameof(NpgsqlArrayExtensions.Contains):
                return VersionAtLeast(8, 2)
                    ? new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "@>", typeof(bool))
                    : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.Contains), 8, 2);

            case nameof(NpgsqlArrayExtensions.ContainedBy):
                return VersionAtLeast(8, 2)
                    ? new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<@", typeof(bool))
                    : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.ContainedBy), 8, 2);

            case nameof(NpgsqlArrayExtensions.Overlaps):
                return VersionAtLeast(8, 2)
                    ? new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "&&", typeof(bool))
                    : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.Overlaps), 8, 2);

            case nameof(NpgsqlArrayExtensions.ArrayFill):
                return VersionAtLeast(8, 4)
                    ? new SqlFunctionExpression("array_fill", expression.Method.ReturnType, expression.Arguments.Skip(1))
                    : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.ArrayFill), 8, 4);

            case nameof(NpgsqlArrayExtensions.ListFill):
                return VersionAtLeast(8, 4)
                    ? new SqlFunctionExpression("array_fill", expression.Method.ReturnType, expression.Arguments.Skip(1))
                    : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.ListFill), 8, 4);

            case nameof(NpgsqlArrayExtensions.MatrixFill):
                return VersionAtLeast(8, 4)
                    ? new SqlFunctionExpression("array_fill", expression.Method.ReturnType, expression.Arguments.Skip(1))
                    : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.MatrixFill), 8, 4);

            case nameof(NpgsqlArrayExtensions.ArrayDimensions):
                return VersionAtLeast(7, 4)
                    ? new SqlFunctionExpression("array_dims", expression.Method.ReturnType, expression.Arguments.Skip(1).Take(1))
                    : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.ArrayDimensions), 7, 4);

            case nameof(NpgsqlArrayExtensions.ArrayPositions):
                return VersionAtLeast(9, 5)
                    ? new SqlFunctionExpression("array_positions", expression.Method.ReturnType, expression.Arguments.Skip(1))
                    : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.ArrayPositions), 9, 5);

            case nameof(NpgsqlArrayExtensions.ArrayRemove):
                return VersionAtLeast(9, 3)
                    ? new SqlFunctionExpression("array_remove", expression.Method.ReturnType, expression.Arguments.Skip(1).Take(2))
                    : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.ArrayRemove), 9, 3);

            case nameof(NpgsqlArrayExtensions.ArrayReplace):
                return VersionAtLeast(9, 3)
                    ? new SqlFunctionExpression("array_replace", expression.Method.ReturnType, expression.Arguments.Skip(1).Take(3))
                    : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.ArrayReplace), 9, 3);

            case nameof(NpgsqlArrayExtensions.ArrayToString):
                return VersionAtLeast(9, 1)
                    ? new SqlFunctionExpression("array_to_string", expression.Method.ReturnType, expression.Arguments.Skip(1).Take(3))
                    : VersionAtLeast(7, 4)
                        ? new SqlFunctionExpression("array_to_string", expression.Method.ReturnType, expression.Arguments.Skip(1).Take(2))
                        : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.ArrayToString), 7, 4);

            case nameof(NpgsqlArrayExtensions.StringToArray):
                return VersionAtLeast(9, 1)
                    ? new SqlFunctionExpression("string_to_array", expression.Method.ReturnType, expression.Arguments.Skip(1).Take(3))
                    : VersionAtLeast(7, 4)
                        ? new SqlFunctionExpression("string_to_array", expression.Method.ReturnType, expression.Arguments.Skip(1).Take(2))
                        : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.StringToArray), 7, 4);

            case nameof(NpgsqlArrayExtensions.StringToList):
                return VersionAtLeast(9, 1)
                    ? new SqlFunctionExpression("string_to_array", expression.Method.ReturnType, expression.Arguments.Skip(1).Take(3))
                    : VersionAtLeast(7, 4)
                        ? new SqlFunctionExpression("string_to_array", expression.Method.ReturnType, expression.Arguments.Skip(1).Take(2))
                        : throw VersionNotSupportedException(nameof(NpgsqlArrayExtensions.StringToList), 7, 4);

            default:
                return null;
            }

            NotSupportedException VersionNotSupportedException(string name, int major, int minor)
                => new NotSupportedException($"{nameof(NpgsqlArrayExtensions)}.{name} is not supported before PostgreSQL {major}.{minor}.");
        }

        /// <summary>
        /// Translates static methods defined on <see cref="Array"/>.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <returns>
        /// A translated expression or null if no translation is supported.
        /// </returns>
        [CanBeNull]
        Expression ArrayStaticHandler([NotNull] MethodCallExpression expression)
        {
            if (!expression.Method.IsStatic || expression.Method.DeclaringType != typeof(Array))
                return null;

            switch (expression.Method.Name)
            {
            case nameof(Array.IndexOf) when VersionAtLeast(9, 5) &&
                                            expression.Arguments[0].Type.GetArrayRank() == 1 &&
                                            expression.Method.GetParameters().Length <= 3:
                return
                    Expression.Subtract(
                        Expression.Coalesce(
                            new SqlFunctionExpression(
                                "array_position",
                                typeof(int?),
                                expression.Arguments.Take(3)),
                            Expression.Constant(0)),
                        Expression.Constant(1));

            default:
                return null;
            }
        }

        /// <summary>
        /// Translates instance methods defined on <see cref="Array"/>.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <returns>
        /// A translated expression or null if no translation is supported.
        /// </returns>
        [CanBeNull]
        Expression ArrayInstanceHandler([NotNull] MethodCallExpression expression)
        {
            var instance = expression.Object;

            if (instance == null || !instance.Type.IsArray)
                return null;

            switch (expression.Method.Name)
            {
            case nameof(Array.GetLength) when VersionAtLeast(8, 4):
                return
                    Expression.Coalesce(
                        new SqlFunctionExpression(
                            "array_length",
                            typeof(int?),
                            new[] { instance, GenerateOneBasedIndexExpression(expression.Arguments[0]) }),
                        Expression.Constant(0));

            case nameof(Array.GetLowerBound) when VersionAtLeast(7, 4):
                return
                    Expression.Subtract(
                        Expression.Coalesce(
                            new SqlFunctionExpression(
                                "array_lower",
                                typeof(int?),
                                new[] { instance, GenerateOneBasedIndexExpression(expression.Arguments[0]) }),
                            Expression.Constant(0)),
                        Expression.Constant(1));

            case nameof(Array.GetUpperBound) when VersionAtLeast(7, 4):
                return
                    Expression.Subtract(
                        Expression.Coalesce(
                            new SqlFunctionExpression(
                                "array_upper",
                                typeof(int?),
                                new[] { instance, GenerateOneBasedIndexExpression(expression.Arguments[0]) }),
                            Expression.Constant(0)),
                        Expression.Constant(1));

            default:
                return null;
            }
        }

        /// <summary>
        /// Translates instance methods defined on <see cref="List{T}"/>.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <returns>
        /// A translated expression or null if no translation is supported.
        /// </returns>
        [CanBeNull]
        Expression ListInstanceHandler([NotNull] MethodCallExpression expression)
        {
            var instance = expression.Object;

            if (instance == null || !instance.Type.IsGenericType || instance.Type.GetGenericTypeDefinition() != typeof(List<>))
                return null;

            switch (expression.Method.Name)
            {
            case "get_Item":
                return MakeIndex(instance, expression.Arguments[0]);

            case nameof(IList.IndexOf) when VersionAtLeast(9, 5):
                return
                    Expression.Subtract(
                        Expression.Coalesce(
                            new SqlFunctionExpression(
                                "array_position",
                                typeof(int?),
                                new[] { instance, expression.Arguments[0] }),
                            Expression.Constant(0)),
                        Expression.Constant(1));

            default:
                return null;
            }
        }

        /// <summary>
        /// Translates instance methods defined on <see cref="string"/>.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <returns>
        /// A translated expression or null if no translation is supported.
        /// </returns>
        [CanBeNull]
        static Expression StringInstanceHandler([NotNull] MethodCallExpression expression)
        {
            var instance = expression.Object;

            if (instance == null || instance.Type != typeof(string))
                return null;

            switch (expression.Method.Name)
            {
            case "get_Chars":
                return MakeIndex(instance, expression.Arguments[0]);

            default:
                return null;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// True if <see cref="_postgresVersion"/> is null, greater than, or equal to the specified version.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <returns>
        /// True if <see cref="_postgresVersion"/> is null, greater than, or equal to the specified version.
        /// </returns>
        bool VersionAtLeast(int major, int minor) => _postgresVersion is null || new Version(major, minor) <= _postgresVersion;

        /// <summary>
        /// Creates an expression of type <see cref="T:ExpressionType.ArrayIndex"/> or <see cref="T:ExpressionType.Index"/>
        /// from the <paramref name="instance"/> and the <paramref name="index"/>.
        /// </summary>
        /// <param name="instance">The instance to index.</param>
        /// <param name="index">The index value.</param>
        /// <returns>
        /// An <see cref="T:ExpressionType.ArrayIndex"/> or <see cref="T:ExpressionType.Index"/>.
        /// </returns>
        [NotNull]
        static Expression MakeIndex([NotNull] Expression instance, [NotNull] Expression index)
        {
            if (instance.Type.IsArray)
            {
                return instance.Type.GetArrayRank() == 1
                    ? (Expression)Expression.ArrayIndex(instance, index)
                    : Expression.ArrayAccess(instance, index);
            }

            return Expression.MakeIndex(
                instance,
                instance.Type
                        .GetRuntimeProperties()
                        .Where(x => x.Name == instance.Type.GetCustomAttribute<DefaultMemberAttribute>()?.MemberName)
                        .Select(x => (Indexer: x, Parameters: x.GetGetMethod().GetParameters()))
                        .Where(x => x.Parameters.Length == 1)
                        .SingleOrDefault(x => x.Parameters.Single().ParameterType == index.Type)
                        .Indexer,
                new[] { index });
        }

        /// <summary>
        /// PostgreSQL array indexing is 1-based. If the index happens to be a constant,
        /// just increment it. Otherwise, append a +1 in the SQL.
        /// </summary>
        [NotNull]
        static Expression GenerateOneBasedIndexExpression([NotNull] Expression expression)
            => expression is ConstantExpression constantExpression
                ? Expression.Constant(Convert.ToInt32(constantExpression.Value) + 1)
                : (Expression)Expression.Add(expression, Expression.Constant(1));

        #endregion
    }
}
