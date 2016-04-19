using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public static class NpgsqlDatabaseModelExtensions
    {
        public static NpgsqlColumnModelAnnotations Npgsql([NotNull] this ColumnModel column)
            => new NpgsqlColumnModelAnnotations(column);

        public static NpgsqlIndexModelAnnotations Npgsql([NotNull] this IndexModel index)
            => new NpgsqlIndexModelAnnotations(index);
    }
}
