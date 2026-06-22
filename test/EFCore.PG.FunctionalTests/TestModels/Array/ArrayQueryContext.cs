namespace Microsoft.EntityFrameworkCore.TestModels.Array;

public class ArrayQueryContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<ArrayEntity> SomeEntities { get; set; }
    public DbSet<ArrayContainerEntity> SomeEntityContainers { get; set; }

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

                e.Property(ae => ae.ListOfStringConvertedToDelimitedString)
                    .HasConversion(
                        v => string.Join(",", v),
                        v => v.Split(',', StringSplitOptions.None).ToList(),
                        new ValueComparer<List<string>>(
                            (c1, c2) => c1 == null && c2 == null || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToList()));

                e.Property(ae => ae.ArrayOfStringConvertedToDelimitedString)
                    .HasConversion(
                        v => string.Join(",", v),
                        v => v.Split(',', StringSplitOptions.None).ToArray(),
                        new ValueComparer<string[]>(
                            (c1, c2) => c1 == null && c2 == null || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToArray()));

                e.PrimitiveCollection(ae => ae.ValueConvertedArrayOfEnum)
                    .ElementType(eb => eb.HasConversion(typeof(EnumToStringConverter<SomeEnum>)));

                e.PrimitiveCollection(ae => ae.ValueConvertedListOfEnum)
                    .ElementType(eb => eb.HasConversion(typeof(EnumToStringConverter<SomeEnum>)));

                e.HasIndex(ae => ae.NonNullableText);
            });

    public static async Task SeedAsync(ArrayQueryContext context)
    {
        var arrayEntities = ArrayQueryData.CreateArrayEntities();

        context.SomeEntities.AddRange(arrayEntities);
        context.SomeEntityContainers.Add(new ArrayContainerEntity { Id = 1, ArrayEntities = arrayEntities.ToList() });
        await context.SaveChangesAsync();
    }
}
