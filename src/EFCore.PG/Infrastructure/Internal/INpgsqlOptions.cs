using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal
{
    public interface INpgsqlOptions : ISingletonOptions
    {
        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull]
        Version PostgresVersion { get; }

        /// <summary>
        /// True if reverse null ordering is enabled; otherwise, false.
        /// </summary>
        bool ReverseNullOrderingEnabled { get; }

        IReadOnlyList<NpgsqlEntityFrameworkPlugin> Plugins { get; }
    }
}
