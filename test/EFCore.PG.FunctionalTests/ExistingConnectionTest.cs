using System.Data;

namespace Microsoft.EntityFrameworkCore;

public class ExistingConnectionTest
{
    // See aspnet/Data#135
    [Fact]
    public async Task Can_use_an_existing_closed_connection()
        => await Can_use_an_existing_closed_connection_test(openConnection: false);

    [Fact]
    public async Task Can_use_an_existing_open_connection()
        => await Can_use_an_existing_closed_connection_test(openConnection: true);

    private static async Task Can_use_an_existing_closed_connection_test(bool openConnection)
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkNpgsql()
            .BuildServiceProvider();

        await using (var store = await NpgsqlTestStore.GetNorthwindStoreAsync())
        {
            store.CloseConnection();

            var openCount = 0;
            var closeCount = 0;

            await using (var connection = new NpgsqlConnection(store.ConnectionString))
            {
                if (openConnection)
                {
                    await connection.OpenAsync();
                }

                connection.StateChange += (_, a) =>
                {
                    if (a.CurrentState == ConnectionState.Open)
                    {
                        openCount++;
                    }
                    else if (a.CurrentState == ConnectionState.Closed)
                    {
                        closeCount++;
                    }
                };

                await using (var context = new NorthwindContext(serviceProvider, connection))
                {
                    Assert.Equal(91, await context.Customers.CountAsync());
                }

                if (openConnection)
                {
                    Assert.Equal(ConnectionState.Open, connection.State);
                    Assert.Equal(0, openCount);
                    Assert.Equal(0, closeCount);
                }
                else
                {
                    Assert.Equal(ConnectionState.Closed, connection.State);
                    Assert.Equal(1, openCount);
                    Assert.Equal(1, closeCount);
                }
            }
        }
    }

    private class NorthwindContext(IServiceProvider serviceProvider, NpgsqlConnection connection) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly NpgsqlConnection _connection = connection;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseNpgsql(_connection)
                .UseInternalServiceProvider(_serviceProvider);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(
                b =>
                {
                    b.HasKey(c => c.CustomerId);
                    ((EntityTypeBuilder)b).ToTable("Customers");
                });
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class Customer
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string CustomerId { get; set; } = null!;

        // ReSharper disable once UnusedMember.Local
        public string CompanyName { get; set; } = null!;

        // ReSharper disable once UnusedMember.Local
        public string Fax { get; set; } = null!;
    }
}
