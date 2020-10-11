using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlQueryableExtensions
    {
        #region StringAggregate

        internal static readonly MethodInfo StringAggregateWithoutSelectorMethod
            = typeof(NpgsqlQueryableExtensions).GetTypeInfo().GetDeclaredMethods(nameof(StringAggregate))
                .Single(m => m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(string));

        internal static readonly MethodInfo StringAggregateWithSelectorMethod
            = typeof(NpgsqlQueryableExtensions).GetTypeInfo().GetDeclaredMethods(nameof(StringAggregate))
                .Single(m => m.GetParameters().Length == 3);

        /// <summary>
        /// Concatenates the non-null input values into a string.
        /// Each value after the first is preceded by the corresponding delimiter (if it's not null).
        /// </summary>
        /// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</param>
        /// <param name="delimiter">The delimiter for the concatenation. Defaults to empty string.</param>
        /// <typeparam name="TSource">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</typeparam>
        /// <returns>A task that represents the asynchronous operation. The task result contains sequence concatenation.</returns>
        /// <remarks>
        /// Calls PostgreSQL <c>string_agg</c>, see https://www.postgresql.org/docs/current/functions-aggregate.html.
        /// </remarks>
        public static string StringAggregate<TSource>([NotNull] this IQueryable<TSource> source, [CanBeNull] string delimiter = null)
        {
            Check.NotNull(source, nameof(source));

            return source.Provider.Execute<string>(
                    Expression.Call(
                        instance: null,
                        StringAggregateWithoutSelectorMethod.MakeGenericMethod(typeof(TSource)),
                        source.Expression,
                        Expression.Constant(delimiter ?? string.Empty)));
        }

        /// <summary>
        /// Concatenates the non-null input values into a string.
        /// Each value after the first is preceded by the corresponding delimiter (if it's not null).
        /// </summary>
        /// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</param>
        /// <param name="delimiter">The delimiter for the concatenation. Defaults to empty string.</param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <typeparam name="TSource">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</typeparam>
        /// <typeparam name="TResult">
        /// The type of the value returned by the function represented by <paramref name="selector" />.
        /// </typeparam>
        /// <returns>A task that represents the asynchronous operation. The task result contains sequence concatenation.</returns>
        /// <remarks>
        /// Calls PostgreSQL <c>string_agg</c>, see https://www.postgresql.org/docs/current/functions-aggregate.html.
        /// </remarks>
        public static string StringAggregate<TSource, TResult>(
            [NotNull] this IQueryable<TSource> source,
            [CanBeNull] string delimiter,
            [NotNull] Expression<Func<TSource, TResult>> selector)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return source.Provider.Execute<string>(
                    Expression.Call(
                        instance: null,
                        StringAggregateWithSelectorMethod.MakeGenericMethod(typeof(TSource), typeof(TResult)),
                        source.Expression,
                        Expression.Constant(delimiter ?? string.Empty),
                        Expression.Quote(selector)));
        }

        /// <summary>
        /// Concatenates the non-null input values into a string.
        /// </summary>
        /// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <typeparam name="TSource">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</typeparam>
        /// <typeparam name="TResult">
        /// The type of the value returned by the function represented by <paramref name="selector" />.
        /// </typeparam>
        /// <returns>A task that represents the asynchronous operation. The task result contains sequence concatenation.</returns>
        /// <remarks>
        /// Calls PostgreSQL <c>string_agg</c>, see https://www.postgresql.org/docs/current/functions-aggregate.html.
        /// </remarks>
        public static string StringAggregate<TSource, TResult>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, TResult>> selector)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return source.Provider.Execute<string>(
                Expression.Call(
                    instance: null,
                    StringAggregateWithSelectorMethod.MakeGenericMethod(typeof(TSource), typeof(TResult)),
                    source.Expression,
                    Expression.Constant(string.Empty),
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Concatenates the non-null input values into a string.
        /// Each value after the first is preceded by the corresponding delimiter (if it's not null).
        /// </summary>
        /// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</param>
        /// <param name="delimiter">The delimiter for the concatenation. Defaults to empty string.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <typeparam name="TSource">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</typeparam>
        /// <returns>A task that represents the asynchronous operation. The task result contains sequence concatenation.</returns>
        /// <remarks>
        /// Calls PostgreSQL <c>string_agg</c>, see https://www.postgresql.org/docs/current/functions-aggregate.html.
        /// </remarks>
        public static Task<string> StringAggregateAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [CanBeNull] string delimiter = null,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<string>>(
                StringAggregateWithoutSelectorMethod,
                source,
                Expression.Constant(delimiter ?? string.Empty),
                cancellationToken);
        }

        /// <summary>
        /// Concatenates the non-null input values into a string.
        /// Each value after the first is preceded by the corresponding delimiter (if it's not null).
        /// </summary>
        /// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</param>
        /// <param name="delimiter">The delimiter for the concatenation. Defaults to empty string.</param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <typeparam name="TSource">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</typeparam>
        /// <typeparam name="TResult">
        /// The type of the value returned by the function represented by <paramref name="selector" />.
        /// </typeparam>
        /// <returns>A task that represents the asynchronous operation. The task result contains sequence concatenation.</returns>
        /// <remarks>
        /// Calls PostgreSQL <c>string_agg</c>, see https://www.postgresql.org/docs/current/functions-aggregate.html.
        /// </remarks>
        public static Task<string> StringAggregateAsync<TSource, TResult>(
            [NotNull] this IQueryable<TSource> source,
            [CanBeNull] string delimiter,
            [NotNull] Expression<Func<TSource, TResult>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<string>>(
                StringAggregateWithSelectorMethod,
                source,
                Expression.Constant(delimiter ?? string.Empty),
                Expression.Quote(selector),
                cancellationToken);
        }

        /// <summary>
        /// Concatenates the non-null input values into a string.
        /// </summary>
        /// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <typeparam name="TSource">An <see cref="IQueryable{T}" /> that contains the elements to concatenate.</typeparam>
        /// <typeparam name="TResult">
        /// The type of the value returned by the function represented by <paramref name="selector" />.
        /// </typeparam>
        /// <returns>A task that represents the asynchronous operation. The task result contains sequence concatenation.</returns>
        /// <remarks>
        /// Calls PostgreSQL <c>string_agg</c>, see https://www.postgresql.org/docs/current/functions-aggregate.html.
        /// </remarks>
        public static Task<string> StringAggregateAsync<TSource, TResult>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, TResult>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<string>>(
                StringAggregateWithSelectorMethod,
                source,
                Expression.Constant(string.Empty),
                Expression.Quote(selector),
                cancellationToken);
        }

        #endregion StringAggregate

        #region Impl.

        // Copied from EntityFrameworkQueryableExtensions

        static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            Expression arg1,
            CancellationToken cancellationToken = default)
        {
            if (source.Provider is IAsyncQueryProvider provider)
            {
                if (operatorMethodInfo.IsGenericMethod)
                {
                    operatorMethodInfo
                        = operatorMethodInfo.GetGenericArguments().Length == 2
                            ? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult).GetGenericArguments().Single())
                            : operatorMethodInfo.MakeGenericMethod(typeof(TSource));
                }

                return provider.ExecuteAsync<TResult>(
                    Expression.Call(instance: null, operatorMethodInfo, source.Expression, arg1),
                    cancellationToken);
            }

            throw new InvalidOperationException(CoreStrings.IQueryableProviderNotAsync);
        }

        static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            Expression arg1,
            Expression arg2,
            CancellationToken cancellationToken = default)
        {
            if (source.Provider is IAsyncQueryProvider provider)
            {
                if (operatorMethodInfo.IsGenericMethod)
                {
                    operatorMethodInfo
                        = operatorMethodInfo.GetGenericArguments().Length == 2
                            ? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult).GetGenericArguments().Single())
                            : operatorMethodInfo.MakeGenericMethod(typeof(TSource));
                }

                return provider.ExecuteAsync<TResult>(
                    Expression.Call(instance: null, operatorMethodInfo, source.Expression, arg1, arg2),
                    cancellationToken);
            }

            throw new InvalidOperationException(CoreStrings.IQueryableProviderNotAsync);
        }

        #endregion
    }
}
