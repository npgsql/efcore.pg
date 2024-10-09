using System.Collections.Immutable;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DictionaryTranslator : IMethodCallTranslator, IMemberTranslator
{
    #region Types

    private static readonly Type StringDictionaryType = typeof(Dictionary<string, string>);
    private static readonly Type ImmutableStringDictionaryType = typeof(ImmutableDictionary<string, string>);
    private static readonly Type ExtensionsType = typeof(DictionaryDbFunctionsExtensions);
    private static readonly Type GenericKvpType = typeof(KeyValuePair<,>).MakeGenericType(
        Type.MakeGenericMethodParameter(0), Type.MakeGenericMethodParameter(1));
    private static readonly Type EnumerableType = typeof(IEnumerable<>).MakeGenericType(Type.MakeGenericMethodParameter(0));
    private static readonly Type GenericListType = typeof(List<>);
    private static readonly Type StringType = typeof(string);
    private static readonly Type StringListType = typeof(List<string>);


    #endregion

    #region MethodInfo(s)

    private static readonly MethodInfo Enumerable_Any =
        typeof(Enumerable).GetMethod(nameof(Enumerable.Any), BindingFlags.Public | BindingFlags.Static, [EnumerableType])!;

    private static readonly MethodInfo Enumerable_Count =
        typeof(Enumerable).GetMethod(nameof(Enumerable.Count), BindingFlags.Public | BindingFlags.Static, [EnumerableType])!;

    private static readonly MethodInfo Enumerable_ToList =
        typeof(Enumerable).GetMethod(nameof(Enumerable.ToList), BindingFlags.Public | BindingFlags.Static, [EnumerableType])!;

    private static readonly MethodInfo Enumerable_ToDictionary =
        typeof(Enumerable).GetMethod(
            nameof(Enumerable.ToDictionary), BindingFlags.Public | BindingFlags.Static,
            [typeof(IEnumerable<>).MakeGenericType(GenericKvpType)])!;

    private static readonly MethodInfo GenericImmutableDictionary_ToImmutableDictionary =
        typeof(ImmutableDictionary).GetMethod(
            nameof(ImmutableDictionary.ToImmutableDictionary), BindingFlags.Public | BindingFlags.Static,
            [typeof(IEnumerable<>).MakeGenericType(GenericKvpType)])!;

    private static readonly MethodInfo Enumerable_Concat = typeof(Enumerable).GetMethod(
        nameof(Enumerable.Concat), BindingFlags.Public | BindingFlags.Static,
        [EnumerableType, EnumerableType])!;

    private static readonly MethodInfo Enumerable_Except = typeof(Enumerable).GetMethod(
        nameof(Enumerable.Except), BindingFlags.Public | BindingFlags.Static,
        [EnumerableType, EnumerableType])!;

    private static readonly MethodInfo Enumerable_SequenceEqual = typeof(Enumerable).GetMethod(
        nameof(Enumerable.SequenceEqual), BindingFlags.Public | BindingFlags.Static,
        [EnumerableType, EnumerableType])!;

    #endregion

    #region Extension MethodInfo(s)

    private static readonly MethodInfo Extension_ValuesForKeys =
        ExtensionsType.GetMethod(nameof(DictionaryDbFunctionsExtensions.ValuesForKeys))!;

    private static readonly MethodInfo Extension_ToHstore =
        ExtensionsType.GetMethod(nameof(DictionaryDbFunctionsExtensions.ToHstore))!;

    private static readonly MethodInfo Extension_ToJson =
        ExtensionsType.GetMethod(nameof(DictionaryDbFunctionsExtensions.ToJson))!;

    private static readonly MethodInfo Extension_ToJsonb =
        ExtensionsType.GetMethod(nameof(DictionaryDbFunctionsExtensions.ToJsonb))!;

    private static readonly MethodInfo Extension_ToJsonLoose =
        ExtensionsType.GetMethod(nameof(DictionaryDbFunctionsExtensions.ToJsonLoose))!;

    private static readonly MethodInfo Extension_ToJsonbLoose =
        ExtensionsType.GetMethod(nameof(DictionaryDbFunctionsExtensions.ToJsonbLoose))!;

    private static readonly MethodInfo Extension_Contains =
        ExtensionsType.GetMethod(nameof(DictionaryDbFunctionsExtensions.Contains))!;

    private static readonly MethodInfo Extension_ContainedBy =
        ExtensionsType.GetMethod(nameof(DictionaryDbFunctionsExtensions.ContainedBy))!;

    private static readonly MethodInfo Extension_Remove =
        ExtensionsType.GetMethod(nameof(DictionaryDbFunctionsExtensions.Remove))!;

    private static readonly MethodInfo Extension_Slice =
        ExtensionsType.GetMethod(nameof(DictionaryDbFunctionsExtensions.Slice))!;

    private static readonly MethodInfo Extension_ToKeysAndValues =
        ExtensionsType.GetMethod(nameof(DictionaryDbFunctionsExtensions.ToKeyValueList))!;

    private static readonly MethodInfo Extension_FromKeysAndValues_List =
        ExtensionsType.GetMethod(
            nameof(DictionaryDbFunctionsExtensions.DictionaryFromKeyValueList), BindingFlags.Public | BindingFlags.Static,
            null, [typeof(DbFunctions), typeof(IList<string>)], null)!;

    private static readonly MethodInfo Extension_FromKeysAndValues_List_List =
        ExtensionsType.GetMethod(
            nameof(DictionaryDbFunctionsExtensions.DictionaryFromKeyValueList), BindingFlags.Public | BindingFlags.Static,
            null, [typeof(DbFunctions), typeof(IList<string>), typeof(IList<string>)], null)!;

    #endregion

    #region Fields

    private readonly RelationalTypeMapping _stringListTypeMapping;
    private readonly RelationalTypeMapping _stringTypeMapping;
    private readonly RelationalTypeMapping _stringDictionaryMapping;
    private readonly RelationalTypeMapping _jsonTypeMapping;
    private readonly RelationalTypeMapping _jsonbTypeMapping;
    private readonly RelationalTypeMapping _immutableStringDictionaryMapping;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly IModel _model;

    #endregion

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DictionaryTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        _typeMappingSource = typeMappingSource;
        _model = model;
        _sqlExpressionFactory = sqlExpressionFactory;
        _stringListTypeMapping = typeMappingSource.FindMapping(StringListType, model)!;
        _stringTypeMapping = typeMappingSource.FindMapping(StringType, model)!;
        _stringDictionaryMapping = typeMappingSource.FindMapping(StringDictionaryType, model)!;
        _immutableStringDictionaryMapping = typeMappingSource.FindMapping(ImmutableStringDictionaryType, model)!;
        _jsonTypeMapping = typeMappingSource.FindMapping("json")!;
        _jsonbTypeMapping = typeMappingSource.FindMapping("jsonb")!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null)
        {
            if (arguments.Count is 3)
            {
                if (!IsDictionaryType(arguments[1].Type))
                {
                    return null;
                }

                if (method == Extension_ValuesForKeys)
                {
                    return ValuesForKeys(arguments[1], arguments[2]);
                }

                if (method == Extension_FromKeysAndValues_List_List)
                {
                    return FromKeysAndValues(arguments[1], arguments[2]);
                }

                if (method == Extension_Slice)
                {
                    return Slice(arguments[1], arguments[2]);
                }

                if (method.IsClosedFormOf(Extension_Contains))
                {
                    return _sqlExpressionFactory.MakePostgresBinary(
                        PgExpressionType.Contains, arguments[1], ToHstoreOrJsonb(arguments[2])!);
                }

                if (method.IsClosedFormOf(Extension_ContainedBy))
                {
                    return _sqlExpressionFactory.MakePostgresBinary(
                        PgExpressionType.ContainedBy, arguments[1], ToHstoreOrJsonb(arguments[2])!);
                }

                if (method.IsClosedFormOf(Extension_Remove))
                {
                    return Subtract(arguments[1], arguments[2]);
                }

                return null;
            }

            if (arguments.Count is 2)
            {
                if (!IsDictionaryType(arguments[1].Type) && !IsDictionaryType(arguments[0].Type))
                {
                    return null;
                }

                if (method == Extension_ToHstore)
                {
                    return arguments[1].TypeMapping?.StoreType == "hstore" ? arguments[1] : ToHstore(arguments[1]);
                }

                if (method.IsClosedFormOf(Extension_ToJson))
                {
                    return ToJson(arguments[1]);
                }

                if (method.IsClosedFormOf(Extension_ToJsonb))
                {
                    return ToJsonb(arguments[1]);
                }

                if (method.IsClosedFormOf(Extension_ToKeysAndValues))
                {
                    return ToKeysAndValues(arguments[1], method.ReturnType.GetGenericArguments()[0]);
                }

                if (method.IsClosedFormOf(Extension_FromKeysAndValues_List))
                {
                    return FromKeysAndValues(arguments[1]);
                }

                if (method.IsClosedFormOf(Extension_ToJsonLoose))
                {
                    return ToJsonLoose(arguments[1]);
                }

                if (method.IsClosedFormOf(Extension_ToJsonbLoose))
                {
                    return ToJsonbLoose(arguments[1]);
                }

                if (method.IsClosedFormOf(Enumerable_SequenceEqual))
                {
                    return Equal(arguments[0], arguments[1]);
                }

                if (method.IsClosedFormOf(Enumerable_Concat))
                {
                    return Concat(arguments[0], arguments[1]);
                }

                if (method.IsClosedFormOf(Enumerable_Except))
                {
                    return Subtract(arguments[0], arguments[1], false);
                }

                return null;
            }

            if (arguments.Count is 1)
            {
                if (method.IsClosedFormOf(Enumerable_Any))
                {
                    var keyValueType = method.GetGenericArguments()[0];
                    if (!keyValueType.IsGenericType || keyValueType.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
                    {
                        return null;
                    }
                    return NotEmpty(arguments[0], keyValueType.GetGenericArguments()[0], keyValueType.GetGenericArguments()[1]);
                }

                if (method.IsClosedFormOf(Enumerable_Count))
                {
                    var keyValueType = method.GetGenericArguments()[0];
                    if (!keyValueType.IsGenericType || keyValueType.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
                    {
                        return null;
                    }
                    return Count(arguments[0], keyValueType.GetGenericArguments()[0]);
                }

                if (method.IsClosedFormOf(Enumerable_ToDictionary))
                {
                    return arguments[0].Type == method.ReturnType
                        ? arguments[0]
                        : arguments[0].TypeMapping?.StoreType == "hstore"
                            ? arguments[0] is SqlConstantExpression or SqlParameterExpression
                                ? _sqlExpressionFactory.ApplyTypeMapping(arguments[0], _stringDictionaryMapping)
                                : _sqlExpressionFactory.Convert(arguments[0], StringDictionaryType, _stringDictionaryMapping)
                            : null;
                }

                if (method.IsClosedFormOf(GenericImmutableDictionary_ToImmutableDictionary))
                {
                    return arguments[0].Type == method.ReturnType
                        ? arguments[0]
                        : arguments[0].TypeMapping?.StoreType == "hstore"
                            ? arguments[0] is SqlConstantExpression or SqlParameterExpression
                                ? _sqlExpressionFactory.ApplyTypeMapping(arguments[0], _immutableStringDictionaryMapping)
                                : _sqlExpressionFactory.Convert(
                                    arguments[0], ImmutableStringDictionaryType, _immutableStringDictionaryMapping)
                            : null;
                }

                // Hstore: store.Keys.ToList() => akeys(store)
                //         store.Values.ToList() -> avals(store)
                // Json: store.Keys.ToList() => array(select json_object_keys(instance))
                //       store.Values.ToList() => select array_agg(value) from json_each_text(instance))
                // Jsonb: store.Keys.ToList() => array(select jsonb_object_keys(instance))
                //        store.Values.ToList() => select array_agg(value) from jsonb_each_text(instance))
                if (method.IsClosedFormOf(Enumerable_ToList)
                    && (arguments[0] is SqlFunctionExpression { Name: "akeys" or "avals", Arguments: [{ TypeMapping.StoreType: "hstore" }] }
                        || arguments[0] is SqlFunctionExpression
                        {
                            Name: "array",
                            Arguments:
                            [
                                ScalarSubqueryExpression
                                {
                                    Subquery.Projection:
                                    [
                                        {
                                            Expression: SqlFunctionExpression
                                            {
                                                Name: "json_object_keys" or "jsonb_object_keys",
                                                Arguments: [{ TypeMapping.StoreType: "json" or "jsonb" }]
                                            }
                                        }
                                    ]
                                }
                            ]
                        }
                        || arguments[0] is ScalarSubqueryExpression
                        {
                            Subquery.Tables:
                            [
                                TableValuedFunctionExpression
                                {
                                    Name: "json_each_text" or "jsonb_each_text", Arguments: [{ TypeMapping.StoreType: "json" or "jsonb" }]
                                }
                            ]
                        }))
                {
                    return arguments[0];
                }

                return null;
            }

            return null;
        }

        if (!IsDictionaryMethod(method))
        {
            return null;
        }

        if (method.Name == "get_Item")
        {
            return ValueForKey(instance, arguments[0]);
        }

        if (method.Name == nameof(ImmutableDictionary<string, string>.Remove))
        {
            return Subtract(instance, arguments[0]);
        }

        if (method.Name == nameof(Dictionary<string, string>.ContainsKey))
        {
            return ContainsKey(instance, arguments[0]);
        }

        if (method.Name == nameof(Dictionary<string, string>.ContainsValue))
        {
            return ContainsValue(instance, arguments[0], arguments[0].Type);
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null || !IsDictionaryType(instance.Type))
        {
            return null;
        }

        var args = instance.Type.GetGenericArguments();
        return member.Name switch
        {
            nameof(Dictionary<string, string>.Keys) => Keys(instance, args[0]),
            nameof(Dictionary<string, string>.Count) => Count(instance, args[0], true),
            nameof(ImmutableDictionary<string, string>.IsEmpty) => Empty(instance, args[0], args[1]),
            nameof(Dictionary<string, string>.Values) => Values(instance, args[1]),
            _ => null
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? Keys(SqlExpression instance, Type keyType)
        => instance.TypeMapping?.StoreType switch
        {
            "json" => JsonObjectKeys(instance, keyType, "json_object_keys"),
            "jsonb" => JsonObjectKeys(instance, keyType, "jsonb_object_keys"),
            "hstore" => _sqlExpressionFactory.Function(
                "akeys", [instance], true, TrueArrays[1], StringListType, _stringListTypeMapping),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? Values(SqlExpression instance, Type valueType)
        => instance.TypeMapping?.StoreType switch
        {
            "json" => JsonObjectValues(instance, valueType, "json_each_text"),
            "jsonb" => JsonObjectValues(instance, valueType, "jsonb_each_text"),
            "hstore" => _sqlExpressionFactory.Function(
                "avals", [instance], true, TrueArrays[1], StringListType, _stringListTypeMapping),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? Count(SqlExpression instance, Type keyType, bool nullable = false)
        => IsDictionaryStore(instance)
            ? _sqlExpressionFactory.Function("cardinality", [Keys(instance, keyType)!], nullable, TrueArrays[1], typeof(int))
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? NotEmpty(SqlExpression instance, Type keyType, Type valueType)
    {
        var emptyDictionary = keyType == StringType && valueType == StringType
            ? new Dictionary<string, string>()
            : typeof(Dictionary<,>).MakeGenericType(keyType, valueType).GetConstructor([])!.Invoke([]);
        return instance.TypeMapping!.StoreType switch
        {
            "json" => _sqlExpressionFactory.NotEqual(
                ConvertToJsonb(instance),
                _sqlExpressionFactory.Constant(emptyDictionary, _jsonbTypeMapping)),
            "jsonb" => _sqlExpressionFactory.NotEqual(
                instance,
                _sqlExpressionFactory.Constant(emptyDictionary, _jsonbTypeMapping)),
            "hstore" => _sqlExpressionFactory.NotEqual(Count(instance, keyType)!, _sqlExpressionFactory.Constant(0)),
            _ => null
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? Empty(SqlExpression instance, Type keyType, Type valueType)
    {
        var emptyDictionary = keyType == StringType && valueType == StringType
            ? new Dictionary<string, string>()
            : typeof(Dictionary<,>).MakeGenericType(keyType, valueType).GetConstructor([])!.Invoke([]);
        return instance.TypeMapping!.StoreType switch
        {
            "json" => _sqlExpressionFactory.Equal(
                ConvertToJsonb(instance),
                _sqlExpressionFactory.Constant(emptyDictionary, _jsonbTypeMapping)),
            "jsonb" => _sqlExpressionFactory.Equal(
                instance,
                _sqlExpressionFactory.Constant(emptyDictionary, _jsonbTypeMapping)),
            "hstore" => _sqlExpressionFactory.Equal(Count(instance, keyType)!, _sqlExpressionFactory.Constant(0)),
            _ => null
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? Subtract(SqlExpression left, SqlExpression right, bool leftType = true)
    {
        var leftCoerced = ToHstoreOrJsonb(left);
        var rightCoerced = ToHstoreStringArrayStringOrJsonb(right);
        if (leftCoerced is null || rightCoerced is null)
        {
            return null;
        }
        return _sqlExpressionFactory.MakePostgresBinary(
            PgExpressionType.HStoreSubtract, left, right, (leftType ? left : right).TypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? Concat(SqlExpression left, SqlExpression right)
    {
        var coerced = CoerceToSameStoreType(left, right, false);
        if (!coerced.HasValue)
        {
            return null;
        }
        return _sqlExpressionFactory.MakePostgresBinary(
            PgExpressionType.HStoreConcat, coerced.Value.Item1, coerced.Value.Item2, coerced.Value.Item2.TypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? Equal(SqlExpression left, SqlExpression right)
    {
        var coerced = CoerceToSameStoreType(left, right, true);
        if (!coerced.HasValue)
        {
            return null;
        }
        return _sqlExpressionFactory.MakeBinary(
            ExpressionType.Equal, coerced.Value.Item1, coerced.Value.Item1, coerced.Value.Item1.TypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression FromKeysAndValues(params SqlExpression[] arguments)
        => _sqlExpressionFactory.Function(
            "hstore", arguments, true, TrueArrays[arguments.Length], StringDictionaryType, _stringDictionaryMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? ToKeysAndValues(SqlExpression instance, Type keyValueType)
        => instance.TypeMapping!.StoreType switch
        {
            "json" => JsonObjectKeysAndValues(instance, keyValueType, "json_each_text"),
            "jsonb" => JsonObjectKeysAndValues(instance, keyValueType, "jsonb_each_text"),
            "hstore" => _sqlExpressionFactory.Function(
                "hstore_to_array", [instance], true, TrueArrays[1], GenericListType.MakeGenericType(keyValueType),
                StringType == keyValueType
                    ? _stringListTypeMapping
                    : _typeMappingSource.FindMapping(GenericListType.MakeGenericType(keyValueType), _model)),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? ValuesForKeys(SqlExpression instance, SqlExpression keys)
        => IsDictionaryStore(instance)
            ? _sqlExpressionFactory.MakePostgresBinary(
                PgExpressionType.HStoreValueForKey, ToHstore(instance)!, keys, _stringListTypeMapping)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? ValueForKey(SqlExpression instance, SqlExpression key)
        => instance.TypeMapping?.StoreType switch
        {
            "hstore" => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.HStoreValueForKey, instance, key, _stringTypeMapping),
            "json" => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.JsonValueForKeyAsText, instance, key, _stringTypeMapping),
            "jsonb" => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.JsonValueForKeyAsText, instance, key, _stringTypeMapping),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? Slice(SqlExpression instance, SqlExpression keys)
        => IsDictionaryStore(instance)
            ? _sqlExpressionFactory.Function(
                "slice", [ToHstore(instance)!, keys], true, TrueArrays[2], instance.Type, instance.TypeMapping)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? ContainsKey(SqlExpression instance, SqlExpression key)
        => IsDictionaryStore(instance)
            ? _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.HStoreContainsKey, ToHstoreOrJsonb(instance)!, key)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? ContainsValue(SqlExpression instance, SqlExpression value, Type valueType)
        => IsDictionaryStore(instance)
            ? _sqlExpressionFactory.Any(value, Values(instance, valueType)!, PgAnyOperatorType.Equal)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? ToJsonb(SqlExpression instance)
        => instance.TypeMapping?.StoreType switch
        {
            "hstore" => _sqlExpressionFactory.Function(
                "hstore_to_jsonb", [instance], true, TrueArrays[1], StringDictionaryType, _jsonbTypeMapping),
            "json" => ConvertToJsonb(instance),
            "jsonb" => instance,
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? ToJson(SqlExpression instance)
        => instance.TypeMapping?.StoreType switch
        {
            "hstore" => _sqlExpressionFactory.Function(
                "hstore_to_json", [instance], true, TrueArrays[1], StringDictionaryType, _jsonTypeMapping),
            "json" => instance,
            "jsonb" => instance is SqlParameterExpression or SqlConstantExpression
                ? _sqlExpressionFactory.ApplyTypeMapping(instance, _jsonTypeMapping)
                : _sqlExpressionFactory.Convert(instance, instance.Type, _jsonTypeMapping),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? ToJsonLoose(SqlExpression instance)
        => instance.TypeMapping?.StoreType is "hstore"
            ? _sqlExpressionFactory.Function(
                "hstore_to_json_loose", [instance], true, TrueArrays[1], typeof(Dictionary<string, object?>), _jsonTypeMapping)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? ToJsonbLoose(SqlExpression instance)
        => instance.TypeMapping?.StoreType is "hstore"
            ? _sqlExpressionFactory.Function(
                "hstore_to_jsonb_loose", [instance], true, TrueArrays[1], typeof(Dictionary<string, object?>), _jsonbTypeMapping)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlExpression? ToHstore(SqlExpression instance)
        => instance.TypeMapping?.StoreType switch
        {
            "hstore" => instance,
            "jsonb" => ToHstoreFromJsonb(instance),
            "json" => ToHstoreFromJson(instance),
            _ => null
        };

    private SqlExpression? ToHstoreOrJsonb(SqlExpression instance)
        => instance.TypeMapping?.StoreType switch
        {
            "hstore" => instance,
            "jsonb" => instance,
            "json" => ConvertToJsonb(instance),
            _ => null
        };
    private SqlExpression? ToHstoreStringArrayStringOrJsonb(SqlExpression instance)
        => instance.Type == StringType ? instance : instance.TypeMapping?.StoreType switch
        {
            "hstore" => instance,
            "jsonb" => instance,
            "text[]" => instance,
            "json" => ConvertToJsonb(instance),
            _ => null
        };

    private (SqlExpression, SqlExpression)? CoerceToSameStoreType(SqlExpression left, SqlExpression right, bool allowJson)
    {
        if (left.TypeMapping is null || right.TypeMapping is null)
        {
            return null;
        }

        if (left.TypeMapping.StoreType == right.TypeMapping.StoreType)
        {
            return !allowJson && left.TypeMapping.StoreType == "json" ? (ConvertToJsonb(left), ConvertToJsonb(right)) : (left, right);
        }

        if ((left.TypeMapping.StoreType == "hstore" && right.TypeMapping.StoreType is "json" or "jsonb")
            || (right.TypeMapping.StoreType == "hstore" && left.TypeMapping.StoreType is "json" or "jsonb"))
        {
            return (ToHstore(left)!, ToHstore(right)!);
        }

        if ((left.TypeMapping.StoreType == "json" && right.TypeMapping.StoreType is "jsonb")
            || (right.TypeMapping.StoreType == "json" && left.TypeMapping.StoreType is "jsonb"))
        {
            return (ToHstoreOrJsonb(left)!, ToHstoreOrJsonb(right)!);
        }

        return null;
    }

    private ScalarSubqueryExpression ToHstoreFromJsonb(SqlExpression instance, bool immutable = false)
        => ToHstore(instance, "jsonb_each_text", immutable);

    private ScalarSubqueryExpression ToHstoreFromJson(SqlExpression instance, bool immutable = false)
        => ToHstore(instance, "json_each_text", immutable);

    private SqlExpression ConvertToJsonb(SqlExpression instance)
        => instance is SqlConstantExpression or SqlParameterExpression
            ? _sqlExpressionFactory.ApplyTypeMapping(instance, _jsonbTypeMapping)
            : _sqlExpressionFactory.Convert(instance, instance.Type, _jsonbTypeMapping);

    private static bool IsDictionaryStore(SqlExpression expr)
        => expr.TypeMapping?.StoreType is "json" or "jsonb" or "hstore";

    private static bool IsDictionaryMethod(MethodInfo method)
        => IsDictionaryType(method.DeclaringType!);

    private static bool IsDictionaryType(Type type)
        => type.IsGenericType
            && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                || type.GetGenericTypeDefinition() == typeof(ImmutableDictionary<,>));

#pragma warning disable EF1001 // SelectExpression constructors are currently internal

    private SqlExpression JsonObjectKeys(SqlExpression instance, Type keyType, string jsonObjectKeysFn)
        => _sqlExpressionFactory.Function(
            "array", [
                new ScalarSubqueryExpression(
                    new(
                        null,
                        [],
                        null, [], null,
                        [
                            new(
                                _sqlExpressionFactory.Function(
                                    jsonObjectKeysFn, [instance],
                                    true, TrueArrays[1], keyType, keyType == StringType
                                        ? _stringTypeMapping
                                        : _typeMappingSource.FindMapping(keyType, _model)),
                                string.Empty)
                        ], false, [], null, null)
                )
            ], true, TrueArrays[1], GenericListType.MakeGenericType(keyType), keyType == StringType
                ? _stringListTypeMapping
                : _typeMappingSource.FindMapping(GenericListType.MakeGenericType(keyType), _model));

    private ScalarSubqueryExpression JsonObjectValues(SqlExpression instance, Type valueType, string jsonEachTextFn)
        => new(
            new(
                null,
                [new TableValuedFunctionExpression("j1", jsonEachTextFn, [instance])],
                null, [], null,
                [
                    new(
                        _sqlExpressionFactory.Function(
                            "array_agg",
                            [
                                new ColumnExpression(
                                    "value", "j1", valueType, valueType == StringType
                                        ? _stringTypeMapping
                                        : _typeMappingSource.FindMapping(valueType, _model), false)
                            ],
                            true, TrueArrays[1], GenericListType.MakeGenericType(valueType), valueType == StringType
                                ? _stringListTypeMapping
                                : _typeMappingSource.FindMapping(GenericListType.MakeGenericType(valueType), _model)), string.Empty)
                ], false, [], null, null));

    private ScalarSubqueryExpression JsonObjectKeysAndValues(SqlExpression instance, Type keyValueType, string jsonEachTextFn)
    {
        var keyValueTypeMapping = keyValueType == StringType
            ? _stringTypeMapping
            : _typeMappingSource.FindMapping(keyValueType, _model);
        return new(
            new(
                null,
                [new TableValuedFunctionExpression("j1", jsonEachTextFn, [instance])],
                null, [], null,
                [
                    new(
                        _sqlExpressionFactory.Function(
                            "unnest",
                            [
                                _sqlExpressionFactory.Function(
                                    "array_agg",
                                    [
                                        _sqlExpressionFactory.NewArray(
                                        [
                                            new ColumnExpression("key", "j1", keyValueType, keyValueTypeMapping, false),
                                            new ColumnExpression("value", "j1", keyValueType, keyValueTypeMapping, false)
                                        ], GenericListType.MakeGenericType(keyValueType), keyValueType == StringType
                                            ? _stringListTypeMapping
                                            : _typeMappingSource.FindMapping(GenericListType.MakeGenericType(keyValueType), _model))
                                    ],
                                    true, TrueArrays[1], GenericListType.MakeGenericType(keyValueType))
                            ], true, TrueArrays[1], GenericListType.MakeGenericType(keyValueType)), string.Empty)
                ], false, [], null, null));
    }

    private ScalarSubqueryExpression ToHstore(SqlExpression instance, string jsonEachTextFn, bool immutable)
        => new(
            new(
                null,
                [new TableValuedFunctionExpression("j1", jsonEachTextFn, [instance])],
                null, [], null,
                [
                    new(
                        _sqlExpressionFactory.Function(
                            "hstore",
                            [
                                _sqlExpressionFactory.Function(
                                    "array_agg", [new ColumnExpression("key", "j1", StringType, _stringTypeMapping, false)],
                                    true, TrueArrays[1], StringListType),
                                _sqlExpressionFactory.Function(
                                    "array_agg", [new ColumnExpression("value", "j1", StringType, _stringTypeMapping, true)],
                                    true, TrueArrays[1], StringListType)
                            ],
                            true, TrueArrays[2], immutable ? ImmutableStringDictionaryType : StringDictionaryType,
                            immutable ? _immutableStringDictionaryMapping : _stringDictionaryMapping), string.Empty)
                ], false, [], null, null));
#pragma warning restore EF1001
}
