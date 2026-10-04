namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NpgsqlFuzzyStringMatchMethodTranslatorTest
{
    [Theory]
    [InlineData(16)]
    [InlineData(18)]
    public void Daitch_mokotoff_returns_mapped_text_array(int postgresVersion)
    {
        using var context = new DbContext(
            new DbContextOptionsBuilder().UseNpgsql(o => o.SetPostgresVersion(postgresVersion, 0)).Options);
        var sqlExpressionFactory = context.GetService<ISqlExpressionFactory>();
        var translator = context.GetService<IMethodCallTranslatorProvider>();
        var method = typeof(NpgsqlFuzzyStringMatchDbFunctionsExtensions)
            .GetMethod(nameof(NpgsqlFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchDaitchMokotoff))!;

        var translation = Assert.IsType<SqlFunctionExpression>(
            translator.Translate(
                context.Model,
                null,
                method,
                [sqlExpressionFactory.Constant(EF.Functions), sqlExpressionFactory.Constant("John")],
                context.GetService<IDiagnosticsLogger<DbLoggerCategory.Query>>()));

        Assert.Equal("daitch_mokotoff", translation.Name);
        Assert.Equal(typeof(string[]), translation.Type);
        Assert.Same(context.GetService<IRelationalTypeMappingSource>().FindMapping(typeof(string[])), translation.TypeMapping);
        Assert.Equal("text[]", translation.TypeMapping!.StoreType);
        Assert.Single(translation.Arguments!);
        Assert.True(translation.IsNullable);
        Assert.Equal(new[] { false }, translation.ArgumentsPropagateNullability);
    }

    [Fact]
    public void Daitch_mokotoff_is_not_translated_before_PostgreSQL_16()
    {
        using var context = new DbContext(new DbContextOptionsBuilder().UseNpgsql(o => o.SetPostgresVersion(15, 0)).Options);
        var sqlExpressionFactory = context.GetService<ISqlExpressionFactory>();
        var translator = context.GetService<IMethodCallTranslatorProvider>();
        var method = typeof(NpgsqlFuzzyStringMatchDbFunctionsExtensions)
            .GetMethod(nameof(NpgsqlFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchDaitchMokotoff))!;

        Assert.Null(
            translator.Translate(
                context.Model,
                null,
                method,
                [sqlExpressionFactory.Constant(EF.Functions), sqlExpressionFactory.Constant("John")],
                context.GetService<IDiagnosticsLogger<DbLoggerCategory.Query>>()));
    }

    [Fact]
    public void Daitch_mokotoff_throws_on_client()
        => Assert.Throws<InvalidOperationException>(() => EF.Functions.FuzzyStringMatchDaitchMokotoff("John"));
}
