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
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlEFTypeMapper : RelationalTypeMapper
    {
        private readonly Dictionary<string, IList<RelationalTypeMapping>> _storeTypeMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
        private readonly ConcurrentDictionary<Type, RelationalTypeMapping> _extraClrMappings;

        public override IStringRelationalTypeMapper StringMapper { get; }

        public NpgsqlEFTypeMapper(
            [NotNull] CoreTypeMapperDependencies coreDependencies,
            [NotNull] RelationalTypeMapperDependencies dependencies)
            : base(coreDependencies, dependencies)
        {
            _storeTypeMappings = TypeHandlerRegistry.HandlerTypes.Values
                    .Where(tam => tam.Mapping.NpgsqlDbType.HasValue)
                    .Select(tam => new
                    {
                        Name = tam.Mapping.PgName,
                        Mapping = (IList<RelationalTypeMapping>)new List<RelationalTypeMapping> { new NpgsqlBaseTypeMapping(tam.Mapping.PgName, GetTypeHandlerTypeArgument(tam.HandlerType), tam.Mapping.NpgsqlDbType.Value) }
                    }).ToDictionary(x => x.Name, x => x.Mapping);

            _clrTypeMappings = TypeHandlerRegistry.HandlerTypes.Values
                .Select(tam => tam.Mapping)
                .Where(m => m.NpgsqlDbType.HasValue)
                .SelectMany(m => m.ClrTypes, (m, t) => new
                {
                    Type = t,
                    Mapping = (RelationalTypeMapping)new NpgsqlBaseTypeMapping(m.PgName, t, m.NpgsqlDbType.Value)
                })
                .ToDictionary(x => x.Type, x => x.Mapping);

            _extraClrMappings = new ConcurrentDictionary<Type, RelationalTypeMapping>();

            StringMapper = new NpgsqlStringRelationalTypeMapper();

            AddCustomizedStoreMappings();
            AddCustomizedMappings();
            AddArrayStoreMappings();
        }

        void AddCustomizedStoreMappings()
        {
            //Custom Mappings Store
            _storeTypeMappings["text"] = new List<RelationalTypeMapping> { new NpgsqlStringTypeMapping("text", NpgsqlDbType.Text) };
            _storeTypeMappings["varchar"] = new List<RelationalTypeMapping> { new NpgsqlStringTypeMapping("varchar", NpgsqlDbType.Varchar) };
            _storeTypeMappings["citext"] = new List<RelationalTypeMapping> { new NpgsqlStringTypeMapping("citext", NpgsqlDbType.Citext) };
            _storeTypeMappings["json"] = new List<RelationalTypeMapping> { new NpgsqlStringTypeMapping("json", NpgsqlDbType.Json) };
            _storeTypeMappings["jsonb"] = new List<RelationalTypeMapping> { new NpgsqlStringTypeMapping("jsonb", NpgsqlDbType.Jsonb) };
            _storeTypeMappings["timestamp"] = new List<RelationalTypeMapping> { new DateTimeTypeMapping("timestamp", DbType.DateTime) };
            _storeTypeMappings["timestamptz"] = new List<RelationalTypeMapping> { new NpgsqlDateTimeOffsetTypeMapping("timestamptz", DbType.DateTimeOffset) };
            _storeTypeMappings["bool"] = new List<RelationalTypeMapping> { new NpgsqlBoolTypeMapping() };
            _storeTypeMappings["uuid"] = new List<RelationalTypeMapping> { new GuidTypeMapping("uuid", DbType.Guid) };
            _storeTypeMappings["bytea"] = new List<RelationalTypeMapping> { new NpgsqlByteArrayTypeMapping() };
            _storeTypeMappings["int2"] = new List<RelationalTypeMapping> { new ShortTypeMapping("int2", DbType.Int16) };
            _storeTypeMappings["int4"] = new List<RelationalTypeMapping> { new IntTypeMapping("int4", DbType.Int32) };
            _storeTypeMappings["int8"] = new List<RelationalTypeMapping> { new IntTypeMapping("int8", DbType.Int64) };
        }

        void AddCustomizedMappings()
        {
            var compileChar = new CharToStringConverter().ConvertToStoreExpression.Compile();

            // Mappings where we need literal string generation
            _clrTypeMappings[typeof(string)] = _storeTypeMappings["text"][0];
            _clrTypeMappings[typeof(char)] = new CharTypeMapping(
                "text",
                new ValueConverter<char, string>(v => compileChar(v), v => char.Parse(v)),
                DbType.String);

            _clrTypeMappings[typeof(DateTime)] = _storeTypeMappings["timestamp"][0];
            _clrTypeMappings[typeof(DateTimeOffset)] = _storeTypeMappings["timestamptz"][0];
            _clrTypeMappings[typeof(bool)] = _storeTypeMappings["bool"][0];

            // Note that "decimal" in PostgreSQL is just an alias for numeric, PostgreSQL itself always reports numeric for column types.
            _clrTypeMappings[typeof(decimal)] = new DecimalTypeMapping("numeric", DbType.Decimal);

            _clrTypeMappings[typeof(Guid)] = _storeTypeMappings["uuid"][0];
            _clrTypeMappings[typeof(byte[])] = _storeTypeMappings["bytea"][0];

            // The following isn't necessary for int itself - a simple ToString() (the default) produces the right
            // literal representation. However, with an explicit int mapping the standard mapping would be returned
            // for enums, and there a simple ToString produces the enum *name*.
            // Example for test for enum literal: InheritanceNpgsqlTest.Can_query_just_roses

            _clrTypeMappings[typeof(short)] = _storeTypeMappings["int2"][0];
            _clrTypeMappings[typeof(int)] = _storeTypeMappings["int4"][0];
            _clrTypeMappings[typeof(long)] = _storeTypeMappings["int8"][0];

            // uint is special: there are three internal system uint types: oid, xid, cid. None are supposed to
            // be truly user-facing, so we don't want to automatically map uint properties to any of them.
            // However, if the user explicitly sets the properties store type to oid/xid/cid, we want to allow
            // that (especially since the xmin system column is important for optimistic concurrency).
            // EFCore doesn't allow a situation where a CLR type has no default store type, so we arbitrarily
            // choose oid.
            //_clrTypeMappings[typeof(uint)] = new NpgsqlBaseTypeMapping("oid", typeof(uint), NpgsqlDbType.Oid);
        }

        void AddArrayStoreMappings()
        {
            foreach (var elementMapping in _storeTypeMappings.Values.ToList())
            {
                foreach (var element in elementMapping)
                {
                    var arrayMapping = new NpgsqlArrayTypeMapping(element.ClrType.MakeArrayType(), element);
                    _storeTypeMappings[arrayMapping.StoreType] = new List<RelationalTypeMapping> { arrayMapping };
                }
            }
        }

        protected override string GetColumnType(IProperty property)
            => property.Npgsql().ColumnType;

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _clrTypeMappings;

        protected override IReadOnlyDictionary<string, IList<RelationalTypeMapping>> GetMultipleStoreTypeMappings()
            => _storeTypeMappings;

        [CanBeNull]
        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            RelationalTypeMapping mapping;
            var unwrappedType = clrType.UnwrapNullableType().UnwrapEnumType();

            if (_clrTypeMappings.TryGetValue(unwrappedType, out mapping))
                return mapping;
            if (_extraClrMappings.TryGetValue(unwrappedType, out mapping))
                return mapping;

            // Type hasn't been seen before - we may need to add a mapping (e.g. array)
            // Try to see if it is an array type

            var arrayElementType = GetArrayElementType(unwrappedType);
            if (arrayElementType != null)
            {
                var elementMapping = FindMapping(arrayElementType);

                // If an element isn't supported, neither is its array
                if (elementMapping == null)
                    return null;

                // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
                if (elementMapping is NpgsqlArrayTypeMapping)
                    return null;

                return _extraClrMappings.GetOrAdd(unwrappedType, t => new NpgsqlArrayTypeMapping(unwrappedType, elementMapping));
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

        [CanBeNull]
        static Type GetArrayElementType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsArray)
                return type.GetElementType();

            var ilist = typeInfo.ImplementedInterfaces.FirstOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            if (ilist != null)
                return ilist.GetGenericArguments()[0];

            if (typeof(IList).IsAssignableFrom(type))
                throw new NotSupportedException("Non-generic IList is a supported parameter, but the NpgsqlDbType parameter must be set on the parameter");

            return null;
        }
    }
}
