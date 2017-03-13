using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Tests.Update
{
    public class NpgsqlModificationCommandBatchTest
    {
        [Fact]
        public void AddCommand_returns_false_when_max_batch_size_is_reached()
        {
            var batch = new NpgsqlModificationCommandBatch(
                new RelationalCommandBuilderFactory(
                    new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                    new DiagnosticListener("Fake"),
                    new NpgsqlTypeMapper(new RelationalTypeMapperDependencies())),
                new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                new NpgsqlUpdateSqlGenerator(
                    new UpdateSqlGeneratorDependencies(
                        new NpgsqlSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies()))
                    ),
                new TypedRelationalValueBufferFactoryFactory(
                    new RelationalValueBufferFactoryDependencies()),
                1);

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, p => p.Npgsql())));
            Assert.False(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, p => p.Npgsql())));
        }
    }
}
