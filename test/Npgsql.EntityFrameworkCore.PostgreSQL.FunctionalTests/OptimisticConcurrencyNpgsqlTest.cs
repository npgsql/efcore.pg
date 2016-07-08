using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class OptimisticConcurrencyNpgsqlTest : OptimisticConcurrencyTestBase<NpgsqlTestStore, F1NpgsqlFixture>
    {
        public OptimisticConcurrencyNpgsqlTest(F1NpgsqlFixture fixture) : base(fixture) {}

        [Fact]
        public async Task Modifying_concurrency_token_only_is_noop()
        {
            uint firstVersion;
            using (var context = CreateF1Context())
            {
                var driver = context.Drivers.Single(d => d.CarNumber == 1);
                driver.Podiums = StorePodiums;
                firstVersion = context.Entry(driver).Property<uint>("xmin").CurrentValue;
                await context.SaveChangesAsync();
            }

            uint secondVersion;
            using (var context = CreateF1Context())
            {
                var driver = context.Drivers.Single(d => d.CarNumber == 1);
                Assert.NotEqual(firstVersion, context.Entry(driver).Property<uint>("xmin").CurrentValue);
                Assert.Equal(StorePodiums, driver.Podiums);

                secondVersion = context.Entry(driver).Property<uint>("xmin").CurrentValue;
                context.Entry(driver).Property<uint>("xmin").CurrentValue = firstVersion;
                await context.SaveChangesAsync();
            }

            using (var validationContext = CreateF1Context())
            {
                var driver = validationContext.Drivers.Single(d => d.CarNumber == 1);
                Assert.Equal(secondVersion, validationContext.Entry(driver).Property<uint>("xmin").CurrentValue);
                Assert.Equal(StorePodiums, driver.Podiums);
            }
        }

    }
}
