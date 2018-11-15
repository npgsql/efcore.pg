namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Options for modifying sort ordering of index values.
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// Specifies ascending sort order, which is the default.
        /// </summary>
        Ascending = 0,

        /// <summary>
        /// Specifies descending sort order.
        /// </summary>
        Descending = 1,
    }
}
