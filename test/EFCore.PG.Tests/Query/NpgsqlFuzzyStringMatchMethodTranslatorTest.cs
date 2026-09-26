namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NpgsqlFuzzyStringMatchMethodTranslatorTest
{
    [Fact]
    public void Daitch_mokotoff_returns_mapped_text_array()
    {
        using var context = new DbContext(new DbContextOptionsBuilder().UseNpgsql().Options);
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
    public void Daitch_mokotoff_throws_on_client()
        => Assert.Throws<InvalidOperationException>(() => EF.Functions.FuzzyStringMatchDaitchMokotoff("John"));
}
