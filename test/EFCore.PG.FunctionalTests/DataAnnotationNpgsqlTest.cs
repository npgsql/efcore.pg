using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class DataAnnotationNpgsqlTest : DataAnnotationTestBase<NpgsqlTestStore, DataAnnotationNpgsqlFixture>
    {
        public DataAnnotationNpgsqlTest(DataAnnotationNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

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
    }
}
