using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    internal static class EntityFrameworkMemberInfoExtensions
    {
        public static Type GetMemberType(this MemberInfo memberInfo)
            => (memberInfo as PropertyInfo)?.PropertyType ?? ((FieldInfo)memberInfo).FieldType;

        public static bool IsSameAs(this MemberInfo? propertyInfo, MemberInfo? otherPropertyInfo)
            => propertyInfo == null
                ? otherPropertyInfo == null
                : (otherPropertyInfo != null
                    && (Equals(propertyInfo, otherPropertyInfo)
                        || (propertyInfo.Name == otherPropertyInfo.Name
                            && propertyInfo.DeclaringType != null
                            && otherPropertyInfo.DeclaringType != null
                            && (propertyInfo.DeclaringType == otherPropertyInfo.DeclaringType
                                || propertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(otherPropertyInfo.DeclaringType)
                                || otherPropertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(propertyInfo.DeclaringType)
                                || propertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces.Contains(otherPropertyInfo.DeclaringType)
                                || otherPropertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces
                                    .Contains(propertyInfo.DeclaringType)))));

        public static string GetSimpleMemberName(this MemberInfo member)
        {
            var name = member.Name;
            var index = name.LastIndexOf('.');
            return index >= 0 ? name.Substring(index + 1) : name;
        }
    }
}
