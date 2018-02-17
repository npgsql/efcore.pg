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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.Internal.Mapping;
using Npgsql;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlCoreTypeMapper : RelationalCoreTypeMapperBase
    {
        readonly ConcurrentDictionary<string, RelationalTypeMapping> _storeTypeMappings;
        readonly ConcurrentDictionary<Type, RelationalTypeMapping> _clrTypeMappings;

        static readonly string[] SizableStoreTypes =
        {
            "character varying", "varchar",
            "character", "char",
            "bit", "bit varying"
        };

        #region Mappings

        readonly NpgsqlBoolTypeMapping      _bool      = new NpgsqlBoolTypeMapping();
        readonly NpgsqlByteArrayTypeMapping _bytea     = new NpgsqlByteArrayTypeMapping();
        readonly FloatTypeMapping           _float4    = new FloatTypeMapping("real", DbType.Single);
        readonly DoubleTypeMapping          _float8    = new DoubleTypeMapping("double precision", DbType.Double);
        readonly DecimalTypeMapping         _numeric   = new DecimalTypeMapping("numeric", DbType.Decimal);
        readonly DecimalTypeMapping         _money     = new DecimalTypeMapping("money");
        readonly GuidTypeMapping            _uuid      = new GuidTypeMapping("uuid", DbType.Guid);
        readonly ShortTypeMapping           _int2      = new ShortTypeMapping("smallint", DbType.Int16);
        readonly IntTypeMapping             _int4      = new IntTypeMapping("integer", DbType.Int32);
        readonly LongTypeMapping            _int8      = new LongTypeMapping("bigint", DbType.Int64);
        readonly StringTypeMapping          _text      = new StringTypeMapping("text", DbType.String);
        readonly StringTypeMapping          _varchar   = new StringTypeMapping("character varying", DbType.String);
        readonly StringTypeMapping          _char      = new StringTypeMapping("character", DbType.String);
        readonly NpgsqlJsonbTypeMapping     _jsonb     = new NpgsqlJsonbTypeMapping();
        readonly NpgsqlJsonTypeMapping      _json      = new NpgsqlJsonTypeMapping();
        readonly DateTimeTypeMapping        _timestamp = new DateTimeTypeMapping("timestamp without time zone", DbType.DateTime);
        // TODO: timestamptz
        readonly NpgsqlIntervalTypeMapping  _interval  = new NpgsqlIntervalTypeMapping();
        // TODO: time
        readonly NpgsqlTimeTzTypeMapping    _timetz    = new NpgsqlTimeTzTypeMapping();
        readonly NpgsqlMacaddrTypeMapping   _macaddr   = new NpgsqlMacaddrTypeMapping();
        readonly NpgsqlInetTypeMapping      _inet      = new NpgsqlInetTypeMapping();
        readonly NpgsqlBitTypeMapping       _bit       = new NpgsqlBitTypeMapping();
        readonly NpgsqlVarbitTypeMapping    _varbit    = new NpgsqlVarbitTypeMapping();
        readonly NpgsqlHstoreTypeMapping    _hstore    = new NpgsqlHstoreTypeMapping();
        readonly NpgsqlPointTypeMapping     _point     = new NpgsqlPointTypeMapping();
        readonly NpgsqlLineTypeMapping      _line      = new NpgsqlLineTypeMapping();
        readonly NpgsqlXidTypeMapping       _xid       = new NpgsqlXidTypeMapping();
        readonly NpgsqlOidTypeMapping       _oid       = new NpgsqlOidTypeMapping();
        readonly NpgsqlCidTypeMapping       _cid       = new NpgsqlCidTypeMapping();
        readonly NpgsqlRegtypeTypeMapping   _regtype   = new NpgsqlRegtypeTypeMapping();

        #endregion Mappings

        public NpgsqlCoreTypeMapper([NotNull] CoreTypeMapperDependencies dependencies,
            [NotNull] RelationalTypeMapperDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
            // Note that PostgreSQL has aliases to some built-in types (e.g. int4 for integer),
            // these are mapped as well.
            // https://www.postgresql.org/docs/9.5/static/datatype.html#DATATYPE-TABLE
            var storeTypeMappings = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
            {
                { "boolean",                     _bool      },
                { "bool",                        _bool      },
                { "bytea",                       _bytea     },
                { "real",                        _float4    },
                { "float4",                      _float4    },
                { "double precision",            _float8    },
                { "float8",                      _float8    },
                { "numeric",                     _numeric   },
                { "decimal",                     _numeric   },
                { "money",                       _money     },
                { "uuid",                        _uuid      },
                { "smallint",                    _int2      },
                { "int2",                        _int2      },
                { "integer",                     _int4      },
                { "int",                         _int4      },
                { "int4",                        _int4      },
                { "bigint",                      _int8      },
                { "int8",                        _int8      },
                { "text",                        _text      },
                { "jsonb",                       _jsonb     },
                { "json",                        _json      },
                { "character varying",           _varchar   },
                { "varchar",                     _varchar   },
                { "character",                   _char      },
                { "char",                        _char      },
                { "timestamp without time zone", _timestamp },
                { "timestamp",                   _timestamp },
                { "interval",                    _interval  },
                { "time with time zone",         _timetz    },
                { "timetz",                      _timetz    },
                { "macaddr",                     _macaddr   },
                { "inet",                        _inet      },
                { "bit",                         _bit       },
                { "bit varying",                 _varbit    },
                { "varbit",                      _varbit    },
                { "hstore",                      _hstore    },
                { "point",                       _point     },
                { "line",                        _line      },
                { "xid",                         _xid       },
                { "oid",                         _oid       },
                { "cid",                         _cid       },
                { "regtype",                     _regtype   },
            };

            var clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(bool),                       _bool      },
                { typeof(byte[]),                     _bytea     },
                { typeof(float),                      _float4    },
                { typeof(double),                     _float8    },
                { typeof(decimal),                    _numeric   },
                { typeof(Guid),                       _uuid      },
                { typeof(short),                      _int2      },
                { typeof(int),                        _int4      },
                { typeof(long),                       _int8      },
                { typeof(string),                     _text      },
                { typeof(DateTime),                   _timestamp },
                { typeof(TimeSpan),                   _interval  },
                { typeof(PhysicalAddress),            _macaddr   },
                { typeof(IPAddress),                  _inet      },
                { typeof(BitArray),                   _varbit    },
                { typeof(Dictionary<string, string>), _hstore    },
                { typeof(NpgsqlPoint),                _point     },
                { typeof(NpgsqlLine),                 _line      },
            };

            _storeTypeMappings = new ConcurrentDictionary<string, RelationalTypeMapping>(storeTypeMappings);
            _clrTypeMappings = new ConcurrentDictionary<Type, RelationalTypeMapping>(clrTypeMappings);
        }

        protected override RelationalTypeMapping FindMapping(RelationalTypeMappingInfo mappingInfo)
        {
            RelationalTypeMapping mapping;

            var storeType = mappingInfo.StoreTypeName;
            if (storeType != null)
            {
                if (!_storeTypeMappings.TryGetValue(mappingInfo.StoreTypeName, out mapping))
                    mapping = FindSizableMapping(storeType);
                if (mapping != null)
                    return mapping;
            }

            var clrType = mappingInfo.TargetClrType;
            if (clrType == null)
            {
                //Log.Warn($"Received RelationalTypeMappingInfo without {mappingInfo.StoreTypeName} or {mappingInfo.TargetClrType}");
                return null;
            }

            // TODO: Cache sized mappings?
            if (mappingInfo.Size.HasValue)
            {
                if (clrType == typeof(string))
                    return _varchar.Clone($"varchar({mappingInfo.Size})", mappingInfo.Size);
                if (clrType == typeof(BitArray))
                    return _varbit.Clone($"varbit({mappingInfo.Size})", mappingInfo.Size);
            }

            if (_clrTypeMappings.TryGetValue(clrType, out mapping))
                return mapping;

            mapping = FindArrayMapping(mappingInfo);
            return mapping;

            // TODO: range, enum, composite
        }

        RelationalTypeMapping FindArrayMapping(RelationalTypeMappingInfo mappingInfo)
        {
            // PostgreSQL array types prefix the element type with underscore
            var storeType = mappingInfo.StoreTypeName;
            if (storeType != null && storeType.StartsWith("_"))
            {
                var elementMapping = FindMapping(storeType.Substring(1));
                if (elementMapping != null)
                    return _storeTypeMappings.GetOrAdd(storeType, new NpgsqlArrayTypeMapping(elementMapping));
            }

            var clrType = mappingInfo.TargetClrType;
            if (clrType == null)
                return null;

            // Try to see if it is an array type
            var arrayElementType = GetArrayElementType(clrType);
            if (arrayElementType != null)
            {
                var elementMapping = (RelationalTypeMapping)FindMapping(arrayElementType);

                // If an element isn't supported, neither is its array
                if (elementMapping == null)
                    return null;

                // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
                if (elementMapping is NpgsqlArrayTypeMapping)
                    return null;

                return _clrTypeMappings.GetOrAdd(clrType, new NpgsqlArrayTypeMapping(elementMapping, clrType));
            }

            return null;
        }

        RelationalTypeMapping FindSizableMapping(string storeType)
        {
            var openParen = storeType.IndexOf("(", StringComparison.Ordinal);
            if (openParen <= 0)
                return null;

            var baseStoreType = storeType.Substring(0, openParen).ToLower();

            if (!SizableStoreTypes.Contains(baseStoreType))
                return null;

            // TODO: Shouldn't happen, at least warn
            if (!_storeTypeMappings.TryGetValue(baseStoreType, out var mapping))
            {
                Debug.Fail($"Type is in {nameof(SizableStoreTypes)} but wasn't found in {nameof(_storeTypeMappings)}");
                return null;
            }

            var closeParen = storeType.IndexOf(")", openParen + 1, StringComparison.Ordinal);

            // TODO: Cache sized mappings?
            if (closeParen > openParen
                && int.TryParse(storeType.Substring(openParen + 1, closeParen - openParen - 1), out var size))
            {
                return mapping.Clone(storeType, size);
            }

            return null;
        }

        [CanBeNull]
        static Type GetArrayElementType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsArray)
                return type.GetElementType();

            var ilist = typeInfo.ImplementedInterfaces.FirstOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            return ilist != null ? ilist.GetGenericArguments()[0] : null;
        }
#if NO
        private readonly Dictionary<string, IList<RelationalTypeMapping>> _storeTypeMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
        private readonly ConcurrentDictionary<Type, RelationalTypeMapping> _extraClrMappings;

        public override IStringRelationalTypeMapper StringMapper { get; }

        public NpgsqlCoreTypeMapper(
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
#endif
    }
}
