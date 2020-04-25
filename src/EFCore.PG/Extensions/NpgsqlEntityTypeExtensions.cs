using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Extension methods for <see cref="IEntityType" /> for Npgsql-specific metadata.
    /// </summary>
    public static class NpgsqlEntityTypeExtensions
    {
        #region Storage parameters

        public static Dictionary<string, object> GetStorageParameters([NotNull] this IEntityType entityType)
            => entityType.GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix))
                .ToDictionary(
                    a => a.Name.Substring(NpgsqlAnnotationNames.StorageParameterPrefix.Length),
                    a => a.Value
                );

        public static string GetStorageParameter([NotNull] this IEntityType entityType, [NotNull] string parameterName)
        {
            Check.NotEmpty(parameterName, nameof(parameterName));

            return (string)entityType[NpgsqlAnnotationNames.StorageParameterPrefix + parameterName];
        }

        public static void SetStorageParameter(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] string parameterName,
            [CanBeNull] object parameterValue)
        {
            Check.NotEmpty(parameterName, nameof(parameterName));

            entityType.SetOrRemoveAnnotation(NpgsqlAnnotationNames.StorageParameterPrefix + parameterName, parameterValue);
        }

        public static object SetStorageParameter(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] string parameterName,
            [CanBeNull] object parameterValue,
            bool fromDataAnnotation = false)
        {
            Check.NotEmpty(parameterName, nameof(parameterName));

            entityType.SetOrRemoveAnnotation(NpgsqlAnnotationNames.StorageParameterPrefix + parameterName, parameterValue, fromDataAnnotation);

            return parameterName;
        }

        public static ConfigurationSource? GetStorageParameterConfigurationSource(
            [NotNull] this IConventionEntityType index,
            [NotNull] string parameterName)
        {
            Check.NotEmpty(parameterName, nameof(parameterName));

            return index.FindAnnotation(NpgsqlAnnotationNames.StorageParameterPrefix + parameterName)?.GetConfigurationSource();
        }

        #endregion Storage parameters

        #region Unlogged

        public static bool GetIsUnlogged([NotNull] this IEntityType entityType)
            => entityType[NpgsqlAnnotationNames.UnloggedTable] as bool? ?? false;

        public static void SetIsUnlogged([NotNull] this IMutableEntityType entityType, bool unlogged)
            => entityType.SetOrRemoveAnnotation(NpgsqlAnnotationNames.UnloggedTable, unlogged);

        public static bool SetIsUnlogged(
            [NotNull] this IConventionEntityType entityType,
            bool unlogged,
            bool fromDataAnnotation = false)
        {
            entityType.SetOrRemoveAnnotation(NpgsqlAnnotationNames.UnloggedTable, unlogged, fromDataAnnotation);

            return unlogged;
        }

        public static ConfigurationSource? GetIsUnloggedConfigurationSource([NotNull] this IConventionEntityType index)
            => index.FindAnnotation(NpgsqlAnnotationNames.UnloggedTable)?.GetConfigurationSource();

        #endregion Unlogged

        #region CockroachDb interleave in parent

        public static CockroachDbInterleaveInParent GetCockroachDbInterleaveInParent([NotNull] this IEntityType entityType)
            => new CockroachDbInterleaveInParent(entityType);

        #endregion CockroachDb interleave in parent
    }
}
