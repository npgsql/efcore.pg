using System.Collections;
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

        var input = Parameter(typeof(TInput), "value");
        var output = Parameter(typeof(TConcreteOutput), "result");
        var loopVariable = Parameter(typeof(int), "i");
        var lengthVariable = Variable(typeof(int), "length");

        var expressions = new List<Expression>();
        var variables = new List<ParameterExpression>(4)
        {
            output,
            lengthVariable,
        };

        Expression getInputLength;
        Func<Expression, Expression> indexer;

        if (typeof(TInput).IsArray)
        {
            getInputLength = ArrayLength(input);
            indexer = i => ArrayAccess(input, i);
        }
        else if (typeof(TInput).IsGenericType
                 && typeof(TInput).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
        {
            getInputLength = Property(
                input,
                typeof(TInput).GetProperty("Count")
                // If TInput is an interface (IList<T>), its Count property needs to be found on ICollection<T>
                ?? typeof(ICollection<>).MakeGenericType(typeof(TInput).GetGenericArguments()[0]).GetProperty("Count")!);
            indexer = i => Property(input, input.Type.FindIndexerProperty()!, i);
        }
        else
        {
            // Input collection isn't typed as an ICollection<T>; it can be *typed* as an IEnumerable<T>, but we only support concrete
            // instances being ICollection<T>. Emit code that casts the type at runtime.
            var iListType = typeof(IList<>).MakeGenericType(typeof(TInput).GetGenericArguments()[0]);

            var convertedInput = Variable(iListType, "convertedInput");
            variables.Add(convertedInput);

            expressions.Add(Assign(convertedInput, Convert(input, convertedInput.Type)));

            // TODO: Check and properly throw for non-IList<T>, e.g. set
            getInputLength = Property(
                convertedInput, typeof(ICollection<>).MakeGenericType(typeof(TInput).GetGenericArguments()[0]).GetProperty("Count")!);
            indexer = i => Property(convertedInput, iListType.FindIndexerProperty()!, i);
        }

        expressions.AddRange(
        [
            // Get the length of the input array or list
                // var length = input.Length;
                Assign(lengthVariable, getInputLength),

                // Allocate an output array or list
                // var result = new int[length];
                Assign(
                    output, typeof(TConcreteOutput).IsArray
                        ? NewArrayBounds(outputElementType, lengthVariable)
                        : typeof(TConcreteOutput).GetConstructor([typeof(int)]) is ConstructorInfo ctorWithLength
                            ? New(ctorWithLength, lengthVariable)
                            : New(typeof(TConcreteOutput).GetConstructor([])!)),

                // Loop over the elements, applying the element converter on them one by one
                // for (var i = 0; i < length; i++)
                // {
                //     result[i] = input[i];
                // }
                ForLoop(
                    loopVar: loopVariable,
                    initValue: Constant(0),
                    condition: LessThan(loopVariable, lengthVariable),
                    increment: AddAssign(loopVariable, Constant(1)),
                    loopContent:
                    typeof(TConcreteOutput).IsArray
                        ? Assign(
                            ArrayAccess(output, loopVariable),
                            elementConversionExpression is null
                                ? indexer(loopVariable)
                                : Invoke(elementConversionExpression, indexer(loopVariable)))
                        : Call(
                            output,
                            typeof(TConcreteOutput).GetMethod("Add", [outputElementType])!,
                            elementConversionExpression is null
                                ? indexer(loopVariable)
                                : Invoke(elementConversionExpression, indexer(loopVariable)))),
                output
        ]);

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
