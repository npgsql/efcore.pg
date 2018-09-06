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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL binary string functions and operators.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-binarystring.html
    /// </remarks>
    public class NpgsqlBinaryStringTranslator : IMethodCallTranslator
    {
        #region MethodInfo

        [NotNull] static readonly MethodInfo ComputeHash1 =
            typeof(HashAlgorithm).GetRuntimeMethod(nameof(HashAlgorithm.ComputeHash), new[] { typeof(byte[]) });

        [NotNull] [ItemNotNull] static readonly MethodInfo[] SupportedCreateMethods =
        {
            typeof(MD5).GetRuntimeMethod(nameof(MD5.Create), Type.EmptyTypes),
            typeof(SHA256).GetRuntimeMethod(nameof(SHA256.Create), Type.EmptyTypes),
            typeof(SHA384).GetRuntimeMethod(nameof(SHA384.Create), Type.EmptyTypes),
            typeof(SHA512).GetRuntimeMethod(nameof(SHA512.Create), Type.EmptyTypes),
        };

        #endregion

        /// <summary>
        /// The backend version to target.
        /// </summary>
        [CanBeNull] readonly Version _postgresVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlBinaryStringTranslator"/> class.
        /// </summary>
        /// <param name="postgresVersion">The backend version to target.</param>
        public NpgsqlBinaryStringTranslator([CanBeNull] Version postgresVersion)
            => _postgresVersion = postgresVersion;

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression e)
        {
            // TODO: disable when version is unspecified until 11.0 is released.
            if (_postgresVersion == null)
                return null;

            if (SupportedCreateMethods.Contains(e.Method))
                return e;

            return TranslateComputeHash1(e) ?? TranslateExtensions(e);
        }

        [CanBeNull]
        Expression TranslateComputeHash1([NotNull] MethodCallExpression e)
        {
            if (e.Method != ComputeHash1)
                return null;

            switch (e.Object?.Type.Name)
            {
            case nameof(MD5) when VersionAtLeast(8, 1):
                return new SqlFunctionExpression(
                    "decode",
                    typeof(byte[]),
                    new Expression[]
                    {
                        new SqlFunctionExpression("md5", e.Type, new[] { e.Arguments[0] }),
                        Expression.Constant("hex", typeof(string))
                    });

            case nameof(SHA256) when VersionAtLeast(11, 0):
                return new SqlFunctionExpression("sha256", e.Type, new[] { e.Arguments[0] });

            case nameof(SHA384) when VersionAtLeast(11, 0):
                return new SqlFunctionExpression("sha384", e.Type, new[] { e.Arguments[0] });

            case nameof(SHA512) when VersionAtLeast(11, 0):
                return new SqlFunctionExpression("sha512", e.Type, new[] { e.Arguments[0] });

            default:
                return null;
            }
        }

        [CanBeNull]
        Expression TranslateExtensions([NotNull] MethodCallExpression e)
        {
            if (e.Method.DeclaringType != typeof(NpgsqlBinaryStringExtensions))
                return null;

            switch (e.Method.Name)
            {
            case nameof(NpgsqlBinaryStringExtensions.MD5) when VersionAtLeast(8, 1):
                return new SqlFunctionExpression("md5", e.Type, e.Arguments.Skip(1));

            case nameof(NpgsqlBinaryStringExtensions.SHA224) when VersionAtLeast(11, 0):
                return new SqlFunctionExpression("sha224", e.Type, e.Arguments.Skip(1));

            case nameof(NpgsqlBinaryStringExtensions.SHA256) when VersionAtLeast(11, 0):
                return new SqlFunctionExpression("sha256", e.Type, e.Arguments.Skip(1));

            case nameof(NpgsqlBinaryStringExtensions.SHA384) when VersionAtLeast(11, 0):
                return new SqlFunctionExpression("sha384", e.Type, e.Arguments.Skip(1));

            case nameof(NpgsqlBinaryStringExtensions.SHA512) when VersionAtLeast(11, 0):
                return new SqlFunctionExpression("sha512", e.Type, e.Arguments.Skip(1));

            default:
                return null;
            }
        }

        #region Helpers

        bool VersionAtLeast(int major, int minor) => _postgresVersion is null || new Version(major, minor) <= _postgresVersion;

        #endregion
    }
}
