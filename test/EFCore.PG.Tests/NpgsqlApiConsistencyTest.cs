using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlDbContextOptionsBuilderExtensions = Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities.NpgsqlDbContextOptionsBuilderExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlApiConsistencyTest : ApiConsistencyTestBase<NpgsqlApiConsistencyTest.NpgsqlApiConsistencyFixture>
    {
        public NpgsqlApiConsistencyTest(NpgsqlApiConsistencyFixture fixture)
            : base(fixture)
        {
        }

        protected override void AddServices(ServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkNpgsql();

        protected override Assembly TargetAssembly => typeof(NpgsqlRelationalConnection).Assembly;

        public class NpgsqlApiConsistencyFixture : ApiConsistencyFixtureBase
        {
            public override bool TryGetProviderOptionsDelegate(out Action<DbContextOptionsBuilder> configureOptions)
            {
                configureOptions = b => NpgsqlTestHelpers.Instance.UseProviderOptions(b);

                return true;
            }

            public override HashSet<Type> FluentApiTypes { get; } = new HashSet<Type>()
            {
                typeof(NpgsqlDbContextOptionsBuilder),
                typeof(NpgsqlDbContextOptionsBuilderExtensions),
                typeof(NpgsqlMigrationBuilderExtensions),
                typeof(NpgsqlIndexBuilderExtensions),
                typeof(NpgsqlModelBuilderExtensions),
                typeof(NpgsqlPropertyBuilderExtensions),
                typeof(NpgsqlEntityTypeBuilderExtensions),
                typeof(NpgsqlServiceCollectionExtensions)
            };

            public override List<(Type Type, Type ReadonlyExtensions, Type MutableExtensions, Type ConventionExtensions, Type ConventionBuilderExtensions)> MetadataExtensionTypes { get; }
                = new List<(Type, Type, Type, Type, Type)>
                {
                    { (typeof(IModel), typeof(NpgsqlModelExtensions), typeof(NpgsqlModelExtensions), typeof(NpgsqlModelExtensions), typeof(NpgsqlModelBuilderExtensions)) },
                    { (typeof(IEntityType), typeof(NpgsqlEntityTypeExtensions), typeof(NpgsqlEntityTypeExtensions), typeof(NpgsqlEntityTypeExtensions), typeof(NpgsqlEntityTypeBuilderExtensions)) },
                    { (typeof(IProperty), typeof(NpgsqlPropertyExtensions), typeof(NpgsqlPropertyExtensions), typeof(NpgsqlPropertyExtensions), typeof(NpgsqlPropertyBuilderExtensions)) },
                    { (typeof(IIndex), typeof(NpgsqlIndexExtensions), typeof(NpgsqlIndexExtensions), typeof(NpgsqlIndexExtensions), typeof(NpgsqlIndexBuilderExtensions)) }
                };

            public override HashSet<MethodInfo> MetadataMethodExceptions { get; } = new HashSet<MethodInfo>
            {
                typeof(NpgsqlEntityTypeExtensions).GetRuntimeMethod(
                    nameof(NpgsqlEntityTypeExtensions.SetStorageParameter),
                    new[]
                    {
                        typeof(IConventionEntityType),
                        typeof(string),
                        typeof(object),
                        typeof(bool)
                    })
            };
        }
    }
}
