namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Options for modifying sort ordering of <c>NULL</c>-values in indexes.
    /// </summary>
    public enum NullSortOrder
    {
        /// <summary>
        /// Represents an unspecified sort order. The database default will be used.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Specifies that nulls sort before non-nulls.
        /// </summary>
        /// <remarks>
        /// This is the default when <see cref="SortOrder.Descending"/> is specified.
        /// </remarks>
        NullsFirst = 1,

        /// <summary>
        /// Specifies that nulls sort after non-nulls.
        /// </summary>
        /// <remarks>
        /// This is the default when <see cref="SortOrder.Descending"/> is not specified.
        /// </remarks>
        NullsLast = 2,
    }
}
