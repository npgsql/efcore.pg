namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class NpgsqlModelBuilderTestBase : RelationalModelBuilderTest
{
    public abstract class NpgsqlNonRelationship(NpgsqlModelBuilderFixture fixture)
        : RelationalNonRelationshipTestBase(fixture), IClassFixture<NpgsqlModelBuilderFixture>;

    public abstract class NpgsqlComplexType(NpgsqlModelBuilderFixture fixture)
        : RelationalComplexTypeTestBase(fixture), IClassFixture<NpgsqlModelBuilderFixture>;

    public abstract class NpgsqlComplexCollection(NpgsqlModelBuilderFixture fixture)
        : RelationalComplexCollectionTestBase(fixture), IClassFixture<NpgsqlModelBuilderFixture>;

    public abstract class NpgsqlInheritance(NpgsqlModelBuilderFixture fixture)
        : RelationalInheritanceTestBase(fixture), IClassFixture<NpgsqlModelBuilderFixture>;

    public abstract class NpgsqlOneToMany(NpgsqlModelBuilderFixture fixture)
        : RelationalOneToManyTestBase(fixture), IClassFixture<NpgsqlModelBuilderFixture>;

    public abstract class NpgsqlManyToOne(NpgsqlModelBuilderFixture fixture)
        : RelationalManyToOneTestBase(fixture), IClassFixture<NpgsqlModelBuilderFixture>;

    public abstract class NpgsqlOneToOne(NpgsqlModelBuilderFixture fixture)
        : RelationalOneToOneTestBase(fixture), IClassFixture<NpgsqlModelBuilderFixture>;

    public abstract class NpgsqlManyToMany(NpgsqlModelBuilderFixture fixture)
        : RelationalManyToManyTestBase(fixture), IClassFixture<NpgsqlModelBuilderFixture>;

    public abstract class NpgsqlOwnedTypes(NpgsqlModelBuilderFixture fixture)
        : RelationalOwnedTypesTestBase(fixture), IClassFixture<NpgsqlModelBuilderFixture>;

    public class NpgsqlModelBuilderFixture : RelationalModelBuilderFixture
    {
        public override TestHelpers TestHelpers
            => NpgsqlTestHelpers.Instance;
    }
}
