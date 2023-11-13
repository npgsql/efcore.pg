using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class NpgsqlApiConsistencyTest : ApiConsistencyTestBase<NpgsqlApiConsistencyTest.NpgsqlApiConsistencyFixture>
{
    public NpgsqlApiConsistencyTest(NpgsqlApiConsistencyFixture fixture)
        : base(fixture)
    {
    }

    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkNpgsql();

    protected override Assembly TargetAssembly
        => typeof(NpgsqlRelationalConnection).Assembly;

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

        public override
            Dictionary<Type,
                (Type ReadonlyExtensions,
                Type MutableExtensions,
                Type ConventionExtensions,
                Type ConventionBuilderExtensions,
                Type RuntimeExtensions)> MetadataExtensionTypes { get; }
            = new()
            {
                {
                    typeof(IReadOnlyModel), (
                        typeof(NpgsqlModelExtensions),
                        typeof(NpgsqlModelExtensions),
                        typeof(NpgsqlModelExtensions),
                        typeof(NpgsqlModelBuilderExtensions),
                        null
                    )
                },
                {
                    typeof(IReadOnlyEntityType), (
                        typeof(NpgsqlEntityTypeExtensions),
                        typeof(NpgsqlEntityTypeExtensions),
                        typeof(NpgsqlEntityTypeExtensions),
                        typeof(NpgsqlEntityTypeBuilderExtensions),
                        null
                    )
                },
                {
                    typeof(IReadOnlyProperty), (
                        typeof(NpgsqlPropertyExtensions),
                        typeof(NpgsqlPropertyExtensions),
                        typeof(NpgsqlPropertyExtensions),
                        typeof(NpgsqlPropertyBuilderExtensions),
                        null
                    )
                },
                {
                    typeof(IReadOnlyIndex), (
                        typeof(NpgsqlIndexExtensions),
                        typeof(NpgsqlIndexExtensions),
                        typeof(NpgsqlIndexExtensions),
                        typeof(NpgsqlIndexBuilderExtensions),
                        null
                    )
                }
            };

        public override HashSet<MethodInfo> MetadataMethodExceptions { get; } = new()
        {
            typeof(NpgsqlEntityTypeExtensions).GetRuntimeMethod(
                nameof(NpgsqlEntityTypeExtensions.SetStorageParameter),
                new[] { typeof(IConventionEntityType), typeof(string), typeof(object), typeof(bool) })
        };
    }
}
