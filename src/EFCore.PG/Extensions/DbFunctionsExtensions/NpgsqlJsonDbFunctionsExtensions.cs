using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides methods for supporting translation to PostgreSQL JSON operators and functions.
    /// </summary>
    public static class NpgsqlJsonDbFunctionsExtensions
    {
        /// <summary>
        /// Checks if <paramref name="json"/> contains <paramref name="contained"/> as top-level entries.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a <see cref="JsonDocument"/>, a string property mapped to JSON,
        /// or a user POCO mapped to JSON.
        /// </param>
        /// <param name="contained">
        /// A JSON column or value. Can be a <see cref="JsonDocument"/>, a string, or a user POCO mapped to JSON.
        /// </param>
        /// <remarks>
        /// This operation is only supported with PostgreSQL <c>jsonb</c>, not <c>json</c>.
        ///
        /// See https://www.postgresql.org/docs/current/functions-json.html.
        /// </remarks>
        public static bool JsonContains(
            [CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] object contained)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonContains)));

        /// <summary>
        /// Checks if <paramref name="contained"/> is contained in <paramref name="json"/> as top-level entries.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="contained">
        /// A JSON column or value. Can be a <see cref="JsonDocument"/>, a string, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="json">
        /// A JSON column or value. Can be a <see cref="JsonDocument"/>, a string, or a user POCO mapped to JSON.
        /// </param>
        /// <remarks>
        /// This operation is only supported with PostgreSQL <c>jsonb</c>, not <c>json</c>.
        ///
        /// See https://www.postgresql.org/docs/current/functions-json.html.
        /// </remarks>
        public static bool JsonContained(
            [CanBeNull] this DbFunctions _, [NotNull] object contained, [NotNull] object json)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonContained)));

        /// <summary>
        /// Checks if <paramref name="key"/> exists as a top-level key within <paramref name="json"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a <see cref="JsonDocument"/>, a string, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="key">A key to be checked inside <paramref name="json"/>.</param>
        /// <remarks>
        /// This operation is only supported with PostgreSQL <c>jsonb</c>, not <c>json</c>.
        ///
        /// See https://www.postgresql.org/docs/current/functions-json.html.
        /// </remarks>
        public static bool JsonExists([CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] string key)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonExists)));

        /// <summary>
        /// Checks if any of the given <paramref name="keys"/> exist as top-level keys within <paramref name="json"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a <see cref="JsonDocument"/>, a string, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="keys">A set of keys to be checked inside <paramref name="json"/>.</param>
        /// <remarks>
        /// This operation is only supported with PostgreSQL <c>jsonb</c>, not <c>json</c>.
        ///
        /// See https://www.postgresql.org/docs/current/functions-json.html.
        /// </remarks>
        public static bool JsonExistAny(
            [CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] params string[] keys)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonExistAny)));

        /// <summary>
        /// Checks if all of the given <paramref name="keys"/> exist as top-level keys within <paramref name="json"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a <see cref="JsonDocument"/>, a string, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="keys">A set of keys to be checked inside <paramref name="json"/>.</param>
        /// <remarks>
        /// This operation is only supported with PostgreSQL <c>jsonb</c>, not <c>json</c>.
        ///
        /// See https://www.postgresql.org/docs/current/functions-json.html.
        /// </remarks>
        public static bool JsonExistAll(
            [CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] params string[] keys)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonExistAll)));

        /// <summary>
        /// Returns the type of the outermost JSON value as a text string.
        /// Possible types are object, array, string, number, boolean, and null.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a <see cref="JsonDocument"/>, a string, or a user POCO mapped to JSON.
        /// </param>
        /// <remarks>
        /// See https://www.postgresql.org/docs/current/functions-json.html.
        /// </remarks>
        public static string JsonTypeof([CanBeNull] this DbFunctions _, [NotNull] object json)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonTypeof)));
    }
}
