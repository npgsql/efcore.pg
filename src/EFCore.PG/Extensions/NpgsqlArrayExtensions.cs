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
using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides extension methods for <see cref="List{T}"/> supporting PostgreSQL translation.
    /// </summary>
    public static class NpgsqlArrayExtensions
    {
        /// <summary>
        /// Determines whether a range contains a specified value.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="array">The list to conver to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="array"/>.</typeparam>
        /// <returns>
        /// <value>true</value> if the range contains the specified value; otherwise, <value>false</value>.
        /// </returns>
        public static string ArrayToString<T>([CanBeNull] this DbFunctions _, [NotNull] T[] array, [CanBeNull] string delimiter)
            => throw new NotSupportedException();

        /// <summary>
        /// Determines whether a range contains a specified value.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="array">The list to conver to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <param name="nullString">The value used to represent a null value.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="array"/>.</typeparam>
        /// <returns>
        /// <value>true</value> if the range contains the specified value; otherwise, <value>false</value>.
        /// </returns>
        public static string ArrayToString<T>([CanBeNull] this DbFunctions _, [NotNull] T[] array, [CanBeNull] string delimiter, [CanBeNull] string nullString)
            => throw new NotSupportedException();

        /// <summary>
        /// Determines whether a range contains a specified value.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="list">The list to conver to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="list"/>.</typeparam>
        /// <returns>
        /// <value>true</value> if the range contains the specified value; otherwise, <value>false</value>.
        /// </returns>
        public static string ArrayToString<T>([CanBeNull] this DbFunctions _, [NotNull] List<T> list, [CanBeNull] string delimiter)
            => throw new NotSupportedException();

        /// <summary>
        /// Determines whether a range contains a specified value.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="list">The list to conver to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <param name="nullString">The value used to represent a null value.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="list"/>.</typeparam>
        /// <returns>
        /// <value>true</value> if the range contains the specified value; otherwise, <value>false</value>.
        /// </returns>
        public static string ArrayToString<T>([CanBeNull] this DbFunctions _, [NotNull] List<T> list, [CanBeNull] string delimiter, [CanBeNull] string nullString)
            => throw new NotSupportedException();
    }
}
