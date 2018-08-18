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
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using Npgsql.TypeHandlers;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlTypeMappingSource : RelationalTypeMappingSource
    {
        public ConcurrentDictionary<string, RelationalTypeMapping[]> StoreTypeMappings { get; }
        public ConcurrentDictionary<Type, RelationalTypeMapping> ClrTypeMappings { get; }

        readonly HashSet<string> _userDefinedTypes = new HashSet<string>();

        #region Mappings

        // Numeric types
        readonly FloatTypeMapping              _float4             = new FloatTypeMapping("real", DbType.Single);
        readonly DoubleTypeMapping             _float8             = new DoubleTypeMapping("double precision", DbType.Double);
        readonly DecimalTypeMapping            _numeric            = new DecimalTypeMapping("numeric", DbType.Decimal);
        readonly DecimalTypeMapping            _money              = new DecimalTypeMapping("money");
        readonly GuidTypeMapping               _uuid               = new GuidTypeMapping("uuid", DbType.Guid);
        readonly ShortTypeMapping              _int2               = new ShortTypeMapping("smallint", DbType.Int16);
        readonly ByteTypeMapping               _int2Byte           = new ByteTypeMapping("smallint", DbType.Byte);
        readonly IntTypeMapping                _int4               = new IntTypeMapping("integer", DbType.Int32);
        readonly LongTypeMapping               _int8               = new LongTypeMapping("bigint", DbType.Int64);

        // Character types
        readonly NpgsqlStringTypeMapping       _text               = new NpgsqlStringTypeMapping("text");
        readonly NpgsqlStringTypeMapping       _varchar            = new NpgsqlStringTypeMapping("character varying");
        readonly NpgsqlStringTypeMapping       _char               = new NpgsqlStringTypeMapping("character");
        readonly CharTypeMapping               _singleChar         = new CharTypeMapping("character(1)", DbType.String);
        readonly NpgsqlStringTypeMapping       _stringAsSingleChar = new NpgsqlStringTypeMapping("character(1)");
        readonly NpgsqlJsonbTypeMapping        _jsonb              = new NpgsqlJsonbTypeMapping();
        readonly NpgsqlJsonTypeMapping         _json               = new NpgsqlJsonTypeMapping();
        readonly NpgsqlXmlTypeMapping          _xml                = new NpgsqlXmlTypeMapping();
        readonly NpgsqlCitextTypeMapping       _citext             = new NpgsqlCitextTypeMapping();

        // Date/Time types
        readonly NpgsqlDateTypeMapping         _date               = new NpgsqlDateTypeMapping();
        readonly NpgsqlTimestampTypeMapping    _timestamp          = new NpgsqlTimestampTypeMapping();
        readonly NpgsqlTimestampTzTypeMapping  _timestamptz        = new NpgsqlTimestampTzTypeMapping(typeof(DateTime));
        readonly NpgsqlTimestampTzTypeMapping  _timestamptzDto     = new NpgsqlTimestampTzTypeMapping(typeof(DateTimeOffset));
        readonly NpgsqlIntervalTypeMapping     _interval           = new NpgsqlIntervalTypeMapping();
        readonly NpgsqlTimeTypeMapping         _time               = new NpgsqlTimeTypeMapping();
        readonly NpgsqlTimeTzTypeMapping       _timetz             = new NpgsqlTimeTzTypeMapping();

        // Network address types
        readonly NpgsqlMacaddrTypeMapping      _macaddr            = new NpgsqlMacaddrTypeMapping();
        readonly NpgsqlMacaddr8TypeMapping     _macaddr8           = new NpgsqlMacaddr8TypeMapping();
        readonly NpgsqlInetTypeMapping         _inet               = new NpgsqlInetTypeMapping();
        readonly NpgsqlCidrTypeMapping         _cidr               = new NpgsqlCidrTypeMapping();

        // Built-in geometric types
        readonly NpgsqlPointTypeMapping        _point              = new NpgsqlPointTypeMapping();
        readonly NpgsqlBoxTypeMapping          _box                = new NpgsqlBoxTypeMapping();
        readonly NpgsqlLineTypeMapping         _line               = new NpgsqlLineTypeMapping();
        readonly NpgsqlLineSegmentTypeMapping  _lseg               = new NpgsqlLineSegmentTypeMapping();
        readonly NpgsqlPathTypeMapping         _path               = new NpgsqlPathTypeMapping();
        readonly NpgsqlPolygonTypeMapping      _polygon            = new NpgsqlPolygonTypeMapping();
        readonly NpgsqlCircleTypeMapping       _circle             = new NpgsqlCircleTypeMapping();

        // uint mappings
        readonly NpgsqlUintTypeMapping         _xid                = new NpgsqlUintTypeMapping("xid", NpgsqlDbType.Xid);
        readonly NpgsqlUintTypeMapping         _oid                = new NpgsqlUintTypeMapping("oid", NpgsqlDbType.Oid);
        readonly NpgsqlUintTypeMapping         _cid                = new NpgsqlUintTypeMapping("cid", NpgsqlDbType.Cid);
        readonly NpgsqlUintTypeMapping         _regtype            = new NpgsqlUintTypeMapping("regtype", NpgsqlDbType.Regtype);
        readonly NpgsqlUintTypeMapping         _lo                 = new NpgsqlUintTypeMapping("lo", NpgsqlDbType.Oid);

        // Full text search mappings
        readonly NpgsqlTsQueryTypeMapping   _tsquery               = new NpgsqlTsQueryTypeMapping();
        readonly NpgsqlTsVectorTypeMapping  _tsvector              = new NpgsqlTsVectorTypeMapping();
        readonly NpgsqlRegconfigTypeMapping _regconfig             = new NpgsqlRegconfigTypeMapping();
        readonly NpgsqlTsRankingNormalizationTypeMapping _rankingNormalization = new NpgsqlTsRankingNormalizationTypeMapping();

        // Built-in ranges
        readonly NpgsqlRangeTypeMapping        _int4range;
        readonly NpgsqlRangeTypeMapping        _int8range;
        readonly NpgsqlRangeTypeMapping        _numrange;
        readonly NpgsqlRangeTypeMapping        _tsrange;
        readonly NpgsqlRangeTypeMapping        _tstzrange;
        readonly NpgsqlRangeTypeMapping        _daterange;

        // Other types
        readonly NpgsqlBoolTypeMapping         _bool               = new NpgsqlBoolTypeMapping();
        readonly NpgsqlBitTypeMapping          _bit                = new NpgsqlBitTypeMapping();
        readonly NpgsqlVarbitTypeMapping       _varbit             = new NpgsqlVarbitTypeMapping();
        readonly NpgsqlByteArrayTypeMapping    _bytea              = new NpgsqlByteArrayTypeMapping();
        readonly NpgsqlHstoreTypeMapping       _hstore             = new NpgsqlHstoreTypeMapping();
        readonly NpgsqlTidTypeMapping          _tid                = new NpgsqlTidTypeMapping();

        #endregion Mappings

        public NpgsqlTypeMappingSource([NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies,
            [CanBeNull] INpgsqlOptions npgsqlOptions=null)
            : base(dependencies, relationalDependencies)
        {
            // Initialize some mappings which depend on other mappings
            _int4range = new NpgsqlRangeTypeMapping("int4range", typeof(NpgsqlRange<int>), _int4);
            _int8range = new NpgsqlRangeTypeMapping("int8range", typeof(NpgsqlRange<long>), _int8);
            _numrange  = new NpgsqlRangeTypeMapping("numrange",  typeof(NpgsqlRange<decimal>), _numeric);
            _tsrange   = new NpgsqlRangeTypeMapping("tsrange",   typeof(NpgsqlRange<DateTime>), _timestamp);
            _tstzrange = new NpgsqlRangeTypeMapping("tstzrange", typeof(NpgsqlRange<DateTime>), _timestamptz);
            _daterange = new NpgsqlRangeTypeMapping("daterange", typeof(NpgsqlRange<DateTime>), _timestamptz);

            // Note that PostgreSQL has aliases to some built-in type name aliases (e.g. int4 for integer),
            // these are mapped as well.
            // https://www.postgresql.org/docs/current/static/datatype.html#DATATYPE-TABLE
            var storeTypeMappings = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "boolean",                     new[] { _bool                         } },
                { "bool",                        new[] { _bool                         } },
                { "bytea",                       new[] { _bytea                        } },
                { "real",                        new[] { _float4                       } },
                { "float4",                      new[] { _float4                       } },
                { "double precision",            new[] { _float8                       } },
                { "float8",                      new[] { _float8                       } },
                { "numeric",                     new[] { _numeric                      } },
                { "decimal",                     new[] { _numeric                      } },
                { "money",                       new[] { _money                        } },
                { "uuid",                        new[] { _uuid                         } },
                { "smallint",                    new RelationalTypeMapping[] { _int2, _int2Byte } },
                { "int2",                        new RelationalTypeMapping[] { _int2, _int2Byte } },
                { "integer",                     new[] { _int4                         } },
                { "int",                         new[] { _int4                         } },
                { "int4",                        new[] { _int4                         } },
                { "bigint",                      new[] { _int8                         } },
                { "int8",                        new[] { _int8                         } },
                { "text",                        new[] { _text                         } },
                { "jsonb",                       new[] { _jsonb                        } },
                { "json",                        new[] { _json                         } },
                { "xml",                         new[] { _xml                          } },
                { "citext",                      new[] { _citext                       } },
                { "character varying",           new[] { _varchar                      } },
                { "varchar",                     new[] { _varchar                      } },
                { "character",                   new[] { _char                         } },
                { "char",                        new[] { _char                         } },
                { "char(1)",                     new RelationalTypeMapping[] { _singleChar, _stringAsSingleChar } },
                { "character(1)",                new RelationalTypeMapping[] { _singleChar, _stringAsSingleChar } },
                { "date",                        new[] { _date                         } },
                { "timestamp without time zone", new[] { _timestamp                    } },
                { "timestamp",                   new[] { _timestamp                    } },
                { "timestamp with time zone",    new[] { _timestamptz, _timestamptzDto } },
                { "timestamptz",                 new[] { _timestamptz, _timestamptzDto } },
                { "interval",                    new[] { _interval                     } },
                { "time without time zone",      new[] { _time                         } },
                { "time",                        new[] { _time                         } },
                { "time with time zone",         new[] { _timetz                       } },
                { "timetz",                      new[] { _timetz                       } },
                { "macaddr",                     new[] { _macaddr                      } },
                { "macaddr8",                    new[] { _macaddr8                     } },
                { "inet",                        new[] { _inet                         } },
                { "cidr",                        new[] { _cidr                         } },
                { "bit",                         new[] { _bit                          } },
                { "bit varying",                 new[] { _varbit                       } },
                { "varbit",                      new[] { _varbit                       } },
                { "hstore",                      new[] { _hstore                       } },
                { "point",                       new[] { _point                        } },
                { "box",                         new[] { _box                          } },
                { "line",                        new[] { _line                         } },
                { "lseg",                        new[] { _lseg                         } },
                { "path",                        new[] { _path                         } },
                { "polygon",                     new[] { _polygon                      } },
                { "circle",                      new[] { _circle                       } },
                { "xid",                         new[] { _xid                          } },
                { "oid",                         new[] { _oid                          } },
                { "cid",                         new[] { _cid                          } },
                { "regtype",                     new[] { _regtype                      } },
                { "lo",                          new[] { _lo                           } },
                { "tid",                         new[] { _tid                          } },

                { "int4range",                   new[] { _int4range                    } },
                { "int8range",                   new[] { _int8range                    } },
                { "numrange",                    new[] { _numrange                     } },
                { "tsrange",                     new[] { _tsrange                      } },
                { "tstzrange",                   new[] { _tstzrange                    } },
                { "daterange",                   new[] { _daterange                    } },

                { "tsquery",                     new[] { _tsquery                      } },
                { "tsvector",                    new[] { _tsvector                     } },
                { "regconfig",                   new[] { _regconfig                    } }
            };

            var clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(bool),                         _bool                 },
                { typeof(byte[]),                       _bytea                },
                { typeof(float),                        _float4               },
                { typeof(double),                       _float8               },
                { typeof(decimal),                      _numeric              },
                { typeof(Guid),                         _uuid                 },
                { typeof(byte),                         _int2Byte             },
                { typeof(short),                        _int2                 },
                { typeof(int),                          _int4                 },
                { typeof(long),                         _int8                 },
                { typeof(string),                       _text                 },
                { typeof(char),                         _singleChar           },
                { typeof(DateTime),                     _timestamp            },
                { typeof(TimeSpan),                     _interval             },
                { typeof(DateTimeOffset),               _timestamptzDto       },
                { typeof(PhysicalAddress),              _macaddr              },
                { typeof(IPAddress),                    _inet                 },
                { typeof((IPAddress, int)),             _cidr                 },
                { typeof(BitArray),                     _varbit               },
                { typeof(Dictionary<string, string>),   _hstore               },
                { typeof(NpgsqlTid),                    _tid                  },
                { typeof(NpgsqlPoint),                  _point                },
                { typeof(NpgsqlBox),                    _box                  },
                { typeof(NpgsqlLine),                   _line                 },
                { typeof(NpgsqlLSeg),                   _lseg                 },
                { typeof(NpgsqlPath),                   _path                 },
                { typeof(NpgsqlPolygon),                _polygon              },
                { typeof(NpgsqlCircle),                 _circle               },

                { typeof(NpgsqlRange<int>),             _int4range            },
                { typeof(NpgsqlRange<long>),            _int8range            },
                { typeof(NpgsqlRange<decimal>),         _numrange             },
                { typeof(NpgsqlRange<DateTime>),        _tsrange              },

                { typeof(NpgsqlTsQuery),                _tsquery              },
                { typeof(NpgsqlTsVector),               _tsvector             },
                { typeof(NpgsqlTsRankingNormalization), _rankingNormalization }
            };

            StoreTypeMappings = new ConcurrentDictionary<string, RelationalTypeMapping[]>(storeTypeMappings, StringComparer.OrdinalIgnoreCase);
            ClrTypeMappings = new ConcurrentDictionary<Type, RelationalTypeMapping>(clrTypeMappings);

            if (npgsqlOptions != null)
            {
                foreach (var (elementClrType, rangeName, subtypeName) in npgsqlOptions.RangeMappings)
                {
                    RelationalTypeMapping subtypeMapping;
                    if (subtypeName == null)
                    {
                        if (!ClrTypeMappings.TryGetValue(elementClrType, out subtypeMapping))
                            throw new Exception($"Could not map range {rangeName}, no mapping was found for element type {elementClrType}");
                    }
                    else
                    {
                        if (StoreTypeMappings.TryGetValue(subtypeName, out var subtypeMappings))
                            throw new Exception($"Could not map range {rangeName}, no mapping was found for subtype {subtypeName}");
                        subtypeMapping = subtypeMappings[0];
                    }

                    var rangeClrType = typeof(NpgsqlRange<>).MakeGenericType(elementClrType);
                    var rangeMapping = new NpgsqlRangeTypeMapping(rangeName, rangeClrType, subtypeMapping);
                    StoreTypeMappings[rangeName] = new RelationalTypeMapping[] { rangeMapping };
                    ClrTypeMappings[rangeClrType] = rangeMapping;
                }

                foreach (var plugin in npgsqlOptions.Plugins)
                    plugin.AddMappings(this);
            }

            LoadUserDefinedTypeMappings();
        }

        /// <summary>
        /// To be used in case user-defined mappings are added late, after this TypeMappingSource has already been initialized.
        /// This is basically only for test usage.
        /// </summary>
        public void LoadUserDefinedTypeMappings()
        {
            SetupEnumMappings();
        }

        /// <summary>
        /// Gets all global enum mappings from the ADO.NET layer and creates mappings for them
        /// </summary>
        void SetupEnumMappings()
        {
            foreach (var adoMapping in NpgsqlConnection.GlobalTypeMapper.Mappings.Where(m => m.TypeHandlerFactory is IEnumTypeHandlerFactory))
            {
                var storeType = adoMapping.PgTypeName;
                var clrType = adoMapping.ClrTypes.SingleOrDefault();
                if (clrType == null)
                {
                    // TODO: Log skipping the enum
                    continue;
                }

                var nameTranslator = ((IEnumTypeHandlerFactory)adoMapping.TypeHandlerFactory).NameTranslator;

                var mapping = new NpgsqlEnumTypeMapping(storeType, clrType, nameTranslator);
                ClrTypeMappings[clrType] = mapping;
                StoreTypeMappings[storeType] = new[] { mapping };
                _userDefinedTypes.Add(storeType);
            }
        }

        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var baseMapping = FindBaseTypeMapping(mappingInfo);
            if (baseMapping != null)
                return baseMapping;

            // We couldn't find a base (simple) type mapping. Try to find an array.
            var arrayMapping = FindArrayMapping(mappingInfo);
            return arrayMapping ?? null;
        }

        protected virtual RelationalTypeMapping FindBaseTypeMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

            if (storeTypeName != null)
            {
                if (StoreTypeMappings.TryGetValue(storeTypeName, out var mappings))
                {
                    if (clrType == null)
                        return mappings[0];

                    foreach (var m in mappings)
                        if (m.ClrType == clrType)
                            return m;

                    return null;
                }

                if (StoreTypeMappings.TryGetValue(storeTypeNameBase, out mappings))
                {
                    if (clrType == null)
                        return mappings[0].Clone(in mappingInfo);

                    foreach (var m in mappings)
                        if (m.ClrType == clrType)
                            return m.Clone(in mappingInfo);

                    return null;
                }
            }

            if (clrType == null)
                return null;

            if (!ClrTypeMappings.TryGetValue(clrType, out var mapping))
                return null;

            // If needed, clone the mapping with the configured length/precision/scale
            // TODO: Cache size/precision/scale mappings?
            if (mappingInfo.Size.HasValue)
            {
                if (clrType == typeof(string))
                {
                    mapping = mappingInfo.IsFixedLength ?? false ? _char : _varchar;

                    // See #342 for when size > 10485760
                    return mappingInfo.Size <= 10485760
                        ? mapping.Clone($"{mapping.StoreType}({mappingInfo.Size})", mappingInfo.Size)
                        : _text;
                }

                if (clrType == typeof(BitArray))
                {
                    mapping = mappingInfo.IsFixedLength ?? false ? (RelationalTypeMapping)_bit : _varbit;
                    return mapping.Clone($"{mapping.StoreType}({mappingInfo.Size})", mappingInfo.Size);
                }
            }
            else if (mappingInfo.Precision.HasValue)
            {
                if (clrType == typeof(decimal))
                {
                    return _numeric.Clone(mappingInfo.Scale.HasValue
                        ? $"numeric({mappingInfo.Precision.Value},{mappingInfo.Scale.Value})"
                        : $"numeric({mappingInfo.Precision.Value})",
                        null);
                }

                if (clrType == typeof(DateTime) ||
                    clrType == typeof(DateTimeOffset) ||
                    clrType == typeof(TimeSpan))
                {
                    return mapping.Clone($"{mapping.StoreType}({mappingInfo.Precision.Value})", null);
                }
            }

            return mapping;
        }

        RelationalTypeMapping FindArrayMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            // PostgreSQL array type names are the element plus []
            var storeType = mappingInfo.StoreTypeName;
            if (storeType != null)
            {
                if (!storeType.EndsWith("[]"))
                    return null;

                // Note that we scaffold PostgreSQL arrays to C# arrays, not lists (which are also supported)

                // TODO: In theory support the multiple mappings just like we do with scalars above
                // (e.g. DateTimeOffset[] vs. DateTime[]
                var elementMapping = FindMapping(storeType.Substring(0, storeType.Length - 2));
                if (elementMapping != null)
                    return StoreTypeMappings.GetOrAdd(storeType,
                        new RelationalTypeMapping[] { new NpgsqlArrayTypeMapping(storeType, elementMapping) })[0];
            }

            var clrType = mappingInfo.ClrType;
            if (clrType == null)
                return null;

            if (clrType.IsArray)
            {
                var elementType = clrType.GetElementType();
                Debug.Assert(elementType != null, "Detected array type but element type is null");

                // If an element isn't supported, neither is its array
                var elementMapping = (RelationalTypeMapping)FindMapping(elementType);
                if (elementMapping == null)
                    return null;

                // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
                if (elementMapping is NpgsqlArrayTypeMapping)
                    return null;

                return ClrTypeMappings.GetOrAdd(clrType, new NpgsqlArrayTypeMapping(elementMapping, clrType));
            }

            if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = clrType.GetGenericArguments()[0];

                // If an element isn't supported, neither is its array
                var elementMapping = (RelationalTypeMapping)FindMapping(elementType);
                if (elementMapping == null)
                    return null;

                // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
                if (elementMapping is NpgsqlArrayTypeMapping)
                    return null;

                return ClrTypeMappings.GetOrAdd(clrType, new NpgsqlListTypeMapping(elementMapping, clrType));
            }

            return null;
        }

        public bool IsUserDefinedType(string storeType) => _userDefinedTypes.Contains(storeType);
    }
}
