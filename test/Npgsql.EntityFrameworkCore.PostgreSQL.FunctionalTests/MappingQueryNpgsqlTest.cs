using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class MappingQueryNpgsqlTest : MappingQueryTestBase, IClassFixture<MappingQueryNpgsqlFixture>
    {
        readonly MappingQueryNpgsqlFixture _fixture;

        public MappingQueryNpgsqlTest(MappingQueryNpgsqlFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
