using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using Npgsql.TypeHandlers;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlTypeMappingSource : RelationalTypeMappingSource
    {
        [NotNull] readonly ISqlGenerationHelper _sqlGenerationHelper;

        protected virtual ConcurrentDictionary<string, RelationalTypeMapping[]> StoreTypeMappings { get; }
        protected virtual ConcurrentDictionary<Type, RelationalTypeMapping> ClrTypeMappings { get; }

        readonly IReadOnlyList<UserRangeDefinition> _userRangeDefinitions;

        #region Mappings

        // Numeric types
        readonly NpgsqlFloatTypeMapping        _float4             = new NpgsqlFloatTypeMapping();
        readonly NpgsqlDoubleTypeMapping       _float8             = new NpgsqlDoubleTypeMapping();
        readonly DecimalTypeMapping            _numeric            = new DecimalTypeMapping("numeric", DbType.Decimal);
        readonly NpgsqlMoneyTypeMapping        _money              = new NpgsqlMoneyTypeMapping();
        readonly GuidTypeMapping               _uuid               = new GuidTypeMapping("uuid", DbType.Guid);
        readonly ShortTypeMapping              _int2               = new ShortTypeMapping("smallint", DbType.Int16);
        readonly ByteTypeMapping               _int2Byte           = new ByteTypeMapping("smallint", DbType.Byte);
        readonly IntTypeMapping                _int4               = new IntTypeMapping("integer", DbType.Int32);
        readonly LongTypeMapping               _int8               = new LongTypeMapping("bigint", DbType.Int64);

        // Character types
        readonly StringTypeMapping             _text               = new StringTypeMapping("text", DbType.String);
        readonly StringTypeMapping             _varchar            = new StringTypeMapping("character varying", DbType.String);
        readonly NpgsqlCharacterTypeMapping    _char               = new NpgsqlCharacterTypeMapping("character");
        readonly CharTypeMapping               _singleChar         = new CharTypeMapping("character(1)", DbType.String);
        readonly NpgsqlCharacterTypeMapping    _stringAsSingleChar = new NpgsqlCharacterTypeMapping("character(1)");
        readonly NpgsqlStringTypeMapping       _xml                = new NpgsqlStringTypeMapping("xml", NpgsqlDbType.Xml);
        readonly NpgsqlStringTypeMapping       _citext             = new NpgsqlStringTypeMapping("citext", NpgsqlDbType.Citext);

        // JSON mappings
        readonly NpgsqlJsonTypeMapping         _jsonbString        = new NpgsqlJsonTypeMapping("jsonb", typeof(string));
        readonly NpgsqlJsonTypeMapping         _jsonString         = new NpgsqlJsonTypeMapping("json", typeof(string));
        readonly NpgsqlJsonTypeMapping         _jsonbDocument      = new NpgsqlJsonTypeMapping("jsonb", typeof(JsonDocument));
        readonly NpgsqlJsonTypeMapping         _jsonDocument       = new NpgsqlJsonTypeMapping("json", typeof(JsonDocument));
        readonly NpgsqlJsonTypeMapping         _jsonbElement       = new NpgsqlJsonTypeMapping("jsonb", typeof(JsonElement));
        readonly NpgsqlJsonTypeMapping         _jsonElement        = new NpgsqlJsonTypeMapping("json", typeof(JsonElement));

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

        // Unaccent mapping
        readonly NpgsqlRegdictionaryTypeMapping _regdictionary = new NpgsqlRegdictionaryTypeMapping();

        // Built-in ranges
        readonly NpgsqlRangeTypeMapping        _int4range;
        readonly NpgsqlRangeTypeMapping        _int8range;
        readonly NpgsqlRangeTypeMapping        _numrange;
        readonly NpgsqlRangeTypeMapping        _tsrange;
        readonly NpgsqlRangeTypeMapping        _tstzrange;
        readonly NpgsqlRangeTypeMapping        _daterange;

        // Other types
        readonly NpgsqlBoolTypeMapping            _bool            = new NpgsqlBoolTypeMapping();
        readonly NpgsqlBitTypeMapping             _bit             = new NpgsqlBitTypeMapping();
        readonly NpgsqlVarbitTypeMapping          _varbit          = new NpgsqlVarbitTypeMapping();
        readonly NpgsqlByteArrayTypeMapping       _bytea           = new NpgsqlByteArrayTypeMapping();
        readonly NpgsqlHstoreTypeMapping          _hstore          = new NpgsqlHstoreTypeMapping(typeof(Dictionary<string, string>));
        readonly NpgsqlHstoreTypeMapping          _immutableHstore = new NpgsqlHstoreTypeMapping(typeof(ImmutableDictionary<string, string>));
        readonly NpgsqlTidTypeMapping             _tid             = new NpgsqlTidTypeMapping();

        // Special stuff
        // ReSharper disable once InconsistentNaming
        public readonly StringTypeMapping      EStringTypeMapping  = new NpgsqlEStringTypeMapping();

        #endregion Mappings

        public NpgsqlTypeMappingSource([NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [CanBeNull] INpgsqlOptions npgsqlOptions=null)
            : base(dependencies, relationalDependencies)
        {
            _sqlGenerationHelper = Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));

            // Initialize some mappings which depend on other mappings
            _int4range = new NpgsqlRangeTypeMapping("int4range", typeof(NpgsqlRange<int>),      _int4,        sqlGenerationHelper);
            _int8range = new NpgsqlRangeTypeMapping("int8range", typeof(NpgsqlRange<long>),     _int8,        sqlGenerationHelper);
            _numrange  = new NpgsqlRangeTypeMapping("numrange",  typeof(NpgsqlRange<decimal>),  _numeric,     sqlGenerationHelper);
            _tsrange   = new NpgsqlRangeTypeMapping("tsrange",   typeof(NpgsqlRange<DateTime>), _timestamp,   sqlGenerationHelper);
            _tstzrange = new NpgsqlRangeTypeMapping("tstzrange", typeof(NpgsqlRange<DateTime>), _timestamptz, sqlGenerationHelper);
            _daterange = new NpgsqlRangeTypeMapping("daterange", typeof(NpgsqlRange<DateTime>), _timestamptz, sqlGenerationHelper);

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
                { "jsonb",                       new RelationalTypeMapping[] { _jsonbString, _jsonbDocument, _jsonbElement } },
                { "json",                        new RelationalTypeMapping[] { _jsonString, _jsonDocument, _jsonElement } },
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
                { "hstore",                      new RelationalTypeMapping[] { _hstore, _immutableHstore } },
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
                { "regconfig",                   new[] { _regconfig                    } },

                { "regdictionary",               new[] { _regdictionary                } }
            };

            var clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(bool),                                _bool                 },
                { typeof(byte[]),                              _bytea                },
                { typeof(float),                               _float4               },
                { typeof(double),                              _float8               },
                { typeof(decimal),                             _numeric              },
                { typeof(Guid),                                _uuid                 },
                { typeof(byte),                                _int2Byte             },
                { typeof(short),                               _int2                 },
                { typeof(int),                                 _int4                 },
                { typeof(long),                                _int8                 },
                { typeof(string),                              _text                 },
                { typeof(JsonDocument),                        _jsonbDocument        },
                { typeof(JsonElement),                         _jsonbElement         },
                { typeof(char),                                _singleChar           },
                { typeof(DateTime),                            _timestamp            },
                { typeof(TimeSpan),                            _interval             },
                { typeof(DateTimeOffset),                      _timestamptzDto       },
                { typeof(PhysicalAddress),                     _macaddr              },
                { typeof(IPAddress),                           _inet                 },
                { typeof((IPAddress, int)),                    _cidr                 },
                { typeof(BitArray),                            _varbit               },
                { typeof(ImmutableDictionary<string, string>), _immutableHstore      },
                { typeof(Dictionary<string, string>),          _hstore               },
                { typeof(NpgsqlTid),                           _tid                  },

                { typeof(NpgsqlPoint),                         _point                },
                { typeof(NpgsqlBox),                           _box                  },
                { typeof(NpgsqlLine),                          _line                 },
                { typeof(NpgsqlLSeg),                          _lseg                 },
                { typeof(NpgsqlPath),                          _path                 },
                { typeof(NpgsqlPolygon),                       _polygon              },
                { typeof(NpgsqlCircle),                        _circle               },

                { typeof(NpgsqlRange<int>),                    _int4range            },
                { typeof(NpgsqlRange<long>),                   _int8range            },
                { typeof(NpgsqlRange<decimal>),                _numrange             },
                { typeof(NpgsqlRange<DateTime>),               _tsrange              },

                { typeof(NpgsqlTsQuery),                       _tsquery              },
                { typeof(NpgsqlTsVector),                      _tsvector             },
                { typeof(NpgsqlTsRankingNormalization),        _rankingNormalization }
            };

            StoreTypeMappings = new ConcurrentDictionary<string, RelationalTypeMapping[]>(storeTypeMappings, StringComparer.OrdinalIgnoreCase);
            ClrTypeMappings = new ConcurrentDictionary<Type, RelationalTypeMapping>(clrTypeMappings);

            LoadUserDefinedTypeMappings(sqlGenerationHelper);

            _userRangeDefinitions = npgsqlOptions?.UserRangeDefinitions ?? new UserRangeDefinition[0];
        }

        /// <summary>
        /// To be used in case user-defined mappings are added late, after this TypeMappingSource has already been initialized.
        /// This is basically only for test usage.
        /// </summary>
        public virtual void LoadUserDefinedTypeMappings([NotNull] ISqlGenerationHelper sqlGenerationHelper)
            => SetupEnumMappings(sqlGenerationHelper);

        /// <summary>
        /// Gets all global enum mappings from the ADO.NET layer and creates mappings for them
        /// </summary>
        protected virtual void SetupEnumMappings([NotNull] ISqlGenerationHelper sqlGenerationHelper)
        {
            foreach (var adoMapping in NpgsqlConnection.GlobalTypeMapper.Mappings.Where(m => m.TypeHandlerFactory is IEnumTypeHandlerFactory).ToArray())
            {
                var storeType = adoMapping.PgTypeName;
                var clrType = adoMapping.ClrTypes.SingleOrDefault();
                if (clrType == null)
                {
                    // TODO: Log skipping the enum
                    continue;
                }

                var nameTranslator = ((IEnumTypeHandlerFactory)adoMapping.TypeHandlerFactory).NameTranslator;

                // TODO: update with schema per https://github.com/npgsql/npgsql/issues/2121
                var components = storeType.Split('.');
                var schema = components.Length > 1 ? components.First() : null;
                var name = components.Length > 1 ? string.Join(null, components.Skip(1)) : storeType;

                var mapping = new NpgsqlEnumTypeMapping(name, schema, clrType, sqlGenerationHelper, nameTranslator);
                ClrTypeMappings[clrType] = mapping;
                StoreTypeMappings[mapping.StoreType] = new RelationalTypeMapping[] { mapping };
            }
        }

        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo) =>
            // First, try any plugins, allowing them to override built-in mappings (e.g. NodaTime)
            base.FindMapping(mappingInfo) ??
            FindBaseMapping(mappingInfo)?.Clone(mappingInfo) ??
            FindArrayMapping(mappingInfo) ??
            FindUserRangeMapping(mappingInfo);

        protected virtual RelationalTypeMapping FindBaseMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

            if (storeTypeName != null)
            {
                if (StoreTypeMappings.TryGetValue(storeTypeName, out var mappings))
                {
                    // We found the user-specified store type. No CLR type was provided - we're probably
                    // scaffolding from an existing database, take the first mapping as the default.
                    if (clrType == null)
                        return mappings[0];

                    // A CLR type was provided - look for a mapping between the store and CLR types. If not found, fail
                    // immediately.
                    foreach (var m in mappings)
                        if (m.ClrType == clrType)
                            return m;

                    // Map arbitrary user POCOs to JSON
                    if (storeTypeName == "jsonb" || storeTypeName == "json")
                        return new NpgsqlJsonTypeMapping(storeTypeName, clrType);

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

                // A store type name was provided, but is unknown. This could be a domain (alias) type, in which case
                // we proceed with a CLR type lookup (if the type doesn't exist at all the failure will come later).
            }

            if (clrType == null ||
                !ClrTypeMappings.TryGetValue(clrType, out var mapping) ||
                // Special case for byte[] mapped as smallint[] - don't return bytea mapping
                storeTypeName != null && storeTypeName == "smallint[]")
            {
                return null;
            }

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

        protected virtual RelationalTypeMapping FindArrayMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            Type elementClrType = null;

            if (clrType != null && !clrType.TryGetElementType(out elementClrType))
                return null; // Not an array/list

            var storeType = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;
            if (storeType != null)
            {
                // PostgreSQL array type names are the element plus []
                if (!storeType.EndsWith("[]"))
                    return null;

                var elementStoreType = storeType.Substring(0, storeType.Length - 2);
                var elementStoreTypeNameBase = storeTypeNameBase.Substring(0, storeTypeNameBase.Length - 2);

                RelationalTypeMapping elementMapping;

                if (elementClrType == null)
                {
                    elementMapping = FindMapping(new RelationalTypeMappingInfo(
                        elementStoreType, elementStoreTypeNameBase,
                        mappingInfo.IsUnicode, mappingInfo.Size, mappingInfo.Precision, mappingInfo.Scale));
                }
                else
                {
                    elementMapping = FindMapping(new RelationalTypeMappingInfo(
                        elementClrType, elementStoreType, elementStoreTypeNameBase,
                        mappingInfo.IsKeyOrIndex, mappingInfo.IsUnicode, mappingInfo.Size, mappingInfo.IsRowVersion,
                        mappingInfo.IsFixedLength, mappingInfo.Precision, mappingInfo.Scale));

                    // If an element mapping was found only with the help of a value converter, return null and EF will
                    // construct the corresponding array mapping with a value converter.
                    if (elementMapping?.Converter != null)
                        return null;
                }

                // If no mapping was found for the element, there's no mapping for the array.
                // Also, arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
                if (elementMapping == null || elementMapping is NpgsqlArrayTypeMapping)
                    return null;

                return new NpgsqlArrayArrayTypeMapping(storeType, elementMapping);
            }

            if (clrType == null)
                return null;

            if (clrType.IsArray)
            {
                var elementType = clrType.GetElementType();
                Debug.Assert(elementType != null, "Detected array type but element type is null");

                // If an element isn't supported, neither is its array. If the element is only supported via value
                // conversion we also don't support it.
                var elementMapping = (RelationalTypeMapping)FindMapping(elementType);
                if (elementMapping == null || elementMapping.Converter != null)
                    return null;

                // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
                if (elementMapping is NpgsqlArrayTypeMapping)
                    return null;

                // Not that the element mapping found above was stripped of nullability
                // (so we get a mapping for int, not int?).
                Debug.Assert(
                    Nullable.GetUnderlyingType(elementType) is null ||
                    Nullable.GetUnderlyingType(elementType) == elementMapping.ClrType);

                return new NpgsqlArrayArrayTypeMapping(elementMapping, clrType);
            }

            if (clrType.IsGenericList())
            {
                var elementType = clrType.GetGenericArguments()[0];

                // If an element isn't supported, neither is its array
                var elementMapping = (RelationalTypeMapping)FindMapping(elementType);
                if (elementMapping == null)
                    return null;

                // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
                if (elementMapping is NpgsqlArrayTypeMapping)
                    return null;

                return new NpgsqlArrayListTypeMapping(elementMapping, clrType);
            }

            return null;
        }

        protected virtual RelationalTypeMapping FindUserRangeMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            UserRangeDefinition rangeDefinition = null;
            var rangeStoreType = mappingInfo.StoreTypeName;
            var rangeClrType = mappingInfo.ClrType;

            // If the incoming MappingInfo contains a ClrType, make sure it's an NpgsqlRange<T>, otherwise bail
            if (rangeClrType != null &&
                (!rangeClrType.IsGenericType || rangeClrType.GetGenericTypeDefinition() != typeof(NpgsqlRange<>)))
            {
                return null;
            }

            // Try to find a user range definition (defined by the user on their context options), based on the
            // incoming MappingInfo's StoreType or ClrType
            if (rangeStoreType != null)
            {
                rangeDefinition = _userRangeDefinitions.SingleOrDefault(m => m.RangeName == rangeStoreType);

                if (rangeDefinition == null)
                    return null;

                if (rangeClrType == null)
                {
                    // The incoming MappingInfo does not contain a ClrType, only a StoreType (i.e. scaffolding).
                    // Construct the range ClrType from the range definition's subtype ClrType
                    rangeClrType = typeof(NpgsqlRange<>).MakeGenericType(rangeDefinition.SubtypeClrType);
                }
                else if (rangeClrType != typeof(NpgsqlRange<>).MakeGenericType(rangeDefinition.SubtypeClrType))
                {
                    // If the incoming MappingInfo also contains a ClrType (in addition to the StoreType), make sure it
                    // corresponds to the subtype ClrType on the range definition
                    return null;
                }
            }
            else if (rangeClrType != null)
                rangeDefinition = _userRangeDefinitions.SingleOrDefault(m => m.SubtypeClrType == rangeClrType.GetGenericArguments()[0]);

            if (rangeDefinition == null)
                return null;

            // We now have a user-defined range definition from the context options. Use it to get the subtype's
            // mapping
            var subtypeMapping = (RelationalTypeMapping)(rangeDefinition.SubtypeName == null
                ? FindMapping(rangeDefinition.SubtypeClrType)
                : FindMapping(rangeDefinition.SubtypeName));

            if (subtypeMapping == null)
                throw new Exception($"Could not map range {rangeDefinition.RangeName}, no mapping was found its subtype");

            return new NpgsqlRangeTypeMapping(rangeDefinition.RangeName, rangeDefinition.SchemaName, rangeClrType, subtypeMapping, _sqlGenerationHelper);
        }

        static bool NameBasesUsesPrecision(ReadOnlySpan<char> span)
            => span.ToString() switch
            {
                "decimal"     => true,
                "dec"         => true,
                "numeric"     => true,
                "timestamp"   => true,
                "timestamptz" => true,
                "time"        => true,
                "interval"    => true,
                _             => false
            };

        // We override to support parsing array store names (e.g. varchar(32)[]), timestamp(5) with time zone, etc.
        protected override string ParseStoreTypeName(
            string storeTypeName,
            out bool? unicode,
            out int? size,
            out int? precision,
            out int? scale)
        {
            (unicode, size, precision, scale) = (null, null, null, null);

            if (storeTypeName is null)
                return null;

            var span = storeTypeName.AsSpan().Trim();

            var openParen = span.IndexOf("(", StringComparison.Ordinal);
            if (openParen == -1)
                return storeTypeName;
            var afterOpenParen = span.Slice(openParen + 1).TrimStart();
            var closeParen = afterOpenParen.IndexOf(")", StringComparison.Ordinal);
            if (closeParen == -1)
                return storeTypeName;

            var preParens = span[..openParen].Trim();
            var inParens = afterOpenParen[..closeParen].Trim();
            // There may be stuff after the closing parentheses (e.g. varchar(32)[], timestamp(3) with time zone)
            var postParens = afterOpenParen.Slice(closeParen + 1);

            var comma = inParens.IndexOf(",", StringComparison.Ordinal);
            if (comma != -1)
            {
                if (int.TryParse(inParens[..comma].Trim(), out var parsedPrecision))
                    precision = parsedPrecision;
                if (int.TryParse(inParens.Slice(comma + 1), out var parsedScale))
                    scale = parsedScale;
            }
            else if (int.TryParse(inParens, out var parsedSize))
            {
                if (NameBasesUsesPrecision(preParens))
                {
                    precision = parsedSize;
                    scale = 0;
                }
                else
                    size = parsedSize;
            }
            else
                return storeTypeName;

            if (postParens.Length > 0)
            {
                return new StringBuilder()
                    .Append(preParens)
                    .Append(postParens)
                    .ToString();
            }

            return preParens.ToString();
        }
    }
}
