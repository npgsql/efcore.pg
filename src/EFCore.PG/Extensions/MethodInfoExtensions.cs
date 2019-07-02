using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    [DebuggerStepThrough]
    internal static class MethodInfoExtensions
    {
        internal static bool IsClosedFormOf(
            [NotNull] this MethodInfo methodInfo, [NotNull] MethodInfo genericMethod)
            => methodInfo.IsGenericMethod
               && Equals(
                   methodInfo.GetGenericMethodDefinition(),
                   genericMethod);
    }
}
