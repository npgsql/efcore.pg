using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Data;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Text.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using Npgsql.Internal.TypeMapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlTypeMappingSource : RelationalTypeMappingSource
{
#if DEBUG
    internal static bool LegacyTimestampBehavior;
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    internal static bool DisableDateTimeInfinityConversions;
#else
        internal static readonly bool LegacyTimestampBehavior;
        internal static readonly bool DisableDateTimeInfinityConversions;
#endif

    static NpgsqlTypeMappingSource()
    {
        LegacyTimestampBehavior = AppContext.TryGetSwitch("Npgsql.EnableLegacyTimestampBehavior", out var enabled) && enabled;
        DisableDateTimeInfinityConversions = AppContext.TryGetSwitch("Npgsql.DisableDateTimeInfinityConversions", out enabled) && enabled;
    }

    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private readonly NullabilityInfoContext _nullabilityInfoContext = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual ConcurrentDictionary<string, RelationalTypeMapping[]> StoreTypeMappings { get; }
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual ConcurrentDictionary<Type, RelationalTypeMapping> ClrTypeMappings { get; }

    private readonly IReadOnlyList<UserRangeDefinition> _userRangeDefinitions;

    /// <summary>
    /// Maps range subtypes to a list of type mappings for those ranges.
    /// </summary>
    private readonly Dictionary<Type, List<NpgsqlRangeTypeMapping>> _rangeTypeMappings;

    /// <summary>
    /// Maps multirange subtypes to a list of type mappings for those multiranges.
    /// </summary>
    private readonly Dictionary<Type, List<NpgsqlMultirangeTypeMapping>> _multirangeTypeMappings;

    private static MethodInfo? _adoUserTypeMappingsGetMethodInfo;

    private readonly bool _supportsMultiranges;

    #region Mappings

    // Numeric types
    private readonly NpgsqlFloatTypeMapping        _float4             = new();
    private readonly NpgsqlDoubleTypeMapping       _float8             = new();
    private readonly NpgsqlDecimalTypeMapping      _numeric            = new();
    private readonly NpgsqlBigIntegerTypeMapping   _bigInteger         = new();
    private readonly NpgsqlMoneyTypeMapping        _money              = new();
    private readonly GuidTypeMapping               _uuid               = new("uuid", DbType.Guid);
    private readonly ShortTypeMapping              _int2               = new("smallint", DbType.Int16);
    private readonly ByteTypeMapping               _int2Byte           = new("smallint", DbType.Byte);
    private readonly IntTypeMapping                _int4               = new("integer", DbType.Int32);
    private readonly LongTypeMapping               _int8               = new("bigint", DbType.Int64);

    // Character types
    private readonly StringTypeMapping                _text               = new("text", DbType.String);
    private readonly NpgsqlStringTypeMapping          _varchar            = new("character varying", NpgsqlDbType.Varchar);
    private readonly NpgsqlCharacterStringTypeMapping _char               = new("character");
    private readonly NpgsqlCharacterCharTypeMapping   _singleChar         = new("character(1)");
    private readonly NpgsqlStringTypeMapping          _xml                = new("xml", NpgsqlDbType.Xml);
    private readonly NpgsqlStringTypeMapping          _citext             = new("citext", NpgsqlDbType.Citext);

    // JSON mappings
    private readonly NpgsqlJsonTypeMapping         _jsonbString        = new("jsonb", typeof(string));
    private readonly NpgsqlJsonTypeMapping         _jsonString         = new("json", typeof(string));
    private readonly NpgsqlJsonTypeMapping         _jsonbDocument      = new("jsonb", typeof(JsonDocument));
    private readonly NpgsqlJsonTypeMapping         _jsonDocument       = new("json", typeof(JsonDocument));
    private readonly NpgsqlJsonTypeMapping         _jsonbElement       = new("jsonb", typeof(JsonElement));
    private readonly NpgsqlJsonTypeMapping         _jsonElement        = new("json", typeof(JsonElement));

    // Date/Time types
    private readonly NpgsqlDateTypeMapping         _dateDateTime       = new(typeof(DateTime));
    private readonly NpgsqlTimestampTypeMapping    _timestamp          = new();
    private readonly NpgsqlTimestampTzTypeMapping  _timestamptz        = new(typeof(DateTime));
    private readonly NpgsqlTimestampTzTypeMapping  _timestamptzDto     = new(typeof(DateTimeOffset));
    private readonly NpgsqlIntervalTypeMapping     _interval           = new();
    private readonly NpgsqlTimeTypeMapping         _timeTimeSpan       = new(typeof(TimeSpan));
    private readonly NpgsqlTimeTzTypeMapping       _timetz             = new();

    private readonly NpgsqlDateTypeMapping         _dateDateOnly       = new(typeof(DateOnly));
    private readonly NpgsqlTimeTypeMapping         _timeTimeOnly       = new(typeof(TimeOnly));

    // Network address types
    private readonly NpgsqlMacaddrTypeMapping      _macaddr            = new();
    private readonly NpgsqlMacaddr8TypeMapping     _macaddr8           = new();
    private readonly NpgsqlInetTypeMapping         _inet               = new();
    private readonly NpgsqlCidrTypeMapping         _cidr               = new();

    // Built-in geometric types
    private readonly NpgsqlPointTypeMapping        _point              = new();
    private readonly NpgsqlBoxTypeMapping          _box                = new();
    private readonly NpgsqlLineTypeMapping         _line               = new();
    private readonly NpgsqlLineSegmentTypeMapping  _lseg               = new();
    private readonly NpgsqlPathTypeMapping         _path               = new();
    private readonly NpgsqlPolygonTypeMapping      _polygon            = new();
    private readonly NpgsqlCircleTypeMapping       _circle             = new();

    // uint mappings
    private readonly NpgsqlUintTypeMapping         _xid                = new("xid", NpgsqlDbType.Xid);
    private readonly NpgsqlUintTypeMapping         _oid                = new("oid", NpgsqlDbType.Oid);
    private readonly NpgsqlUintTypeMapping         _cid                = new("cid", NpgsqlDbType.Cid);
    private readonly NpgsqlUintTypeMapping         _regtype            = new("regtype", NpgsqlDbType.Regtype);
    private readonly NpgsqlUintTypeMapping         _lo                 = new("lo", NpgsqlDbType.Oid);

    // Full text search mappings
    private readonly NpgsqlTsQueryTypeMapping   _tsquery               = new();
    private readonly NpgsqlTsVectorTypeMapping  _tsvector              = new();
    private readonly NpgsqlRegconfigTypeMapping _regconfig             = new();
    private readonly NpgsqlTsRankingNormalizationTypeMapping _rankingNormalization = new();

    // Unaccent mapping
    private readonly NpgsqlRegdictionaryTypeMapping _regdictionary = new();

    // Built-in ranges
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    private readonly NpgsqlRangeTypeMapping        _int4range;
    private readonly NpgsqlRangeTypeMapping        _int8range;
    private readonly NpgsqlRangeTypeMapping        _numrange;
    private readonly NpgsqlRangeTypeMapping        _tsrange;
    private readonly NpgsqlRangeTypeMapping        _tstzrange;
    private readonly NpgsqlRangeTypeMapping        _dateOnlyDaterange;
    private readonly NpgsqlRangeTypeMapping        _dateTimeDaterange;

    // Built-in multiranges
    private readonly NpgsqlMultirangeTypeMapping _int4multirangeArray;
    private readonly NpgsqlMultirangeTypeMapping _int8multirangeArray;
    private readonly NpgsqlMultirangeTypeMapping _nummultirangeArray;
    private readonly NpgsqlMultirangeTypeMapping _tsmultirangeArray;
    private readonly NpgsqlMultirangeTypeMapping _tstzmultirangeArray;
    private readonly NpgsqlMultirangeTypeMapping _dateTimeDatemultirangeArray;
    private readonly NpgsqlMultirangeTypeMapping _dateOnlyDatemultirangeArray;

    private readonly NpgsqlMultirangeTypeMapping _int4multirangeList;
    private readonly NpgsqlMultirangeTypeMapping _int8multirangeList;
    private readonly NpgsqlMultirangeTypeMapping _nummultirangeList;
    private readonly NpgsqlMultirangeTypeMapping _tsmultirangeList;
    private readonly NpgsqlMultirangeTypeMapping _tstzmultirangeList;
    private readonly NpgsqlMultirangeTypeMapping _dateTimeMultirangeList;
    private readonly NpgsqlMultirangeTypeMapping _dateOnlyDatemultirangeList;
    // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

    // Other types
    private readonly NpgsqlBoolTypeMapping            _bool            = new();
    private readonly NpgsqlBitTypeMapping             _bit             = new();
    private readonly NpgsqlVarbitTypeMapping          _varbit          = new();
    private readonly NpgsqlByteArrayTypeMapping       _bytea           = new();
    private readonly NpgsqlHstoreTypeMapping          _hstore          = new(typeof(Dictionary<string, string>));
    private readonly NpgsqlHstoreTypeMapping          _immutableHstore = new(typeof(ImmutableDictionary<string, string>));
    private readonly NpgsqlTidTypeMapping             _tid             = new();
    private readonly NpgsqlPgLsnTypeMapping           _pgLsn           = new();

    private readonly NpgsqlLTreeTypeMapping           _ltree           = new();
    private readonly NpgsqlStringTypeMapping          _ltreeString     = new("ltree", NpgsqlDbType.LTree);
    private readonly NpgsqlStringTypeMapping          _lquery          = new("lquery", NpgsqlDbType.LQuery);
    private readonly NpgsqlStringTypeMapping          _ltxtquery       = new("ltxtquery", NpgsqlDbType.LTxtQuery);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // Special stuff
    // ReSharper disable once InconsistentNaming
    public readonly StringTypeMapping EStringTypeMapping  = new NpgsqlEStringTypeMapping();

    #endregion Mappings

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlTypeMappingSource(TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies,
        ISqlGenerationHelper sqlGenerationHelper,
        INpgsqlSingletonOptions npgsqlSingletonOptions)
        : base(dependencies, relationalDependencies)
    {
        _sqlGenerationHelper = Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
        _supportsMultiranges = npgsqlSingletonOptions.PostgresVersionWithoutDefault is null
            || npgsqlSingletonOptions.PostgresVersionWithoutDefault.AtLeast(14);

        // Initialize some mappings which depend on other mappings
        _int4range         = new NpgsqlRangeTypeMapping("int4range", typeof(NpgsqlRange<int>),      _int4,         sqlGenerationHelper);
        _int8range         = new NpgsqlRangeTypeMapping("int8range", typeof(NpgsqlRange<long>),     _int8,         sqlGenerationHelper);
        _numrange          = new NpgsqlRangeTypeMapping("numrange",  typeof(NpgsqlRange<decimal>),  _numeric,      sqlGenerationHelper);
        _tsrange           = new NpgsqlRangeTypeMapping("tsrange",   typeof(NpgsqlRange<DateTime>), _timestamp,    sqlGenerationHelper);
        _tstzrange         = new NpgsqlRangeTypeMapping("tstzrange", typeof(NpgsqlRange<DateTime>), _timestamptz,  sqlGenerationHelper);
        _dateOnlyDaterange = new NpgsqlRangeTypeMapping("daterange", typeof(NpgsqlRange<DateOnly>), _dateDateOnly, sqlGenerationHelper);
        _dateTimeDaterange = new NpgsqlRangeTypeMapping("daterange", typeof(NpgsqlRange<DateTime>), _dateDateTime, sqlGenerationHelper);

        _rangeTypeMappings = new()
        {
            { typeof(int), new() { _int4range } },
            { typeof(long), new() { _int8range } },
            { typeof(decimal), new() { _numrange } },
            { typeof(DateOnly), new() { _dateOnlyDaterange } },
            { typeof(DateTime), new() { _tsrange, _tstzrange, _dateTimeDaterange } }
        };

        _int4multirangeArray         = new NpgsqlMultirangeTypeMapping("int4multirange", typeof(NpgsqlRange<int>[]),          _int4range,         sqlGenerationHelper);
        _int8multirangeArray         = new NpgsqlMultirangeTypeMapping("int8multirange", typeof(NpgsqlRange<long>[]),         _int8range,         sqlGenerationHelper);
        _nummultirangeArray          = new NpgsqlMultirangeTypeMapping("nummultirange",  typeof(NpgsqlRange<decimal>[]),      _numrange,          sqlGenerationHelper);
        _tsmultirangeArray           = new NpgsqlMultirangeTypeMapping("tsmultirange",   typeof(NpgsqlRange<DateTime>[]),     _tsrange,           sqlGenerationHelper);
        _tstzmultirangeArray         = new NpgsqlMultirangeTypeMapping("tstzmultirange", typeof(NpgsqlRange<DateTime>[]),     _tstzrange,         sqlGenerationHelper);
        _dateOnlyDatemultirangeArray = new NpgsqlMultirangeTypeMapping("datemultirange", typeof(NpgsqlRange<DateOnly>[]),     _dateOnlyDaterange, sqlGenerationHelper);
        _dateTimeDatemultirangeArray = new NpgsqlMultirangeTypeMapping("datemultirange", typeof(NpgsqlRange<DateTime>[]),     _dateTimeDaterange, sqlGenerationHelper);

        _int4multirangeList          = new NpgsqlMultirangeTypeMapping("int4multirange", typeof(List<NpgsqlRange<int>>),      _int4range,         sqlGenerationHelper);
        _int8multirangeList          = new NpgsqlMultirangeTypeMapping("int8multirange", typeof(List<NpgsqlRange<long>>),     _int8range,         sqlGenerationHelper);
        _nummultirangeList           = new NpgsqlMultirangeTypeMapping("nummultirange",  typeof(List<NpgsqlRange<decimal>>),  _numrange,          sqlGenerationHelper);
        _tsmultirangeList            = new NpgsqlMultirangeTypeMapping("tsmultirange",   typeof(List<NpgsqlRange<DateTime>>), _tsrange,           sqlGenerationHelper);
        _tstzmultirangeList          = new NpgsqlMultirangeTypeMapping("tstzmultirange", typeof(List<NpgsqlRange<DateTime>>), _tstzrange,         sqlGenerationHelper);
        _dateOnlyDatemultirangeList  = new NpgsqlMultirangeTypeMapping("datemultirange", typeof(List<NpgsqlRange<DateOnly>>), _dateOnlyDaterange, sqlGenerationHelper);
        _dateTimeMultirangeList      = new NpgsqlMultirangeTypeMapping("datemultirange", typeof(List<NpgsqlRange<DateTime>>), _dateTimeDaterange, sqlGenerationHelper);

        _multirangeTypeMappings = new()
        {
            { typeof(int), new() { _int4multirangeArray, _int4multirangeList } },
            { typeof(long), new() { _int8multirangeArray, _int8multirangeList } },
            { typeof(decimal), new() { _nummultirangeArray, _nummultirangeList } },
            { typeof(DateOnly), new() { _dateOnlyDatemultirangeArray, _dateOnlyDatemultirangeList } },
            {
                typeof(DateTime), new()
                {
                    _tsmultirangeArray, _tsmultirangeList,
                    _tstzmultirangeArray, _tstzmultirangeList,
                    _dateTimeDatemultirangeArray, _dateTimeMultirangeList
                }
            }
        };

// ReSharper disable CoVariantArrayConversion
        // Note that PostgreSQL has aliases to some built-in type name aliases (e.g. int4 for integer),
        // these are mapped as well.
        // https://www.postgresql.org/docs/current/static/datatype.html#DATATYPE-TABLE
        var storeTypeMappings = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "smallint",                    new RelationalTypeMapping[] { _int2, _int2Byte } },
            { "int2",                        new RelationalTypeMapping[] { _int2, _int2Byte } },
            { "integer",                     new[] { _int4                         } },
            { "int",                         new[] { _int4                         } },
            { "int4",                        new[] { _int4                         } },
            { "bigint",                      new[] { _int8                         } },
            { "int8",                        new[] { _int8                         } },
            { "real",                        new[] { _float4                       } },
            { "float4",                      new[] { _float4                       } },
            { "double precision",            new[] { _float8                       } },
            { "float8",                      new[] { _float8                       } },
            { "numeric",                     new RelationalTypeMapping[] { _numeric, _bigInteger         } },
            { "decimal",                     new RelationalTypeMapping[] { _numeric, _bigInteger         } },
            { "money",                       new[] { _money                        } },

            { "text",                        new[] { _text                         } },
            { "jsonb",                       new RelationalTypeMapping[] { _jsonbString, _jsonbDocument, _jsonbElement } },
            { "json",                        new RelationalTypeMapping[] { _jsonString, _jsonDocument, _jsonElement } },
            { "xml",                         new[] { _xml                          } },
            { "citext",                      new[] { _citext                       } },
            { "character varying",           new[] { _varchar                      } },
            { "varchar",                     new[] { _varchar                      } },
            // See FindBaseMapping below for special treatment of 'character'

            { "timestamp without time zone", new[] { _timestamp                    } },
            { "timestamp with time zone",    new[] { _timestamptz, _timestamptzDto } },
            { "interval",                    new[] { _interval                     } },
            { "date",                        new RelationalTypeMapping[] { _dateDateOnly, _dateDateTime } },
            { "time without time zone",      new RelationalTypeMapping[] { _timeTimeOnly, _timeTimeSpan } },
            { "time with time zone",         new[] { _timetz                       } },

            { "boolean",                     new[] { _bool                         } },
            { "bool",                        new[] { _bool                         } },
            { "bytea",                       new[] { _bytea                        } },
            { "uuid",                        new[] { _uuid                         } },
            { "bit",                         new[] { _bit                          } },
            { "bit varying",                 new[] { _varbit                       } },
            { "varbit",                      new[] { _varbit                       } },
            { "hstore",                      new RelationalTypeMapping[] { _hstore, _immutableHstore } },

            { "macaddr",                     new[] { _macaddr                      } },
            { "macaddr8",                    new[] { _macaddr8                     } },
            { "inet",                        new[] { _inet                         } },
            { "cidr",                        new[] { _cidr                         } },

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
            { "pg_lsn",                      new[] { _pgLsn                        } },

            { "int4range",                   new[] { _int4range                    } },
            { "int8range",                   new[] { _int8range                    } },
            { "numrange",                    new[] { _numrange                     } },
            { "tsrange",                     new[] { _tsrange                      } },
            { "tstzrange",                   new[] { _tstzrange                    } },
            { "daterange",                   new[] { _dateOnlyDaterange, _dateTimeDaterange } },

            { "tsquery",                     new[] { _tsquery                      } },
            { "tsvector",                    new[] { _tsvector                     } },
            { "regconfig",                   new[] { _regconfig                    } },

            { "ltree",                       new[] { _ltree, _ltreeString          } },
            { "lquery",                      new[] { _lquery                       } },
            { "ltxtquery",                   new[] { _ltxtquery                    } },

            { "regdictionary",               new[] { _regdictionary                } }
        };
// ReSharper restore CoVariantArrayConversion

        // Set up aliases
        storeTypeMappings["timestamp"] = storeTypeMappings["timestamp without time zone"];
        storeTypeMappings["timestamptz"] = storeTypeMappings["timestamp with time zone"];
        storeTypeMappings["time"] = storeTypeMappings["time without time zone"];
        storeTypeMappings["timetz"] = storeTypeMappings["time with time zone"];

        var clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
        {
            { typeof(bool),                                _bool                 },
            { typeof(byte[]),                              _bytea                },
            { typeof(Guid),                                _uuid                 },

            { typeof(byte),                                _int2Byte             },
            { typeof(short),                               _int2                 },
            { typeof(int),                                 _int4                 },
            { typeof(long),                                _int8                 },
            { typeof(float),                               _float4               },
            { typeof(double),                              _float8               },
            { typeof(decimal),                             _numeric              },
            { typeof(BigInteger),                          _bigInteger           },

            { typeof(string),                              _text                 },
            { typeof(JsonDocument),                        _jsonbDocument        },
            { typeof(JsonElement),                         _jsonbElement         },
            { typeof(char),                                _singleChar           },

            { typeof(DateTime),                            LegacyTimestampBehavior ? _timestamp : _timestamptz },
            { typeof(DateOnly),                            _dateDateOnly         },
            { typeof(TimeOnly),                            _timeTimeOnly         },
            { typeof(TimeSpan),                            _interval             },
            { typeof(DateTimeOffset),                      _timestamptzDto       },

            { typeof(PhysicalAddress),                     _macaddr              },
            { typeof(IPAddress),                           _inet                 },
            { typeof((IPAddress, int)),                    _cidr                 },

            { typeof(BitArray),                            _varbit               },
            { typeof(ImmutableDictionary<string, string>), _immutableHstore      },
            { typeof(Dictionary<string, string>),          _hstore               },
            { typeof(NpgsqlTid),                           _tid                  },
            { typeof(NpgsqlLogSequenceNumber),             _pgLsn                },

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
            { typeof(NpgsqlRange<DateTime>),               LegacyTimestampBehavior ? _tsrange : _tstzrange },
            { typeof(NpgsqlRange<DateTimeOffset>),          _tstzrange           },
            { typeof(NpgsqlRange<DateOnly>),               _dateOnlyDaterange },

            { typeof(NpgsqlTsQuery),                       _tsquery              },
            { typeof(NpgsqlTsVector),                      _tsvector             },
            { typeof(NpgsqlTsRankingNormalization),        _rankingNormalization },

            { typeof(LTree),                               _ltree                }
        };

        if (_supportsMultiranges)
        {
            storeTypeMappings["int4multirange"] = new[] { _int4multirangeArray, _int4multirangeList };
            storeTypeMappings["int8multirange"] = new[] { _int8multirangeArray, _int8multirangeList };
            storeTypeMappings["nummultirange"] = new[] { _nummultirangeArray, _nummultirangeList   };
            storeTypeMappings["tsmultirange"] = new[] { _tsmultirangeArray, _tsmultirangeList     };
            storeTypeMappings["tstzmultirange"] = new[] { _tstzmultirangeArray, _tstzmultirangeList };
            storeTypeMappings["datemultirange"] = new[] { _dateOnlyDatemultirangeArray, _dateOnlyDatemultirangeList, _dateTimeDatemultirangeArray, _dateTimeMultirangeList };

            clrTypeMappings[typeof(NpgsqlRange<int>[])] = _int4multirangeArray;
            clrTypeMappings[typeof(NpgsqlRange<long>[])] = _int8multirangeArray;
            clrTypeMappings[typeof(NpgsqlRange<decimal>[])] = _nummultirangeArray;
            clrTypeMappings[typeof(NpgsqlRange<DateTime>[])] = LegacyTimestampBehavior ? _tsmultirangeArray : _tstzmultirangeArray;
            clrTypeMappings[typeof(NpgsqlRange<DateOnly>[])] = _dateOnlyDatemultirangeArray;

            clrTypeMappings[typeof(List<NpgsqlRange<int>>)] = _int4multirangeList;
            clrTypeMappings[typeof(List<NpgsqlRange<long>>)] = _int8multirangeList;
            clrTypeMappings[typeof(List<NpgsqlRange<decimal>>)] = _nummultirangeList;
            clrTypeMappings[typeof(List<NpgsqlRange<DateTime>>)] = LegacyTimestampBehavior ? _tsmultirangeList : _tstzmultirangeList;
            clrTypeMappings[typeof(List<NpgsqlRange<DateOnly>>)] = _dateOnlyDatemultirangeList;
        }

        StoreTypeMappings = new ConcurrentDictionary<string, RelationalTypeMapping[]>(storeTypeMappings, StringComparer.OrdinalIgnoreCase);
        ClrTypeMappings = new ConcurrentDictionary<Type, RelationalTypeMapping>(clrTypeMappings);

        LoadUserDefinedTypeMappings(sqlGenerationHelper);

        _userRangeDefinitions = npgsqlSingletonOptions?.UserRangeDefinitions ?? Array.Empty<UserRangeDefinition>();
    }

    /// <summary>
    /// To be used in case user-defined mappings are added late, after this TypeMappingSource has already been initialized.
    /// This is basically only for test usage.
    /// </summary>
    public virtual void LoadUserDefinedTypeMappings(ISqlGenerationHelper sqlGenerationHelper)
        => SetupEnumMappings(sqlGenerationHelper);

    /// <summary>
    /// Gets all global enum mappings from the ADO.NET layer and creates mappings for them
    /// </summary>
    protected virtual void SetupEnumMappings(ISqlGenerationHelper sqlGenerationHelper)
    {
        _adoUserTypeMappingsGetMethodInfo ??= NpgsqlConnection.GlobalTypeMapper.GetType().GetProperty("UserTypeMappings")?.GetMethod;

        if (_adoUserTypeMappingsGetMethodInfo is null)
        {
            return;
        }

        var adoUserTypeMappings = (IDictionary<string, IUserTypeMapping>)_adoUserTypeMappingsGetMethodInfo.Invoke(NpgsqlConnection.GlobalTypeMapper, Array.Empty<object>())!;

        foreach (var adoUserTypeMapping in adoUserTypeMappings.Values.OfType<IUserEnumTypeMapping>())
        {
            // TODO: update with schema per https://github.com/npgsql/npgsql/issues/2121
            var components = adoUserTypeMapping.PgTypeName.Split('.');
            var schema = components.Length > 1 ? components.First() : null;
            var name = components.Length > 1 ? string.Join(null, components.Skip(1)) : adoUserTypeMapping.PgTypeName;

            var mapping = new NpgsqlEnumTypeMapping(
                name, schema, adoUserTypeMapping.ClrType, sqlGenerationHelper, adoUserTypeMapping.NameTranslator);
            ClrTypeMappings[adoUserTypeMapping.ClrType] = mapping;
            StoreTypeMappings[mapping.StoreType] = new RelationalTypeMapping[] { mapping };
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo) =>
        // First, try any plugins, allowing them to override built-in mappings (e.g. NodaTime)
        base.FindMapping(mappingInfo) ??
        FindBaseMapping(mappingInfo)?.Clone(mappingInfo) ??
        FindArrayMapping(mappingInfo)?.Clone(mappingInfo) ??
        FindUserRangeMapping(mappingInfo);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual RelationalTypeMapping? FindBaseMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;
        var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

        if (storeTypeName is not null)
        {
            if (StoreTypeMappings.TryGetValue(storeTypeName, out var mappings))
            {
                // We found the user-specified store type. No CLR type was provided - we're probably
                // scaffolding from an existing database, take the first mapping as the default.
                if (clrType is null)
                {
                    return mappings[0];
                }

                // A CLR type was provided - look for a mapping between the store and CLR types. If not found, fail
                // immediately.
                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m;
                    }
                }

                // Map arbitrary user POCOs to JSON
                if (storeTypeName == "jsonb" || storeTypeName == "json")
                {
                    return new NpgsqlJsonTypeMapping(storeTypeName, clrType);
                }

                return null;
            }

            if (StoreTypeMappings.TryGetValue(storeTypeNameBase!, out mappings))
            {
                if (clrType is null)
                {
                    return mappings[0].Clone(in mappingInfo);
                }

                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m.Clone(in mappingInfo);
                    }
                }

                return null;
            }

            // 'character' is special: 'character' (no size) and 'character(1)' map to a single char, whereas 'character(n)' maps
            // to a string
            if (storeTypeNameBase is "character" or "char")
            {
                if (mappingInfo.Size is null or 1 && clrType is null || clrType == typeof(char))
                {
                    return _singleChar.Clone(mappingInfo);
                }

                if (clrType is null || clrType == typeof(string))
                {
                    return _char.Clone(mappingInfo);
                }
            }

            // A store type name was provided, but is unknown. This could be a domain (alias) type, in which case
            // we proceed with a CLR type lookup (if the type doesn't exist at all the failure will come later).
        }

        if (clrType is null ||
            !ClrTypeMappings.TryGetValue(clrType, out var mapping) ||
            // Special case for byte[] mapped as smallint[] - don't return bytea mapping
            storeTypeName is not null && storeTypeName == "smallint[]")
        {
            return null;
        }

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
                mapping = mappingInfo.IsFixedLength ?? false ? _bit : _varbit;
                return mapping.Clone($"{mapping.StoreType}({mappingInfo.Size})", mappingInfo.Size);
            }
        }

        return mapping;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual RelationalTypeMapping? FindArrayMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        Type? elementClrType = null;

        if (clrType is not null && !clrType.TryGetElementType(out elementClrType))
        {
            return null; // Not an array/list
        }

        var storeType = mappingInfo.StoreTypeName;
        var storeTypeNameBase = mappingInfo.StoreTypeNameBase;
        if (storeType is not null)
        {
            // PostgreSQL array type names are the element plus []
            if (!storeType.EndsWith("[]", StringComparison.Ordinal))
            {
                return null;
            }

            var elementStoreType = storeType.Substring(0, storeType.Length - 2);
            var elementStoreTypeNameBase = storeTypeNameBase!.Substring(0, storeTypeNameBase.Length - 2);

            var elementMapping = elementClrType is null
                ? FindMapping(new RelationalTypeMappingInfo(
                    elementStoreType, elementStoreTypeNameBase,
                    mappingInfo.IsUnicode, mappingInfo.Size, mappingInfo.Precision, mappingInfo.Scale))
                : FindMapping(new RelationalTypeMappingInfo(
                    elementClrType, elementStoreType, elementStoreTypeNameBase,
                    mappingInfo.IsKeyOrIndex, mappingInfo.IsUnicode, mappingInfo.Size, mappingInfo.IsRowVersion,
                    mappingInfo.IsFixedLength, mappingInfo.Precision, mappingInfo.Scale));

            // If no mapping was found for the element, there's no mapping for the array.
            // Also, arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
            if (elementMapping is null || elementMapping is NpgsqlArrayTypeMapping)
            {
                return null;
            }

            return clrType is null || clrType.IsArray
                ? new NpgsqlArrayArrayTypeMapping(storeType, elementMapping)
                : new NpgsqlArrayListTypeMapping(storeType, elementMapping);
        }

        if (clrType is null)
        {
            return null;
        }

        if (clrType.IsArray)
        {
            var elementType = clrType.GetElementType();
            Debug.Assert(elementType is not null, "Detected array type but element type is null");

            var elementMapping = FindMapping(elementType);

            // If no mapping was found for the element, there's no mapping for the array.
            // Also, arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
            if (elementMapping is null || elementMapping is NpgsqlArrayTypeMapping)
            {
                return null;
            }

            // Not that the element mapping found above was stripped of nullability
            // (so we get a mapping for int, not int?).
            Debug.Assert(
                Nullable.GetUnderlyingType(elementType) is null ||
                Nullable.GetUnderlyingType(elementType) == elementMapping.ClrType);

            return new NpgsqlArrayArrayTypeMapping(clrType, elementMapping);
        }

        if (clrType.IsGenericList())
        {
            var elementType = clrType.GetGenericArguments()[0];

            // If an element isn't supported, neither is its array
            var elementMapping = FindMapping(elementType);
            if (elementMapping is null)
            {
                return null;
            }

            // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
            if (elementMapping is NpgsqlArrayTypeMapping)
            {
                return null;
            }

            return new NpgsqlArrayListTypeMapping(clrType, elementMapping);
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual RelationalTypeMapping? FindUserRangeMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        UserRangeDefinition? rangeDefinition = null;
        var rangeStoreType = mappingInfo.StoreTypeName;
        var rangeClrType = mappingInfo.ClrType;

        // If the incoming MappingInfo contains a ClrType, make sure it's an NpgsqlRange<T>, otherwise bail
        if (rangeClrType is not null &&
            (!rangeClrType.IsGenericType || rangeClrType.GetGenericTypeDefinition() != typeof(NpgsqlRange<>)))
        {
            return null;
        }

        // Try to find a user range definition (defined by the user on their context options), based on the
        // incoming MappingInfo's StoreType or ClrType
        if (rangeStoreType is not null)
        {
            rangeDefinition = _userRangeDefinitions.SingleOrDefault(m => m.RangeName == rangeStoreType);

            if (rangeDefinition is null)
            {
                return null;
            }

            if (rangeClrType is null)
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
        else if (rangeClrType is not null)
        {
            rangeDefinition = _userRangeDefinitions.SingleOrDefault(m => m.SubtypeClrType == rangeClrType.GetGenericArguments()[0]);
        }

        if (rangeClrType is null || rangeDefinition is null)
        {
            return null;
        }

        // We now have a user-defined range definition from the context options. Use it to get the subtype's mapping
        var subtypeMapping = rangeDefinition.SubtypeName is null
            ? FindMapping(rangeDefinition.SubtypeClrType)
            : FindMapping(rangeDefinition.SubtypeName);

        if (subtypeMapping is null)
        {
            throw new Exception($"Could not map range {rangeDefinition.RangeName}, no mapping was found its subtype");
        }

        return new NpgsqlRangeTypeMapping(rangeDefinition.RangeName, rangeDefinition.SchemaName, rangeClrType, subtypeMapping, _sqlGenerationHelper);
    }

    /// <summary>
    /// Finds the mapping for a container given its CLR type and its containee's type mapping; this is currently used to infer type
    /// mappings for ranges and multiranges from their values.
    /// </summary>
    public virtual RelationalTypeMapping? FindContainerMapping(Type containerClrType, RelationalTypeMapping containeeTypeMapping)
    {
        if (containerClrType.TryGetRangeSubtype(out var subtypeType))
        {
            return _rangeTypeMappings.TryGetValue(subtypeType, out var candidateMappings)
                ? candidateMappings.FirstOrDefault(m => m.SubtypeMapping.StoreType == containeeTypeMapping.StoreType)
                : null;
        }

        if (_supportsMultiranges && containerClrType.TryGetMultirangeSubtype(out subtypeType))
        {
            return _multirangeTypeMappings.TryGetValue(subtypeType, out var candidateMappings)
                ? candidateMappings.FirstOrDefault(m => m.SubtypeMapping.StoreType == containeeTypeMapping.StoreType)
                : null;
        }

        return null;
    }

    private static bool NameBasesUsesPrecision(ReadOnlySpan<char> span)
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // We override to support parsing array store names (e.g. varchar(32)[]), timestamp(5) with time zone, etc.
    protected override string? ParseStoreTypeName(
        string? storeTypeName,
        out bool? unicode,
        out int? size,
        out int? precision,
        out int? scale)
    {
        (unicode, size, precision, scale) = (null, null, null, null);

        if (storeTypeName is null)
        {
            return null;
        }

        var span = storeTypeName.AsSpan().Trim();

        var openParen = span.IndexOf("(", StringComparison.Ordinal);
        if (openParen == -1)
        {
            return storeTypeName;
        }

        var afterOpenParen = span.Slice(openParen + 1).TrimStart();
        var closeParen = afterOpenParen.IndexOf(")", StringComparison.Ordinal);
        if (closeParen == -1)
        {
            return storeTypeName;
        }

        var preParens = span[..openParen].Trim();
        var inParens = afterOpenParen[..closeParen].Trim();
        // There may be stuff after the closing parentheses (e.g. varchar(32)[], timestamp(3) with time zone)
        var postParens = afterOpenParen.Slice(closeParen + 1);

        var comma = inParens.IndexOf(",", StringComparison.Ordinal);
        if (comma != -1)
        {
            if (int.TryParse(inParens[..comma].Trim(), out var parsedPrecision))
            {
                precision = parsedPrecision;
            }

            if (int.TryParse(inParens.Slice(comma + 1), out var parsedScale))
            {
                scale = parsedScale;
            }
        }
        else if (int.TryParse(inParens, out var parsedSize))
        {
            if (NameBasesUsesPrecision(preParens))
            {
                precision = parsedSize;
                scale = 0;
            }
            else
            {
                size = parsedSize;
            }
        }
        else
        {
            return storeTypeName;
        }

        if (postParens.Length > 0)
        {
            return new StringBuilder()
                .Append(preParens)
                .Append(postParens)
                .ToString();
        }

        return preParens.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override CoreTypeMapping? FindMapping(IProperty property)
    {
        var mapping = base.FindMapping(property);

        // For arrays over reference types, the CLR type doesn't convey nullability (unlike with arrays over value types).
        // We decode NRT annotations here to return the correct type mapping.
        if (mapping is NpgsqlArrayTypeMapping arrayMapping
            && !arrayMapping.ElementMapping.ClrType.IsValueType
            && !property.IsShadowProperty())
        {
            var nullabilityInfo =
                property.PropertyInfo is { } propertyInfo
                    ? _nullabilityInfoContext.Create(propertyInfo)
                    : property.FieldInfo is { } fieldInfo
                        ? _nullabilityInfoContext.Create(fieldInfo)
                        : null;

            // We already know from the mapping check above that the member is either an array or a generic list
            var elementNullabilityInfo = nullabilityInfo?.ElementType
                ?? (nullabilityInfo?.GenericTypeArguments.Length > 0 ? nullabilityInfo.GenericTypeArguments[0] : null);

            if (elementNullabilityInfo?.ReadState == NullabilityState.NotNull
                && elementNullabilityInfo.WriteState == NullabilityState.NotNull)
            {
                return arrayMapping.MakeNonNullable();
            }
        }

        return mapping;
    }
}
