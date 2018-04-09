using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    // TODO: Should be in Tests, not FnctionalTests
    public class NpgsqlNtsTypeMappingTest
    {
        [Fact]
        public void GenerateSqlLiteral_returns_point_literal()
        {
            var mapping = GetMapping(typeof(Point));
            Assert.Equal("ST_MakePoint(3.3, 4.4)", mapping.GenerateSqlLiteral(new Point(3.3, 4.4)));
            Assert.Equal("ST_MakePoint(3.3, 4.4, 5.5)", mapping.GenerateSqlLiteral(new Point(3.3, 4.4, 5.5)));

            // TODO: Test literal generation for XYZM, XYM
        }

        #region Support

        static NpgsqlNtsTypeMappingTest()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            var npgsqlBuilder = new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite();
            var options = new NpgsqlOptions();
            options.Initialize(optionsBuilder.Options);

            Mapper = new NpgsqlTypeMappingSource(
                new TypeMappingSourceDependencies(
                    new ValueConverterSelector(new ValueConverterSelectorDependencies())
                ),
                new RelationalTypeMappingSourceDependencies(),
                options
            );
        }

        static readonly NpgsqlTypeMappingSource Mapper;

        static RelationalTypeMapping GetMapping(string storeType)
            => Mapper.FindMapping(storeType);

        public static RelationalTypeMapping GetMapping(Type clrType)
            => (RelationalTypeMapping)Mapper.FindMapping(clrType);

        #endregion Support
    }
}
