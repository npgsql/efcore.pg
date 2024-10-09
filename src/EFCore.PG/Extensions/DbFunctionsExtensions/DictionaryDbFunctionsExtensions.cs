// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides extension methods supporting `Dictionary&lt;TKey,TValue>` and `ImmutableDictionary&lt;TKey,TValue>` function translation for PostgreSQL for the `hstore`, `json` and `jsonb` store types.
/// </summary>
public static class DictionaryDbFunctionsExtensions
{
    /// <summary>
    /// Returns string values associated with given string keys, or NULL if not present.
    ///
    /// Works with `hstore`, `json` and `jsonb` type columns
    /// <br/>
    /// SQL translation: input -> keys
    ///
    /// Note: input will automatically translated to `hstore` if `jsonb` or `json`.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input dictionary.</param>
    /// <param name="keys">The keys to return the values of from the input hstore.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static List<string> ValuesForKeys(this DbFunctions _, IEnumerable<KeyValuePair<string, string>> input, IList<string> keys)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ValuesForKeys)));

    /// <summary>
    /// Does left operand contain right?
    ///
    /// Works with `hstore`, `json` and `jsonb` type columns
    /// <br/>
    /// SQL translation: left @> right
    ///
    /// Note: for `json` type columns, they will be automatically translated to `jsonb` or `hstore` which requires PostgreSQL 9.3 and does not support indexing
    /// Note: for `jsonb` columns being compared to `hstore`, they will be converted to `hstore` which does not support indexing
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="left">The input left dictionary.</param>
    /// <param name="right">The input right dictionary.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static bool Contains<TKey, TValue>(this DbFunctions _, IEnumerable<KeyValuePair<TKey, TValue>> left, IEnumerable<KeyValuePair<TKey, TValue>> right)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    /// Is left operand contained by right?
    /// <br/>
    /// SQL translation: left &lt;@ right
    ///
    /// Note: for `json` type columns, they will be automatically translated to `jsonb` or `hstore` which requires PostgreSQL 9.3 and does not support indexing
    /// Note: for `jsonb` columns being compared to `hstore`, they will be converted to `hstore` which does not support indexing
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="left">The input left dictionary.</param>
    /// <param name="right">The input right dictionary.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static bool ContainedBy<TKey, TValue>(this DbFunctions _, IEnumerable<KeyValuePair<TKey, TValue>> left, IEnumerable<KeyValuePair<TKey, TValue>> right)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainedBy)));

    /// <summary>
    /// Deletes keys from input operand Dictionary. Returns the same store type as the provided input for `hstore` and `jsonb` columns.
    ///
    /// Works with `hstore`, `json` and `jsonb` type columns
    /// <br/>
    /// SQL translation: input - key
    ///
    /// Note: for `json` type columns, input will be cast to `jsonb` and will output `jsonb` which requires PostgreSQL 9.3
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input dictionary.</param>
    /// <param name="key">The key to remove.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static T Remove<T, TKey, TValue>(this DbFunctions _, T input, TKey key)
        where T : IEnumerable<KeyValuePair<TKey, TValue>>
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Remove)));

    /// <summary>
    /// Extracts a subset of a string dictionary containing only the specified keys.
    ///
    /// For `jsonb` and `json` input they will be automatically converted to `hstore` but
    /// the result will always be an `hstore` type
    /// <br/>
    /// SQL translation: slice(input, keys)
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input hstore.</param>
    /// <param name="keys">The keys to extract the subset of.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static T Slice<T>(this DbFunctions _, T input, IList<string> keys)
        where T : IEnumerable<KeyValuePair<string, string>>
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Slice)));

    /// <summary>
    /// Converts dictionary that has keys and values of the same type to an array of alternating keys and values.
    /// <br/>
    /// HStore SQL translation: hstore_to_array(input)
    /// Json SQL translation: SELECT unnest(array_agg(ARRAY[key, value])) FROM json_each_text(input)
    /// Jsonb SQL translation: SELECT unnest(array_agg(ARRAY[key, value])) FROM jsonb_each_text(input)
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input hstore.</param>
    /// <typeparam name="TKeyAndValue">The key and value type</typeparam>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static List<TKeyAndValue> ToKeyValueList<TKeyAndValue>(this DbFunctions _, IEnumerable<KeyValuePair<TKeyAndValue, TKeyAndValue>> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToKeyValueList)));

    /// <summary>
    /// Constructs an hstore `Dictionary&lt;string, string>` from a key/value pair string array
    /// <br/>
    /// SQL translation: hstore(input)
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input string array of key value pairs.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static Dictionary<string, string> DictionaryFromKeyValueList(this DbFunctions _, IList<string> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DictionaryFromKeyValueList)));

    /// <summary>
    /// Constructs an hstore `Dictionary&lt;string, string>` from a string array of keys and a string array of values
    /// <br/>
    /// SQL translation: hstore(keys, values)
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="keys">The input string array of keys.</param>
    /// <param name="values">The input string array of values.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static Dictionary<string, string> DictionaryFromKeysAndValues(this DbFunctions _, IList<string> keys, IList<string> values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DictionaryFromKeysAndValues)));

    /// <summary>
    /// Converts an `jsonb` or `hstore` type value to a `jsonb` type `Dictionary&lt;TKey, TKey>`
    /// <br/>
    ///
    /// Hstore SQL translation: hstore_to_json(input)
    /// Json SQL translation: input
    /// Jsonb SQL translation: input::json
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input hstore.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static Dictionary<TKey, TValue> ToJson<TKey, TValue>(this DbFunctions _, IEnumerable<KeyValuePair<TKey, TValue>> input)
        where TKey : notnull
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToJson)));

    /// <summary>
    /// Converts an `hstore` or `json` type value to a `jsonb` type `Dictionary&lt;TKey, TValue>`
    ///
    /// Can be used during a migration of changing a column's StoreType from 'hstore' to 'jsonb' with the `Using` clause
    /// <br/>
    /// HStore SQL translation: hstore_to_jsonb(input)
    /// Json SQL translation: input::jsonb
    /// Jsonb SQL translation: input
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input hstore.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static Dictionary<TKey, TValue> ToJsonb<TKey, TValue>(this DbFunctions _, IEnumerable<KeyValuePair<TKey, TValue>> input)
        where TKey : notnull
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToJsonb)));


    /// <summary>
    /// Converts a `json` or `jsonb` type `IEnumerable&lt;KeyValuePair&lt;string, string>>` (i.e. Dictionary&lt;string, string>> or related type) to an hstore type `Dictionary&lt;string, string>>`
    /// <br/>
    /// Can be used during a migration of changing a column's StoreType from 'json' to 'hstore' with the `Using` clause
    ///
    /// HStore SQL translation: input
    /// Json SQL translation: select hstore(array_agg(key), array_agg(value)) FROM json_each_text(input)
    /// Json SQL translation: select hstore(array_agg(key), array_agg(value)) FROM jsonb_each_text(input)
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input hstore.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static Dictionary<string, string> ToHstore(this DbFunctions _, IEnumerable<KeyValuePair<string, string>> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToHstore)));

    /// <summary>
    /// Converts an `hstore` to a `json` value type `Dictionary&lt;string, object?>`, but attempts to distinguish numerical and boolean values so they are unquoted in the JSON.
    /// <br/>
    /// SQL translation: hstore_to_json_loose(input)
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input hstore.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static Dictionary<string, object?> ToJsonLoose(this DbFunctions _, IEnumerable<KeyValuePair<string, string>> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToJsonLoose)));

    /// <summary>
    /// Converts an `hstore` to a `jsonb` value type `Dictionary&lt;string, object?>`, but attempts to distinguish numerical and boolean values so they are unquoted in the JSON.
    /// <br/>
    /// SQL translation: hstore_to_jsonb_loose(input)
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input hstore.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static Dictionary<string, object?> ToJsonbLoose(this DbFunctions _, IEnumerable<KeyValuePair<string, string>> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToJsonbLoose)));
}
