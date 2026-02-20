using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.NameTranslation;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

public class EnumDefinitionTest
{
    [ConditionalFact]
    public void Enum_with_duplicate_values_uses_first_label()
    {
        var definition = new EnumDefinition(
            typeof(EnumWithDuplicateValues),
            name: null,
            schema: null,
            new NpgsqlSnakeCaseNameTranslator());

        Assert.Equal(2, definition.Labels.Count);
        Assert.Equal("alive", definition.Labels[EnumWithDuplicateValues.Alive]);
        Assert.Equal("deceased", definition.Labels[EnumWithDuplicateValues.Deceased]);
    }

    [ConditionalFact]
    public void Enum_with_duplicate_values_and_same_pg_name_succeeds()
    {
        var definition = new EnumDefinition(
            typeof(EnumWithDuplicateValuesAndSamePgName),
            name: null,
            schema: null,
            new NpgsqlSnakeCaseNameTranslator());

        Assert.Equal(2, definition.Labels.Count);
        Assert.Equal("custom_alive", definition.Labels[EnumWithDuplicateValuesAndSamePgName.Alive]);
        Assert.Equal("deceased", definition.Labels[EnumWithDuplicateValuesAndSamePgName.Deceased]);
    }

    [ConditionalFact]
    public void Enum_with_duplicate_values_and_different_pg_names_throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => new EnumDefinition(
                typeof(EnumWithDuplicateValuesAndDifferentPgNames),
                name: null,
                schema: null,
                new NpgsqlSnakeCaseNameTranslator()));

        Assert.Contains("different", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[PgName]", exception.Message);
    }

    [ConditionalFact]
    public void Enum_with_duplicate_values_where_only_one_has_pg_name_throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => new EnumDefinition(
                typeof(EnumWithDuplicateValuesOnePgName),
                name: null,
                schema: null,
                new NpgsqlSnakeCaseNameTranslator()));

        Assert.Contains("different", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[PgName]", exception.Message);
    }

    private enum EnumWithDuplicateValues
    {
        Alive,
        Deceased,
        Comatose = Alive,
    }

    private enum EnumWithDuplicateValuesAndSamePgName
    {
        [PgName("custom_alive")]
        Alive,
        Deceased,
        [PgName("custom_alive")]
        Comatose = Alive,
    }

    private enum EnumWithDuplicateValuesAndDifferentPgNames
    {
        [PgName("label_a")]
        Alive,
        Deceased,
        [PgName("label_b")]
        Comatose = Alive,
    }

    private enum EnumWithDuplicateValuesOnePgName
    {
        [PgName("custom_alive")]
        Alive,
        Deceased,
        Comatose = Alive,
    }
}
