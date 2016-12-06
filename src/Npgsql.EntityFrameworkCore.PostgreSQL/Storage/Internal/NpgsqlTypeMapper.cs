#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage;
using Npgsql.TypeHandlers;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlTypeMapper : RelationalTypeMapper
    {
        readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
        readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

        readonly ConcurrentDictionary<Type, NpgsqlArrayTypeMapping> _arrayMappings
            = new ConcurrentDictionary<Type, NpgsqlArrayTypeMapping>();

        public override IStringRelationalTypeMapper StringMapper { get; }

        public NpgsqlTypeMapper()
        {
            // First, PostgreSQL type name (string) -> RelationalTypeMapping
            _storeTypeMappings = TypeHandlerRegistry.HandlerTypes.Values
                // Base types
                .Where(tam => tam.Mapping.NpgsqlDbType.HasValue)
                .Select(tam => new {
                    Name = tam.Mapping.PgName,
                    Mapping = (RelationalTypeMapping)new NpgsqlBaseTypeMapping(tam.Mapping.PgName, GetTypeHandlerTypeArgument(tam.HandlerType), tam.Mapping.NpgsqlDbType.Value)
                })
                // Enums
                //.Concat(TypeHandlerRegistry.GlobalEnumMappings.Select(kv => new {
                //    Name = kv.Key,
                //    Mapping = (RelationalTypeMapping)new NpgsqlTypeMapping(kv.Key, ((IEnumHandler)kv.Value).EnumType)
                //}))
                // Composites
                //.Concat(TypeHandlerRegistry.GlobalCompositeMappings.Select(kv => new {
                //    Name = kv.Key,
                //    Mapping = (RelationalTypeMapping)new NpgsqlTypeMapping(kv.Key, ((ICompositeHandler)kv.Value).CompositeType)
                //}))
                // Output
                .ToDictionary(x => x.Name, x => x.Mapping);

            // Second, CLR type -> RelationalTypeMapping
            _clrTypeMappings = TypeHandlerRegistry.HandlerTypes.Values
                // Base types
                .Select(tam => tam.Mapping)
                .Where(m => m.NpgsqlDbType.HasValue)
                .SelectMany(m => m.ClrTypes, (m, t) => new {
                    Type = t,
                    Mapping = (RelationalTypeMapping)new NpgsqlBaseTypeMapping(m.PgName, t, m.NpgsqlDbType.Value)
                })
                // Enums
                //.Concat(TypeHandlerRegistry.GlobalEnumMappings.Select(kv => new {
                //    Type = ((IEnumHandler)kv.Value).EnumType,
                //    Mapping = (RelationalTypeMapping)new NpgsqlTypeMapping(kv.Key, ((IEnumHandler)kv.Value).EnumType)
                //}))
                // Composites
                //.Concat(TypeHandlerRegistry.GlobalCompositeMappings.Select(kv => new {
                //    Type = ((ICompositeHandler)kv.Value).CompositeType,
                //    Mapping = (RelationalTypeMapping)new NpgsqlTypeMapping(kv.Key, ((ICompositeHandler)kv.Value).CompositeType)
                //}))
                // Output
                .ToDictionary(x => x.Type, x => x.Mapping);

            // uint is special: there are three internal system uint types: oid, xid, cid. None are supposed to
            // be truly user-facing, so we don't want to automatically map uint properties to any of them.
            // However, if the user explicitly sets the properties store type to oid/xid/cid, we want to allow
            // that (especially since the xmin system column is important for optimistic concurrency).
            // EFCore doesn't allow a situation where a CLR type has no default store type, so we arbitrarily
            // choose oid.
            _clrTypeMappings[typeof(uint)] = new NpgsqlBaseTypeMapping("oid", typeof(uint), NpgsqlDbType.Oid);

            StringMapper = new NpgsqlStringRelationalTypeMapper();
        }

        protected override string GetColumnType(IProperty property) => property.Npgsql().ColumnType;

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _clrTypeMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _storeTypeMappings;

        [CanBeNull]
        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            var mapping = base.FindMapping(clrType);
            if (mapping != null)
                return mapping;

            // Check if it's an array or generic IList
            Type arrayElementType = null;
            if (clrType.IsArray)
                arrayElementType = clrType.GetElementType();
            else if (typeof(IList).IsAssignableFrom(clrType) && clrType.GetTypeInfo().IsGenericType)
                arrayElementType = clrType.GetGenericArguments()[0];

            if (arrayElementType != null)
            {
                // At least for now, we only support arrays of base (scalar) types.
                // Notably, arrays of arrays aren't supported (as opposed to multidimensional arrays) because PostgreSQL
                // doesn't support them.
                var elementMapping = FindMapping(arrayElementType) as NpgsqlBaseTypeMapping;

                // If an element isn't supported, neither is its array
                if (elementMapping?.NpgsqlDbType == null)
                    return null;

                return _arrayMappings.GetOrAdd(clrType, t => new NpgsqlArrayTypeMapping(clrType, elementMapping));
            }

            return null;
        }

        [CanBeNull]
        protected override RelationalTypeMapping FindCustomMapping(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var clrType = property.ClrType.UnwrapNullableType();

            return clrType == typeof(string)
                ? GetStringMapping(property)
                : null;
        }

        static Type GetTypeHandlerTypeArgument(Type handler)
        {
            while (!handler.GetTypeInfo().IsGenericType || handler.GetGenericTypeDefinition() != typeof(TypeHandler<>))
            {
                handler = handler.GetTypeInfo().BaseType;
                if (handler == null)
                {
                    throw new Exception("Npgsql type handler doesn't inherit from TypeHandler<>?");
                }
            }

            return handler.GetGenericArguments()[0];
        }
    }
}
