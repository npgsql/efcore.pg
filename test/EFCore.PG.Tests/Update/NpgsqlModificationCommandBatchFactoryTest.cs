using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update
{
    public class NpgsqlModificationCommandBatchFactoryTest
    {
        [Fact]
        public void Uses_MaxBatchSize_specified_in_NpgsqlOptionsExtension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql("Database=Crunchie", b => b.MaxBatchSize(1));

            var typeMapper = new NpgsqlTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
                new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                new NpgsqlOptions());

            var logger = new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>();

            var factory = new NpgsqlModificationCommandBatchFactory(
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
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
            Assert.False(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
        }

        [Fact]
        public void MaxBatchSize_is_optional()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql("Database=Crunchie");

            var typeMapper = new NpgsqlTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
                new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                new NpgsqlOptions());

            var logger = new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>();

            var factory = new NpgsqlModificationCommandBatchFactory(
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
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
        }

        class FakeDbContext : DbContext
        {
        }
    }
}
