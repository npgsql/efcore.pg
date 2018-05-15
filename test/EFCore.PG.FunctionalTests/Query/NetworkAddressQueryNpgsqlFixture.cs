using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    /// <summary>
    /// Represents a fixture suitable for testing network address operators.
    /// </summary>
    public class NetworkAddressQueryNpgsqlFixture : IDisposable
    {
        /// <summary>
        /// The <see cref="NpgsqlTestStore"/> used for testing.
        /// </summary>
        private readonly NpgsqlTestStore _testStore;

        /// <summary>
        /// The <see cref="DbContextOptions"/> used for testing.
        /// </summary>
        private readonly DbContextOptions _options;

        /// <summary>
        /// The logger factory used for testing.
        /// </summary>
        public TestSqlLoggerFactory TestSqlLoggerFactory { get; }

        /// <summary>
        /// Initializes a <see cref="NetworkAddressQueryNpgsqlFixture"/>.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public NetworkAddressQueryNpgsqlFixture()
        {
            TestSqlLoggerFactory = new TestSqlLoggerFactory();

            _testStore = NpgsqlTestStore.CreateScratch();

            _options =
                new DbContextOptionsBuilder()
                    .UseNpgsql(_testStore.ConnectionString, b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkNpgsql()
                            .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                            .BuildServiceProvider())
                    .Options;

            using (NetContext context = CreateContext())
            {
                context.Database.EnsureCreated();

                context.NetTestEntities
                       .AddRange(
                           new NetTestEntity { Id = 1, InetMappedToIPAddress = new IPAddress(1), CidrMappedToNpgsqlInet = new IPAddress(1) },
                           new NetTestEntity { Id = 2, InetMappedToIPAddress = new IPAddress(2), CidrMappedToNpgsqlInet = new IPAddress(2) },
                           new NetTestEntity { Id = 3, InetMappedToIPAddress = new IPAddress(3), CidrMappedToNpgsqlInet = new IPAddress(3) },
                           new NetTestEntity { Id = 4, InetMappedToIPAddress = new IPAddress(4), CidrMappedToNpgsqlInet = new IPAddress(4) },
                           new NetTestEntity { Id = 5, InetMappedToIPAddress = new IPAddress(5), CidrMappedToNpgsqlInet = new IPAddress(5) },
                           new NetTestEntity { Id = 6, InetMappedToIPAddress = new IPAddress(6), CidrMappedToNpgsqlInet = new IPAddress(6) },
                           new NetTestEntity { Id = 7, InetMappedToIPAddress = new IPAddress(7), CidrMappedToNpgsqlInet = new IPAddress(7) },
                           new NetTestEntity { Id = 8, InetMappedToIPAddress = new IPAddress(8), CidrMappedToNpgsqlInet = new IPAddress(8) },
                           new NetTestEntity { Id = 9, InetMappedToIPAddress = new IPAddress(9), CidrMappedToNpgsqlInet = new IPAddress(9) },
                           new NetTestEntity { Id = 10, InetMappedToIPAddress = new IPAddress(10), CidrMappedToNpgsqlInet = new IPAddress(10) });

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Creates a new <see cref="NetContext"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="NetContext"/> for testing.
        /// </returns>
        public NetContext CreateContext()
        {
            return new NetContext(_options);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _testStore.Dispose();
        }
    }

    /// <summary>
    /// Represents an entity suitable for testing network address operators.
    /// </summary>
    public class NetTestEntity
    {
        /// <summary>
        /// The primary key.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The network address.
        /// </summary>
        [Column(TypeName = "inet")]
        public IPAddress InetMappedToIPAddress { get; set; }

        /// <summary>
        /// The network address.
        /// </summary>
        [Column(TypeName = "cidr")]
        public NpgsqlInet CidrMappedToNpgsqlInet { get; set; }
    }

    /// <summary>
    /// Represents a database suitable for testing network address operators.
    /// </summary>
    public class NetContext : DbContext
    {
        /// <summary>
        /// Represents a set of entities with <see cref="IPAddress"/> properties.
        /// </summary>
        public DbSet<NetTestEntity> NetTestEntities { get; set; }

        /// <summary>
        /// Initializes a <see cref="NetContext"/>.
        /// </summary>
        /// <param name="options">
        /// The options to be used for configuration.
        /// </param>
        public NetContext(DbContextOptions options) : base(options) { }
    }
}
