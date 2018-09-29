using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
                new TypeMappingSourceDependencies (
                    new ValueConverterSelector(new ValueConverterSelectorDependencies()),
                    Array.Empty<ITypeMappingSourcePlugin>()
                ),
                new RelationalTypeMappingSourceDependencies(Array.Empty<IRelationalTypeMappingSourcePlugin>()),
                new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                new NpgsqlOptions()
            );
            var factory = new NpgsqlModificationCommandBatchFactory(
                new RelationalCommandBuilderFactory(
                    new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>(),
                    typeMapper),
                new RelationalSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new NpgsqlUpdateSqlGenerator(
                    new UpdateSqlGeneratorDependencies(
                        new RelationalSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies()),
                        typeMapper)),
                new TypedRelationalValueBufferFactoryFactory(
                    new RelationalValueBufferFactoryDependencies(
                        typeMapper, new CoreSingletonOptions())),
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
                new TypeMappingSourceDependencies (
                    new ValueConverterSelector(new ValueConverterSelectorDependencies()),
                    Array.Empty<ITypeMappingSourcePlugin>()
                ),
                new RelationalTypeMappingSourceDependencies(Array.Empty<IRelationalTypeMappingSourcePlugin>()),
                new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                new NpgsqlOptions()
            );
            var factory = new NpgsqlModificationCommandBatchFactory(
                new RelationalCommandBuilderFactory(
                    new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>(),
                    typeMapper),
                new RelationalSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new NpgsqlUpdateSqlGenerator(
                    new UpdateSqlGeneratorDependencies(
                        new RelationalSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies()),
                        typeMapper)),
                new TypedRelationalValueBufferFactoryFactory(
                    new RelationalValueBufferFactoryDependencies(
                        typeMapper, new CoreSingletonOptions())),
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
        }
    }
}
