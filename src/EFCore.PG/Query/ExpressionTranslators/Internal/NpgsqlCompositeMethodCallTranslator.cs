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

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        static readonly IMethodCallTranslator[] _methodCallTranslators =
        {
            new NpgsqlArraySequenceEqualTranslator(),
            new NpgsqlConvertTranslator(),
            new NpgsqlStringSubstringTranslator(),
            new NpgsqlLikeTranslator(),
            new NpgsqlMathTranslator(),
            new NpgsqlObjectToStringTranslator(),
            new NpgsqlStringEndsWithTranslator(),
            new NpgsqlStringStartsWithTranslator(),
            new NpgsqlStringContainsTranslator(),
            new NpgsqlStringIndexOfTranslator(),
            new NpgsqlStringIsNullOrWhiteSpaceTranslator(),
            new NpgsqlStringReplaceTranslator(),
            new NpgsqlStringToLowerTranslator(),
            new NpgsqlStringToUpperTranslator(),
            new NpgsqlStringTrimTranslator(),
            new NpgsqlStringTrimEndTranslator(),
            new NpgsqlStringTrimStartTranslator(),
            new NpgsqlRegexIsMatchTranslator(),
            new NpgsqlFullTextSearchMethodTranslator()
        };

        public NpgsqlCompositeMethodCallTranslator(
            [NotNull] RelationalCompositeMethodCallTranslatorDependencies dependencies)
            : base(dependencies)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(_methodCallTranslators);
        }
    }
}
