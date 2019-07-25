using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Tests.Update
{
    public class NpgsqlModificationCommandBatchTest
    {
        [Fact]
        public void AddCommand_returns_false_when_max_batch_size_is_reached()
        {
            var typeMapper = new NpgsqlTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
                new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                new NpgsqlOptions());

            var logger = new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>();

            var batch = new NpgsqlModificationCommandBatch(
                new ModificationCommandBatchFactoryDependencies(
                    new RelationalCommandBuilderFactory(
                        new RelationalCommandBuilderDependencies(
                            typeMapper)),
                    new NpgsqlSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies()),
                    new NpgsqlUpdateSqlGenerator(
                        new UpdateSqlGeneratorDependencies(
                            new NpgsqlSqlGenerationHelper(
                                new RelationalSqlGenerationHelperDependencies()),
                            typeMapper)),
                    new TypedRelationalValueBufferFactoryFactory(
                        new RelationalValueBufferFactoryDependencies(
                            typeMapper, new CoreSingletonOptions())),
                    new CurrentDbContext(new FakeDbContext()),
                    logger),
                1);

            Assert.True(
                batch.AddCommand(
                    new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
            Assert.False(
                batch.AddCommand(
                    new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
        }

        class FakeDbContext : DbContext
        {
        }
    }
}
