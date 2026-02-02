namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

internal static class NpgsqlAnnotationHelper
{
    internal static bool IsRelationalModelAnnotation(IAnnotation annotation)
        => annotation.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal)
            || annotation.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal)
            || annotation.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal)
            || annotation.Name.StartsWith(NpgsqlAnnotationNames.CollationDefinitionPrefix, StringComparison.Ordinal);
}
