using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class NpgsqlIndexExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool AreCompatibleForNpgsql(
        this IReadOnlyIndex index,
        IReadOnlyIndex duplicateIndex,
        in StoreObjectIdentifier storeObject,
        bool shouldThrow)
    {
        if (index.GetIncludeProperties() != duplicateIndex.GetIncludeProperties())
        {
            if (index.GetIncludeProperties() is null
                || duplicateIndex.GetIncludeProperties() is null
                || !SameColumnNames(index, duplicateIndex, storeObject))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        NpgsqlStrings.DuplicateIndexIncludedMismatch(
                            index.Properties.Format(),
                            index.DeclaringEntityType.DisplayName(),
                            duplicateIndex.Properties.Format(),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            index.GetDatabaseName(storeObject),
                            FormatInclude(index, storeObject),
                            FormatInclude(duplicateIndex, storeObject)));
                }

                return false;
            }
        }

        if (index.IsCreatedConcurrently() != duplicateIndex.IsCreatedConcurrently())
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    NpgsqlStrings.DuplicateIndexConcurrentCreationMismatch(
                        index.Properties.Format(),
                        index.DeclaringEntityType.DisplayName(),
                        duplicateIndex.Properties.Format(),
                        duplicateIndex.DeclaringEntityType.DisplayName(),
                        index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        index.GetDatabaseName(storeObject)));
            }

            return false;
        }

        if (index.GetCollation() != duplicateIndex.GetCollation())
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    NpgsqlStrings.DuplicateIndexCollationMismatch(
                        index.Properties.Format(),
                        index.DeclaringEntityType.DisplayName(),
                        duplicateIndex.Properties.Format(),
                        duplicateIndex.DeclaringEntityType.DisplayName(),
                        index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                        index.GetDatabaseName(storeObject)));
            }

            return false;
        }

        return true;

        static bool SameColumnNames(IReadOnlyIndex index, IReadOnlyIndex duplicateIndex, StoreObjectIdentifier storeObject)
            => index.GetIncludeProperties()!.Select(
                    p => index.DeclaringEntityType.FindProperty(p)!.GetColumnName(storeObject))
                .SequenceEqual(
                    duplicateIndex.GetIncludeProperties()!.Select(
                        p => duplicateIndex.DeclaringEntityType.FindProperty(p)!.GetColumnName(storeObject)));
    }

    private static string FormatInclude(IReadOnlyIndex index, StoreObjectIdentifier storeObject)
        => index.GetIncludeProperties() is null
            ? "{}"
            : "{'"
            + string.Join(
                "', '",
                index.GetIncludeProperties()!.Select(
                    p => index.DeclaringEntityType.FindProperty(p)
                        ?.GetColumnName(storeObject)))
            + "'}";
}