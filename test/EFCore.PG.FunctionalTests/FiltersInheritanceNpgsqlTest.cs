using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    /*
     * Skipping fixture because of race conditions in InheritanceFixtureBase -
     * both FiltersInheritanceNpgsqlTest and InheritanceNpgsqlTest attempt to create and
     * seed the same database. Already fixed in EF Core dev branch.
    public class FiltersInheritanceNpgsqlTest : FiltersInheritanceTestBase<InheritanceNpgsqlFixture>
    {
        public FiltersInheritanceNpgsqlTest(InheritanceNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
    */
}
