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
        public Expression Translate(MethodCallExpression e)
        {
            var declaringType = e.Method.DeclaringType;

            if (declaringType != null &&
                declaringType != typeof(Enumerable) &&
                declaringType != typeof(Array) &&
                declaringType != typeof(NpgsqlArrayExtensions) &&
                !declaringType.IsArray &&
                !IsList(declaringType))
                return null;

            return EnumerableHandler(e) ??
                   NpgsqlArrayExtensionsHandler(e) ??
                   ArrayStaticHandler(e) ??
                   ArrayInstanceHandler(e) ??
                   ListInstanceHandler(e);
        }

        #region Handlers

        [CanBeNull]
        Expression EnumerableHandler([NotNull] MethodCallExpression e)
        {
            if (e.Method.DeclaringType != typeof(Enumerable))
                return null;

            var type = e.Arguments[0].Type;

            if (!type.IsArray && !IsList(type))
                return null;

            switch (e.Method.Name)
            {
            case nameof(Enumerable.Count) when VersionAtLeast(8, 4):
                return Expression.Coalesce(
                    new SqlFunctionExpression(
                        "array_length",
                        typeof(int?),
                        new[] { e.Arguments[0], Expression.Constant(1) }),
                    Expression.Constant(0));

            case nameof(Enumerable.ElementAt) when e.Arguments[0].Type.IsArray:
                return MakeArrayIndex(e.Arguments[0], e.Arguments[1]);

            case nameof(Enumerable.ElementAt):
                return MakeListIndex(e.Arguments[0], e.Arguments[1]);

            case nameof(Enumerable.Append):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "||", e.Arguments[0].Type);

            case nameof(Enumerable.Prepend):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[0], "||", e.Arguments[0].Type);

            case nameof(Enumerable.SequenceEqual):
                return Expression.MakeBinary(ExpressionType.Equal, e.Arguments[0], e.Arguments[1]);

            default:
                return null;
            }
        }

        [CanBeNull]
        Expression NpgsqlArrayExtensionsHandler([NotNull] MethodCallExpression e)
        {
            if (e.Method.DeclaringType != typeof(NpgsqlArrayExtensions))
                return null;

            switch (e.Method.Name)
            {
            case nameof(NpgsqlArrayExtensions.Contains) when VersionAtLeast(8, 2):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "@>", typeof(bool));

            case nameof(NpgsqlArrayExtensions.ContainedBy) when VersionAtLeast(8, 2):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "<@", typeof(bool));

            case nameof(NpgsqlArrayExtensions.Overlaps) when VersionAtLeast(8, 2):
                return new CustomBinaryExpression(e.Arguments[1], e.Arguments[2], "&&", typeof(bool));

            case nameof(NpgsqlArrayExtensions.ArrayFill) when VersionAtLeast(8, 4):
                return new SqlFunctionExpression("array_fill", e.Method.ReturnType, e.Arguments.Skip(1));

            case nameof(NpgsqlArrayExtensions.ListFill) when VersionAtLeast(8, 4):
                return new SqlFunctionExpression("array_fill", e.Method.ReturnType, e.Arguments.Skip(1));

            case nameof(NpgsqlArrayExtensions.ArrayDimensions):
                return new SqlFunctionExpression("array_dims", e.Method.ReturnType, e.Arguments.Skip(1).Take(1));

            case nameof(NpgsqlArrayExtensions.ArrayPositions) when VersionAtLeast(9, 5):
                return new SqlFunctionExpression("array_positions", e.Method.ReturnType, e.Arguments.Skip(1));

            case nameof(NpgsqlArrayExtensions.ArrayRemove) when VersionAtLeast(9, 3):
                return new SqlFunctionExpression("array_remove", e.Method.ReturnType, e.Arguments.Skip(1).Take(2));

            case nameof(NpgsqlArrayExtensions.ArrayReplace) when VersionAtLeast(9, 3):
                return new SqlFunctionExpression("array_replace", e.Method.ReturnType, e.Arguments.Skip(1).Take(3));

            case nameof(NpgsqlArrayExtensions.ArrayToString) when VersionAtLeast(9, 1):
                return new SqlFunctionExpression("array_to_string", e.Method.ReturnType, e.Arguments.Skip(1).Take(3));

            case nameof(NpgsqlArrayExtensions.ArrayToString):
                return new SqlFunctionExpression("array_to_string", e.Method.ReturnType, e.Arguments.Skip(1).Take(2));

            case nameof(NpgsqlArrayExtensions.StringToArray) when VersionAtLeast(9, 1):
                return new SqlFunctionExpression("string_to_array", e.Method.ReturnType, e.Arguments.Skip(1).Take(3));

            case nameof(NpgsqlArrayExtensions.StringToArray):
                return new SqlFunctionExpression("string_to_array", e.Method.ReturnType, e.Arguments.Skip(1).Take(2));

            case nameof(NpgsqlArrayExtensions.StringToList) when VersionAtLeast(9, 1):
                return new SqlFunctionExpression("string_to_array", e.Method.ReturnType, e.Arguments.Skip(1).Take(3));

            case nameof(NpgsqlArrayExtensions.StringToList):
                return new SqlFunctionExpression("string_to_array", e.Method.ReturnType, e.Arguments.Skip(1).Take(2));

            default:
                return null;
            }
        }

        [CanBeNull]
        Expression ArrayStaticHandler([NotNull] MethodCallExpression e)
        {
            if (!e.Method.IsStatic || e.Method.DeclaringType != typeof(Array))
                return null;

            switch (e.Method.Name)
            {
            case nameof(Array.IndexOf) when VersionAtLeast(9, 5) &&
                                            e.Arguments[0].Type.GetArrayRank() == 1 &&
                                            e.Method.GetParameters().Length <= 3:
                return Expression.Subtract(
                    Expression.Coalesce(
                        new SqlFunctionExpression(
                            "array_position",
                            typeof(int?),
                            e.Arguments.Take(3)),
                        Expression.Constant(0)),
                    Expression.Constant(1));

            default:
                return null;
            }
        }

        [CanBeNull]
        Expression ArrayInstanceHandler([NotNull] MethodCallExpression e)
        {
            var instance = e.Object;

            if (instance == null || !instance.Type.IsArray)
                return null;

            switch (e.Method.Name)
            {
            case nameof(Array.GetLength) when VersionAtLeast(8, 4):
                return Expression.Coalesce(
                    new SqlFunctionExpression(
                        "array_length",
                        typeof(int?),
                        new[] { instance, GenerateOneBasedIndexExpression(e.Arguments[0]) }),
                    Expression.Constant(0));

            case nameof(Array.GetLowerBound):
                return Expression.Subtract(
                    Expression.Coalesce(
                        new SqlFunctionExpression(
                            "array_lower",
                            typeof(int?),
                            new[] { instance, GenerateOneBasedIndexExpression(e.Arguments[0]) }),
                        Expression.Constant(0)),
                    Expression.Constant(1));

            case nameof(Array.GetUpperBound):
                return Expression.Subtract(
                    Expression.Coalesce(
                        new SqlFunctionExpression(
                            "array_upper",
                            typeof(int?),
                            new[] { instance, GenerateOneBasedIndexExpression(e.Arguments[0]) }),
                        Expression.Constant(0)),
                    Expression.Constant(1));

            default:
                return null;
            }
        }

        [CanBeNull]
        Expression ListInstanceHandler([NotNull] MethodCallExpression e)
        {
            var instance = e.Object;

            if (instance == null || !IsList(instance.Type))
                return null;

            switch (e.Method.Name)
            {
            case "get_Item":
                return MakeListIndex(instance, e.Arguments[0]);

            case nameof(IList.IndexOf) when VersionAtLeast(9, 5):
                return Expression.Subtract(
                    Expression.Coalesce(
                        new SqlFunctionExpression(
                            "array_position",
                            typeof(int?),
                            new[] { instance, e.Arguments[0] }),
                        Expression.Constant(0)),
                    Expression.Constant(1));

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
        bool VersionAtLeast(int major, int minor) => _postgresVersion == null || new Version(major, minor) <= _postgresVersion;

        [NotNull]
        static Expression MakeListIndex([NotNull] Expression instance, [NotNull] Expression index)
            => Expression.MakeIndex(
                instance,
                instance.Type
                        .GetRuntimeProperties()
                        .Where(x => x.Name == instance.Type.GetCustomAttribute<DefaultMemberAttribute>()?.MemberName)
                        .Select(x => (Indexer: x, Parameters: x.GetGetMethod().GetParameters()))
                        .Where(x => x.Parameters.Length == 1)
                        .SingleOrDefault(x => x.Parameters.Single().ParameterType == index.Type)
                        .Indexer,
                new[] { index });

        [NotNull]
        static Expression MakeArrayIndex([NotNull] Expression instance, [NotNull] Expression index)
            => instance.Type.GetArrayRank() == 1
                ? (Expression)Expression.ArrayIndex(instance, index)
                : Expression.ArrayAccess(instance, index);

        /// <summary>
        /// PostgreSQL array indexing is 1-based. If the index happens to be a constant,
        /// just increment it. Otherwise, append a +1 in the SQL.
        /// </summary>
        [NotNull]
        static Expression GenerateOneBasedIndexExpression([NotNull] Expression expression)
            => expression is ConstantExpression constantExpression
                ? Expression.Constant(Convert.ToInt32(constantExpression.Value) + 1)
                : (Expression)Expression.Add(expression, Expression.Constant(1));

        static bool IsList([NotNull] Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

        #endregion
    }
}
