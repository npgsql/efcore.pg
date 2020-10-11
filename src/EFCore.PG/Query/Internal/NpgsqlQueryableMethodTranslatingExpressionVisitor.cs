using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
    {
        readonly NpgsqlSqlTranslatingExpressionVisitor _sqlTranslator;

        public NpgsqlQueryableMethodTranslatingExpressionVisitor(
            [NotNull] QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies, relationalDependencies, queryCompilationContext)
        {
            _sqlTranslator = (NpgsqlSqlTranslatingExpressionVisitor)
                relationalDependencies.RelationalSqlTranslatingExpressionVisitorFactory.Create(queryCompilationContext, this);
        }

        protected NpgsqlQueryableMethodTranslatingExpressionVisitor(
            [NotNull] RelationalQueryableMethodTranslatingExpressionVisitor parentVisitor)
            : base(parentVisitor)
        {
        }

        protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
            => new NpgsqlQueryableMethodTranslatingExpressionVisitor(this);

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            ShapedQueryExpression CheckTranslated(ShapedQueryExpression translated)
            {
                return translated
                       ?? throw new InvalidOperationException(
                           TranslationErrorDetails == null
                               ? CoreStrings.TranslationFailed(methodCallExpression.Print())
                               : CoreStrings.TranslationFailedWithDetails(
                                   methodCallExpression.Print(),
                                   TranslationErrorDetails));
            }

            var method = methodCallExpression.Method;
            if (method.DeclaringType == typeof(NpgsqlQueryableExtensions))
            {
                var source = Visit(methodCallExpression.Arguments[0]);
                if (source is ShapedQueryExpression shapedQueryExpression)
                {
                    var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
                    switch (method.Name)
                    {
                        case nameof(NpgsqlQueryableExtensions.StringAggregate)
                            when genericMethod == NpgsqlQueryableExtensions.StringAggregateWithoutSelectorMethod:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(
                                TranslateStringAggregate(shapedQueryExpression, methodCallExpression.Arguments[1], selector: null));

                        case nameof(NpgsqlQueryableExtensions.StringAggregate)
                            when genericMethod == NpgsqlQueryableExtensions.StringAggregateWithSelectorMethod:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(
                                TranslateStringAggregate(
                                    shapedQueryExpression,
                                    methodCallExpression.Arguments[1],
                                    selector: GetLambdaExpressionFromArgument(2)));

                    }

                    LambdaExpression GetLambdaExpressionFromArgument(int argumentIndex)
                        => methodCallExpression.Arguments[argumentIndex].UnwrapLambdaFromQuote();
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        // Copied from RelationalQueryableMethodTranslatingExpressionVisitor.TranslateMax
        protected virtual ShapedQueryExpression TranslateStringAggregate(
            ShapedQueryExpression source, Expression delimiter, LambdaExpression selector)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(delimiter, nameof(delimiter));

            var delimiterTranslation = TranslateExpression(delimiter);
            if (delimiterTranslation is null)
            {
                return null;
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();
            HandleGroupByForAggregate(selectExpression);

            var newSelector = selector == null
                              || selector.Body == selector.Parameters[0]
                ? selectExpression.Projection.Count == 0
                    ? selectExpression.GetMappedProjection(new ProjectionMember())
                    : null
                : RemapLambdaBody(source, selector);

            if (newSelector == null)
            {
                return null;
            }

            var translatedSelector = TranslateExpression(newSelector);
            if (translatedSelector == null)
            {
                return null;
            }

            var projection = _sqlTranslator.TranslateStringAggregate(translatedSelector, delimiterTranslation);

            return AggregateResultShaper(source, projection, throwWhenEmpty: true, typeof(string));
        }

        #region Copied from RelationalQueryableMethodTranslatingExpressionVisitor

        static void HandleGroupByForAggregate(SelectExpression selectExpression, bool eraseProjection = false)
        {
            if (selectExpression.GroupBy.Count > 0)
            {
                if (eraseProjection)
                {
                    selectExpression.ReplaceProjectionMapping(new Dictionary<ProjectionMember, Expression>());
                    selectExpression.AddToProjection(selectExpression.GroupBy[0]);
                    selectExpression.PushdownIntoSubquery();
                    selectExpression.ClearProjection();
                }
                else
                {
                    selectExpression.PushdownIntoSubquery();
                }
            }
        }

        Expression RemapLambdaBody(ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
        {
            var lambdaBody = ReplacingExpressionVisitor.Replace(
                lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression, lambdaExpression.Body);

            // TODO: Can't duplicate WeakEntityExpandingExpressionVisitor (ColumnExpression ctors are internal)
            // return ExpandWeakEntities((SelectExpression)shapedQueryExpression.QueryExpression, lambdaBody);

            return lambdaBody;
        }

        SqlExpression TranslateExpression(Expression expression)
        {
            var translation = _sqlTranslator.Translate(expression);
            if (translation == null && _sqlTranslator.TranslationErrorDetails != null)
            {
                AddTranslationErrorDetails(_sqlTranslator.TranslationErrorDetails);
            }

            return translation;
        }

        ShapedQueryExpression AggregateResultShaper(
            ShapedQueryExpression source,
            Expression projection,
            bool throwWhenEmpty,
            Type resultType)
        {
            if (projection == null)
            {
                return null;
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ReplaceProjectionMapping(
                new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), projection } });

            selectExpression.ClearOrdering();
            Expression shaper;

            if (throwWhenEmpty)
            {
                // Avg/Max/Min case.
                // We always read nullable value
                // If resultType is nullable then we always return null. Only non-null result shows throwing behavior.
                // otherwise, if projection.Type is nullable then server result is passed through DefaultIfEmpty, hence we return default
                // otherwise, server would return null only if it is empty, and we throw
                var nullableResultType = resultType.MakeNullable();
                shaper = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), nullableResultType);
                var resultVariable = Expression.Variable(nullableResultType, "result");
                var returnValueForNull = resultType.IsNullableType()
                    ? Expression.Constant(null, resultType)
                    : projection.Type.IsNullableType()
                        ? (Expression)Expression.Default(resultType)
                        : Expression.Throw(
                            Expression.New(
                                typeof(InvalidOperationException).GetConstructors()
                                    .Single(ci => ci.GetParameters().Length == 1),
                                Expression.Constant(CoreStrings.SequenceContainsNoElements)),
                            resultType);

                shaper = Expression.Block(
                    new[] { resultVariable },
                    Expression.Assign(resultVariable, shaper),
                    Expression.Condition(
                        Expression.Equal(resultVariable, Expression.Default(nullableResultType)),
                        returnValueForNull,
                        resultType != resultVariable.Type
                            ? Expression.Convert(resultVariable, resultType)
                            : (Expression)resultVariable));
            }
            else
            {
                // Sum case. Projection is always non-null. We read nullable value.
                shaper = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), projection.Type.MakeNullable());

                if (resultType != shaper.Type)
                {
                    shaper = Expression.Convert(shaper, resultType);
                }
            }

            return source.UpdateShaperExpression(shaper);
        }

        #endregion
    }
}
