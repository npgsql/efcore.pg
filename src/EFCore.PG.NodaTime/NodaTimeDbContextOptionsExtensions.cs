#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System;
using JetBrains.Annotations;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides extension methods for the NodaTime plugin for the Npgsql Entity Framework Core provider.
    /// </summary>
    public static class NodaTimeDbContextOptionsExtensions
    {
        /// <summary>
        /// Registers the NodaTime plugin for the Npgsql Entity Framework Core provider.
        /// </summary>
        /// <param name="optionsBuilder">The options builder.</param>
        /// <returns>
        /// The options builder configured with the NodaTime plugin.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="optionsBuilder"/></exception>
        [NotNull]
        public static NpgsqlDbContextOptionsBuilder UseNodaTime([NotNull] this NpgsqlDbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            // TODO: Global-only setup at the ADO.NET level for now, optionally allow per-connection?
            NpgsqlConnection.GlobalTypeMapper.UseNodaTime();

            optionsBuilder.UsePlugin(new NodaTimePlugin());

            return optionsBuilder;
        }
    }
}
