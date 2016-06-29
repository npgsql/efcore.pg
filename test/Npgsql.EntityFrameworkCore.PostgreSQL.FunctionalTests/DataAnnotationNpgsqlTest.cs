using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class DataAnnotationNpgsqlTest : DataAnnotationTestBase<NpgsqlTestStore, DataAnnotationNpgsqlFixture>
    {
        public DataAnnotationNpgsqlTest(DataAnnotationNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            base.ConcurrencyCheckAttribute_throws_if_value_in_database_changed();

            // TODO: Assert on SQL
        }

        public override void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            base.DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity();

            // TODO: Assert on SQL
        }

        public override void RequiredAttribute_for_navigation_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_navigation_throws_while_inserting_null_value();

            // TODO: Assert on SQL
        }

        public override void RequiredAttribute_for_property_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_property_throws_while_inserting_null_value();

            // TODO: Assert on SQL
        }

        public override void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            // Npgsql does not support length
        }

        public override void TimestampAttribute_throws_if_value_in_database_changed()
        {
            // Npgsql does not support length
        }

        public override void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            // Npgsql does not support length            
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
