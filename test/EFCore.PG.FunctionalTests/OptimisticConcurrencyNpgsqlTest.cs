using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class OptimisticConcurrencyNpgsqlTest : OptimisticConcurrencyTestBase<F1NpgsqlFixture>
    {
        public OptimisticConcurrencyNpgsqlTest(F1NpgsqlFixture fixture) : base(fixture) {}

        [Fact]
        public async Task Modifying_concurrency_token_only_is_noop()
        {
            using (var c = CreateF1Context())
            {
                await c.Database.CreateExecutionStrategy().ExecuteAsync(c, async context =>
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        var driver = context.Drivers.Single(d => d.CarNumber == 1);
                        Assert.NotEqual(1u, context.Entry(driver).Property<uint>("xmin").CurrentValue);
                        driver.Podiums = StorePodiums;
                        var firstVersion = context.Entry(driver).Property<uint>("xmin").CurrentValue;
                        await context.SaveChangesAsync();

                        using (var innerContext = CreateF1Context())
                        {
                            innerContext.Database.UseTransaction(transaction.GetDbTransaction());
                            driver = innerContext.Drivers.Single(d => d.CarNumber == 1);
                            Assert.NotEqual(firstVersion, innerContext.Entry(driver).Property<uint>("xmin").CurrentValue);
                            Assert.Equal(StorePodiums, driver.Podiums);

                            var secondVersion = innerContext.Entry(driver).Property<uint>("xmin").CurrentValue;
                            innerContext.Entry(driver).Property<uint>("xmin").CurrentValue = firstVersion;
                            await innerContext.SaveChangesAsync();
                            using (var validationContext = CreateF1Context())
                            {
                                validationContext.Database.UseTransaction(transaction.GetDbTransaction());
                                driver = validationContext.Drivers.Single(d => d.CarNumber == 1);
                                Assert.Equal(secondVersion, validationContext.Entry(driver).Property<uint>("xmin").CurrentValue);
                                Assert.Equal(StorePodiums, driver.Podiums);
                            }
                        }
                    }
                });
            }
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
