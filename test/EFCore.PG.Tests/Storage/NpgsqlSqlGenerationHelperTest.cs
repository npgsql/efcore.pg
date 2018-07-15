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

using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage
{
    public class NpgsqlSqlGenerationHelperTest
    {
        [Theory]
        [InlineData("all_lowercase", null, "all_lowercase", "all_lowercase")]
        [InlineData("digit", null, "digit", "digit")]
        [InlineData("under_score", null, "under_score", "under_score")]
        [InlineData("dollar$", null, "dollar$", "dollar$")]
        [InlineData("ALL_CAPS", null, "\"ALL_CAPS\"", "\"ALL_CAPS\"")]
        [InlineData("oneCap", null, "\"oneCap\"", "\"oneCap\"")]
        [InlineData("0starts_with_digit", null, "\"0starts_with_digit\"", "\"0starts_with_digit\"")]
        [InlineData("all_lowercase", "all_lowercase", "all_lowercase.all_lowercase", "all_lowercase")]
        [InlineData("all_lowercase", "oneCap", "\"oneCap\".all_lowercase", "all_lowercase")]
        [InlineData("oneCap", "all_lowercase", "all_lowercase.\"oneCap\"", "\"oneCap\"")]
        [InlineData("CAPS", "CAPS", "\"CAPS\".\"CAPS\"", "\"CAPS\"")]
        [InlineData("select", "null", "\"null\".\"select\"", "\"select\"")]
        public virtual void DelimitIdentifier_quotes_properly(
            string identifier, string schema, string outputWithSchema, string outputWithoutSchema)
        {
            var helper = CreateSqlGenerationHelper();
            Assert.Equal(outputWithoutSchema, helper.DelimitIdentifier(identifier));
            Assert.Equal(outputWithSchema, helper.DelimitIdentifier(identifier, schema));

            var sb = new StringBuilder();
            helper.DelimitIdentifier(sb, identifier);
            Assert.Equal(outputWithoutSchema, sb.ToString());

            sb = new StringBuilder();
            helper.DelimitIdentifier(sb, identifier, schema);
            Assert.Equal(outputWithSchema, sb.ToString());
        }

        ISqlGenerationHelper CreateSqlGenerationHelper()
            => new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
    }
}
