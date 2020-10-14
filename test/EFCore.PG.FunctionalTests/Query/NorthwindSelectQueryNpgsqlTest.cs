using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindSelectQueryNpgsqlTest : NorthwindSelectQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public NorthwindSelectQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Select_datetime_DayOfWeek_component(bool async)
        {
            await base.Select_datetime_DayOfWeek_component(async);

            AssertSql(
                @"SELECT floor(date_part('dow', o.""OrderDate""))::INT
FROM ""Orders"" AS o");
        }

        [ConditionalTheory(Skip = "To be fixed in PG 12.0, https://www.postgresql.org/message-id/CADT4RqAz7oN4vkPir86Kg1_mQBmBxCp-L_%3D9vRpgSNPJf0KRkw%40mail.gmail.com")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Select_with_complex_expression_that_can_be_funcletized(bool async)
            => base.Select_with_complex_expression_that_can_be_funcletized(async);

        public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
            => AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));

        public override Task Reverse_without_explicit_ordering_throws(bool async)
            => AssertTranslationFailedWithDetails(
                () => base.Reverse_without_explicit_ordering_throws(async), RelationalStrings.MissingOrderingInSelectExpression);

        public override async Task Projecting_after_navigation_and_distinct_throws(bool async)
            => Assert.Equal(
                RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Projecting_after_navigation_and_distinct_throws(async))).Message);

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
