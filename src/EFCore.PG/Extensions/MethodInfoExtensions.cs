// ReSharper disable once CheckNamespace

namespace System.Reflection;

[DebuggerStepThrough]
internal static class MethodInfoExtensions
{
    internal static bool IsClosedFormOf(
        this MethodInfo methodInfo,
        MethodInfo genericMethod)
        => methodInfo.IsGenericMethod
            && Equals(
                methodInfo.GetGenericMethodDefinition(),
                genericMethod);
}
