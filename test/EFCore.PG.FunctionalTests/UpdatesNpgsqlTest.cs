using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class UpdatesNpgsqlTest : UpdatesRelationalTestBase<UpdatesNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public UpdatesNpgsqlTest(UpdatesNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
        => Fixture.TestSqlLoggerFactory.Clear();

    public override void Identifiers_are_generated_correctly()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(
            LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
        ));
        Assert.Equal(
            "LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatI~",
            entityType.GetTableName());
        Assert.Equal(
            "PK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameTh~",
            entityType.GetKeys().Single().GetName());
        Assert.Equal(
            "FK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameTh~",
            entityType.GetForeignKeys().Single().GetConstraintName());
        Assert.Equal(
            "IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameTh~",
            entityType.GetIndexes().Single().GetDatabaseName());

        var entityType2 = context.Model.FindEntityType(
            typeof(
                LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
            ));

        Assert.Equal(
            "LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThat~1",
            entityType2.GetTableName());
        Assert.Equal(
            "PK_LoginDetails",
            entityType2.GetKeys().Single().GetName());
        Assert.Equal(
            "ExtraPropertyWithAnExtremelyLongAndOverlyConvolutedNameThatIsU~",
            entityType2.GetProperties().ElementAt(1).GetColumnName());
        Assert.Equal(
            "ExtraPropertyWithAnExtremelyLongAndOverlyConvolutedNameThatIs~1",
            entityType2.GetProperties().ElementAt(2).GetColumnName());
        Assert.Equal(
            "IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameT~1",
            entityType2.GetIndexes().Single().GetDatabaseName());
    }
}
