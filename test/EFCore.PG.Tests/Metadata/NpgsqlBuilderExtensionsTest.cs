using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

public class NpgsqlBuilderExtensionsTest
{
    [Fact]
    public void Can_set_database_encoding()
    {
        var modelBuilder = CreateConventionModelBuilder();

        Assert.Null(modelBuilder.Model.GetDatabaseEncoding());
        Assert.Same(modelBuilder, modelBuilder.UseDatabaseEncoding("UTF8"));
        Assert.Equal("UTF8", modelBuilder.Model.GetDatabaseEncoding());
        Assert.Null(modelBuilder.FinalizeModel().GetDatabaseEncoding());
    }

    [Fact]
    public void Can_disable_identity_generation()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity("Before").Property<int>("Id");

        Assert.Same(modelBuilder, modelBuilder.UseNoIdentityGeneration());

        modelBuilder.Entity("After").Property<int>("Id");
        var model = modelBuilder.FinalizeModel();

        Assert.Equal(NpgsqlValueGenerationStrategy.None, model.GetValueGenerationStrategy());
        foreach (var property in model.GetEntityTypes().Select(entityType => entityType.FindProperty("Id")))
        {
            Assert.Equal(NpgsqlValueGenerationStrategy.None, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Disabling_identity_generation_clears_sequence_options(bool useHiLo)
    {
        var modelBuilder = CreateConventionModelBuilder();
        if (useHiLo)
        {
            modelBuilder.UseHiLo("HiLoSequence", "HiLoSchema");
        }
        else
        {
            modelBuilder.UseKeySequences("SequenceSuffix", "SequenceSchema");
        }

        var model = modelBuilder.UseNoIdentityGeneration().Model;

        Assert.Equal(NpgsqlValueGenerationStrategy.None, model.GetValueGenerationStrategy());
        Assert.Null(model.FindAnnotation(NpgsqlAnnotationNames.SequenceNameSuffix));
        Assert.Null(model.FindAnnotation(NpgsqlAnnotationNames.SequenceSchema));
        Assert.Null(model.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceName));
        Assert.Null(model.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema));
    }

    [Fact]
    public void Disabling_identity_generation_preserves_explicit_property_strategies()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var entityBuilder = modelBuilder.Entity("Post");
        entityBuilder.Property<int>("Id").UseIdentityAlwaysColumn();

        modelBuilder.UseNoIdentityGeneration();

        entityBuilder.Property<int>("OtherId").UseSerialColumn();
        var entityType = modelBuilder.FinalizeModel().FindEntityType("Post");

        Assert.Equal(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn, entityType.FindProperty("Id").GetValueGenerationStrategy());
        Assert.Equal(NpgsqlValueGenerationStrategy.SerialColumn, entityType.FindProperty("OtherId").GetValueGenerationStrategy());
    }

    [Fact]
    public void CockroachDbInterleaveInParent()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Customer>()
            .ToTable("customers", "my_schema")
            .UseCockroachDbInterleaveInParent(typeof(Customer), ["col_a", "col_b"]);

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        var interleaveInParent = entityType.GetCockroachDbInterleaveInParent();

        Assert.Equal("my_schema", interleaveInParent.ParentTableSchema);
        Assert.Equal("customers", interleaveInParent.ParentTableName);
        var interleavePrefix = interleaveInParent.InterleavePrefix;
        Assert.Equal(2, interleavePrefix.Count);
        Assert.Equal("col_a", interleavePrefix[0]);
        Assert.Equal("col_b", interleavePrefix[1]);
    }

    [Fact]
    public void Can_set_identity_sequence_options_on_property()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(
                startValue: 5,
                incrementBy: 2,
                minValue: 3,
                maxValue: 2000,
                cyclic: true,
                numbersToCache: 10);

        var model = modelBuilder.Model;
        var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

        Assert.Equal(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn, property.GetValueGenerationStrategy());
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        Assert.Equal(5, property.GetIdentityStartValue());
        Assert.Equal(3, property.GetIdentityMinValue());
        Assert.Equal(2000, property.GetIdentityMaxValue());
        Assert.Equal(2, property.GetIdentityIncrementBy());
        Assert.True(property.GetIdentityIsCyclic());
        Assert.Equal(10, property.GetIdentityNumbersToCache());

        Assert.Null(model.FindSequence(NpgsqlModelExtensions.DefaultHiLoSequenceName));
        Assert.Null(model.FindSequence(NpgsqlModelExtensions.DefaultHiLoSequenceName));
    }

    protected virtual ModelBuilder CreateConventionModelBuilder()
        => NpgsqlTestHelpers.Instance.CreateConventionBuilder();

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<Order> Orders { get; set; }
    }

    private class Order
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public OrderDetails Details { get; set; }
    }

    private class OrderDetails
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
