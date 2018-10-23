namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal
{
    public static class CockroachDbAnnotationNames
    {
        public const string Prefix = NpgsqlAnnotationNames.Prefix + "CockroachDB:";

        public const string InterleaveInParent = Prefix + "InterleaveInParent";
    }
}
