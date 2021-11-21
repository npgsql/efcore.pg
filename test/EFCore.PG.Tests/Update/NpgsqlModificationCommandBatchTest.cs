using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Tests.Update;

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

        var logger = new FakeRelationalCommandDiagnosticsLogger();

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
                CreateModificationCommand("T1", null, false)));
        Assert.False(
            batch.AddCommand(
                CreateModificationCommand("T1", null, false)));
    }

    private class FakeDbContext : DbContext
    {
    }

    private static IModificationCommand CreateModificationCommand(
        string name,
        string schema,
        bool sensitiveLoggingEnabled)
    {
        var modificationCommandParameters = new ModificationCommandParameters(
            name, schema, sensitiveLoggingEnabled);

        var modificationCommand = new ModificationCommandFactory().CreateModificationCommand(
            modificationCommandParameters);

        return modificationCommand;
    }
}