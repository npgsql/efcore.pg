using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class UpdatesNpgsqlFixture : UpdatesRelationalFixture
    {
        protected override string StoreName { get; } = "PartialUpdateNpgsqlTest";
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly>()
                .Property(l => l.ProfileId3).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Profile>()
                    .Property(l => l.Id3).HasColumnType("decimal(18,2)");
        }
    }
}
