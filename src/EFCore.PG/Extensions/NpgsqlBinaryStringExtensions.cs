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
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    // ReSharper disable UnusedParameter.Global
    /// <summary>
    /// Provides extension methods for <see cref="DbFunctions"/> supporting PostgreSQL binary string functions and operators.
    /// </summary>
    public static class NpgsqlBinaryStringExtensions
    {
        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Computes the MD5 hash for the input data.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="data">The data to hash.</param>
        /// <returns>
        /// The MD5 hash of the input data.
        /// </returns>
        /// <exception cref="NotSupportedException">{method} is only intended for use via SQL translation as part of an EF Core LINQ query.</exception>
        [NotNull]
        public static string MD5([CanBeNull] this DbFunctions _, [NotNull] string data) => throw ClientEvaluationNotSupportedException();

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Computes the SHA224 hash for the input data.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="data">The data to hash.</param>
        /// <returns>
        /// The SHA224 hash of the input data.
        /// </returns>
        /// <exception cref="NotSupportedException">{method} is only intended for use via SQL translation as part of an EF Core LINQ query.</exception>
        [NotNull]
        public static byte[] SHA224([CanBeNull] this DbFunctions _, [NotNull] byte[] data) => throw ClientEvaluationNotSupportedException();

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Computes the SHA256 hash for the input data.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="data">The data to hash.</param>
        /// <returns>
        /// The SHA256 hash of the input data.
        /// </returns>
        /// <exception cref="NotSupportedException">{method} is only intended for use via SQL translation as part of an EF Core LINQ query.</exception>
        [NotNull]
        public static byte[] SHA256([CanBeNull] this DbFunctions _, [NotNull] byte[] data) => throw ClientEvaluationNotSupportedException();

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Computes the SHA384 hash for the input data.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="data">The data to hash.</param>
        /// <returns>
        /// The SHA384 hash of the input data.
        /// </returns>
        /// <exception cref="NotSupportedException">{method} is only intended for use via SQL translation as part of an EF Core LINQ query.</exception>
        [NotNull]
        public static byte[] SHA384([CanBeNull] this DbFunctions _, [NotNull] byte[] data) => throw ClientEvaluationNotSupportedException();

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Computes the SHA512 hash for the input data.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="data">The data to hash.</param>
        /// <returns>
        /// The SHA512 hash of the input data.
        /// </returns>
        /// <exception cref="NotSupportedException">{method} is only intended for use via SQL translation as part of an EF Core LINQ query.</exception>
        [NotNull]
        public static byte[] SHA512([CanBeNull] this DbFunctions _, [NotNull] byte[] data) => throw ClientEvaluationNotSupportedException();

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
