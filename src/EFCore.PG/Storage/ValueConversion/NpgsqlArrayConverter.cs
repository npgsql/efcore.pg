using System.Collections;
using System.Collections.Frozen;
using static System.Linq.Expressions.Expression;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

/// <summary>
///     A value converter that can convert between array types; accepts an optional <see cref="ValueConverter" /> for the element, but can be
///     used without one to convert e.g. from a list to an array.
/// </summary>
public class NpgsqlArrayConverter<TModelCollection, TConcreteModelCollection, TProviderCollection>
    : ValueConverter<TModelCollection, TProviderCollection>
    where TModelCollection : IEnumerable
    where TConcreteModelCollection : IEnumerable
    where TProviderCollection : IEnumerable
{
    /// <summary>
    ///     The value converter for the element type of the array.
    /// </summary>
    public virtual ValueConverter? ElementConverter { get; }

    /// <summary>
    ///     Constructs a new instance of <see cref="NpgsqlArrayConverter{TModel, TConcreteModel, TProvider}" />.
    /// </summary>
    public NpgsqlArrayConverter()
        : this(elementConverter: null)
    {
    }

    /// <summary>
    ///     Constructs a new instance of <see cref="NpgsqlArrayConverter{TModel, TConcreteModel, TProvider}" />.
    /// </summary>
    public NpgsqlArrayConverter(ValueConverter? elementConverter)
        : base(
            // We assume that TProviderCollection is always a concrete, instantiable type (in fact it's always an array over the element)
            ArrayConversionExpression<TModelCollection, TProviderCollection, TProviderCollection>(
                elementConverter?.ConvertToProviderExpression),
            ArrayConversionExpression<TProviderCollection, TModelCollection, TConcreteModelCollection>(
                elementConverter?.ConvertFromProviderExpression))
    {
        var modelElementType = typeof(TModelCollection).TryGetElementType(typeof(IEnumerable<>));
        var providerElementType = typeof(TProviderCollection).TryGetElementType(typeof(IEnumerable<>));
        if (modelElementType is null || providerElementType is null)
        {
            throw new ArgumentException("Can only convert between arrays");
        }

        if (elementConverter is not null)
        {
            if (modelElementType.UnwrapNullableType() != elementConverter.ModelClrType.UnwrapNullableType())
            {
                throw new ArgumentException(
                    $"The element's value converter model type ({elementConverter.ModelClrType}), doesn't match the array's ({modelElementType})");
            }

            if (providerElementType.UnwrapNullableType() != elementConverter.ProviderClrType.UnwrapNullableType())
            {
                throw new ArgumentException(
                    $"The element's value converter provider type ({elementConverter.ProviderClrType}), doesn't match the array's ({providerElementType})");
            }
        }

        ElementConverter = elementConverter;
    }

    /// <summary>
    ///     Generates a lambda expression that accepts an array, and converts it to another array by looping and applying
    ///     a conversion lambda to each of its elements.
    /// </summary>
    private static Expression<Func<TInput, TOutput>> ArrayConversionExpression<TInput, TOutput, TConcreteOutput>(
        LambdaExpression? elementConversionExpression)
    {
        var inputElementType = typeof(TInput).IsArray
            ? typeof(TInput).GetElementType()
            : typeof(TInput).TryGetElementType(typeof(IEnumerable<>));

        var outputElementType = typeof(TOutput).IsArray
            ? typeof(TOutput).GetElementType()
            : typeof(TOutput).TryGetElementType(typeof(IEnumerable<>));

        if (inputElementType is null || outputElementType is null)
        {
            throw new ArgumentException("Both TInput and TOutput must be arrays or IList<T>");
        }

        // elementConversionExpression is always over non-nullable value types. If the array is over nullable types,
        // we need to sanitize via an external null check.
        if (elementConversionExpression is not null && inputElementType.IsNullableType() && outputElementType.IsNullableType())
        {
            // p => p is null ? null : elementConversionExpression(p)
            var p = Parameter(inputElementType, "foo");
            elementConversionExpression = Lambda(
                Condition(
                    Equal(p, Constant(null, inputElementType)),
                    Constant(null, outputElementType),
                    Convert(
                        Invoke(
                            elementConversionExpression,
                            // The user-provided conversion lambda typically accepts non-nullable (value) types, with EF Core doing the
                            // null-sanitization and conversion to non-nullable; do this here unless the user-provided lambda happens to
                            // accept a nullable value type parameter.
                            elementConversionExpression.Parameters[0].Type.IsNullableType()
                                ? p
                                : Convert(p, inputElementType.UnwrapNullableType())),
                        outputElementType)),
                p);
        }

        var input = Parameter(typeof(TInput), "input");
        var convertedInput = input;

        var outputIsImmutable = typeof(TConcreteOutput) is { IsGenericType: true } generic
            && generic == typeof(FrozenSet<>).MakeGenericType(outputElementType);
        var mutableOutputType = outputIsImmutable
                ? typeof(HashSet<>).MakeGenericType(outputElementType)
                : typeof(TConcreteOutput);

        var output = Parameter(mutableOutputType, "result");

        var lengthVariable = Variable(typeof(int), "length");

        var expressions = new List<Expression>();
        var variables = new List<ParameterExpression> { output, lengthVariable };

        Expression getInputLength;
        Func<Expression, Expression>? indexer;

        // The conversion is going to depend on what kind of input we have: array, list, collection, or arbitrary IEnumerable.
        // For array/list we can get the length and index inside, so we can do an efficient for loop.
        // For other ICollections (e.g. HashSet) we can get the length (and so pre-allocate the output), but we can't index; so we
        // get an enumerator and use that.
        // For arbitrary IEnumerable, we can't get the length so we can't preallocate output arrays; so we to call ToList() on it and then
        // process that (note that we could avoid that when the output is a List rather than an array).
        var inputInterfaces = input.Type.GetInterfaces();
        switch (input.Type)
        {
            // Input is typed as an array - we can get its length and index into it
            case { IsArray: true }:
                getInputLength = ArrayLength(input);
                indexer = i => ArrayAccess(input, i);
                break;

            // Input is typed as an IList - we can get its length and index into it
            case { IsGenericType: true } when inputInterfaces.Append(input.Type)
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)):
            {
                getInputLength = Property(
                    input,
                    input.Type.GetProperty("Count")
                    // If TInput is an interface (IList<T>), its Count property needs to be found on ICollection<T>
                    ?? typeof(ICollection<>).MakeGenericType(input.Type.GetGenericArguments()[0]).GetProperty("Count")!);
                indexer = i => Property(input, input.Type.FindIndexerProperty()!, i);
                break;
            }

            // Input is typed as an ICollection - we can get its length, but we can't index into it
            case { IsGenericType: true } when inputInterfaces.Append(input.Type)
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)):
            {
                getInputLength = Property(
                    input, typeof(ICollection<>).MakeGenericType(input.Type.GetGenericArguments()[0]).GetProperty("Count")!);
                indexer = null;
                break;
            }

            // Input is typed as an IEnumerable - we can't get its length, and we can't index into it.
            // All we can do is call ToList() on it and then process that.
            case { IsGenericType: true } when inputInterfaces.Append(input.Type)
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)):
            {
                // TODO: In theory, we could add runtime checks for array/list/collection, downcast for those cases and include
                // the logic from the other switch cases here.
                convertedInput = Variable(typeof(List<>).MakeGenericType(inputElementType), "convertedInput");
                variables.Add(convertedInput);
                expressions.Add(
                    Assign(
                        convertedInput,
                        Call(typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!.MakeGenericMethod(inputElementType), input)));
                getInputLength = Property(convertedInput, convertedInput.Type.GetProperty("Count")!);
                indexer = i => Property(convertedInput, convertedInput.Type.FindIndexerProperty()!, i);
                break;
            }

            default:
                throw new NotSupportedException($"Array value converter input type must be an IEnumerable, but is {typeof(TInput)}");
        }

        expressions.AddRange(
        [
            // Get the length of the input array or list
            // var length = input.Length;
            Assign(lengthVariable, getInputLength),

            // Allocate an output array or list
            // var result = new int[length];
            Assign(
                output, mutableOutputType.IsArray
                    ? NewArrayBounds(outputElementType, lengthVariable)
                    : mutableOutputType.GetConstructor([typeof(int)]) is ConstructorInfo ctorWithLength
                        ? New(ctorWithLength, lengthVariable)
                        : New(mutableOutputType.GetConstructor([])!))
        ]);

        if (indexer is not null)
        {
            // Good case: the input is an array or list, so we can index into it. Generate code for an efficient for loop, which applies
            // the element converter on each element.
            // for (var i = 0; i < length; i++)
            // {
            //     result[i] = input[i];
            // }
            var counter = Parameter(typeof(int), "i");

            expressions.Add(
                ForLoop(
                    loopVar: counter,
                    initValue: Constant(0),
                    condition: LessThan(counter, lengthVariable),
                    increment: AddAssign(counter, Constant(1)),
                    loopContent:
                    mutableOutputType.IsArray
                        ? Assign(
                            ArrayAccess(output, counter),
                            elementConversionExpression is null
                                ? indexer(counter)
                                : Invoke(elementConversionExpression, indexer(counter)))
                        : Call(
                            output,
                            mutableOutputType.GetMethod("Add", [outputElementType])!,
                            elementConversionExpression is null
                                ? indexer(counter)
                                : Invoke(elementConversionExpression, indexer(counter)))));
        }
        else
        {
            // Bad case: the input is not an array or list, but is a collection (e.g. HashSet), so we can't index into it.
            // Generate code for a less efficient enumerator-based iteration.
            // enumerator = input.GetEnumerator();
            // counter = 0;
            // while (enumerator.MoveNext())
            // {
            //     output[counter] = enumerator.Current;
            //     counter++;
            // }
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(inputElementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(inputElementType);

            var enumeratorVariable = Variable(enumeratorType, "enumerator");
            var counterVariable = Variable(typeof(int), "variable");
            variables.AddRange([enumeratorVariable, counterVariable]);

            expressions.AddRange(
            [
                // enumerator = input.GetEnumerator();
                Assign(enumeratorVariable, Call(input, enumerableType.GetMethod(nameof(IEnumerable<object>.GetEnumerator))!)),

                // counter = 0;
                Assign(counterVariable, Constant(0))
            ]);

            var breakLabel = Label("LoopBreak");

            var loop =
                Loop(
                    IfThenElse(
                        Equal(Call(enumeratorVariable, typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext))!), Constant(true)),
                        Block(
                            mutableOutputType.IsArray
                                // output[counter] = enumerator.Current;
                                ? Assign(
                                    ArrayAccess(output, counterVariable),
                                    elementConversionExpression is null
                                        ? Property(enumeratorVariable, "Current")
                                        : Invoke(elementConversionExpression, Property(enumeratorVariable, "Current")))
                                // output.Add(enumerator.Current);
                                : Call(
                                    output,
                                    mutableOutputType.GetMethod("Add", [outputElementType])!,
                                    elementConversionExpression is null
                                        ? Property(enumeratorVariable, "Current")
                                        : Invoke(elementConversionExpression, Property(enumeratorVariable, "Current"))),

                            // counter++;
                            AddAssign(counterVariable, Constant(1))),
                        Break(breakLabel)),
                    breakLabel);

            expressions.Add(
                TryFinally(
                    loop,
                    Call(enumeratorVariable, typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose))!)));
        }

        expressions.Add(
            outputIsImmutable
                // return output.ToFrozenSet(null);
                ? Call(
                    typeof(FrozenSet), nameof(FrozenSet.ToFrozenSet),
                    [outputElementType],
                    output,
                    Constant(null, typeof(IEqualityComparer<>).MakeGenericType(outputElementType)))
                // return output;
                : output);

        return Lambda<Func<TInput, TOutput>>(
            // First, check if the given array value is null and return null immediately if so
            Condition(
                ReferenceEqual(input, Constant(null)),
                Constant(null, typeof(TOutput)),
                Block(typeof(TOutput), variables, expressions)),
            input);
    }

    private static Expression ForLoop(
        ParameterExpression loopVar,
        Expression initValue,
        Expression condition,
        Expression increment,
        Expression loopContent)
    {
        var initAssign = Assign(loopVar, initValue);
        var breakLabel = Label("LoopBreak");
        var loop = Block(
            [loopVar],
            initAssign,
            Loop(
                IfThenElse(
                    condition,
                    Block(
                        loopContent,
                        increment
                    ),
                    Break(breakLabel)
                ),
                breakLabel)
        );

        return loop;
    }
}
