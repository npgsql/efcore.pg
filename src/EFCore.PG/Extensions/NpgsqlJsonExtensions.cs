using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides extension methods for JSON operators supporting PostgreSQL translation.
    /// </summary>
    public static class NpgsqlJsonExtensions
    {
        /// <summary>
        /// Determines whether a JSON object contains a specified key.
        /// </summary>
        /// <param name="propertyName">The database column being searched in.</param>
        /// <param name="jsonKey">The key to locate in json.</param>
        /// <returns>
        /// <value>true</value> if the json object contains the specified key; otherwise, <value>false</value>.
        /// </returns>
        /// <exception cref="NotSupportedException">{method} is only intended for use via SQL translation as part of an EF Core LINQ query.</exception>
        public static bool KeyExists(string propertyName, string jsonKey) => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Gets all values related to a specific json key.
        /// </summary>
        /// <param name="propertyName">The database column being searched in.</param>
        /// <param name="jsonKey">The key to locate in json.</param>
        /// <returns>
        /// <value>string value</value> return value associated with json key as string.
        /// </returns>
        /// <exception cref="NotSupportedException">{method} is only intended for use via SQL translation as part of an EF Core LINQ query.</exception>
        public static string GetValue(string propertyName, string jsonKey) => throw ClientEvaluationNotSupportedException();

        #region Utilities

        /// <summary>
        /// Helper method to throw a <see cref="NotSupportedException"/> with the name of the throwing method.
        /// </summary>
        /// <param name="method">The method that throws the exception.</param>
        /// <returns>
        /// A <see cref="NotSupportedException"/>.
        /// </returns>
        [NotNull]
        static NotSupportedException ClientEvaluationNotSupportedException([CallerMemberName] string method = default)
            => new NotSupportedException($"{method} is only intended for use via SQL translation as part of an EF Core LINQ query.");

        #endregion
    }
}
