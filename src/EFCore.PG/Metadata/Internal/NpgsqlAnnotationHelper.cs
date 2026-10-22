using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

internal static class NpgsqlAnnotationHelper
{
    internal static bool IsRelationalModelAnnotation(IAnnotation annotation)
        => annotation.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal)
            || annotation.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal)
            || annotation.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal)
            || annotation.Name.StartsWith(NpgsqlAnnotationNames.CollationDefinitionPrefix, StringComparison.Ordinal);

    internal static IEnumerable<IAnnotation> GetRelationalModelAnnotations(IModel model)
    {
        var hasCollations = false;
        foreach (var annotation in model.GetAnnotations().Where(IsRelationalModelAnnotation))
        {
            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.CollationDefinitionPrefix, StringComparison.Ordinal))
            {
                var collation = new PostgresCollation(model, annotation.Name);
                yield return new Annotation(
                    PostgresCollation.BuildAnnotationName(collation.Schema ?? model.GetDefaultSchema(), collation.Name),
                    annotation.Value);
                hasCollations = true;
            }
            else
            {
                yield return annotation;
            }
        }

        if (hasCollations)
        {
            // A resolved null schema follows the search path; legacy migration annotations instead fall back to the target model.
            yield return new Annotation(NpgsqlAnnotationNames.CollationSchemasResolved, true);
        }
    }
}
