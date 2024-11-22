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
    private static readonly Type ExtensionsType = typeof(NpgsqlDictionaryDbFunctionsExtensions);
    private static readonly Type GenericKvpType = typeof(KeyValuePair<,>).MakeGenericType(
        Type.MakeGenericMethodParameter(0), Type.MakeGenericMethodParameter(1));
    private static readonly Type EnumerableType = typeof(IEnumerable<>).MakeGenericType(Type.MakeGenericMethodParameter(0));
    private static readonly Type GenericListType = typeof(List<>);
    private static readonly Type StringType = typeof(string);
    private static readonly Type BoolType = typeof(string);
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

    private static readonly MethodInfo Extension_ToHstore =
        ExtensionsType.GetMethod(nameof(NpgsqlDictionaryDbFunctionsExtensions.ToHstore))!;

    private static readonly MethodInfo Extension_ToJson =
        ExtensionsType.GetMethod(nameof(NpgsqlDictionaryDbFunctionsExtensions.ToJson))!;

    private static readonly MethodInfo Extension_ToJsonb =
        ExtensionsType.GetMethod(nameof(NpgsqlDictionaryDbFunctionsExtensions.ToJsonb))!;

    private static readonly MethodInfo Extension_ToJsonLoose =
        ExtensionsType.GetMethod(nameof(NpgsqlDictionaryDbFunctionsExtensions.ToJsonLoose))!;

    private static readonly MethodInfo Extension_ToJsonbLoose =
        ExtensionsType.GetMethod(nameof(NpgsqlDictionaryDbFunctionsExtensions.ToJsonbLoose))!;

    private static readonly MethodInfo Extension_Remove =
        ExtensionsType.GetMethod(nameof(NpgsqlDictionaryDbFunctionsExtensions.Remove))!;

    private static readonly MethodInfo Extension_ToKeysAndValues =
        ExtensionsType.GetMethod(nameof(NpgsqlDictionaryDbFunctionsExtensions.ToKeyValueList))!;

    private static readonly MethodInfo Extension_FromKeysAndValues_List =
        ExtensionsType.GetMethod(
            nameof(NpgsqlDictionaryDbFunctionsExtensions.DictionaryFromKeyValueList), BindingFlags.Public | BindingFlags.Static,
            null, [typeof(DbFunctions), typeof(IList<string>)], null)!;

    private static readonly MethodInfo Extension_FromKeysAndValues_List_List =
        ExtensionsType.GetMethod(
            nameof(NpgsqlDictionaryDbFunctionsExtensions.DictionaryFromKeyValueList), BindingFlags.Public | BindingFlags.Static,
            null, [typeof(DbFunctions), typeof(IList<string>), typeof(IList<string>)], null)!;

    #endregion

    #region Fields

    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
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
        _sqlExpressionFactory = sqlExpressionFactory;
        _model = model;
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

                if (method == Extension_FromKeysAndValues_List_List)
                {
                    return FromKeysAndValues(arguments[1], arguments[2]);
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
                    return ToHstore(arguments[1]);
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
                    return ToKeysAndValues(arguments[1]);
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
                if (method.IsClosedFormOf(Enumerable_Count) || method.IsClosedFormOf(Enumerable_Any))
                {
                    var keyValueType = method.GetGenericArguments()[0];
                    if (!keyValueType.IsGenericType || keyValueType.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
                    {
                        return null;
                    }

                    return method.Name == nameof(Enumerable.Count)
                        ? Count(arguments[0])
                        : NotEmpty(arguments[0]);
                }

                if (method.IsClosedFormOf(Enumerable_ToDictionary))
                {
                    return arguments[0].Type == method.ReturnType
                        ? arguments[0]
                        : arguments[0].TypeMapping?.StoreType == "hstore"
                            ? arguments[0] is SqlConstantExpression or SqlParameterExpression
                                ? _sqlExpressionFactory.ApplyTypeMapping(
                                    arguments[0], _typeMappingSource.FindMapping(StringDictionaryType, _model)!)
                                : _sqlExpressionFactory.Convert(
                                    arguments[0], StringDictionaryType, _typeMappingSource.FindMapping(StringDictionaryType, _model)!)
                            : null;
                }

                if (method.IsClosedFormOf(GenericImmutableDictionary_ToImmutableDictionary))
                {
                    return arguments[0].Type == method.ReturnType
                        ? arguments[0]
                        : arguments[0].TypeMapping?.StoreType == "hstore"
                            ? arguments[0] is SqlConstantExpression or SqlParameterExpression
                                ? _sqlExpressionFactory.ApplyTypeMapping(
                                    arguments[0], _typeMappingSource.FindMapping(ImmutableStringDictionaryType, _model)!)
                                : _sqlExpressionFactory.Convert(
                                    arguments[0], ImmutableStringDictionaryType,
                                    _typeMappingSource.FindMapping(ImmutableStringDictionaryType, _model)!)
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
                                    Name: "json_each_text" or "jsonb_each_text" or "json_each" or "jsonb_each",
                                    Arguments: [{ TypeMapping.StoreType: "json" or "jsonb" }]
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
            return ContainsValue(instance, arguments[0]);
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

        return member.Name switch
        {
            nameof(Dictionary<string, string>.Keys) => Keys(instance),
            nameof(Dictionary<string, string>.Count) => Count(instance, true),
            nameof(ImmutableDictionary<string, string>.IsEmpty) => Empty(instance),
            nameof(Dictionary<string, string>.Values) => Values(instance),
            _ => null
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Keys(SqlExpression instance)
        => instance.TypeMapping?.StoreType switch
        {
            "json" => JsonObjectKeys(instance, "json_object_keys"),
            "jsonb" => JsonObjectKeys(instance, "jsonb_object_keys"),
            "hstore" => _sqlExpressionFactory.Function(
                "akeys", [instance], true, TrueArrays[1], StringListType, _typeMappingSource.FindMapping(StringListType, _model)!),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Values(SqlExpression instance)
        => instance.TypeMapping?.StoreType switch
        {
            "json" => JsonObjectValues(instance, false),
            "jsonb" => JsonObjectValues(instance, true),
            "hstore" => _sqlExpressionFactory.Function(
                "avals", [instance], true, TrueArrays[1], StringListType, _typeMappingSource.FindMapping(StringListType, _model)!),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Count(SqlExpression instance, bool nullable = false)
        => IsDictionaryStore(instance)
            ? _sqlExpressionFactory.Function("cardinality", [Keys(instance)!], nullable, TrueArrays[1], typeof(int))
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? NotEmpty(SqlExpression instance)
    {
        var valueType = GetDictionaryValueType(instance.Type);
        var emptyDictionary = valueType == StringType
            ? new Dictionary<string, string>()
            : typeof(Dictionary<,>).MakeGenericType(StringType, valueType).GetConstructor([])!.Invoke([]);
        return instance.TypeMapping!.StoreType switch
        {
            "json" => _sqlExpressionFactory.NotEqual(
                ConvertToJsonb(instance),
                _sqlExpressionFactory.Constant(emptyDictionary, _typeMappingSource.FindMapping(emptyDictionary.GetType(), "jsonb"))),
            "jsonb" => _sqlExpressionFactory.NotEqual(
                instance,
                _sqlExpressionFactory.Constant(emptyDictionary, _typeMappingSource.FindMapping(emptyDictionary.GetType(), "jsonb"))),
            "hstore" => _sqlExpressionFactory.NotEqual(Count(instance)!, _sqlExpressionFactory.Constant(0)),
            _ => null
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Empty(SqlExpression instance)
    {
        var valueType = GetDictionaryValueType(instance.Type);
        var emptyDictionary = valueType == StringType
            ? new Dictionary<string, string>()
            : typeof(Dictionary<,>).MakeGenericType(StringType, valueType).GetConstructor([])!.Invoke([]);
        return instance.TypeMapping!.StoreType switch
        {
            "json" => _sqlExpressionFactory.Equal(
                ConvertToJsonb(instance),
                _sqlExpressionFactory.Constant(emptyDictionary, _typeMappingSource.FindMapping(emptyDictionary.GetType(), "jsonb"))),
            "jsonb" => _sqlExpressionFactory.Equal(
                instance,
                _sqlExpressionFactory.Constant(emptyDictionary, _typeMappingSource.FindMapping(emptyDictionary.GetType(), "jsonb"))),
            "hstore" => _sqlExpressionFactory.Equal(Count(instance)!, _sqlExpressionFactory.Constant(0)),
            _ => null
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Subtract(SqlExpression left, SqlExpression right, bool leftType = true)
    {
        var leftCoerced = ToHstoreOrJsonb(left);
        var rightCoerced = ToHstoreStringArrayStringOrJsonb(right);
        if (leftCoerced is null || rightCoerced is null)
        {
            return null;
        }

        return _sqlExpressionFactory.MakePostgresBinary(
            PgExpressionType.DictionarySubtract, left, right, (leftType ? left : right).TypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Concat(SqlExpression left, SqlExpression right)
    {
        var coerced = CoerceToSameStoreType(left, right, false);
        if (!coerced.HasValue)
        {
            return null;
        }

        return _sqlExpressionFactory.MakePostgresBinary(
            PgExpressionType.DictionaryConcat, coerced.Value.Item1, coerced.Value.Item2, coerced.Value.Item2.TypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Equal(SqlExpression left, SqlExpression right)
    {
        var coerced = CoerceToSameStoreType(left, right, true);
        if (!coerced.HasValue)
        {
            return null;
        }

        return _sqlExpressionFactory.MakeBinary(
            ExpressionType.Equal, coerced.Value.Item1, coerced.Value.Item1, null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Contains(SqlExpression left, SqlExpression right)
    {
        var coerced = CoerceToSameStoreType(left, right, false);
        if (!coerced.HasValue)
        {
            return null;
        }

        return _sqlExpressionFactory.MakePostgresBinary(
            PgExpressionType.Contains, coerced.Value.Item1, coerced.Value.Item1);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? ContainedBy(SqlExpression left, SqlExpression right)
    {
        var coerced = CoerceToSameStoreType(left, right, false);
        if (!coerced.HasValue)
        {
            return null;
        }

        return _sqlExpressionFactory.MakePostgresBinary(
            PgExpressionType.ContainedBy, coerced.Value.Item1, coerced.Value.Item1);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression FromKeysAndValues(params SqlExpression[] arguments)
        => _sqlExpressionFactory.Function(
            "hstore", arguments, true, TrueArrays[arguments.Length], StringDictionaryType,
            _typeMappingSource.FindMapping(StringDictionaryType, _model)!);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? ToKeysAndValues(SqlExpression instance)
        => instance.TypeMapping!.StoreType switch
        {
            "hstore" => _sqlExpressionFactory.Function(
                "hstore_to_array", [instance], true, TrueArrays[1], StringListType,
                _typeMappingSource.FindMapping(StringListType, _model)!),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? ValuesForKeys(SqlExpression instance, SqlExpression keys)
        => instance.TypeMapping?.StoreType switch
        {
            "json" => JsonObjectValuesForKeys(instance, keys, false),
            "jsonb" => JsonObjectValuesForKeys(instance, keys, true),
            "hstore" => _sqlExpressionFactory.MakePostgresBinary(
                PgExpressionType.DictionaryValueForKey, instance, keys, _typeMappingSource.FindMapping(StringListType, _model)!),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? ValueForKey(SqlExpression instance, SqlExpression key)
        => instance.TypeMapping?.StoreType switch
        {
            "json" => JsonObjectValueForKey(instance, key, false),
            "jsonb" => JsonObjectValueForKey(instance, key, true),
            "hstore" => _sqlExpressionFactory.MakePostgresBinary(
                PgExpressionType.DictionaryValueForKey, instance, key, _typeMappingSource.FindMapping(StringType, _model)!),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Slice(SqlExpression instance, SqlExpression keys)
        => instance.TypeMapping?.StoreType == "hstore"
            ? _sqlExpressionFactory.Function(
                "slice", [instance, keys], true, TrueArrays[2], instance.Type, instance.TypeMapping)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? ContainsKey(SqlExpression instance, SqlExpression key)
        => IsDictionaryStore(instance)
            ? _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.DictionaryContainsKey, ToHstoreOrJsonb(instance)!, key)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? ContainsValue(SqlExpression instance, SqlExpression value)
        => IsDictionaryStore(instance)
            ? _sqlExpressionFactory.Any(value, Values(instance)!, PgAnyOperatorType.Equal)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? ToJsonb(SqlExpression instance)
        => instance.TypeMapping?.StoreType switch
        {
            "json" => ConvertToJsonb(instance),
            "jsonb" => instance,
            "hstore" => _sqlExpressionFactory.Function(
                "hstore_to_jsonb", [instance], true, TrueArrays[1], StringDictionaryType, _typeMappingSource.FindMapping("jsonb")),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? ToJson(SqlExpression instance)
        => instance.TypeMapping?.StoreType switch
        {
            "json" => instance,
            "jsonb" => instance is SqlParameterExpression or SqlConstantExpression
                ? _sqlExpressionFactory.ApplyTypeMapping(instance, _typeMappingSource.FindMapping(instance.Type, "json"))
                : _sqlExpressionFactory.Convert(instance, instance.Type, _typeMappingSource.FindMapping(instance.Type, "json")),
            "hstore" => _sqlExpressionFactory.Function(
                "hstore_to_json", [instance], true, TrueArrays[1], StringDictionaryType, _typeMappingSource.FindMapping("json")),
            _ => null
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? ToJsonLoose(SqlExpression instance)
        => instance.TypeMapping?.StoreType is "hstore"
            ? _sqlExpressionFactory.Function(
                "hstore_to_json_loose", [instance], true, TrueArrays[1], typeof(Dictionary<string, object?>),
                _typeMappingSource.FindMapping(typeof(Dictionary<string, object?>), "json"))
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? ToJsonbLoose(SqlExpression instance)
        => instance.TypeMapping?.StoreType is "hstore"
            ? _sqlExpressionFactory.Function(
                "hstore_to_jsonb_loose", [instance], true, TrueArrays[1], typeof(Dictionary<string, object?>),
                _typeMappingSource.FindMapping(typeof(Dictionary<string, object?>), "jsonb"))
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? ToHstore(SqlExpression instance)
        => instance.TypeMapping?.StoreType switch
        {
            "json" => ToHstoreFromJson(instance),
            "jsonb" => ToHstoreFromJsonb(instance),
            "hstore" => instance,
            _ => null
        };

    private SqlExpression? ToHstoreOrJsonb(SqlExpression instance)
        => instance.TypeMapping?.StoreType switch
        {
            "json" => ConvertToJsonb(instance),
            "jsonb" => instance,
            "hstore" => instance,
            _ => null
        };

    private SqlExpression? ToHstoreStringArrayStringOrJsonb(SqlExpression instance)
        => instance.Type == StringType
            ? instance
            : instance.TypeMapping?.StoreType switch
            {
                "json" => ConvertToJsonb(instance),
                "jsonb" => instance,
                "hstore" => instance,
                "text[]" => instance,
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
            ? _sqlExpressionFactory.ApplyTypeMapping(instance, _typeMappingSource.FindMapping(instance.Type, "jsonb"))
            : _sqlExpressionFactory.Convert(instance, instance.Type, _typeMappingSource.FindMapping(instance.Type, "jsonb"));

    private static bool IsDictionaryStore(SqlExpression expr)
        => expr.TypeMapping?.StoreType is "json" or "jsonb" or "hstore";

    private static bool IsDictionaryMethod(MethodInfo method)
        => IsDictionaryType(method.DeclaringType!);

    private static bool IsDictionaryType(Type type)
        => type.IsGenericType
            && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                || type.GetGenericTypeDefinition() == typeof(ImmutableDictionary<,>));

    private static Type GetDictionaryValueType(Type dictionaryType)
        => dictionaryType.GetGenericArguments()[1];

    private SqlExpression JsonObjectValueForKey(SqlExpression instance, SqlExpression key, bool jsonb)
    {
        var valueType = GetDictionaryValueType(instance.Type);
        if (valueType == StringType)
        {
            return _sqlExpressionFactory.MakePostgresBinary(
                PgExpressionType.JsonValueForKeyAsText, instance, key, _typeMappingSource.FindMapping(StringType, _model)!);
        }

        return _sqlExpressionFactory.JsonTraversal(
            instance, [key], false, valueType, _typeMappingSource.FindMapping(jsonb ? "jsonb" : "json"));
    }

#pragma warning disable EF1001 // SelectExpression constructors are currently internal

    private SqlExpression JsonObjectKeys(SqlExpression instance, string jsonObjectKeysFn)
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
                                    true, TrueArrays[1], StringType, _typeMappingSource.FindMapping(StringType, _model)!),
                                string.Empty)
                        ], false, [], null, null))
            ], true, TrueArrays[1], StringListType, _typeMappingSource.FindMapping(StringListType, _model)!);

    private ScalarSubqueryExpression JsonObjectValues(SqlExpression instance, bool jsonb, SqlExpression? predicate = null)
    {
        var valueType = GetDictionaryValueType(instance.Type);
        var jsonTypeMapping = _typeMappingSource.FindMapping(valueType, jsonb ? "jsonb" : "json");
        var isStringValue = valueType == StringType;
        var valueNeedsConversion = !isStringValue && valueType == BoolType || valueType.IsNumeric();
        var valueTypeMapping = isStringValue ? _typeMappingSource.FindMapping(StringType, _model)! :
            valueNeedsConversion ? _typeMappingSource.FindMapping(valueType, _model) :
            jsonTypeMapping;
        return new(
            new(
                null,
                [
                    new TableValuedFunctionExpression(
                        "j1", isStringValue ? (jsonb ? "jsonb_each_text" : "json_each_text") : (jsonb ? "jsonb_each" : "json_each"),
                        [instance])
                ],
                predicate, [], null,
                [
                    new(
                        _sqlExpressionFactory.Function(
                            "array_agg",
                            [
                                valueNeedsConversion
                                    ? _sqlExpressionFactory.Convert(
                                        new ColumnExpression(
                                            "value", "j1", valueType, jsonTypeMapping, false), valueType, valueTypeMapping)
                                    : new ColumnExpression(
                                        "value", "j1", valueType, valueTypeMapping, false)
                            ],
                            true, TrueArrays[1],
                            isStringValue ? StringListType : GenericListType.MakeGenericType(valueType),
                            isStringValue ? _typeMappingSource.FindMapping(StringListType, _model)! :
                            valueNeedsConversion ? _typeMappingSource.FindMapping(GenericListType.MakeGenericType(valueType), _model) :
                            _typeMappingSource.FindMapping(GenericListType.MakeGenericType(valueType), jsonb ? "jsonb[]" : "json[]")),
                        string.Empty)
                ], false, [], null, null));
    }

    private ScalarSubqueryExpression JsonObjectValuesForKeys(
        SqlExpression instance,
        SqlExpression keys,
        bool jsonb)
        => JsonObjectValues(
            instance, jsonb, _sqlExpressionFactory.Any(
                new ColumnExpression(
                    "key", "j1", StringType, _typeMappingSource.FindMapping(StringType, _model)!, false), keys, PgAnyOperatorType.Equal));

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
                                    "array_agg",
                                    [
                                        new ColumnExpression(
                                            "key", "j1", StringType, _typeMappingSource.FindMapping(StringType, _model)!, false)
                                    ],
                                    true, TrueArrays[1], StringListType, _typeMappingSource.FindMapping(StringListType, _model)!),
                                _sqlExpressionFactory.Function(
                                    "array_agg",
                                    [
                                        new ColumnExpression(
                                            "value", "j1", StringType, _typeMappingSource.FindMapping(StringType, _model)!, true)
                                    ],
                                    true, TrueArrays[1], StringListType, _typeMappingSource.FindMapping(StringListType, _model)!)
                            ],
                            true, TrueArrays[2], immutable ? ImmutableStringDictionaryType : StringDictionaryType,
                            immutable
                                ? _typeMappingSource.FindMapping(ImmutableStringDictionaryType, _model)!
                                : _typeMappingSource.FindMapping(StringDictionaryType, _model)!), string.Empty)
                ], false, [], null, null));
#pragma warning restore EF1001
}
