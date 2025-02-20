// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides extension methods supporting `Dictionary&lt;TKey,TValue>` and `ImmutableDictionary&lt;TKey,TValue>` function translation for PostgreSQL for the `hstore`, `json` and `jsonb` store types.
/// </summary>
public static class NpgsqlDictionaryDbFunctionsExtensions
{
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
    public static T Remove<T, TValue>(this DbFunctions _, T input, string key)
        where T : IEnumerable<KeyValuePair<string, TValue>>
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Remove)));

    /// <summary>
    /// Converts string dictionary to an array of alternating keys and values.
    /// <br/>
    /// HStore SQL translation: hstore_to_array(input)
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input hstore.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/hstore.html#HSTORE-OPS-FUNCS">PostgreSQL documentation for 'hstore' functions.</seealso>
    public static List<string> ToKeyValueList(this DbFunctions _, IEnumerable<KeyValuePair<string, string>> input)
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
    public static Dictionary<string, TValue> ToJson<TValue>(this DbFunctions _, IEnumerable<KeyValuePair<string, TValue>> input)
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
    public static Dictionary<string, TValue> ToJsonb<TValue>(this DbFunctions _, IEnumerable<KeyValuePair<string, TValue>> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToJsonb)));
}
