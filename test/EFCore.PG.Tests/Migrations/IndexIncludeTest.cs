using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

public class IndexIncludeTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Complex_property_is_included(bool useExpression)
    {
        using var context = new IncludeContext(
            b =>
            {
                var index = b.HasIndex(e => e.Name);
                if (useExpression)
                {
                    index.IncludeProperties(e => e.Address.PostalCode);
                }
                else
                {
                    index.IncludeProperties("Address.PostalCode");
                }
            });
        var model = context.GetService<IDesignTimeModel>().Model;

        Assert.Equal(["Address.PostalCode"], Assert.Single(model.FindEntityType(typeof(Customer)).GetIndexes()).GetIncludeProperties());
        Assert.Equal(
            """CREATE INDEX "IX_Customers_Name" ON "Customers" ("Name") INCLUDE (postal_code);""",
            GenerateIndexSql(context, model));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Multiple_properties_with_nested_complex_property_are_included(bool useExpression)
    {
        using var context = new IncludeContext(
            b =>
            {
                var index = b.HasIndex(e => e.Id);
                if (useExpression)
                {
                    index.IncludeProperties(e => new { e.Name, e.Address.PostalCode, e.Address.Country.Code });
                }
                else
                {
                    ((IndexBuilder)index).IncludeProperties("Name", "Address.PostalCode", "Address.Country.Code");
                }
            });
        var model = context.GetService<IDesignTimeModel>().Model;

        Assert.Equal(
            ["Name", "Address.PostalCode", "Address.Country.Code"],
            Assert.Single(model.FindEntityType(typeof(Customer)).GetIndexes()).GetIncludeProperties());
        Assert.Equal(
            """CREATE INDEX "IX_Customers_Id" ON "Customers" ("Id") INCLUDE ("Name", postal_code, country_code);""",
            GenerateIndexSql(context, model));
    }

    [Fact]
    public void Nested_value_type_property_is_included()
    {
        using var context = new IncludeContext(b => b.HasIndex(e => e.Name).IncludeProperties(e => e.Address.Country.Code));
        var model = context.GetService<IDesignTimeModel>().Model;

        Assert.Equal(
            """CREATE INDEX "IX_Customers_Name" ON "Customers" ("Name") INCLUDE (country_code);""",
            GenerateIndexSql(context, model));
    }

    [Fact]
    public void Collection_columns_can_be_included()
    {
        using var context = new IncludeContext(
            b => b.HasIndex(e => e.Name).IncludeProperties(e => new { e.Tags, e.Scores }));
        var model = context.GetService<IDesignTimeModel>().Model;

        Assert.Equal(
            """CREATE INDEX "IX_Customers_Name" ON "Customers" ("Name") INCLUDE ("Tags", "Scores");""",
            GenerateIndexSql(context, model));
    }

    [Fact]
    public void Collection_traversals_cannot_be_included()
    {
        var index = new ModelBuilder().Entity<Customer>().HasIndex(e => e.Name);

        Assert.Throws<ArgumentException>(() => index.IncludeProperties(e => e.Tags[0]));
        Assert.Throws<ArgumentException>(() => index.IncludeProperties(e => e.Scores[0]));
        Assert.Throws<ArgumentException>(() => index.IncludeProperties(e => e.Tags.Select(t => t)));
        Assert.Throws<ArgumentException>(() => index.IncludeProperties(e => new { e.Name, Tag = e.Tags[0] }));
    }

    [Theory]
    [InlineData("Legacy.Code")]
    [InlineData("Address.PostalCode")]
    public void Literal_dotted_property_name_can_be_included(string name)
    {
        using var context = new IncludeContext(
            b =>
            {
                b.IndexerProperty<string>(name).HasColumnName("legacy_code");
                b.HasIndex(e => e.Name).IncludeProperties(name);
            });
        var model = context.GetService<IDesignTimeModel>().Model;

        Assert.Equal(
            """CREATE INDEX "IX_Customers_Name" ON "Customers" ("Name") INCLUDE (legacy_code);""",
            GenerateIndexSql(context, model));
    }

    [Fact]
    public void Literal_dotted_index_key_cannot_also_be_included()
    {
        using var context = new IncludeContext(
            b =>
            {
                var property = b.IndexerProperty<string>("Legacy.Code").Metadata;
                b.Metadata.AddIndex([property]).SetIncludeProperties(["Legacy.Code"]);
            });

        Assert.Equal(
            NpgsqlStrings.IncludePropertyInIndex(nameof(Customer), "Legacy.Code"),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [Fact]
    public void Literal_dotted_shared_index_include_columns_are_compared()
    {
        using var source = new IncludeContext(
            b =>
            {
                b.IndexerProperty<string>("Legacy.Code").HasColumnName("postal_code");
                b.HasIndex(e => e.Name).IncludeProperties("Legacy.Code");
            });
        using var target = new IncludeContext(b => b.HasIndex(e => e.Name).IncludeProperties(e => e.Address.PostalCode));
        var sourceIndex = Assert.Single(source.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Customer)).GetIndexes());
        var targetIndex = Assert.Single(target.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Customer)).GetIndexes());
        var table = StoreObjectIdentifier.Table("Customers");

        Assert.True(sourceIndex.AreCompatibleForNpgsql(targetIndex, table, shouldThrow: true));

        using var different = new IncludeContext(b => b.HasIndex(e => e.Name).IncludeProperties(e => e.BillingAddress.PostalCode));
        var differentIndex = Assert.Single(different.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Customer)).GetIndexes());
        var exception = Assert.Throws<InvalidOperationException>(
            () => sourceIndex.AreCompatibleForNpgsql(differentIndex, table, shouldThrow: true));
        Assert.Contains("'postal_code'", exception.Message);
        Assert.Contains("'BillingAddress_PostalCode'", exception.Message);
    }

    [Theory]
    [InlineData("Missing")]
    [InlineData("Address.Missing")]
    [InlineData("Address.Country.Missing")]
    [InlineData("Address")]
    public void Missing_include_property_is_rejected(string path)
    {
        using var context = new IncludeContext(b => b.HasIndex(e => e.Name).IncludeProperties(path));

        Assert.Equal(
            NpgsqlStrings.IncludePropertyNotFound(nameof(Customer), path),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [Fact]
    public void Duplicate_include_property_is_rejected()
    {
        using var context = new IncludeContext(
            b => b.HasIndex(e => e.Name).IncludeProperties("Address.PostalCode", "Address.PostalCode"));

        Assert.Equal(
            NpgsqlStrings.IncludePropertyDuplicated(nameof(Customer), "Address.PostalCode"),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("Address.PostalCode")]
    [InlineData("Address.Country.Code")]
    public void Index_key_cannot_also_be_included(string path)
    {
        using var context = new IncludeContext(b => b.HasIndex(path).IncludeProperties(path));

        Assert.Equal(
            NpgsqlStrings.IncludePropertyInIndex(nameof(Customer), path),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [Fact]
    public void Different_properties_with_same_leaf_name_can_be_included()
    {
        using var context = new IncludeContext(
            b => b.HasIndex(e => e.BillingAddress.PostalCode).IncludeProperties(e => e.Address.PostalCode));
        var model = context.GetService<IDesignTimeModel>().Model;

        Assert.Equal(
            """CREATE INDEX "IX_Customers_BillingAddress_PostalCode" ON "Customers" ("BillingAddress_PostalCode") INCLUDE (postal_code);""",
            GenerateIndexSql(context, model));
    }

    [Theory]
    [InlineData("Address.PostalCode")]
    [InlineData("Address.Country.Code")]
    public void Json_member_cannot_be_included(string path)
    {
        using var context = new IncludeContext(
            b =>
            {
                var address = b.ComplexProperty(e => e.Address).ToJson();
                address.Property(e => e.PostalCode).HasColumnName(null);
                address.ComplexProperty(e => e.Country).Property(e => e.Code).HasColumnName(null);
                b.HasIndex(e => e.Name).IncludeProperties(path);
            });

        Assert.Equal(
            NpgsqlStrings.IncludePropertyMappedToJson(nameof(Customer), path),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [Fact]
    public void Shared_index_include_columns_are_compared_by_path()
    {
        using var source = new IncludeContext(b => b.HasIndex(e => e.Name).IncludeProperties(e => e.Address.PostalCode));
        using var target = new IncludeContext(b => b.HasIndex(e => e.Name).IncludeProperties("Address.PostalCode"));
        var sourceIndex = Assert.Single(source.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Customer)).GetIndexes());
        var targetIndex = Assert.Single(target.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Customer)).GetIndexes());

        Assert.True(sourceIndex.AreCompatibleForNpgsql(targetIndex, StoreObjectIdentifier.Table("Customers"), shouldThrow: true));
    }

    [Fact]
    public void Different_shared_index_include_columns_are_reported()
    {
        using var source = new IncludeContext(b => b.HasIndex(e => e.Name).IncludeProperties(e => e.Address.PostalCode));
        using var target = new IncludeContext(b => b.HasIndex(e => e.Name).IncludeProperties(e => e.BillingAddress.PostalCode));
        var sourceIndex = Assert.Single(source.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Customer)).GetIndexes());
        var targetIndex = Assert.Single(target.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Customer)).GetIndexes());

        Assert.False(sourceIndex.AreCompatibleForNpgsql(targetIndex, StoreObjectIdentifier.Table("Customers"), shouldThrow: false));
        var exception = Assert.Throws<InvalidOperationException>(
            () => sourceIndex.AreCompatibleForNpgsql(targetIndex, StoreObjectIdentifier.Table("Customers"), shouldThrow: true));
        Assert.Contains("'postal_code'", exception.Message);
        Assert.Contains("'BillingAddress_PostalCode'", exception.Message);
    }

    private static string GenerateIndexSql(DbContext context, IModel model)
    {
        var operation = Assert.Single(
            context.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel())
                .OfType<CreateIndexOperation>());

        return Assert.Single(context.GetService<IMigrationsSqlGenerator>().Generate([operation], model)).CommandText.TrimEnd();
    }

    private class IncludeContext(Action<EntityTypeBuilder<Customer>> configure) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql().EnableServiceProviderCaching(false);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var customer = modelBuilder.Entity<Customer>();
            customer.ToTable("Customers");
            customer.ComplexProperty(
                e => e.Address, b =>
                {
                    b.IsRequired();
                    b.Property(e => e.PostalCode).HasColumnName("postal_code");
                    b.ComplexProperty(e => e.Country).IsRequired().Property(e => e.Code).HasColumnName("country_code");
                });
            customer.ComplexProperty(e => e.BillingAddress).IsRequired().ComplexProperty(e => e.Country).IsRequired();
            configure(customer);
        }
    }

    private class Customer
    {
        private readonly Dictionary<string, object> _values = [];

        public object this[string name]
        {
            get => _values[name];
            set => _values[name] = value;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public Address BillingAddress { get; set; }
        public int[] Tags { get; set; }
        public List<int> Scores { get; set; }
    }

    private class Address
    {
        public string PostalCode { get; set; }
        public Country Country { get; set; }
    }

    private class Country
    {
        public int Code { get; set; }
    }
}
