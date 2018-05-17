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
using System.Net;
using JetBrains.Annotations;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides extension methods supporting PostgreSQL network address operator translation.
    /// </summary>
    public static class NpgsqlNetworkAddressExtensions
    {
        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> contains another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="ipAddress">The IP address to search.</param>
        /// <param name="other">The IP address to locate.</param>
        /// <returns>
        /// <value>true</value> if the <see cref="IPAddress"/> contains the other <see cref="IPAddress"/>; otherwise, <value>false</value>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Contains([CanBeNull] this DbFunctions _, [CanBeNull] IPAddress ipAddress, [CanBeNull] IPAddress other) => throw new NotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="NpgsqlInet"/> contains another <see cref="NpgsqlInet"/>.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="inet">The inet to search.</param>
        /// <param name="other">The inet to locate.</param>
        /// <returns>
        /// <value>true</value> if the <see cref="NpgsqlInet"/> contains the other <see cref="NpgsqlInet"/>; otherwise, <value>false</value>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool Contains([CanBeNull] this DbFunctions _, NpgsqlInet inet, NpgsqlInet other) => throw new NotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="IPAddress"/> contains or is equal to another <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="ipAddress">The IP address to search.</param>
        /// <param name="other">The IP address to locate.</param>
        /// <returns>
        /// <value>true</value> if the <see cref="IPAddress"/> contains or is equal to the other <see cref="IPAddress"/>; otherwise, <value>false</value>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainsOrEquals([CanBeNull] this DbFunctions _, [CanBeNull] IPAddress ipAddress, [CanBeNull] IPAddress other) => throw new NotSupportedException();

        /// <summary>
        /// Determines whether an <see cref="NpgsqlInet"/> contains or is equal to another <see cref="NpgsqlInet"/>.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="inet">The inet to search.</param>
        /// <param name="other">The inet to locate.</param>
        /// <returns>
        /// <value>true</value> if the <see cref="NpgsqlInet"/> contains or is equal to the other <see cref="NpgsqlInet"/>; otherwise, <value>false</value>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static bool ContainsOrEquals([CanBeNull] this DbFunctions _, NpgsqlInet inet, NpgsqlInet other) => throw new NotSupportedException();
    }
}
