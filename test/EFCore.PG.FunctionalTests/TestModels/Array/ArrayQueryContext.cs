using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestModels.Array
{
    public class ArrayQueryContext : PoolableDbContext
    {
        public DbSet<ArrayEntity> SomeEntities { get; set; }
        public DbSet<ArrayContainerEntity> SomeEntityContainers { get; set; }

        public ArrayQueryContext(DbContextOptions options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<ArrayEntity>(
                e =>
                {
                    // We do negative to make sure our value converter is properly used, and not the built-in one
                    e.Property(ae => ae.EnumConvertedToInt)
                        .HasConversion(w => -(int)w, v => (SomeEnum)(-v));

                    e.Property(ae => ae.EnumConvertedToString)
                        .HasConversion(w => w.ToString(), v => Enum.Parse<SomeEnum>(v));

                    e.Property(ae => ae.NullableEnumConvertedToString)
                        .HasConversion(w => w.ToString(), v => Enum.Parse<SomeEnum>(v));

                    e.Property(ae => ae.NullableEnumConvertedToStringWithNonNullableLambda)
                        .HasConversion(new ValueConverter<SomeEnum, string>(w => w.ToString(), v => Enum.Parse<SomeEnum>(v)));

                    e.Property(ae => ae.ValueConvertedArray)
                        .HasPostgresArrayConversion(w => -(int)w, v => (SomeEnum)(-v));

                    e.Property(ae => ae.ValueConvertedList)
                        .HasPostgresArrayConversion(w => -(int)w, v => (SomeEnum)(-v));
                });

        public static void Seed(ArrayQueryContext context)
        {
            var arrayEntities = ArrayQueryData.CreateArrayEntities();

            context.SomeEntities.AddRange(arrayEntities);
            context.SomeEntityContainers.Add(
                new ArrayContainerEntity
                {
                    Id = 1,
                    ArrayEntities = arrayEntities.ToList()
                }
            );
            context.SaveChanges();
        }
    }
}
