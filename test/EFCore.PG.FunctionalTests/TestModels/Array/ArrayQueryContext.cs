namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestModels.Array;

public class ArrayQueryContext : PoolableDbContext
{
    public DbSet<ArrayEntity> SomeEntities { get; set; }
    public DbSet<ArrayContainerEntity> SomeEntityContainers { get; set; }

    public ArrayQueryContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ArrayEntity>(
            e =>
            {
                e.Property(ae => ae.ByteArray).HasColumnType("smallint[]");

                e.Property(ae => ae.Varchar10).HasColumnType("varchar(10)");
                e.Property(ae => ae.Varchar15).HasColumnType("varchar(15)");

                // We do negative to make sure our value converter is properly used, and not the built-in one
                e.Property(ae => ae.EnumConvertedToInt)
                    .HasConversion(w => -(int)w, v => (SomeEnum)(-v));

                e.Property(ae => ae.EnumConvertedToString)
                    .HasConversion(typeof(EnumToStringConverter<SomeEnum>));

                e.Property(ae => ae.NullableEnumConvertedToString)
                    .HasConversion(typeof(EnumToStringConverter<SomeEnum>));

                e.Property(ae => ae.NullableEnumConvertedToStringWithNonNullableLambda)
                    .HasConversion(new ValueConverter<SomeEnum, string>(w => w.ToString(), v => Enum.Parse<SomeEnum>(v)));

                e.PrimitiveCollection(ae => ae.ValueConvertedArray)
                    .ElementType(eb => eb.HasConversion(typeof(EnumToStringConverter<SomeEnum>)));

                e.PrimitiveCollection(ae => ae.ValueConvertedList)
                    .ElementType(eb => eb.HasConversion(typeof(EnumToStringConverter<SomeEnum>)));

                e.HasIndex(ae => ae.NonNullableText);
            });

    public static void Seed(ArrayQueryContext context)
    {
        var arrayEntities = ArrayQueryData.CreateArrayEntities();

        context.SomeEntities.AddRange(arrayEntities);
        context.SomeEntityContainers.Add(
            new ArrayContainerEntity { Id = 1, ArrayEntities = arrayEntities.ToList() }
        );
        context.SaveChanges();
    }
}
