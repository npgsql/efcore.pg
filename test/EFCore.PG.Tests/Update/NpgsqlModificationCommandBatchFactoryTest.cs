using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Update
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
                    new ValueConverterSelector(new ValueConverterSelectorDependencies())
                ),
                new RelationalTypeMappingSourceDependencies ()
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
                    new RelationalValueBufferFactoryDependencies(typeMapper)),
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
                    new ValueConverterSelector(new ValueConverterSelectorDependencies())
                ),
                new RelationalTypeMappingSourceDependencies()
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
                    new RelationalValueBufferFactoryDependencies(typeMapper)),
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, false, null)));
        }
    }
}
