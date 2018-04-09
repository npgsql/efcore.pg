using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite
{
    public class NetTopologySuitePlugin : IEntityFrameworkNpgsqlPlugin
    {
        public string Name => "NetTopologySuite";
        public string Description => "Plugin to map PostGIS types to NetTopologySuite";

        #region Geometry mappings

        readonly NetTopologySuiteGeometryTypeMapping _point           = new NetTopologySuiteGeometryTypeMapping(typeof(Point));
        readonly NetTopologySuiteGeometryTypeMapping _lineString      = new NetTopologySuiteGeometryTypeMapping(typeof(LineString));
        readonly NetTopologySuiteGeometryTypeMapping _polygon         = new NetTopologySuiteGeometryTypeMapping(typeof(Polygon));
        readonly NetTopologySuiteGeometryTypeMapping _multiPoint      = new NetTopologySuiteGeometryTypeMapping(typeof(MultiPoint));
        readonly NetTopologySuiteGeometryTypeMapping _multiLineString = new NetTopologySuiteGeometryTypeMapping(typeof(MultiLineString));
        readonly NetTopologySuiteGeometryTypeMapping _multiPolygon    = new NetTopologySuiteGeometryTypeMapping(typeof(MultiPolygon));

        readonly NetTopologySuiteGeometryTypeMapping _collection      = new NetTopologySuiteGeometryTypeMapping(typeof(GeometryCollection));
        readonly NetTopologySuiteGeometryTypeMapping _geometry        = new NetTopologySuiteGeometryTypeMapping(typeof(Geometry));

        #endregion Geometry mappings

        #region Geography mappings

        readonly NetTopologySuiteGeographyTypeMapping _geogPoint           = new NetTopologySuiteGeographyTypeMapping(typeof(Point));
        readonly NetTopologySuiteGeographyTypeMapping _geogLineString      = new NetTopologySuiteGeographyTypeMapping(typeof(LineString));
        readonly NetTopologySuiteGeographyTypeMapping _geogPolygon         = new NetTopologySuiteGeographyTypeMapping(typeof(Polygon));
        readonly NetTopologySuiteGeographyTypeMapping _geogMultiPoint      = new NetTopologySuiteGeographyTypeMapping(typeof(MultiPoint));
        readonly NetTopologySuiteGeographyTypeMapping _geogMultiLineString = new NetTopologySuiteGeographyTypeMapping(typeof(MultiLineString));
        readonly NetTopologySuiteGeographyTypeMapping _geogMultiPolygon    = new NetTopologySuiteGeographyTypeMapping(typeof(MultiPolygon));

        readonly NetTopologySuiteGeographyTypeMapping _geogCollection      = new NetTopologySuiteGeographyTypeMapping(typeof(GeometryCollection));
        readonly NetTopologySuiteGeographyTypeMapping _geography           = new NetTopologySuiteGeographyTypeMapping(typeof(Geometry));

        #endregion Geography mappings

        public void AddMappings(NpgsqlTypeMappingSource typeMappingSource)
        {
            typeMappingSource.ClrTypeMappings[typeof(Geometry)]           = _geometry;
            typeMappingSource.ClrTypeMappings[typeof(Point)]              = _point;
            typeMappingSource.ClrTypeMappings[typeof(LineString)]         = _lineString;
            typeMappingSource.ClrTypeMappings[typeof(Polygon)]            = _polygon;
            typeMappingSource.ClrTypeMappings[typeof(MultiPoint)]         = _multiPoint;
            typeMappingSource.ClrTypeMappings[typeof(MultiLineString)]    = _multiLineString;
            typeMappingSource.ClrTypeMappings[typeof(MultiPolygon)]       = _multiPolygon;
            typeMappingSource.ClrTypeMappings[typeof(GeometryCollection)] = _collection;

            typeMappingSource.StoreTypeMappings["geometry"] = new RelationalTypeMapping[]
            {
                _geometry, _point, _lineString, _polygon, _multiPoint, _multiLineString, _multiPolygon, _collection
            };
            typeMappingSource.StoreTypeMappings["geography"] = new RelationalTypeMapping[]
            {
                _geography, _geogPoint, _geogLineString, _geogPolygon, _geogMultiPoint, _geogMultiLineString, _geogMultiPolygon, _geogCollection
            };
        }

        static readonly IMethodCallTranslator[] MethodCallTranslators =
        {
            new NetTopologySuiteMethodCallTranslator(),
        };

        static readonly IMemberTranslator[] MemberTranslators =
        {
            new NetTopologySuiteMemberTranslator(),
        };

        public void AddMethodCallTranslators(NpgsqlCompositeMethodCallTranslator compositeMethodCallTranslator)
            => compositeMethodCallTranslator.AddTranslators(MethodCallTranslators);

        public void AddMemberTranslators(NpgsqlCompositeMemberTranslator compositeMemberTranslator)
            => compositeMemberTranslator.AddTranslators(MemberTranslators);
    }
}
