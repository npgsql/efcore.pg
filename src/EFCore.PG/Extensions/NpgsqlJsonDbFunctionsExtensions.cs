using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Extensions
{
    /// <summary>
    /// Provides methods for <see cref="JsonElement"/> supporting translation to PostgreSQL JSON operators and functions.
    /// </summary>
    public static class NpgsqlJsonDbFunctionsExtensions
    {
        /// <summary>
        /// Checks if <paramref name="left"/> contains <paramref name="right"/> as top-level entries.
        /// </summary>
        public static bool JsonContains(this DbFunctions _, JsonElement left, JsonElement right)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Checks if <paramref name="left"/> contains <paramref name="right"/> as top-level entries.
        /// </summary>
        public static bool JsonContains(this DbFunctions _, JsonElement left, string right)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Checks if <paramref name="left"/> is contained in <paramref name="right"/> as top-level entries.
        /// </summary>
        public static bool JsonContained(this DbFunctions _, JsonElement left, JsonElement right)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Checks if <paramref name="left"/> is contained in <paramref name="right"/> as top-level entries.
        /// </summary>
        public static bool JsonContained(this DbFunctions _, string left, JsonElement right)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Checks if <paramref name="key"/> exists as a top-level key within <paramref name="element"/>.
        /// </summary>
        public static bool JsonExists(this DbFunctions _, JsonElement element, string key)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Checks if any of the given <paramref name="keys"/> exist as top-level keys within <paramref name="element"/>.
        /// </summary>
        public static bool JsonExistAny(this DbFunctions _, JsonElement element, string[] keys)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Checks if all of the given <paramref name="keys"/> exist as top-level keys within <paramref name="element"/>.
        /// </summary>
        public static bool JsonExistAll(this DbFunctions _, JsonElement element, string[] keys)
            => throw ClientEvaluationNotSupportedException();

        static NotSupportedException ClientEvaluationNotSupportedException([CallerMemberName] string method = default)
            => new NotSupportedException($"{method} is only intended for use via SQL translation as part of an EF Core LINQ query.");
    }
}
