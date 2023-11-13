// ReSharper disable once CheckNamespace

namespace System.Reflection;

[DebuggerStepThrough]
internal static class PropertyInfoExtensions
{
    public static bool IsStatic(this PropertyInfo property)
        => (property.GetMethod ?? property.SetMethod)!.IsStatic;

    public static bool IsIndexerProperty(this PropertyInfo propertyInfo)
    {
        var indexParams = propertyInfo.GetIndexParameters();
        return indexParams.Length == 1
            && indexParams[0].ParameterType == typeof(string);
    }
}
