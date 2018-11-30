using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage
{
    public class NpgsqlNetTopologySuiteTypeMappingSourcePluginTest
    {
        [Theory]
        [InlineData(typeof(IPoint), "GEOMETRY (POINT)")]
        [InlineData(typeof(IGeometry), "GEOMETRY")]
        [InlineData(typeof(string), null)]
        public void Geom_by_ClrType(Type clrType, string storeType)
            => Assert.Equal(storeType, _geomPlugin.FindMapping(new RelationalTypeMappingInfo(clrType))?.StoreType);

        [Theory]
        [InlineData(typeof(IPoint), "GEOGRAPHY (POINT)")]
        [InlineData(typeof(IGeometry), "GEOGRAPHY")]
        [InlineData(typeof(string), null)]
        public void Geog_by_ClrType(Type clrType, string storeType)
            => Assert.Equal(storeType, _geogPlugin.FindMapping(new RelationalTypeMappingInfo(clrType))?.StoreType);

        [Theory]
        [InlineData("GEOMETRY (POINT)", typeof(IPoint))]
        [InlineData("GEOGRAPHY (POINT)", typeof(IPoint))]
        [InlineData("GEOMETRY", typeof(IGeometry))]
        [InlineData("text", null)]
        public void By_StoreType(string storeType, Type clrType)
            => Assert.Equal(clrType, _geomPlugin.FindMapping(new RelationalTypeMappingInfo(storeType))?.ClrType);

        [Theory]
        [MemberData(nameof(GetParseData))]
        public void ParseGeometryStoreType(string storeType, (bool IsGeography, string SpatialType, int? Srid)? expected)
            => Assert.Equal(expected, NpgsqlNetTopologySuiteTypeMappingSourcePlugin.ParseGeometryStoreType(storeType));

        static IEnumerable<object[]> GetParseData() => new[]
        {
            new object[] { "geography (point,123)",     (ValueTuple<bool, string, int?>)(true, "point", 123)      },
            new object[] { "GEOGRAPHY (point)",         (ValueTuple<bool, string, int?>)(true, "point", null)     },
            new object[] { "geometry",                  (ValueTuple<bool, string, int?>)(false, null, null)       },
            new object[] { "geometry   (geometry)",     (ValueTuple<bool, string, int?>)(false, "geometry", null) },
            new object[] { "Geography ( Point , 123 )", (ValueTuple<bool, string, int?>)(true, "point", 123)      },
            new object[] { "text",                      null                                                      }
        };

        readonly NpgsqlNetTopologySuiteTypeMappingSourcePlugin _geomPlugin =
            new NpgsqlNetTopologySuiteTypeMappingSourcePlugin(new NpgsqlNetTopologySuiteOptions());
        readonly NpgsqlNetTopologySuiteTypeMappingSourcePlugin _geogPlugin =
            new NpgsqlNetTopologySuiteTypeMappingSourcePlugin(new NpgsqlNetTopologySuiteOptions() { IsGeographyDefault = true });
    }
}
