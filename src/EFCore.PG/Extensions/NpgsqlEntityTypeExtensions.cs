using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Extension methods for <see cref="IEntityType" /> for Npgsql-specific metadata.
    /// </summary>
    public static class NpgsqlEntityTypeExtensions
    {
        public static Dictionary<string, object> GetStorageParameters([NotNull] this IEntityType entityType)
            => entityType.GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix))
                .ToDictionary(
                    a => a.Name.Substring(NpgsqlAnnotationNames.StorageParameterPrefix.Length),
                    a => a.Value
                );

        public static string GetStorageParameter([NotNull] this IEntityType entityType, string parameterName)
            => (string)entityType[NpgsqlAnnotationNames.StorageParameterPrefix + parameterName];

        public static void SetStorageParameter([NotNull] this IMutableEntityType entityType, string parameterName, object parameterValue)
            => entityType.SetOrRemoveAnnotation(NpgsqlAnnotationNames.StorageParameterPrefix + parameterName, parameterValue);

        public static void SetStorageParameter([NotNull] this IConventionEntityType entityType, string parameterName, object parameterValue, bool fromDataAnnotation = false)
            => entityType.SetOrRemoveAnnotation(NpgsqlAnnotationNames.StorageParameterPrefix + parameterName, parameterValue, fromDataAnnotation);

        public static bool GetIsUnlogged([NotNull] this IEntityType entityType)
            => entityType[NpgsqlAnnotationNames.UnloggedTable] as bool? ?? false;

        public static void SetIsUnlogged([NotNull] this IMutableEntityType entityType, bool isUnlogged)
            => entityType.SetOrRemoveAnnotation(NpgsqlAnnotationNames.UnloggedTable, isUnlogged);

        public static void SetIsUnlogged([NotNull] this IConventionEntityType entityType, bool isUnlogged, bool fromDataAnnotation = false)
            => entityType.SetOrRemoveAnnotation(NpgsqlAnnotationNames.UnloggedTable, isUnlogged, fromDataAnnotation);

        public static CockroachDbInterleaveInParent GetCockroachDbInterleaveInParent([NotNull] this IEntityType entityType)
            => new CockroachDbInterleaveInParent(entityType);
    }
}
