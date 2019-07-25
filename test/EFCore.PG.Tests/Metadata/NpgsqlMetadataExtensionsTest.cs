using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class NpgsqlMetadataExtensionsTest
    {
        [ConditionalFact]
        public void Can_get_and_set_column_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal("Name", property.GetColumnName());

            property.SetColumnName("Eman");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.GetColumnName());

            property.SetColumnName("MyNameIs");

            Assert.Equal("Name", property.Name);
            Assert.Equal("MyNameIs", property.GetColumnName());

            property.SetColumnName(null);

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", property.GetColumnName());
        }


        [ConditionalFact]
        public void Can_get_and_set_column_key_name()
        {
            var modelBuilder = GetModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .Metadata;

            Assert.Equal("PK_Customer", key.GetName());

            key.SetName("PrimaryKey");

            Assert.Equal("PrimaryKey", key.GetName());

            key.SetName("PrimarySchool");

            Assert.Equal("PrimarySchool", key.GetName());

            key.SetName(null);

            Assert.Equal("PK_Customer", key.GetName());
        }

        [ConditionalFact]
        public void Can_get_and_set_index_method()
        {
            var modelBuilder = GetModelBuilder();

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .Metadata;

            Assert.Null(index.GetNpgsqlMethod());

            index.SetNpgsqlMethod("gin");

            Assert.Equal("gin", index.GetNpgsqlMethod());

            index.SetNpgsqlMethod(null);

            Assert.Null(index.GetNpgsqlMethod());
        }

        [ConditionalFact]
        public void Can_get_and_set_sequence()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.FindSequence("Foo"));
            Assert.Null(model.FindSequence("Foo"));
            Assert.Null(((IModel)model).FindSequence("Foo"));

            var sequence = model.AddSequence("Foo");

            Assert.Equal("Foo", model.FindSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).FindSequence("Foo").Name);
            Assert.Equal("Foo", model.FindSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).FindSequence("Foo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            Assert.NotNull(model.FindSequence("Foo"));

            var sequence2 = model.FindSequence("Foo");

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

        [ConditionalFact]
        public void Can_get_and_set_sequence_with_schema_name()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.FindSequence("Foo", "Smoo"));
            Assert.Null(model.FindSequence("Foo", "Smoo"));
            Assert.Null(((IModel)model).FindSequence("Foo", "Smoo"));

            var sequence = model.AddSequence("Foo", "Smoo");

            Assert.Equal("Foo", model.FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", model.FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).FindSequence("Foo", "Smoo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            Assert.NotNull(model.FindSequence("Foo", "Smoo"));

            var sequence2 = model.FindSequence("Foo", "Smoo");

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

        [ConditionalFact]
        public void Can_get_multiple_sequences()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            model.AddSequence("Fibonacci");
            model.AddSequence("Golomb");

            var sequences = model.GetSequences();

            Assert.Equal(2, sequences.Count);
            Assert.Contains(sequences, s => s.Name == "Fibonacci");
            Assert.Contains(sequences, s => s.Name == "Golomb");
        }

        [ConditionalFact]
        public void Can_get_multiple_sequences_when_overridden()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            model.AddSequence("Fibonacci").StartValue = 1;
            model.FindSequence("Fibonacci").StartValue = 3;
            model.AddSequence("Golomb");

            var sequences = model.GetSequences();

            Assert.Equal(2, sequences.Count);
            Assert.Contains(sequences, s => s.Name == "Golomb");

            var sequence = sequences.FirstOrDefault(s => s.Name == "Fibonacci");
            Assert.NotNull(sequence);
            Assert.Equal(3, sequence.StartValue);
        }

        [ConditionalFact]
        public void Can_get_and_set_value_generation_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Equal(NpgsqlValueGenerationStrategy.SerialColumn, model.GetNpgsqlValueGenerationStrategy());

            model.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);

            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, model.GetNpgsqlValueGenerationStrategy());

            model.SetNpgsqlValueGenerationStrategy(null);

            Assert.Null(model.GetNpgsqlValueGenerationStrategy());
        }

        [ConditionalFact]
        public void Can_get_and_set_default_sequence_name_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            model.SetNpgsqlHiLoSequenceName("Tasty.Snook");

            Assert.Equal("Tasty.Snook", model.GetNpgsqlHiLoSequenceName());

            model.SetNpgsqlHiLoSequenceName(null);

            Assert.Equal(NpgsqlModelExtensions.DefaultHiLoSequenceName, model.GetNpgsqlHiLoSequenceName());
        }

        [ConditionalFact]
        public void Can_get_and_set_default_sequence_schema_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.GetNpgsqlHiLoSequenceSchema());

            model.SetNpgsqlHiLoSequenceSchema("Tasty.Snook");

            Assert.Equal("Tasty.Snook", model.GetNpgsqlHiLoSequenceSchema());

            model.SetNpgsqlHiLoSequenceSchema(null);

            Assert.Null(model.GetNpgsqlHiLoSequenceSchema());
        }

        [ConditionalFact]
        public void Can_get_and_set_value_generation_on_property()
        {
            var modelBuilder = GetModelBuilder();
            modelBuilder.Model.SetNpgsqlValueGenerationStrategy(null);

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Equal(NpgsqlValueGenerationStrategy.None, property.GetNpgsqlValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);

            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, property.GetNpgsqlValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            property.SetNpgsqlValueGenerationStrategy(null);

            Assert.Equal(NpgsqlValueGenerationStrategy.None, property.GetNpgsqlValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Can_get_and_set_value_generation_on_nullable_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.NullableInt).ValueGeneratedOnAdd()
                .Metadata;

            Assert.Equal(NpgsqlValueGenerationStrategy.SerialColumn, property.GetNpgsqlValueGenerationStrategy());

            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);

            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, property.GetNpgsqlValueGenerationStrategy());

            property.SetNpgsqlValueGenerationStrategy(null);

            Assert.Equal(NpgsqlValueGenerationStrategy.SerialColumn, property.GetNpgsqlValueGenerationStrategy());
        }

        [ConditionalFact]
        public void Can_get_and_set_sequence_name_on_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.GetNpgsqlHiLoSequenceName());
            Assert.Null(((IProperty)property).GetNpgsqlHiLoSequenceName());

            property.SetNpgsqlHiLoSequenceName("Snook");

            Assert.Equal("Snook", property.GetNpgsqlHiLoSequenceName());

            property.SetNpgsqlHiLoSequenceName(null);

            Assert.Null(property.GetNpgsqlHiLoSequenceName());
        }

        [ConditionalFact]
        public void Can_get_and_set_sequence_schema_on_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.GetNpgsqlHiLoSequenceSchema());

            property.SetNpgsqlHiLoSequenceSchema("Tasty");

            Assert.Equal("Tasty", property.GetNpgsqlHiLoSequenceSchema());

            property.SetNpgsqlHiLoSequenceSchema(null);

            Assert.Null(property.GetNpgsqlHiLoSequenceSchema());
        }

        [ConditionalFact]
        public void TryGetSequence_returns_null_if_property_is_not_configured_for_sequence_value_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw");

            Assert.Null(property.FindNpgsqlHiLoSequence());

            property.SetNpgsqlHiLoSequenceName("DaneelOlivaw");

            Assert.Null(property.FindNpgsqlHiLoSequence());

            modelBuilder.Model.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            Assert.Null(property.FindNpgsqlHiLoSequence());

            modelBuilder.Model.SetNpgsqlValueGenerationStrategy(null);
            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            Assert.Null(property.FindNpgsqlHiLoSequence());
        }

        [ConditionalFact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw");
            property.SetNpgsqlHiLoSequenceName("DaneelOlivaw");
            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);

            Assert.Equal("DaneelOlivaw", property.FindNpgsqlHiLoSequence().Name);
        }

        [ConditionalFact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw");
            modelBuilder.Model.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);
            property.SetNpgsqlHiLoSequenceName("DaneelOlivaw");

            Assert.Equal("DaneelOlivaw", property.FindNpgsqlHiLoSequence().Name);
        }

        [ConditionalFact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw");
            modelBuilder.Model.SetNpgsqlHiLoSequenceName("DaneelOlivaw");
            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);

            Assert.Equal("DaneelOlivaw", property.FindNpgsqlHiLoSequence().Name);
        }

        [ConditionalFact]
        public void
            TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw");
            modelBuilder.Model.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);
            modelBuilder.Model.SetNpgsqlHiLoSequenceName("DaneelOlivaw");

            Assert.Equal("DaneelOlivaw", property.FindNpgsqlHiLoSequence().Name);
        }

        [ConditionalFact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
            property.SetNpgsqlHiLoSequenceName("DaneelOlivaw");
            property.SetNpgsqlHiLoSequenceSchema("R");
            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);

            Assert.Equal("DaneelOlivaw", property.FindNpgsqlHiLoSequence().Name);
            Assert.Equal("R", property.FindNpgsqlHiLoSequence().Schema);
        }

        [ConditionalFact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);
            property.SetNpgsqlHiLoSequenceName("DaneelOlivaw");
            property.SetNpgsqlHiLoSequenceSchema("R");

            Assert.Equal("DaneelOlivaw", property.FindNpgsqlHiLoSequence().Name);
            Assert.Equal("R", property.FindNpgsqlHiLoSequence().Schema);
        }

        [ConditionalFact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.SetNpgsqlHiLoSequenceName("DaneelOlivaw");
            modelBuilder.Model.SetNpgsqlHiLoSequenceSchema("R");
            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);

            Assert.Equal("DaneelOlivaw", property.FindNpgsqlHiLoSequence().Name);
            Assert.Equal("R", property.FindNpgsqlHiLoSequence().Schema);
        }

        [ConditionalFact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);
            modelBuilder.Model.SetNpgsqlHiLoSequenceName("DaneelOlivaw");
            modelBuilder.Model.SetNpgsqlHiLoSequenceSchema("R");

            Assert.Equal("DaneelOlivaw", property.FindNpgsqlHiLoSequence().Name);
            Assert.Equal("R", property.FindNpgsqlHiLoSequence().Schema);
        }

        private static ModelBuilder GetModelBuilder() => NpgsqlTestHelpers.Instance.CreateConventionBuilder();

        // ReSharper disable once ClassNeverInstantiated.Local
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
