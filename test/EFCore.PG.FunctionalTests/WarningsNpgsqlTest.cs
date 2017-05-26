using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class WarningsNpgsqlTest : WarningsTestBase<WarningsNpgsqlFixture>
    {
        public WarningsNpgsqlTest(WarningsNpgsqlFixture fixture)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Paging_operation_without_orderby_issues_warning()
        {
            base.Paging_operation_without_orderby_issues_warning();

            Assert.True(Fixture.TestSqlLoggerFactory.Log.Contains(CoreStrings.RowLimitingOperationWithoutOrderBy(
                "(from Customer <generated>_2 in DbSet<Customer> select [<generated>_2]).Skip(__p_0).Take(__p_1)")));
        }

        public override void FirstOrDefault_without_orderby_and_filter_issues_warning_subquery()
        {
            base.FirstOrDefault_without_orderby_and_filter_issues_warning_subquery();

            Assert.True(Fixture.TestSqlLoggerFactory.Log.Contains(CoreStrings.FirstWithoutOrderByAndFilter(
                "(from Order <generated>_1 in [c].Orders select [<generated>_1].OrderID).FirstOrDefault()")));
        }

        public override void FirstOrDefault_without_orderby_but_with_filter_doesnt_issue_warning()
        {
            base.FirstOrDefault_without_orderby_but_with_filter_doesnt_issue_warning();

            Assert.False(Fixture.TestSqlLoggerFactory.Log.Contains(CoreStrings.FirstWithoutOrderByAndFilter(
                @"(from Customer c in DbSet<Customer> where c.CustomerID == ""ALFKI"" select c).FirstOrDefault()")));
        }

        public override void Single_SingleOrDefault_without_orderby_doesnt_issue_warning()
        {
            base.Single_SingleOrDefault_without_orderby_doesnt_issue_warning();

            Assert.False(Fixture.TestSqlLoggerFactory.Log.Contains(CoreStrings.FirstWithoutOrderByAndFilter(
                @"(from Customer c in DbSet<Customer> where c.CustomerID == ""ALFKI"" select c).Single()")));
        }
    }
}
