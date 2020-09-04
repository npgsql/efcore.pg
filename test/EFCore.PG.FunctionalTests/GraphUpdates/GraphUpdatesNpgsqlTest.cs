using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.GraphUpdates
{
    public class GraphUpdatesNpgsqlTest
    {
        public class ClientCascade : GraphUpdatesNpgsqlTestBase<ClientCascade.NpgsqlFixture>
        {
            public ClientCascade(NpgsqlFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class NpgsqlFixture : GraphUpdatesNpgsqlFixtureBase
            {
                public override bool NoStoreCascades => true;

                protected override string StoreName { get; } = "GraphClientCascadeUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    base.OnModelCreating(modelBuilder, context);

                    foreach (var foreignKey in modelBuilder.Model
                        .GetEntityTypes()
                        .SelectMany(e => MutableEntityTypeExtensions.GetDeclaredForeignKeys(e))
                        .Where(e => e.DeleteBehavior == DeleteBehavior.Cascade))
                    {
                        foreignKey.DeleteBehavior = DeleteBehavior.ClientCascade;
                    }
                }
            }
        }

        public class ClientNoAction : GraphUpdatesNpgsqlTestBase<ClientNoAction.NpgsqlFixture>
        {
            public ClientNoAction(NpgsqlFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class NpgsqlFixture : GraphUpdatesNpgsqlFixtureBase
            {
                public override bool ForceClientNoAction => true;

                protected override string StoreName { get; } = "GraphClientNoActionUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    base.OnModelCreating(modelBuilder, context);

                    foreach (var foreignKey in modelBuilder.Model
                        .GetEntityTypes()
                        .SelectMany(e => e.GetDeclaredForeignKeys()))
                    {
                        foreignKey.DeleteBehavior = DeleteBehavior.ClientNoAction;
                    }
                }
            }
        }

        public class Identity : GraphUpdatesNpgsqlTestBase<Identity.NpgsqlFixture>
        {
            public Identity(NpgsqlFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class NpgsqlFixture : GraphUpdatesNpgsqlFixtureBase
            {
                protected override string StoreName { get; } = "GraphIdentityUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    if (TestEnvironment.PostgresVersion >= new Version(10, 0))
                        modelBuilder.UseIdentityColumns();

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class HiLo : GraphUpdatesNpgsqlTestBase<HiLo.NpgsqlFixture>
        {
            public HiLo(NpgsqlFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class NpgsqlFixture : GraphUpdatesNpgsqlFixtureBase
            {
                protected override string StoreName { get; } = "GraphHiLoUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.UseHiLo();

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public abstract class GraphUpdatesNpgsqlTestBase<TFixture> : GraphUpdatesTestBase<TFixture>
            where TFixture : GraphUpdatesNpgsqlTestBase<TFixture>.GraphUpdatesNpgsqlFixtureBase, new()
        {
            protected GraphUpdatesNpgsqlTestBase(TFixture fixture)
                : base(fixture)
            {
            }

            protected override IQueryable<Root> ModifyQueryRoot(IQueryable<Root> query) => query.AsSplitQuery();

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public abstract class GraphUpdatesNpgsqlFixtureBase : GraphUpdatesFixtureBase
            {
                public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
                protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            }
        }
    }
}
