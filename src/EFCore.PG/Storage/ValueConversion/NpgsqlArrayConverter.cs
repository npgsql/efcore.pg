using System;
using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion
{
    /// <summary>
    /// A value converter that can convert between array types, given another <see cref="ValueConverter"/> for the
    /// elements.
    /// </summary>
    public class NpgsqlArrayConverter<TModelArray, TProviderArray> : ValueConverter<TModelArray, TProviderArray>
    {
        public NpgsqlArrayConverter([NotNull] ValueConverter elementConverter)
            : base(ToProviderExpression(elementConverter), FromProviderExpression(elementConverter))
        {
            // TODO: List support
            if (!typeof(TModelArray).IsArray || !typeof(TProviderArray).IsArray)
                throw new ArgumentException("Can only convert between arrays");
            if (typeof(TModelArray).GetElementType() != elementConverter.ModelClrType)
                throw new ArgumentException($"The element's value converter model type ({elementConverter.ModelClrType}), doesn't match the array's ({typeof(TModelArray).GetElementType()})");
            if (typeof(TProviderArray).GetElementType() != elementConverter.ProviderClrType)
                throw new ArgumentException($"The element's value converter provider type ({elementConverter.ProviderClrType}), doesn't match the array's ({typeof(TProviderArray).GetElementType()})");
        }

        static Expression<Func<TModelArray, TProviderArray>> ToProviderExpression(ValueConverter elementConverter)
            => ArrayConversionExpression<TModelArray, TProviderArray>(elementConverter.ConvertToProviderExpression);

        static Expression<Func<TProviderArray, TModelArray>> FromProviderExpression(ValueConverter elementConverter)
            => ArrayConversionExpression<TProviderArray, TModelArray>(elementConverter.ConvertFromProviderExpression);

        /// <summary>
        /// Generates a lambda expression that accepts an array, and converts it to another array by looping and applying
        /// a conversion lambda to each of its elements.
        /// </summary>
        static Expression<Func<TInput, TOutput>> ArrayConversionExpression<TInput, TOutput>(LambdaExpression elementConversionExpression)
        {
            var outputElementType = typeof(TOutput).GetElementType();
            Debug.Assert(outputElementType != null);

            var inputArray = Expression.Parameter(typeof(TInput), "value");
            var outputArray = Expression.Parameter(typeof(TOutput), "result");
            var loopVariable = Expression.Parameter(typeof(int), "i");
            var arrayLengthVariable = Expression.Variable(typeof(int), "arrayLength");

            return Expression.Lambda<Func<TInput, TOutput>>(
                // First, check if the given array value is null and return null immediately if so
                Expression.Condition(
                    Expression.ReferenceEqual(inputArray, Expression.Constant(null)),
                    Expression.Constant(null, typeof(TOutput)),
                    Expression.Block(
                        typeof(TOutput),
                        new[] { outputArray, arrayLengthVariable, loopVariable },

                        // Get the length of the input array, allocate an output array and loop over the elements, converting them
                        Expression.Assign(arrayLengthVariable, Expression.ArrayLength(inputArray)),
                        Expression.Assign(outputArray, Expression.NewArrayBounds(outputElementType, arrayLengthVariable)),
                        ForLoop(
                            loopVar: loopVariable,
                            initValue: Expression.Constant(0),
                            condition: Expression.LessThan(loopVariable, arrayLengthVariable), Expression.AddAssign(loopVariable, Expression.Constant(1)),
                            loopContent:
                            Expression.Assign(
                                Expression.ArrayAccess(outputArray, loopVariable),
                                Expression.Invoke(
                                    elementConversionExpression,
                                    Expression.ArrayAccess(inputArray, loopVariable)))),
                        outputArray
                    )),
                inputArray);
        }

        static Expression ForLoop(ParameterExpression loopVar, Expression initValue, Expression condition, Expression increment, Expression loopContent)
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
