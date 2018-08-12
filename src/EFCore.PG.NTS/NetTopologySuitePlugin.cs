using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite
{
    public class NetTopologySuitePlugin : NpgsqlEntityFrameworkPlugin
    {
        public override string Name => "NetTopologySuite";
        public override string Description => "Plugin to map PostGIS types to NetTopologySuite";

        #region Geometry mappings

        readonly NetTopologySuiteGeometryTypeMapping _point            = new NetTopologySuiteGeometryTypeMapping(typeof(Point));
        readonly NetTopologySuiteGeometryTypeMapping _lineString       = new NetTopologySuiteGeometryTypeMapping(typeof(LineString));
        readonly NetTopologySuiteGeometryTypeMapping _polygon          = new NetTopologySuiteGeometryTypeMapping(typeof(Polygon));
        readonly NetTopologySuiteGeometryTypeMapping _multiPoint       = new NetTopologySuiteGeometryTypeMapping(typeof(MultiPoint));
        readonly NetTopologySuiteGeometryTypeMapping _multiLineString  = new NetTopologySuiteGeometryTypeMapping(typeof(MultiLineString));
        readonly NetTopologySuiteGeometryTypeMapping _multiPolygon     = new NetTopologySuiteGeometryTypeMapping(typeof(MultiPolygon));
        readonly NetTopologySuiteGeometryTypeMapping _collection       = new NetTopologySuiteGeometryTypeMapping(typeof(GeometryCollection));
        readonly NetTopologySuiteGeometryTypeMapping _geometry         = new NetTopologySuiteGeometryTypeMapping(typeof(Geometry));

        readonly NetTopologySuiteGeometryTypeMapping _ipoint           = new NetTopologySuiteGeometryTypeMapping(typeof(IPoint));
        readonly NetTopologySuiteGeometryTypeMapping _ilineString      = new NetTopologySuiteGeometryTypeMapping(typeof(ILineString));
        readonly NetTopologySuiteGeometryTypeMapping _ipolygon         = new NetTopologySuiteGeometryTypeMapping(typeof(IPolygon));
        readonly NetTopologySuiteGeometryTypeMapping _imultiPoint      = new NetTopologySuiteGeometryTypeMapping(typeof(IMultiPoint));
        readonly NetTopologySuiteGeometryTypeMapping _imultiLineString = new NetTopologySuiteGeometryTypeMapping(typeof(IMultiLineString));
        readonly NetTopologySuiteGeometryTypeMapping _imultiPolygon    = new NetTopologySuiteGeometryTypeMapping(typeof(IMultiPolygon));
        readonly NetTopologySuiteGeometryTypeMapping _icollection      = new NetTopologySuiteGeometryTypeMapping(typeof(IGeometryCollection));
        readonly NetTopologySuiteGeometryTypeMapping _igeometry        = new NetTopologySuiteGeometryTypeMapping(typeof(IGeometry));

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

        readonly NetTopologySuiteGeographyTypeMapping _igeogPoint           = new NetTopologySuiteGeographyTypeMapping(typeof(IPoint));
        readonly NetTopologySuiteGeographyTypeMapping _igeogLineString      = new NetTopologySuiteGeographyTypeMapping(typeof(ILineString));
        readonly NetTopologySuiteGeographyTypeMapping _igeogPolygon         = new NetTopologySuiteGeographyTypeMapping(typeof(IPolygon));
        readonly NetTopologySuiteGeographyTypeMapping _igeogMultiPoint      = new NetTopologySuiteGeographyTypeMapping(typeof(IMultiPoint));
        readonly NetTopologySuiteGeographyTypeMapping _igeogMultiLineString = new NetTopologySuiteGeographyTypeMapping(typeof(IMultiLineString));
        readonly NetTopologySuiteGeographyTypeMapping _igeogMultiPolygon    = new NetTopologySuiteGeographyTypeMapping(typeof(IMultiPolygon));
        readonly NetTopologySuiteGeographyTypeMapping _igeogCollection      = new NetTopologySuiteGeographyTypeMapping(typeof(IGeometryCollection));
        readonly NetTopologySuiteGeographyTypeMapping _igeography           = new NetTopologySuiteGeographyTypeMapping(typeof(IGeometry));

        #endregion Geography mappings

        public override void AddMappings(NpgsqlTypeMappingSource typeMappingSource)
        {
            typeMappingSource.ClrTypeMappings[typeof(Geometry)]           = _geometry;
            typeMappingSource.ClrTypeMappings[typeof(Point)]              = _point;
            typeMappingSource.ClrTypeMappings[typeof(LineString)]         = _lineString;
            typeMappingSource.ClrTypeMappings[typeof(Polygon)]            = _polygon;
            typeMappingSource.ClrTypeMappings[typeof(MultiPoint)]         = _multiPoint;
            typeMappingSource.ClrTypeMappings[typeof(MultiLineString)]    = _multiLineString;
            typeMappingSource.ClrTypeMappings[typeof(MultiPolygon)]       = _multiPolygon;
            typeMappingSource.ClrTypeMappings[typeof(GeometryCollection)] = _collection;

            typeMappingSource.ClrTypeMappings[typeof(IGeometry)]           = _igeometry;
            typeMappingSource.ClrTypeMappings[typeof(IPoint)]              = _ipoint;
            typeMappingSource.ClrTypeMappings[typeof(ILineString)]         = _ilineString;
            typeMappingSource.ClrTypeMappings[typeof(IPolygon)]            = _ipolygon;
            typeMappingSource.ClrTypeMappings[typeof(IMultiPoint)]         = _imultiPoint;
            typeMappingSource.ClrTypeMappings[typeof(IMultiLineString)]    = _imultiLineString;
            typeMappingSource.ClrTypeMappings[typeof(IMultiPolygon)]       = _imultiPolygon;
            typeMappingSource.ClrTypeMappings[typeof(IGeometryCollection)] = _icollection;

            typeMappingSource.StoreTypeMappings["geometry"] = new RelationalTypeMapping[]
            {
                _geometry, _point, _lineString, _polygon, _multiPoint, _multiLineString, _multiPolygon, _collection,
                _igeometry, _ipoint, _ilineString, _ipolygon, _imultiPoint, _imultiLineString, _imultiPolygon, _icollection
            };
            typeMappingSource.StoreTypeMappings["geography"] = new RelationalTypeMapping[]
            {
                _geography, _geogPoint, _geogLineString, _geogPolygon, _geogMultiPoint, _geogMultiLineString, _geogMultiPolygon, _geogCollection,
                _igeography, _igeogPoint, _igeogLineString, _igeogPolygon, _igeogMultiPoint, _igeogMultiLineString, _igeogMultiPolygon, _igeogCollection
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

        public override void AddMethodCallTranslators(NpgsqlCompositeMethodCallTranslator compositeMethodCallTranslator)
            => compositeMethodCallTranslator.AddTranslators(MethodCallTranslators);

        public override void AddMemberTranslators(NpgsqlCompositeMemberTranslator compositeMemberTranslator)
            => compositeMemberTranslator.AddTranslators(MemberTranslators);
    }
}
