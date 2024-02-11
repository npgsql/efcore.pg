using Xunit.Sdk;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ModelBuilding;

#nullable enable

public class NpgsqlModelBuilderGenericTest : NpgsqlModelBuilderTestBase
{
    public class NpgsqlGenericNonRelationship(NpgsqlModelBuilderFixture fixture) : NpgsqlNonRelationship(fixture)
    {
        // https://github.com/dotnet/efcore/issues/33059
        public override void Can_add_multiple_indexes()
            => Assert.Throws<EqualException>(() => base.Can_add_multiple_indexes());

        // PostgreSQL actually does support mapping multi-dimensional arrays, so no exception is thrown as expected
        protected override void Mapping_throws_for_non_ignored_three_dimensional_array()
            => Assert.Throws<ThrowsException>(() => base.Mapping_throws_for_non_ignored_three_dimensional_array());

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class NpgsqlGenericComplexType(NpgsqlModelBuilderFixture fixture) : NpgsqlComplexType(fixture)
    {
        // https://github.com/dotnet/efcore/issues/33059
        public override void Access_mode_can_be_overridden_at_entity_and_property_levels()
            => Assert.Throws<EqualException>(() => base.Access_mode_can_be_overridden_at_entity_and_property_levels());

        // https://github.com/dotnet/efcore/issues/33059
        public override void Complex_properties_not_discovered_by_convention()
            => Assert.Throws<EqualException>(() => base.Complex_properties_not_discovered_by_convention());

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
        // https://github.com/dotnet/efcore/issues/33059
        public override void Can_use_implicit_shared_type_with_default_name_and_implicit_relationships_as_join_entity()
            => Assert.Throws<EqualException>(
                () => base.Can_use_implicit_shared_type_with_default_name_and_implicit_relationships_as_join_entity());

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class NpgsqlGenericOwnedTypes(NpgsqlModelBuilderFixture fixture) : NpgsqlOwnedTypes(fixture)
    {
        // https://github.com/dotnet/efcore/issues/33059
        public override void Can_configure_chained_ownerships()
            => Assert.Throws<EqualException>(() => base.Can_configure_chained_ownerships());

        // https://github.com/dotnet/efcore/issues/33059
        public override void Can_configure_chained_ownerships_different_order()
            => Assert.Throws<EqualException>(() => base.Can_configure_chained_ownerships_different_order());

        // PostgreSQL stored procedures do not support result columns
        public override void Can_use_sproc_mapping_with_owned_reference()
            => Assert.Throws<InvalidOperationException>(() => base.Can_use_sproc_mapping_with_owned_reference());

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder>? configure)
            => new GenericTestModelBuilder(Fixture, configure);
    }
}
