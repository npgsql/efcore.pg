using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore
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
