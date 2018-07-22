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
using System.Runtime.CompilerServices;
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
        /// Concatenates elements using the supplied delimiter.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="array">The list to convert to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="array"/>.</typeparam>
        /// <returns>
        /// The string concatenation of the elements with the supplied delimiter.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static string ArrayToString<T>([CanBeNull] this DbFunctions _, [NotNull] T[] array, [CanBeNull] string delimiter)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Concatenates elements using the supplied delimiter.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="list">The list to convert to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="list"/>.</typeparam>
        /// <returns>
        /// The string concatenation of the elements with the supplied delimiter.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static string ArrayToString<T>([CanBeNull] this DbFunctions _, [NotNull] List<T> list, [CanBeNull] string delimiter)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Concatenates elements using the supplied delimiter and the string representation for null elements.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="array">The list to convert to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <param name="nullString">The value used to represent a null value.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="array"/>.</typeparam>
        /// <returns>
        /// The string concatenation of the elements with the supplied delimiter and null string.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static string ArrayToString<T>([CanBeNull] this DbFunctions _, [NotNull] T[] array, [CanBeNull] string delimiter, [CanBeNull] string nullString)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Concatenates elements using the supplied delimiter and the string representation for null elements.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="list">The list to convert to a string in which to locate the value.</param>
        /// <param name="delimiter">The value used to delimit the elements.</param>
        /// <param name="nullString">The value used to represent a null value.</param>
        /// <typeparam name="T">The type of the elements of <paramref name="list"/>.</typeparam>
        /// <returns>
        /// The string concatenation of the elements with the supplied delimiter and null string.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static string ArrayToString<T>([CanBeNull] this DbFunctions _, [NotNull] List<T> list, [CanBeNull] string delimiter, [CanBeNull] string nullString)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Converts the input string into an array using the supplied delimiter and the string representation for null elements.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="input">The input string of delimited values.</param>
        /// <param name="delimiter">The value that delimits the elements.</param>
        /// <param name="nullString">The value that represents a null value.</param>
        /// <typeparam name="T">The type of the elements in the resulting array.</typeparam>
        /// <returns>
        /// The array resulting from splitting the input string based on the supplied delimiter and null string.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static T[] StringToArray<T>([CanBeNull] this DbFunctions _, [NotNull] string input, [CanBeNull] string delimiter, [CanBeNull] string nullString)
            => throw ClientEvaluationNotSupportedException();

        /// <summary>
        /// Converts the input string into a <see cref="List{T}"/> using the supplied delimiter and the string representation for null elements.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="input">The input string of delimited values.</param>
        /// <param name="delimiter">The value that delimits the elements.</param>
        /// <param name="nullString">The value that represents a null value.</param>
        /// <typeparam name="T">The type of the elements in the resulting array.</typeparam>
        /// <returns>
        /// The list resulting from splitting the input string based on the supplied delimiter and null string.
        /// </returns>
        /// <exception cref="ClientEvaluationNotSupportedException">
        /// This method is only intended for use via SQL translation as part of an EF Core LINQ query.
        /// </exception>
        public static List<T> StringToList<T>([CanBeNull] this DbFunctions _, [NotNull] string input, [CanBeNull] string delimiter, [CanBeNull] string nullString)
            => throw ClientEvaluationNotSupportedException();

        #region Utilities

        /// <summary>
        /// Helper method to throw a <see cref="NotSupportedException"/> with the name of the throwing method.
        /// </summary>
        /// <param name="method">The method that throws the exception.</param>
        /// <returns>
        /// A <see cref="NotSupportedException"/>.
        /// </returns>
        [NotNull]
        static NotSupportedException ClientEvaluationNotSupportedException([CallerMemberName] string method = default)
            => new NotSupportedException($"{method} is only intended for use via SQL translation as part of an EF Core LINQ query.");

        #endregion
    }
}
