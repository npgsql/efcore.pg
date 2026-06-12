using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class NpgsqlModelBuilderGenericTest : NpgsqlModelBuilderTestBase
{
    public class NpgsqlGenericNonRelationship(NpgsqlModelBuilderFixture fixture) : NpgsqlNonRelationship(fixture)
    {
        // PostgreSQL actually does support mapping multi-dimensional arrays, so no exception is thrown as expected
        protected override void Mapping_throws_for_non_ignored_three_dimensional_array()
            => Assert.Throws<ThrowsException>(() => base.Mapping_throws_for_non_ignored_three_dimensional_array());

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class NpgsqlGenericComplexType(NpgsqlModelBuilderFixture fixture) : NpgsqlComplexType(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class NpgsqlGenericComplexCollection(NpgsqlModelBuilderFixture fixture) : NpgsqlComplexCollection(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class NpgsqlGenericInheritance(NpgsqlModelBuilderFixture fixture) : NpgsqlInheritance(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class NpgsqlGenericOneToMany(NpgsqlModelBuilderFixture fixture) : NpgsqlOneToMany(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class NpgsqlGenericManyToOne(NpgsqlModelBuilderFixture fixture) : NpgsqlManyToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class NpgsqlGenericOneToOne(NpgsqlModelBuilderFixture fixture) : NpgsqlOneToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class NpgsqlGenericManyToMany(NpgsqlModelBuilderFixture fixture) : NpgsqlManyToMany(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class NpgsqlGenericOwnedTypes(NpgsqlModelBuilderFixture fixture) : NpgsqlOwnedTypes(fixture)
    {
        // PostgreSQL stored procedures do not support result columns
        public override void Can_use_sproc_mapping_with_owned_reference()
            => Assert.Throws<InvalidOperationException>(() => base.Can_use_sproc_mapping_with_owned_reference());

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }
}
