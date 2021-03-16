using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion
{
    /// <summary>
    /// A value converter that can convert between array types, given another <see cref="ValueConverter"/> for the
    /// elements.
    /// </summary>
    public class NpgsqlArrayConverter<TModelArray, TProviderArray> : ValueConverter<TModelArray, TProviderArray>, INpgsqlArrayConverter
    {
        public virtual ValueConverter ElementConverter { get; }

        public NpgsqlArrayConverter([NotNull] ValueConverter elementConverter)
            : base(
                ToArrayProviderExpression(elementConverter.ConvertToProviderExpression),
                FromArrayProviderExpression(elementConverter.ConvertFromProviderExpression))
        {
            if (!typeof(TModelArray).TryGetElementType(out var modelElementType) ||
                !typeof(TProviderArray).TryGetElementType(out var providerElementType))
            {
                throw new ArgumentException("Can only convert between arrays");
            }

            if (modelElementType.UnwrapNullableType() != elementConverter.ModelClrType)
                throw new ArgumentException($"The element's value converter model type ({elementConverter.ModelClrType}), doesn't match the array's ({modelElementType})");
            if (providerElementType.UnwrapNullableType() != elementConverter.ProviderClrType)
                throw new ArgumentException($"The element's value converter provider type ({elementConverter.ProviderClrType}), doesn't match the array's ({providerElementType})");

            ElementConverter = elementConverter;
        }

        private static Expression<Func<TModelArray, TProviderArray>> ToArrayProviderExpression(
            LambdaExpression elementToProviderExpression)
            => ArrayConversionExpression<TModelArray, TProviderArray>(elementToProviderExpression);

        private static Expression<Func<TProviderArray, TModelArray>> FromArrayProviderExpression(
            LambdaExpression elementFromProviderExpression)
            => ArrayConversionExpression<TProviderArray, TModelArray>(elementFromProviderExpression);

        /// <summary>
        /// Generates a lambda expression that accepts an array, and converts it to another array by looping and applying
        /// a conversion lambda to each of its elements.
        /// </summary>
        private static Expression<Func<TInput, TOutput>> ArrayConversionExpression<TInput, TOutput>(LambdaExpression elementConversionExpression)
        {
            if (!typeof(TInput).TryGetElementType(out var inputElementType) ||
                !typeof(TOutput).TryGetElementType(out var outputElementType))
            {
                throw new ArgumentException("Both TInput and TOutput must be arrays or generic Lists");
            }

            // elementConversionExpression is always over non-nullable value types. If the array is over nullable types,
            // we need to sanitize via an external null check.
            if (inputElementType.IsGenericType && inputElementType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // p => p == null ? null : elementConversionExpression(p)
                var p = Expression.Parameter(inputElementType, "foo");
                elementConversionExpression = Expression.Lambda(
                    Expression.Condition(
                        Expression.Equal(p, Expression.Constant(null, inputElementType)),
                        Expression.Constant(null, outputElementType),
                        Expression.Convert(
                            Expression.Invoke(
                                elementConversionExpression,
                                Expression.Convert(p, inputElementType.UnwrapNullableType())),
                            outputElementType)),
                    p);
            }

            var input = Expression.Parameter(typeof(TInput), "value");
            var output = Expression.Parameter(typeof(TOutput), "result");
            var loopVariable = Expression.Parameter(typeof(int), "i");
            var lengthVariable = Expression.Variable(typeof(int), "length");

            return Expression.Lambda<Func<TInput, TOutput>>(
                // First, check if the given array value is null and return null immediately if so
                Expression.Condition(
                    Expression.ReferenceEqual(input, Expression.Constant(null)),
                    Expression.Constant(null, typeof(TOutput)),
                    Expression.Block(
                        typeof(TOutput),
                        new[] { output, lengthVariable, loopVariable },

                        // Get the length of the input array or list
                        Expression.Assign(lengthVariable, typeof(TInput).IsArray
                            ? Expression.ArrayLength(input)
                            : Expression.Property(input, typeof(TInput).GetProperty(nameof(List<TModelArray>.Count))!)),

                        // Allocate an output array or list
                        Expression.Assign(output, typeof(TOutput).IsArray
                            ? Expression.NewArrayBounds(outputElementType, lengthVariable)
                            : Expression.New(typeof(TOutput).GetConstructor(new[] { typeof(int) })!, lengthVariable)),

                        // Loop over the elements, applying the element converter on them one by one
                        ForLoop(
                            loopVar: loopVariable,
                            initValue: Expression.Constant(0),
                            condition: Expression.LessThan(loopVariable, lengthVariable),
                            increment: Expression.AddAssign(loopVariable, Expression.Constant(1)),
                            loopContent:
                            typeof(TOutput).IsArray
                                ? Expression.Assign(
                                    Expression.ArrayAccess(output, loopVariable),
                                    Expression.Invoke(
                                        elementConversionExpression,
                                        AccessArrayOrList(input, loopVariable)))
                                : Expression.Call(
                                    output,
                                    typeof(TOutput).GetMethod(nameof(List<int>.Add), new [] { outputElementType })!,
                                    Expression.Invoke(
                                        elementConversionExpression,
                                        AccessArrayOrList(input, loopVariable)))),
                        output
                    )),
                input);

            static Expression AccessArrayOrList(Expression arrayOrList, Expression index)
                => arrayOrList.Type.IsArray
                    ? Expression.ArrayAccess(arrayOrList, index)
                    : Expression.Property(arrayOrList, arrayOrList.Type.FindIndexerProperty()!, index);
        }

        private static Expression ForLoop(ParameterExpression loopVar, Expression initValue, Expression condition, Expression increment, Expression loopContent)
        {
            var initAssign = Expression.Assign(loopVar, initValue);
            var breakLabel = Expression.Label("LoopBreak");
            var loop = Expression.Block(new[] { loopVar },
                initAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        condition,
                        Expression.Block(
                            loopContent,
                            increment
                        ),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel)
            );

            return loop;
        }
    }
}
