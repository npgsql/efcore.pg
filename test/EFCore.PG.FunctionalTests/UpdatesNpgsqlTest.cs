using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesNpgsqlTest : UpdatesRelationalTestBase<UpdatesNpgsqlFixture>
    {
        public UpdatesNpgsqlTest(UpdatesNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [Fact(Skip="https://github.com/aspnet/EntityFrameworkCore/issues/10796, fixed for 2.1.0-preview2")]
        public override void Identifiers_are_generated_correctly()
        {
            using (var context = CreateContext())
            {
                var entityType = context.Model.FindEntityType(typeof(
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly));
                Assert.Equal("LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatI~", entityType.Relational().TableName);
                Assert.Equal("PK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameTh~", entityType.GetKeys().Single().Relational().Name);
                Assert.Equal("FK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameTh~", entityType.GetForeignKeys().Single().Relational().Name);
                Assert.Equal("IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameTh~", entityType.GetIndexes().Single().Relational().Name);
            }
        }
    }
}
