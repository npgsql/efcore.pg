// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Tests;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Tests.Metadata.Conventions
{
    public class NpgsqlValueGenerationStrategyConventionTest
    {
        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used()
        {
            var model = NpgsqlTestHelpers.Instance.CreateConventionBuilder().Model;

            Assert.Equal(1, model.GetAnnotations().Count());

            Assert.Equal(NpgsqlFullAnnotationNames.Instance.ValueGenerationStrategy, model.GetAnnotations().Single().Name);
            Assert.Equal(NpgsqlValueGenerationStrategy.SerialColumn, model.GetAnnotations().Single().Value);
        }

        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used_with_sequences()
        {
            var model = NpgsqlTestHelpers.Instance.CreateConventionBuilder()
                .ForNpgsqlUseSequenceHiLo()
                .Model;

            var annotations = model.GetAnnotations().OrderBy(a => a.Name);
            Assert.Equal(3, annotations.Count());

            Assert.Equal(NpgsqlFullAnnotationNames.Instance.HiLoSequenceName, annotations.ElementAt(0).Name);
            Assert.Equal(NpgsqlModelAnnotations.DefaultHiLoSequenceName, annotations.ElementAt(0).Value);

            Assert.Equal(
                NpgsqlFullAnnotationNames.Instance.SequencePrefix +
                "." +
                NpgsqlModelAnnotations.DefaultHiLoSequenceName,
                annotations.ElementAt(1).Name);
            Assert.NotNull(annotations.ElementAt(1).Value);

            Assert.Equal(NpgsqlFullAnnotationNames.Instance.ValueGenerationStrategy, annotations.ElementAt(2).Name);
            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, annotations.ElementAt(2).Value);
        }
    }
}
