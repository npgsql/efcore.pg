// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Npgsql specific extension methods for <see cref="IndexBuilder" />.
    /// </summary>
    public static class NpgsqlIndexBuilderExtensions
    {
        /// <summary>
        ///     Configures the name of the index in the database when targeting Npgsql.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForNpgsqlHasName([NotNull] this IndexBuilder indexBuilder, [CanBeNull] string name)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            indexBuilder.Metadata.Npgsql().Name = name;

            return indexBuilder;
        }

        /// <summary>
        /// Determines whether the specified index has filter expression when targeting PostgreSQL.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="filterExpression"> The filter expression for the index. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForNpgsqlHasFilter([NotNull] this IndexBuilder indexBuilder,
            [CanBeNull] string filterExpression)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(filterExpression, nameof(filterExpression));

            indexBuilder.Metadata.Npgsql().Filter = filterExpression;

            return indexBuilder;
        }


        /// <summary>
        /// The PostgreSQL index method to be used. Null selects the default (currently btree).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="method"> The name of the index. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForNpgsqlHasMethod([NotNull] this IndexBuilder indexBuilder, [CanBeNull] string method)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(method, nameof(method));

            indexBuilder.Metadata.Npgsql().Method = method;

            return indexBuilder;
        }
    }
}
