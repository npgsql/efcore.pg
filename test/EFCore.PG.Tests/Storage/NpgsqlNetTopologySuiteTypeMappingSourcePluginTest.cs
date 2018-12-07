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
            => Assert.Equal(storeType, _geomPluginXY.FindMapping(new RelationalTypeMappingInfo(clrType))?.StoreType);

        [Theory]
        [InlineData(typeof(IPoint), "GEOMETRY (POINTZ)")]
        [InlineData(typeof(IGeometry), "GEOMETRY")]
        [InlineData(typeof(string), null)]
        public void Geom_xyz_by_ClrType(Type clrType, string storeType)
            => Assert.Equal(storeType, _geomPluginXYZ.FindMapping(new RelationalTypeMappingInfo(clrType))?.StoreType);

        [Theory]
        [InlineData(typeof(IPoint), "GEOMETRY (POINTM)")]
        [InlineData(typeof(IGeometry), "GEOMETRY")]
        [InlineData(typeof(string), null)]
        public void Geom_xym_by_ClrType(Type clrType, string storeType)
            => Assert.Equal(storeType, _geomPluginXYM.FindMapping(new RelationalTypeMappingInfo(clrType))?.StoreType);

        [Theory]
        [InlineData(typeof(IPoint), "GEOMETRY (POINTZM)")]
        [InlineData(typeof(IGeometry), "GEOMETRY")]
        [InlineData(typeof(string), null)]
        public void Geom_xyzm_by_ClrType(Type clrType, string storeType)
            => Assert.Equal(storeType, _geomPluginXYZM.FindMapping(new RelationalTypeMappingInfo(clrType))?.StoreType);

        [Theory]
        [InlineData(typeof(IPoint), "GEOGRAPHY (POINT)")]
        [InlineData(typeof(IGeometry), "GEOGRAPHY")]
        [InlineData(typeof(string), null)]
        public void Geog_by_ClrType(Type clrType, string storeType)
            => Assert.Equal(storeType, _geogPluginXY.FindMapping(new RelationalTypeMappingInfo(clrType))?.StoreType);

        [Theory]
        [InlineData("GEOMETRY (POINT)", typeof(IPoint))]
        [InlineData("GEOMETRY (POINTZ)", typeof(IPoint))]
        [InlineData("GEOMETRY (POINTZM)", typeof(IPoint))]
        [InlineData("GEOGRAPHY (POINT)", typeof(IPoint))]
        [InlineData("GEOMETRY", typeof(IGeometry))]
        [InlineData("text", null)]
        public void By_StoreType(string storeType, Type clrType)
            => Assert.Equal(clrType, _geomPluginXY.FindMapping(new RelationalTypeMappingInfo(storeType))?.ClrType);

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
            new object[] { "Geography ( Point , 123 )", (ValueTuple<bool, string, int?>)(true, "Point", 123)      },
            new object[] { "text",                      null                                                      }
        };

        readonly NpgsqlNetTopologySuiteTypeMappingSourcePlugin _geomPluginXY =
            new NpgsqlNetTopologySuiteTypeMappingSourcePlugin(new NpgsqlNetTopologySuiteOptions());

        readonly NpgsqlNetTopologySuiteTypeMappingSourcePlugin _geomPluginXYZ =
            new NpgsqlNetTopologySuiteTypeMappingSourcePlugin(new NpgsqlNetTopologySuiteOptions { Ordinates = Ordinates.XYZ });

        readonly NpgsqlNetTopologySuiteTypeMappingSourcePlugin _geomPluginXYM =
            new NpgsqlNetTopologySuiteTypeMappingSourcePlugin(new NpgsqlNetTopologySuiteOptions { Ordinates = Ordinates.XYM });

        readonly NpgsqlNetTopologySuiteTypeMappingSourcePlugin _geomPluginXYZM =
            new NpgsqlNetTopologySuiteTypeMappingSourcePlugin(new NpgsqlNetTopologySuiteOptions { Ordinates = Ordinates.XYZM });

        readonly NpgsqlNetTopologySuiteTypeMappingSourcePlugin _geogPluginXY =
            new NpgsqlNetTopologySuiteTypeMappingSourcePlugin(new NpgsqlNetTopologySuiteOptions { IsGeographyDefault = true });
    }
}
