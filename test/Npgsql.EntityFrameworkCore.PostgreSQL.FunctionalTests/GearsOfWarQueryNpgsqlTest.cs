using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.GearsOfWarModel;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class GearsOfWarQueryNpgsqlTest : GearsOfWarQueryTestBase<NpgsqlTestStore, GearsOfWarQueryNpgsqlFixture>
    {
        public GearsOfWarQueryNpgsqlTest(GearsOfWarQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public override void Include_multiple_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_many();

            // TODO: Assert on SQL
        }

        public override void Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            base.Include_multiple_one_to_one_and_one_to_many_self_reference();

            // TODO: Assert on SQL
        }

        public override void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_one_and_one_to_many();

            // TODO: Assert on SQL
        }

        public override void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            base.Include_multiple_one_to_one_optional_and_one_to_one_required();

            Assert.Equal(
                @"SELECT ""t"".""Id"", ""t"".""GearNickName"", ""t"".""GearSquadId"", ""t"".""Note"", ""g"".""Nickname"", ""g"".""SquadId"", ""g"".""AssignedCityName"", ""g"".""CityOrBirthName"", ""g"".""Discriminator"", ""g"".""FullName"", ""g"".""LeaderNickname"", ""g"".""LeaderSquadId"", ""g"".""Rank"", ""s"".""Id"", ""s"".""InternalNumber"", ""s"".""Name""
FROM ""CogTag"" AS ""t""
LEFT JOIN (
    SELECT ""g"".*
    FROM ""Gear"" AS ""g""
    WHERE ""g"".""Discriminator"" IN ('Officer', 'Gear')
) AS ""g"" ON (""t"".""GearNickName"" = ""g"".""Nickname"") AND (""t"".""GearSquadId"" = ""g"".""SquadId"")
LEFT JOIN ""Squad"" AS ""s"" ON ""g"".""SquadId"" = ""s"".""Id""",
                Sql);
        }

        public override void Include_multiple_circular()
        {
            base.Include_multiple_circular();

            // TODO: Assert on SQL
        }

        public override void Include_multiple_circular_with_filter()
        {
            base.Include_multiple_circular_with_filter();

            // TODO: Assert on SQL
        }

        public override void Include_using_alternate_key()
        {
            base.Include_using_alternate_key();

            // TODO: Assert on SQL
        }

        public override void Where_enum()
        {
            base.Where_enum();

            Assert.Equal(
                @"SELECT ""g"".""Nickname"", ""g"".""SquadId"", ""g"".""AssignedCityName"", ""g"".""CityOrBirthName"", ""g"".""Discriminator"", ""g"".""FullName"", ""g"".""LeaderNickname"", ""g"".""LeaderSquadId"", ""g"".""Rank""
FROM ""Gear"" AS ""g""
WHERE ""g"".""Discriminator"" IN ('Officer', 'Gear') AND (""g"".""Rank"" = 2)",
                Sql);
        }

        public override void Where_nullable_enum_with_constant()
        {
            base.Where_nullable_enum_with_constant();

            Assert.Equal(
                @"SELECT ""w"".""Id"", ""w"".""AmmunitionType"", ""w"".""IsAutomatic"", ""w"".""Name"", ""w"".""OwnerFullName"", ""w"".""SynergyWithId""
FROM ""Weapon"" AS ""w""
WHERE ""w"".""AmmunitionType"" = 1",
                Sql);
        }

        public override void Where_nullable_enum_with_null_constant()
        {
            base.Where_nullable_enum_with_null_constant();

            Assert.Equal(
                @"SELECT ""w"".""Id"", ""w"".""AmmunitionType"", ""w"".""IsAutomatic"", ""w"".""Name"", ""w"".""OwnerFullName"", ""w"".""SynergyWithId""
FROM ""Weapon"" AS ""w""
WHERE ""w"".""AmmunitionType"" IS NULL",
                Sql);
        }

        public override void Where_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_nullable_enum_with_non_nullable_parameter();

            Assert.Equal(
                @"@__ammunitionType_0: Cartridge

SELECT ""w"".""Id"", ""w"".""AmmunitionType"", ""w"".""IsAutomatic"", ""w"".""Name"", ""w"".""OwnerFullName"", ""w"".""SynergyWithId""
FROM ""Weapon"" AS ""w""
WHERE ""w"".""AmmunitionType"" = @__ammunitionType_0",
                Sql);
        }

        public override void Where_nullable_enum_with_nullable_parameter()
        {
            base.Where_nullable_enum_with_nullable_parameter();

            Assert.Equal(
                @"@__ammunitionType_0: Cartridge (Nullable = true)

SELECT ""w"".""Id"", ""w"".""AmmunitionType"", ""w"".""IsAutomatic"", ""w"".""Name"", ""w"".""OwnerFullName"", ""w"".""SynergyWithId""
FROM ""Weapon"" AS ""w""
WHERE ""w"".""AmmunitionType"" = @__ammunitionType_0

SELECT ""w"".""Id"", ""w"".""AmmunitionType"", ""w"".""IsAutomatic"", ""w"".""Name"", ""w"".""OwnerFullName"", ""w"".""SynergyWithId""
FROM ""Weapon"" AS ""w""
WHERE ""w"".""AmmunitionType"" IS NULL",
                Sql);
        }

        public override void Select_ternary_operation_with_boolean()
        {
            base.Select_ternary_operation_with_boolean();

            Assert.Equal(
                @"SELECT ""w"".""Id"", CASE
    WHEN ""w"".""IsAutomatic"" = TRUE
    THEN 1 ELSE 0
END
FROM ""Weapon"" AS ""w""",
                Sql);
        }

        [Fact(Skip="https://github.com/aspnet/EntityFramework/issues/5723")]
        public override void Optional_Navigation_Null_Coalesce_To_Clr_Type()
        {
            base.Optional_Navigation_Null_Coalesce_To_Clr_Type();
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
