using System.Linq;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Utilities
{
    static class SortOrderHelper
    {
        public static bool IsDefaultSortOrder([CanBeNull] SortOrder[] sortOrders)
        {
            return sortOrders == null ? true : sortOrders.All(sortOrder => sortOrder == SortOrder.Ascending);
        }

        public static bool IsDefaultNullSortOrder([CanBeNull] NullSortOrder[] nullSortOrders, [CanBeNull] SortOrder[] sortOrders)
        {
            if (nullSortOrders == null)
            {
                return true;
            }

            for (var i = 0; i < nullSortOrders.Length; i++)
            {
                var nullSortOrder = nullSortOrders[i];

                // We need to consider the ASC/DESC sort order to determine the default NULLS FIRST/LAST sort order.
                var sortOrder = i < sortOrders?.Length ? sortOrders[i] : SortOrder.Ascending;

                if (sortOrder == SortOrder.Descending)
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
}
