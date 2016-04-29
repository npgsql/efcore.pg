// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class BatchingTest : IDisposable
    {
        [Fact]
        public void Batches_are_divided_correctly_with_two_inserted_columns()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql(_testStore.Connection);

            using (var context = new BloggingContext(_serviceProvider, optionsBuilder.Options))
            {
                context.Database.EnsureCreated();

                for (var i = 1; i < 1101; i++)
                {
                    var blog = new Blog { Id = i, Name = "Foo" + i };
                    context.Blogs.Add(blog);
                }

                context.SaveChanges();
            }

            using (var context = new BloggingContext(_serviceProvider, optionsBuilder.Options))
            {
                Assert.Equal(1100, context.Blogs.Count());
            }
        }

        private class BloggingContext : DbContext
        {
            public BloggingContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(new DbContextOptionsBuilder(options).UseInternalServiceProvider(serviceProvider).Options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>(b =>
                {
                    //b.Property(e => e.Id).HasDefaultValueSql("NEWID()");
                    //b.Property(e => e.Version).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                    // TODO: Bring this up to date...
                });
            }

            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Owner> Owners { get; set; }
        }

        public class Blog
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class Owner
        {
            public int Id { get; set; }
        }

        private readonly NpgsqlTestStore _testStore;
        private readonly IServiceProvider _serviceProvider;

        public BatchingTest()
        {
            _testStore = NpgsqlTestStore.CreateScratch();
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .BuildServiceProvider();
        }

        public void Dispose()
        {
            _testStore.Dispose();
        }
    }
}
