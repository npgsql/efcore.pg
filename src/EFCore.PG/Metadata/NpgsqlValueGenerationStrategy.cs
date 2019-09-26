namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public enum NpgsqlValueGenerationStrategy
    {
        /// <summary>
        /// No Npgsql-specific strategy.
        /// </summary>
        None,

        /// <summary>
        /// <para>
        /// A sequence-based hi-lo pattern where blocks of IDs are allocated from the server and
        /// used client-side for generating keys.
        /// </para>
        /// <para>
        /// This is an advanced pattern--only use this strategy if you are certain it is what you need.
        /// </para>
        /// </summary>
        SequenceHiLo,

        /// <summary>
        /// <para>
        /// Selects the serial column strategy, which is a regular column backed by an auto-created index.
        /// </para>
        /// <para>
        /// If you are creating a new project on PostgreSQL 10 or above, consider using <see cref="IdentityByDefaultColumn"/> instead.
        /// </para>
        /// </summary>
        SerialColumn,

        /// <summary>
        /// <para>Selects the always-identity column strategy (a value cannot be provided).</para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        IdentityAlwaysColumn,

        /// <summary>
        /// <para>Selects the by-default-identity column strategy (a value can be provided to override the identity mechanism).</para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        IdentityByDefaultColumn,
    }

    public static class NpgsqlValueGenerationStrategyExtensions
    {
        public static bool IsIdentity(this NpgsqlValueGenerationStrategy strategy)
            => strategy == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn ||
               strategy == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn;

        public static bool IsIdentity(this NpgsqlValueGenerationStrategy? strategy)
            => strategy == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn ||
               strategy == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn;
    }
}
