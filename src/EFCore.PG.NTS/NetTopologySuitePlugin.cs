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

        readonly NetTopologySuiteTypeGeometryMapping _point           = new NetTopologySuiteTypeGeometryMapping(typeof(Point));
        readonly NetTopologySuiteTypeGeometryMapping _lineString      = new NetTopologySuiteTypeGeometryMapping(typeof(LineString));
        readonly NetTopologySuiteTypeGeometryMapping _polygon         = new NetTopologySuiteTypeGeometryMapping(typeof(Polygon));
        readonly NetTopologySuiteTypeGeometryMapping _multiPoint      = new NetTopologySuiteTypeGeometryMapping(typeof(MultiPoint));
        readonly NetTopologySuiteTypeGeometryMapping _multiLineString = new NetTopologySuiteTypeGeometryMapping(typeof(MultiLineString));
        readonly NetTopologySuiteTypeGeometryMapping _multiPolygon    = new NetTopologySuiteTypeGeometryMapping(typeof(MultiPolygon));

        readonly NetTopologySuiteTypeGeometryMapping _collection      = new NetTopologySuiteTypeGeometryMapping(typeof(GeometryCollection));
        readonly NetTopologySuiteTypeGeometryMapping _geometry        = new NetTopologySuiteTypeGeometryMapping(typeof(Geometry));

        #endregion Geometry mappings

        #region Geography mappings

        readonly NetTopologySuiteTypeGeographyMapping _geogPoint           = new NetTopologySuiteTypeGeographyMapping(typeof(Point));
        readonly NetTopologySuiteTypeGeographyMapping _geogLineString      = new NetTopologySuiteTypeGeographyMapping(typeof(LineString));
        readonly NetTopologySuiteTypeGeographyMapping _geogPolygon         = new NetTopologySuiteTypeGeographyMapping(typeof(Polygon));
        readonly NetTopologySuiteTypeGeographyMapping _geogMultiPoint      = new NetTopologySuiteTypeGeographyMapping(typeof(MultiPoint));
        readonly NetTopologySuiteTypeGeographyMapping _geogMultiLineString = new NetTopologySuiteTypeGeographyMapping(typeof(MultiLineString));
        readonly NetTopologySuiteTypeGeographyMapping _geogMultiPolygon    = new NetTopologySuiteTypeGeographyMapping(typeof(MultiPolygon));

        readonly NetTopologySuiteTypeGeographyMapping _geogCollection      = new NetTopologySuiteTypeGeographyMapping(typeof(GeometryCollection));
        readonly NetTopologySuiteTypeGeographyMapping _geography           = new NetTopologySuiteTypeGeographyMapping(typeof(Geometry));

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
                _point, _lineString, _polygon, _multiPoint, _multiLineString, _multiPolygon, _collection, _geometry
            };
            typeMappingSource.StoreTypeMappings["geography"] = new RelationalTypeMapping[]
            {
                _geogPoint, _geogLineString, _geogPolygon, _geogMultiPoint, _geogMultiLineString, _geogMultiPolygon, _geogCollection, _geography
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
