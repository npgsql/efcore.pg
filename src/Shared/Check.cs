using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Utilities;

[DebuggerStepThrough]
internal static class Check
{
    [ContractAnnotation("value:null => halt")]
    [return: NotNull]
    public static T NotNull<T>([NoEnumeration] [AllowNull] [NotNull] T value, [InvokerParameterName] string parameterName)
    {
        if (value is null)
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentNullException(parameterName);
        }

        return value;
    }

    [ContractAnnotation("value:null => halt")]
    public static IReadOnlyList<T> NotEmpty<T>(
        [NotNull] IReadOnlyList<T>? value,
        [InvokerParameterName] string parameterName)
    {
        NotNull(value, parameterName);

        if (value.Count == 0)
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentException(AbstractionsStrings.CollectionArgumentIsEmpty(parameterName));
        }

        return value;
    }

    [ContractAnnotation("value:null => halt")]
    public static string NotEmpty([NotNull] string? value, [InvokerParameterName] string parameterName)
    {
        if (value is null)
        {
            NotEmpty(parameterName, nameof(parameterName));
            throw new ArgumentNullException(parameterName);
        }

        if (value.Trim().Length == 0)
        {
            NotEmpty(parameterName, nameof(parameterName));
            throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
        }

        return value;
    }

    public static string? NullButNotEmpty(string? value, [InvokerParameterName] string parameterName)
    {
        if (value is not null && value.Length == 0)
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
        }

        return value;
    }

    public static IReadOnlyCollection<T>? NullButNotEmpty<T>(
        IReadOnlyCollection<T>? value,
        [InvokerParameterName] string parameterName)
    {
        if (value is { Count: 0 })
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
        }

        return value;
    }

    public static IReadOnlyList<T> HasNoNulls<T>(
        [NotNull] IReadOnlyList<T>? value,
        [InvokerParameterName] string parameterName)
        where T : class
    {
        NotNull(value, parameterName);

        if (value.Any(e => e is null))
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentException(parameterName);
        }

        return value;
    }

    public static IReadOnlyList<string> HasNoEmptyElements(
        [NotNull] IReadOnlyList<string>? value,
        [InvokerParameterName] string parameterName)
    {
        NotNull(value, parameterName);

        if (value.Any(s => string.IsNullOrWhiteSpace(s)))
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentException(AbstractionsStrings.CollectionArgumentHasEmptyElements(parameterName));
        }

        return value;
    }

    [Conditional("DEBUG")]
    public static void DebugAssert([DoesNotReturnIf(false)] bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Check.DebugAssert failed: {message}");
        }
    }

    [Conditional("DEBUG")]
    [DoesNotReturn]
    public static void DebugFail(string message)
        => throw new Exception($"Check.DebugFail failed: {message}");

    public static void IsDefined<T>(T value,
        [InvokerParameterName, CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new InvalidEnumArgumentException(parameterName, Convert.ToInt32(value), typeof(T));
        }
    }
}
