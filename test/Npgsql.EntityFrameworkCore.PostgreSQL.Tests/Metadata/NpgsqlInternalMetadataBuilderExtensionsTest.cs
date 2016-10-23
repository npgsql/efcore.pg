// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Tests.Metadata
{
    public class NpgsqlInternalMetadataBuilderExtensionsTest
    {
        private InternalModelBuilder CreateBuilder()
            => new InternalModelBuilder(new Model());

        [Fact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            Assert.True(builder.Npgsql(ConfigurationSource.Convention).ValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, builder.Metadata.Npgsql().ValueGenerationStrategy);

            Assert.True(builder.Npgsql(ConfigurationSource.DataAnnotation).ValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn));
            Assert.Equal(NpgsqlValueGenerationStrategy.SerialColumn, builder.Metadata.Npgsql().ValueGenerationStrategy);

            Assert.False(builder.Npgsql(ConfigurationSource.Convention).ValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(NpgsqlValueGenerationStrategy.SerialColumn, builder.Metadata.Npgsql().ValueGenerationStrategy);

            Assert.Equal(1, builder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(NpgsqlAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_entity_type()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            Assert.True(typeBuilder.Npgsql(ConfigurationSource.Convention).ToTable("Splew"));
            Assert.Equal("Splew", typeBuilder.Metadata.Npgsql().TableName);

            Assert.True(typeBuilder.Npgsql(ConfigurationSource.DataAnnotation).ToTable("Splow"));
            Assert.Equal("Splow", typeBuilder.Metadata.Npgsql().TableName);

            Assert.False(typeBuilder.Npgsql(ConfigurationSource.Convention).ToTable("Splod"));
            Assert.Equal("Splow", typeBuilder.Metadata.Npgsql().TableName);

            Assert.Equal(1, typeBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(NpgsqlAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot), ConfigurationSource.Convention)
                .Property("Id", typeof(int), ConfigurationSource.Convention);

            Assert.True(propertyBuilder.Npgsql(ConfigurationSource.Convention).HiLoSequenceName("Splew"));
            Assert.Equal("Splew", propertyBuilder.Metadata.Npgsql().HiLoSequenceName);

            Assert.True(propertyBuilder.Npgsql(ConfigurationSource.DataAnnotation).HiLoSequenceName("Splow"));
            Assert.Equal("Splow", propertyBuilder.Metadata.Npgsql().HiLoSequenceName);

            Assert.False(propertyBuilder.Npgsql(ConfigurationSource.Convention).HiLoSequenceName("Splod"));
            Assert.Equal("Splow", propertyBuilder.Metadata.Npgsql().HiLoSequenceName);

            Assert.Equal(1, propertyBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(NpgsqlAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_relationship()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var relationshipBuilder = entityTypeBuilder.HasForeignKey("Splot", new[] { "Id" }, ConfigurationSource.Convention);

            Assert.True(relationshipBuilder.Npgsql(ConfigurationSource.Convention).HasConstraintName("Splew"));
            Assert.Equal("Splew", relationshipBuilder.Metadata.Npgsql().Name);

            Assert.True(relationshipBuilder.Npgsql(ConfigurationSource.DataAnnotation).HasConstraintName("Splow"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.Npgsql().Name);

            Assert.False(relationshipBuilder.Npgsql(ConfigurationSource.Convention).HasConstraintName("Splod"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.Npgsql().Name);

            Assert.Equal(1, relationshipBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(NpgsqlAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        private class Splot
        {
        }
    }
}
