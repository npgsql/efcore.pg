using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

internal static class SortOrderHelper
{
    public static bool IsDefaultNullSortOrder(
        IReadOnlyList<NullSortOrder>? nullSortOrders,
        IReadOnlyList<bool>? isDescendingValues)
    {
        if (nullSortOrders is null)
        {
            return true;
        }

        for (var i = 0; i < nullSortOrders.Count; i++)
        {
            var nullSortOrder = nullSortOrders[i];

            // We need to consider the ASC/DESC sort order to determine the default NULLS FIRST/LAST sort order.
            if (isDescendingValues is not null && (isDescendingValues.Count == 0 || isDescendingValues[i]))
            {
                // NULLS FIRST is the default when DESC is specified.
                if (nullSortOrder != NullSortOrder.NullsFirst)
                {
                    return false;
                }
            }
            else
            {
                // NULLS LAST is the default when DESC is NOT specified.
                if (nullSortOrder != NullSortOrder.NullsLast)
                {
                    return false;
                }
            }
        }

        return true;
    }
}