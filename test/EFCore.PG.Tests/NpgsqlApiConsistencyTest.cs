using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

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
            public override HashSet<Type> FluentApiTypes { get; } = new()
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

            public override HashSet<MethodInfo> UnmatchedMetadataMethods { get; } = new()
            {
                typeof(NpgsqlPropertyBuilderExtensions).GetMethod(
                    nameof(NpgsqlPropertyBuilderExtensions.IsGeneratedTsVectorColumn),
                    new[] { typeof(PropertyBuilder), typeof(string), typeof(string[]) })
            };

            public override List<(Type Type, Type ReadonlyExtensions, Type MutableExtensions, Type ConventionExtensions, Type ConventionBuilderExtensions)> MetadataExtensionTypes { get; }
                = new()
                {
                    { (typeof(IModel), typeof(NpgsqlModelExtensions), typeof(NpgsqlModelExtensions), typeof(NpgsqlModelExtensions), typeof(NpgsqlModelBuilderExtensions)) },
                    { (typeof(IEntityType), typeof(NpgsqlEntityTypeExtensions), typeof(NpgsqlEntityTypeExtensions), typeof(NpgsqlEntityTypeExtensions), typeof(NpgsqlEntityTypeBuilderExtensions)) },
                    { (typeof(IProperty), typeof(NpgsqlPropertyExtensions), typeof(NpgsqlPropertyExtensions), typeof(NpgsqlPropertyExtensions), typeof(NpgsqlPropertyBuilderExtensions)) },
                    { (typeof(IIndex), typeof(NpgsqlIndexExtensions), typeof(NpgsqlIndexExtensions), typeof(NpgsqlIndexExtensions), typeof(NpgsqlIndexBuilderExtensions)) }
                };

            public override HashSet<MethodInfo> MetadataMethodExceptions { get; } = new()
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
