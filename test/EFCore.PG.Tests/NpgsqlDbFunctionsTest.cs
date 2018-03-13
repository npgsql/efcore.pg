using Microsoft.EntityFrameworkCore;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlDbFunctionsTest
    {
        readonly DbFunctions _functions = EF.Functions;

        [Fact]
        public void ILike_when_no_wildcards()
        {
            Assert.True(_functions.ILike("abc", "abc"));
            Assert.True(_functions.ILike("abc", "ABC"));
            Assert.True(_functions.ILike("ABC", "abc"));

            Assert.False(_functions.ILike("ABC", "ab"));
            Assert.False(_functions.ILike("ab", "abc"));
        }
    }
}
