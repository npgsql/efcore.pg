﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class DefaultValuesTest : IDisposable
    {
        readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddEntityFrameworkNpgsql()
            .BuildServiceProvider();

        [Fact]
        public void Can_use_Npgsql_default_values()
        {
            using (var context = new ChipsContext(_serviceProvider, "DefaultKettleChips"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var honeyDijon = context.Add(new KettleChips { Name = "Honey Dijon" }).Entity;
                var buffaloBleu = context.Add(new KettleChips { Name = "Buffalo Bleu", BestBuyDate = new DateTime(2111, 1, 11) }).Entity;

                context.SaveChanges();

                Assert.Equal(new DateTime(2035, 9, 25), honeyDijon.BestBuyDate);
                Assert.Equal(new DateTime(2111, 1, 11), buffaloBleu.BestBuyDate);
            }

            using (var context = new ChipsContext(_serviceProvider, "DefaultKettleChips"))
            {
                Assert.Equal(new DateTime(2035, 9, 25), context.Chips.Single(c => c.Name == "Honey Dijon").BestBuyDate);
                Assert.Equal(new DateTime(2111, 1, 11), context.Chips.Single(c => c.Name == "Buffalo Bleu").BestBuyDate);
            }
        }

        public void Dispose()
        {
            using var context = new ChipsContext(_serviceProvider, "DefaultKettleChips");
            context.Database.EnsureDeleted();
        }

        class ChipsContext : DbContext
        {
            readonly IServiceProvider _serviceProvider;
            readonly string _databaseName;

            public ChipsContext(IServiceProvider serviceProvider, string databaseName)
            {
                _serviceProvider = serviceProvider;
                _databaseName = databaseName;
            }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public DbSet<KettleChips> Chips { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseNpgsql(NpgsqlTestStore.CreateConnectionString(_databaseName))
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<KettleChips>()
                    .Property(e => e.BestBuyDate)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValue(new DateTime(2035, 9, 25));
        }

        class KettleChips
        {
            // ReSharper disable once UnusedMember.Local
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime BestBuyDate { get; set; }
        }
    }
}
