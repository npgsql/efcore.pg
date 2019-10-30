using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ComplexNavigationsQueryNpgsqlTest
        : ComplexNavigationsQueryTestBase<ComplexNavigationsQueryNpgsqlFixture>
    {
        public ComplexNavigationsQueryNpgsqlTest(ComplexNavigationsQueryNpgsqlFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/18679")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Include13(bool isAsync) => base.Include13(isAsync);

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/18679")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Include14(bool isAsync) => base.Include13(isAsync);

        // Should be fixed but could not verify as temporarily disabled upstream
//        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/12970")]
//        [MemberData(nameof(IsAsyncData))]
//        public override Task Null_check_in_anonymous_type_projection_should_not_be_removed(bool isAsync) => null;
    }
}
