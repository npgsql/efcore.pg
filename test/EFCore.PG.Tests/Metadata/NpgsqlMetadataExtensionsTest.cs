using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class NpgsqlMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_column_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal("Name", property.Npgsql().ColumnName);
            Assert.Equal("Name", ((IProperty)property).Npgsql().ColumnName);

            property.Relational().ColumnName = "Eman";

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.Relational().ColumnName);
            Assert.Equal("Eman", property.Npgsql().ColumnName);
            Assert.Equal("Eman", ((IProperty)property).Npgsql().ColumnName);

            property.Npgsql().ColumnName = "MyNameIs";

            Assert.Equal("Name", property.Name);
            Assert.Equal("MyNameIs", property.Relational().ColumnName);
            Assert.Equal("MyNameIs", property.Npgsql().ColumnName);
            Assert.Equal("MyNameIs", ((IProperty)property).Npgsql().ColumnName);

            property.Npgsql().ColumnName = null;

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", property.Relational().ColumnName);
            Assert.Equal("Name", property.Npgsql().ColumnName);
            Assert.Equal("Name", ((IProperty)property).Npgsql().ColumnName);
        }

        [Fact]
        public void Can_get_and_set_table_name()
        {
            var modelBuilder = GetModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Customer", entityType.Npgsql().TableName);
            Assert.Equal("Customer", ((IEntityType)entityType).Npgsql().TableName);

            entityType.Relational().TableName = "Customizer";

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().TableName);
            Assert.Equal("Customizer", entityType.Npgsql().TableName);
            Assert.Equal("Customizer", ((IEntityType)entityType).Npgsql().TableName);

            entityType.Npgsql().TableName = "Custardizer";

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Custardizer", entityType.Relational().TableName);
            Assert.Equal("Custardizer", entityType.Npgsql().TableName);
            Assert.Equal("Custardizer", ((IEntityType)entityType).Npgsql().TableName);

            entityType.Npgsql().TableName = null;

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customer", entityType.Relational().TableName);
            Assert.Equal("Customer", entityType.Npgsql().TableName);
            Assert.Equal("Customer", ((IEntityType)entityType).Npgsql().TableName);
        }

        [Fact]
        public void Can_get_and_set_schema_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Null(entityType.Relational().Schema);
            Assert.Null(entityType.Npgsql().Schema);
            Assert.Null(((IEntityType)entityType).Npgsql().Schema);

            entityType.Relational().Schema = "db0";

            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", entityType.Npgsql().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).Npgsql().Schema);

            entityType.Npgsql().Schema = "dbOh";

            Assert.Equal("dbOh", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.Npgsql().Schema);
            Assert.Equal("dbOh", ((IEntityType)entityType).Npgsql().Schema);

            entityType.Npgsql().Schema = null;

            Assert.Null(entityType.Relational().Schema);
            Assert.Null(entityType.Npgsql().Schema);
            Assert.Null(((IEntityType)entityType).Npgsql().Schema);
        }

        [Fact]
        public void Can_get_and_set_column_type()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().ColumnType);
            Assert.Null(property.Npgsql().ColumnType);
            Assert.Null(((IProperty)property).Npgsql().ColumnType);

            property.Relational().ColumnType = "nvarchar(max)";

            Assert.Equal("nvarchar(max)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(max)", property.Npgsql().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).Npgsql().ColumnType);

            property.Npgsql().ColumnType = "nvarchar(verstappen)";

            Assert.Equal("nvarchar(verstappen)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(verstappen)", property.Npgsql().ColumnType);
            Assert.Equal("nvarchar(verstappen)", ((IProperty)property).Npgsql().ColumnType);

            property.Npgsql().ColumnType = null;

            Assert.Null(property.Relational().ColumnType);
            Assert.Null(property.Npgsql().ColumnType);
            Assert.Null(((IProperty)property).Npgsql().ColumnType);
        }

        [Fact]
        public void Can_get_and_set_column_default_expression()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(property.Npgsql().DefaultValueSql);
            Assert.Null(((IProperty)property).Npgsql().DefaultValueSql);

            property.Relational().DefaultValueSql = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Relational().DefaultValueSql);
            Assert.Equal("newsequentialid()", property.Npgsql().DefaultValueSql);
            Assert.Equal("newsequentialid()", ((IProperty)property).Npgsql().DefaultValueSql);

            property.Npgsql().DefaultValueSql = "expressyourself()";

            Assert.Equal("expressyourself()", property.Relational().DefaultValueSql);
            Assert.Equal("expressyourself()", property.Npgsql().DefaultValueSql);
            Assert.Equal("expressyourself()", ((IProperty)property).Npgsql().DefaultValueSql);

            property.Npgsql().DefaultValueSql = null;

            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(property.Npgsql().DefaultValueSql);
            Assert.Null(((IProperty)property).Npgsql().DefaultValueSql);
        }

        [Fact]
        public void Can_get_and_set_column_computed_expression()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().ComputedColumnSql);
            Assert.Null(property.Npgsql().ComputedColumnSql);
            Assert.Null(((IProperty)property).Npgsql().ComputedColumnSql);

            property.Relational().ComputedColumnSql = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Relational().ComputedColumnSql);
            Assert.Equal("newsequentialid()", property.Npgsql().ComputedColumnSql);
            Assert.Equal("newsequentialid()", ((IProperty)property).Npgsql().ComputedColumnSql);

            property.Npgsql().ComputedColumnSql = "expressyourself()";

            Assert.Equal("expressyourself()", property.Relational().ComputedColumnSql);
            Assert.Equal("expressyourself()", property.Npgsql().ComputedColumnSql);
            Assert.Equal("expressyourself()", ((IProperty)property).Npgsql().ComputedColumnSql);

            property.Npgsql().ComputedColumnSql = null;

            Assert.Null(property.Relational().ComputedColumnSql);
            Assert.Null(property.Npgsql().ComputedColumnSql);
            Assert.Null(((IProperty)property).Npgsql().ComputedColumnSql);
        }

        [Fact]
        public void Can_get_and_set_column_default_value()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.ByteArray)
                .Metadata;

            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(property.Npgsql().DefaultValue);
            Assert.Null(((IProperty)property).Npgsql().DefaultValue);

            property.Relational().DefaultValue = new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 };

            Assert.Equal(new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 }, property.Relational().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 }, property.Npgsql().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 }, ((IProperty)property).Npgsql().DefaultValue);

            property.Npgsql().DefaultValue = new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 };

            Assert.Equal(new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 }, property.Relational().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 }, property.Npgsql().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 }, ((IProperty)property).Npgsql().DefaultValue);

            property.Npgsql().DefaultValue = null;

            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(property.Npgsql().DefaultValue);
            Assert.Null(((IProperty)property).Npgsql().DefaultValue);
        }

        [Theory]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValue), nameof(RelationalPropertyAnnotations.DefaultValueSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValue), nameof(RelationalPropertyAnnotations.ComputedColumnSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValue), nameof(NpgsqlPropertyAnnotations.ValueGenerationStrategy))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValueSql), nameof(RelationalPropertyAnnotations.DefaultValue))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValueSql), nameof(RelationalPropertyAnnotations.ComputedColumnSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValueSql), nameof(NpgsqlPropertyAnnotations.ValueGenerationStrategy))]
        [InlineData(nameof(RelationalPropertyAnnotations.ComputedColumnSql), nameof(RelationalPropertyAnnotations.DefaultValue))]
        [InlineData(nameof(RelationalPropertyAnnotations.ComputedColumnSql), nameof(RelationalPropertyAnnotations.DefaultValueSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.ComputedColumnSql), nameof(NpgsqlPropertyAnnotations.ValueGenerationStrategy))]
        [InlineData(nameof(NpgsqlPropertyAnnotations.ValueGenerationStrategy), nameof(RelationalPropertyAnnotations.DefaultValue))]
        [InlineData(nameof(NpgsqlPropertyAnnotations.ValueGenerationStrategy), nameof(RelationalPropertyAnnotations.DefaultValueSql))]
        [InlineData(nameof(NpgsqlPropertyAnnotations.ValueGenerationStrategy), nameof(RelationalPropertyAnnotations.ComputedColumnSql))]
        public void Metadata_throws_when_setting_conflicting_serverGenerated_values(string firstConfiguration, string secondConfiguration)
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var propertyBuilder = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id);

            ConfigureProperty(propertyBuilder.Metadata, firstConfiguration, "1");

            Assert.Equal(RelationalStrings.ConflictingColumnServerGeneration(secondConfiguration, nameof(Customer.Id), firstConfiguration),
                Assert.Throws<InvalidOperationException>(() =>
                    ConfigureProperty(propertyBuilder.Metadata, secondConfiguration, "2")).Message);
        }

        protected virtual void ConfigureProperty(IMutableProperty property, string configuration, string value)
        {
            var propertyAnnotations = property.Npgsql();
            switch (configuration)
            {
            case nameof(RelationalPropertyAnnotations.DefaultValue):
                property.ValueGenerated = ValueGenerated.OnAdd;
                propertyAnnotations.DefaultValue = int.Parse(value);
                break;
            case nameof(RelationalPropertyAnnotations.DefaultValueSql):
                property.ValueGenerated = ValueGenerated.OnAdd;
                propertyAnnotations.DefaultValueSql = value;
                break;
            case nameof(RelationalPropertyAnnotations.ComputedColumnSql):
                property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
                propertyAnnotations.ComputedColumnSql = value;
                break;
            case nameof(NpgsqlPropertyAnnotations.ValueGenerationStrategy):
                property.ValueGenerated = ValueGenerated.OnAdd;
                propertyAnnotations.ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SerialColumn;
                break;
            default:
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Can_get_and_set_column_key_name()
        {
            var modelBuilder = GetModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .Metadata;

            Assert.Equal("PK_Customer", key.Relational().Name);
            Assert.Equal("PK_Customer", key.Npgsql().Name);
            Assert.Equal("PK_Customer", ((IKey)key).Npgsql().Name);

            key.Relational().Name = "PrimaryKey";

            Assert.Equal("PrimaryKey", key.Relational().Name);
            Assert.Equal("PrimaryKey", key.Npgsql().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Npgsql().Name);

            key.Npgsql().Name = "PrimarySchool";

            Assert.Equal("PrimarySchool", key.Relational().Name);
            Assert.Equal("PrimarySchool", key.Npgsql().Name);
            Assert.Equal("PrimarySchool", ((IKey)key).Npgsql().Name);

            key.Npgsql().Name = null;

            Assert.Equal("PK_Customer", key.Relational().Name);
            Assert.Equal("PK_Customer", key.Npgsql().Name);
            Assert.Equal("PK_Customer", ((IKey)key).Npgsql().Name);
        }

        [Fact]
        public void Can_get_and_set_column_foreign_key_name()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id);

            var foreignKey = modelBuilder
                .Entity<Order>()
                .HasOne<Customer>()
                .WithOne()
                .HasForeignKey<Order>(e => e.CustomerId)
                .Metadata;

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Relational().Name);
            Assert.Equal("FK_Order_Customer_CustomerId", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = "FK";

            Assert.Equal("FK", foreignKey.Relational().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = "KFC";

            Assert.Equal("KFC", foreignKey.Relational().Name);
            Assert.Equal("KFC", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = null;

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Relational().Name);
            Assert.Equal("FK_Order_Customer_CustomerId", ((IForeignKey)foreignKey).Relational().Name);
        }

        [Fact]
        public void Can_get_and_set_index_name()
        {
            var modelBuilder = GetModelBuilder();

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .Metadata;

            Assert.Equal("IX_Customer_Id", index.Relational().Name);
            Assert.Equal("IX_Customer_Id", ((IIndex)index).Relational().Name);

            index.Relational().Name = "MyIndex";

            Assert.Equal("MyIndex", index.Relational().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Relational().Name);

            index.Npgsql().Name = "DexKnows";

            Assert.Equal("DexKnows", index.Relational().Name);
            Assert.Equal("DexKnows", ((IIndex)index).Relational().Name);

            index.Npgsql().Name = null;

            Assert.Equal("IX_Customer_Id", index.Relational().Name);
            Assert.Equal("IX_Customer_Id", ((IIndex)index).Relational().Name);
        }

        [Fact]
        public void Can_get_and_set_index_filter()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .Metadata;

            Assert.Null(index.Relational().Filter);
            Assert.Null(index.Npgsql().Filter);
            Assert.Null(((IIndex) index).Npgsql().Filter);

            index.Relational().Name = "Generic expression";

            Assert.Equal("Generic expression", index.Relational().Name);
            Assert.Equal("Generic expression", index.Npgsql().Name);
            Assert.Equal("Generic expression", ((IIndex) index).Npgsql().Name);

            index.Npgsql().Name = "PostgreSQL-specific expression";

            Assert.Equal("PostgreSQL-specific expression", index.Relational().Name);
            Assert.Equal("PostgreSQL-specific expression", index.Npgsql().Name);
            Assert.Equal("PostgreSQL-specific expression", ((IIndex) index).Npgsql().Name);

            index.Npgsql().Name = null;

            Assert.Null(index.Relational().Filter);
            Assert.Null(index.Npgsql().Filter);
            Assert.Null(((IIndex) index).Npgsql().Filter);
        }

        [Fact]
        public void Can_get_and_set_sequence()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.Relational().FindSequence("Foo"));
            Assert.Null(model.Npgsql().FindSequence("Foo"));
            Assert.Null(((IModel)model).Npgsql().FindSequence("Foo"));

            var sequence = model.Npgsql().GetOrAddSequence("Foo");

            Assert.Equal("Foo", model.Relational().FindSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).Relational().FindSequence("Foo").Name);
            Assert.Equal("Foo", model.Npgsql().FindSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).Npgsql().FindSequence("Foo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            Assert.NotNull(model.Relational().FindSequence("Foo"));

            var sequence2 = model.Npgsql().FindSequence("Foo");

            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.MinValue = 2001;
            sequence.MaxValue = 2010;
            sequence.ClrType = typeof(int);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);

            Assert.Equal(sequence2.Name, sequence.Name);
            Assert.Equal(sequence2.Schema, sequence.Schema);
            Assert.Equal(sequence2.IncrementBy, sequence.IncrementBy);
            Assert.Equal(sequence2.StartValue, sequence.StartValue);
            Assert.Equal(sequence2.MinValue, sequence.MinValue);
            Assert.Equal(sequence2.MaxValue, sequence.MaxValue);
            Assert.Same(sequence2.ClrType, sequence.ClrType);
        }

        [Fact]
        public void Can_get_and_set_sequence_with_schema_name()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.Relational().FindSequence("Foo", "Smoo"));
            Assert.Null(model.Npgsql().FindSequence("Foo", "Smoo"));
            Assert.Null(((IModel)model).Npgsql().FindSequence("Foo", "Smoo"));

            var sequence = model.Npgsql().GetOrAddSequence("Foo", "Smoo");

            Assert.Equal("Foo", model.Relational().FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).Relational().FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", model.Npgsql().FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).Npgsql().FindSequence("Foo", "Smoo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            Assert.NotNull(model.Relational().FindSequence("Foo", "Smoo"));

            var sequence2 = model.Npgsql().FindSequence("Foo", "Smoo");

            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.MinValue = 2001;
            sequence.MaxValue = 2010;
            sequence.ClrType = typeof(int);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);

            Assert.Equal(sequence2.Name, sequence.Name);
            Assert.Equal(sequence2.Schema, sequence.Schema);
            Assert.Equal(sequence2.IncrementBy, sequence.IncrementBy);
            Assert.Equal(sequence2.StartValue, sequence.StartValue);
            Assert.Equal(sequence2.MinValue, sequence.MinValue);
            Assert.Equal(sequence2.MaxValue, sequence.MaxValue);
            Assert.Same(sequence2.ClrType, sequence.ClrType);
        }

        [Fact]
        public void Can_get_multiple_sequences()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;

            model.Relational().GetOrAddSequence("Fibonacci");
            model.Npgsql().GetOrAddSequence("Golomb");

            var sequences = model.Npgsql().Sequences;

            Assert.Equal(2, sequences.Count);
            Assert.Contains(sequences, s => s.Name == "Fibonacci");
            Assert.Contains(sequences, s => s.Name == "Golomb");
        }

        [Fact]
        public void Can_get_multiple_sequences_when_overridden()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;

            model.Relational().GetOrAddSequence("Fibonacci").StartValue = 1;
            model.Npgsql().GetOrAddSequence("Fibonacci").StartValue = 3;
            model.Npgsql().GetOrAddSequence("Golomb");

            var sequences = model.Npgsql().Sequences;

            Assert.Equal(2, sequences.Count);
            Assert.Contains(sequences, s => s.Name == "Golomb");

            var sequence = sequences.FirstOrDefault(s => s.Name == "Fibonacci");
            Assert.NotNull(sequence);
            Assert.Equal(3, sequence.StartValue);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_model()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;

            Assert.Null(model.Npgsql().ValueGenerationStrategy);
            Assert.Null(((IModel)model).Npgsql().ValueGenerationStrategy);

            model.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;

            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, model.Npgsql().ValueGenerationStrategy);
            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, ((IModel)model).Npgsql().ValueGenerationStrategy);

            model.Npgsql().ValueGenerationStrategy = null;

            Assert.Null(model.Npgsql().ValueGenerationStrategy);
            Assert.Null(((IModel)model).Npgsql().ValueGenerationStrategy);
        }

        [Fact]
        public void Can_get_and_set_default_sequence_name_on_model()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;

            Assert.Null(model.Npgsql().HiLoSequenceName);
            Assert.Null(((IModel)model).Npgsql().HiLoSequenceName);

            model.Npgsql().HiLoSequenceName = "Tasty.Snook";

            Assert.Equal("Tasty.Snook", model.Npgsql().HiLoSequenceName);
            Assert.Equal("Tasty.Snook", ((IModel)model).Npgsql().HiLoSequenceName);

            model.Npgsql().HiLoSequenceName = null;

            Assert.Null(model.Npgsql().HiLoSequenceName);
            Assert.Null(((IModel)model).Npgsql().HiLoSequenceName);
        }

        [Fact]
        public void Can_get_and_set_default_sequence_schema_on_model()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;

            Assert.Null(model.Npgsql().HiLoSequenceSchema);
            Assert.Null(((IModel)model).Npgsql().HiLoSequenceSchema);

            model.Npgsql().HiLoSequenceSchema = "Tasty.Snook";

            Assert.Equal("Tasty.Snook", model.Npgsql().HiLoSequenceSchema);
            Assert.Equal("Tasty.Snook", ((IModel)model).Npgsql().HiLoSequenceSchema);

            model.Npgsql().HiLoSequenceSchema = null;

            Assert.Null(model.Npgsql().HiLoSequenceSchema);
            Assert.Null(((IModel)model).Npgsql().HiLoSequenceSchema);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_property()
        {
            var modelBuilder = GetModelBuilder();
            modelBuilder.Model.Npgsql().ValueGenerationStrategy = null;

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.Npgsql().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;

            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, property.Npgsql().ValueGenerationStrategy);
            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, ((IProperty)property).Npgsql().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            property.Npgsql().ValueGenerationStrategy = null;

            Assert.Null(property.Npgsql().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_nullable_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.NullableInt)
                .Metadata;

            Assert.Null(property.Npgsql().ValueGenerationStrategy);

            property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;

            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, property.Npgsql().ValueGenerationStrategy);
            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, ((IProperty)property).Npgsql().ValueGenerationStrategy);

            property.Npgsql().ValueGenerationStrategy = null;

            Assert.Null(property.Npgsql().ValueGenerationStrategy);
        }

        [Fact]
        public void Throws_setting_sequence_generation_for_invalid_type()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Throws<ArgumentException>(
                () => property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo);
        }

        [Fact]
        public void Throws_setting_identity_generation_for_invalid_type()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Throws<ArgumentException>(
                () => property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SerialColumn);
        }

        [Fact]
        public void Throws_setting_identity_generation_for_byte_property()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Byte)
                .Metadata;

            Assert.Throws<ArgumentException>(
                () => property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SerialColumn);
        }

        [Fact]
        public void Throws_setting_identity_generation_for_nullable_byte_property()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.NullableByte)
                .Metadata;

            Assert.Throws<ArgumentException>(
                () => property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SerialColumn);
        }

        [Fact]
        public void Can_get_and_set_sequence_name_on_property()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.Npgsql().HiLoSequenceName);
            Assert.Null(((IProperty)property).Npgsql().HiLoSequenceName);

            property.Npgsql().HiLoSequenceName = "Snook";

            Assert.Equal("Snook", property.Npgsql().HiLoSequenceName);
            Assert.Equal("Snook", ((IProperty)property).Npgsql().HiLoSequenceName);

            property.Npgsql().HiLoSequenceName = null;

            Assert.Null(property.Npgsql().HiLoSequenceName);
            Assert.Null(((IProperty)property).Npgsql().HiLoSequenceName);
        }

        [Fact]
        public void Can_get_and_set_sequence_schema_on_property()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.Npgsql().HiLoSequenceSchema);
            Assert.Null(((IProperty)property).Npgsql().HiLoSequenceSchema);

            property.Npgsql().HiLoSequenceSchema = "Tasty";

            Assert.Equal("Tasty", property.Npgsql().HiLoSequenceSchema);
            Assert.Equal("Tasty", ((IProperty)property).Npgsql().HiLoSequenceSchema);

            property.Npgsql().HiLoSequenceSchema = null;

            Assert.Null(property.Npgsql().HiLoSequenceSchema);
            Assert.Null(((IProperty)property).Npgsql().HiLoSequenceSchema);
        }

        [Fact]
        public void TryGetSequence_returns_null_if_property_is_not_configured_for_sequence_value_generation()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.Npgsql().GetOrAddSequence("DaneelOlivaw");

            Assert.Null(property.Npgsql().FindHiLoSequence());
            Assert.Null(((IProperty)property).Npgsql().FindHiLoSequence());

            property.Npgsql().HiLoSequenceName = "DaneelOlivaw";

            Assert.Null(property.Npgsql().FindHiLoSequence());
            Assert.Null(((IProperty)property).Npgsql().FindHiLoSequence());

            modelBuilder.Model.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SerialColumn;

            Assert.Null(property.Npgsql().FindHiLoSequence());
            Assert.Null(((IProperty)property).Npgsql().FindHiLoSequence());

            modelBuilder.Model.Npgsql().ValueGenerationStrategy = null;
            property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SerialColumn;

            Assert.Null(property.Npgsql().FindHiLoSequence());
            Assert.Null(((IProperty)property).Npgsql().FindHiLoSequence());
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Npgsql().GetOrAddSequence("DaneelOlivaw");
            property.Npgsql().HiLoSequenceName = "DaneelOlivaw";
            property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.Npgsql().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Npgsql().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Npgsql().GetOrAddSequence("DaneelOlivaw");
            modelBuilder.Model.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;
            property.Npgsql().HiLoSequenceName = "DaneelOlivaw";

            Assert.Equal("DaneelOlivaw", property.Npgsql().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Npgsql().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Npgsql().GetOrAddSequence("DaneelOlivaw");
            modelBuilder.Model.Npgsql().HiLoSequenceName = "DaneelOlivaw";
            property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.Npgsql().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Npgsql().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Npgsql().GetOrAddSequence("DaneelOlivaw");
            modelBuilder.Model.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;
            modelBuilder.Model.Npgsql().HiLoSequenceName = "DaneelOlivaw";

            Assert.Equal("DaneelOlivaw", property.Npgsql().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Npgsql().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Npgsql().GetOrAddSequence("DaneelOlivaw", "R");
            property.Npgsql().HiLoSequenceName = "DaneelOlivaw";
            property.Npgsql().HiLoSequenceSchema = "R";
            property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.Npgsql().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Npgsql().FindHiLoSequence().Name);
            Assert.Equal("R", property.Npgsql().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).Npgsql().FindHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Npgsql().GetOrAddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;
            property.Npgsql().HiLoSequenceName = "DaneelOlivaw";
            property.Npgsql().HiLoSequenceSchema = "R";

            Assert.Equal("DaneelOlivaw", property.Npgsql().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Npgsql().FindHiLoSequence().Name);
            Assert.Equal("R", property.Npgsql().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).Npgsql().FindHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Npgsql().GetOrAddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.Npgsql().HiLoSequenceName = "DaneelOlivaw";
            modelBuilder.Model.Npgsql().HiLoSequenceSchema = "R";
            property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.Npgsql().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Npgsql().FindHiLoSequence().Name);
            Assert.Equal("R", property.Npgsql().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).Npgsql().FindHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.Npgsql().GetOrAddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;
            modelBuilder.Model.Npgsql().HiLoSequenceName = "DaneelOlivaw";
            modelBuilder.Model.Npgsql().HiLoSequenceSchema = "R";

            Assert.Equal("DaneelOlivaw", property.Npgsql().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).Npgsql().FindHiLoSequence().Name);
            Assert.Equal("R", property.Npgsql().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).Npgsql().FindHiLoSequence().Schema);
        }

        private static ModelBuilder GetModelBuilder() => NpgsqlTestHelpers.Instance.CreateConventionBuilder();

        private class Customer
        {
            public int Id { get; set; }
            public int? NullableInt { get; set; }
            public string Name { get; set; }
            public byte Byte { get; set; }
            public byte? NullableByte { get; set; }
            public byte[] ByteArray { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
