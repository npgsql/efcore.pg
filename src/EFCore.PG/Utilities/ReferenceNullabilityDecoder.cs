using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using CA = System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Utilities
{
    // Most of the code here is common with EF Core's NonNullableConventionBase
    // Note: this is a very partial implementation that does not correctly decode all scenarios.
    internal class ReferenceNullabilityDecoder
    {
        // For the interpretation of nullability metadata, see
        // https://github.com/dotnet/roslyn/blob/master/docs/features/nullable-metadata.md

        private readonly NonNullabilityConventionState _cache = new();

        private const string NullableAttributeFullName = "System.Runtime.CompilerServices.NullableAttribute";
        private const string NullableContextAttributeFullName = "System.Runtime.CompilerServices.NullableContextAttribute";

        protected virtual bool IsNonNullableReferenceType([NotNull] MemberInfo memberInfo)
        {
            if (memberInfo.GetMemberType().IsValueType)
            {
                return false;
            }

            // First check for [MaybeNull] on the return value. If it exists, the member is nullable.
            // Note: avoid using GetCustomAttribute<> below because of https://github.com/mono/mono/issues/17477
            var isMaybeNull = memberInfo switch
            {
                FieldInfo f
                    => f.CustomAttributes.Any(a => a.AttributeType == typeof(CA.MaybeNullAttribute)),
                PropertyInfo p
                    => p.GetMethod?.ReturnParameter?.CustomAttributes?.Any(a => a.AttributeType == typeof(CA.MaybeNullAttribute)) == true,
                _ => false
            };

            if (isMaybeNull)
            {
                return false;
            }

            // For C# 8.0 nullable types, the C# compiler currently synthesizes a NullableAttribute that expresses nullability into
            // assemblies it produces. If the model is spread across more than one assembly, there will be multiple versions of this
            // attribute, so look for it by name, caching to avoid reflection on every check.
            // Note that this may change - if https://github.com/dotnet/corefx/issues/36222 is done we can remove all of this.

            // First look for NullableAttribute on the member itself
            if (TryGetNullableFlags(memberInfo, out var flags))
            {
                return flags.FirstOrDefault() == 1;
            }

            // No attribute on the member, try to find a NullableContextAttribute on the declaring type
            return IsContextNonNullable(memberInfo.DeclaringType!);
        }

        internal bool IsArrayOrListElementNonNullable(MemberInfo memberInfo)
        {
            if (!memberInfo.GetMemberType().TryGetElementType(out var elementType))
            {
                throw new ArgumentException("Argument isn't an array or generic List", nameof(memberInfo));
            }

            if (elementType.IsValueType)
            {
                return elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            // First look for NullableAttribute on the member itself.
            // Either it has two flags - one for the array/list and one for the element - or a single compressed byte since both have the
            // same nullability.
            if (TryGetNullableFlags(memberInfo, out var flags))
            {
                return flags.Length switch
                {
                    1 => flags[0] == 1,
                    2 => flags[1] == 1,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            // No attribute on the member, try to find a NullableContextAttribute on the declaring type
            return IsContextNonNullable(memberInfo.DeclaringType!);
        }

        bool TryGetNullableFlags(MemberInfo memberInfo, [CA.NotNullWhen(true)] out byte[]? flags)
        {
            if (memberInfo.GetCustomAttributes().FirstOrDefault(a => a.GetType().FullName == NullableAttributeFullName) is Attribute
                attribute)
            {
                var attributeType = attribute.GetType();

                if (attributeType != _cache.NullableAttrType)
                {
                    _cache.NullableFlagsFieldInfo = attributeType.GetField("NullableFlags");
                    _cache.NullableAttrType = attributeType;
                }

                flags = (byte[]?)_cache.NullableFlagsFieldInfo?.GetValue(attribute);
                return flags is not null;
            }

            flags = null;
            return false;
        }

        private bool IsContextNonNullable(Type type)
        {
            if (_cache.TypeCache.TryGetValue(type, out var cachedTypeNonNullable))
            {
                return cachedTypeNonNullable;
            }

            if (Attribute.GetCustomAttributes(type)
                .FirstOrDefault(a => a.GetType().FullName == NullableContextAttributeFullName) is Attribute contextAttr)
            {
                var attributeType = contextAttr.GetType();

                if (attributeType != _cache.NullableContextAttrType)
                {
                    _cache.NullableContextFlagFieldInfo = attributeType.GetField("Flag");
                    _cache.NullableContextAttrType = attributeType;
                }

                if (_cache.NullableContextFlagFieldInfo?.GetValue(contextAttr) is byte flag)
                {
                    return _cache.TypeCache[type] = flag == 1;
                }
            }

            return _cache.TypeCache[type] = false;
        }

        private sealed class NonNullabilityConventionState
        {
            public Type? NullableAttrType;
            public Type? NullableContextAttrType;
            public FieldInfo? NullableFlagsFieldInfo;
            public FieldInfo? NullableContextFlagFieldInfo;
            public Dictionary<Type, bool> TypeCache { get; } = new();
        }
    }
}
