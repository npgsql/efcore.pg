using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite.Mappings;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite.Translators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite.Infrastructure
{
    public class NpgsqlNetTopologySuitePlugin : IEntityFrameworkNpgsqlPlugin
    {
        public string Name => "NetTopologySuite";
        public string Description => "Plugin to map PostGIS types to NetTopologySuite";

        readonly NpgsqlNtsGeometryTypeMapping        _geometry        = new NpgsqlNtsGeometryTypeMapping();

        readonly NpgsqlNtsPointTypeMapping           _point           = new NpgsqlNtsPointTypeMapping();
        readonly NpgsqlNtsLineStringTypeMapping      _lineString      = new NpgsqlNtsLineStringTypeMapping();
        readonly NpgsqlNtsPolygonTypeMapping         _polygon         = new NpgsqlNtsPolygonTypeMapping();
        readonly NpgsqlNtsMultiPointTypeMapping      _multiPoint      = new NpgsqlNtsMultiPointTypeMapping();
        readonly NpgsqlNtsMultiLineStringTypeMapping _multiLineString = new NpgsqlNtsMultiLineStringTypeMapping();
        readonly NpgsqlNtsMultiPolygonTypeMapping    _multiPolygon    = new NpgsqlNtsMultiPolygonTypeMapping();

        public void AddMappings(NpgsqlTypeMappingSource typeMappingSource)
        {
            typeMappingSource.ClrTypeMappings[typeof(Geometry)] =        _geometry;
            typeMappingSource.ClrTypeMappings[typeof(Point)] =           _point;
            typeMappingSource.ClrTypeMappings[typeof(LineString)] =      _lineString;
            typeMappingSource.ClrTypeMappings[typeof(Polygon)] =         _polygon;
            typeMappingSource.ClrTypeMappings[typeof(MultiPoint)] =      _multiPoint;
            typeMappingSource.ClrTypeMappings[typeof(MultiLineString)] = _multiLineString;
            typeMappingSource.ClrTypeMappings[typeof(MultiPolygon)] =    _multiPolygon;

            typeMappingSource.StoreTypeMappings["geometry"] = new RelationalTypeMapping[]
            {
                _geometry, _point, _lineString, _polygon, _multiPoint, _multiLineString, _multiPolygon
            };
        }

        static readonly IMethodCallTranslator[] _methodCallTranslators =
        {
            new NpgsqlNtsTranslator(),
        };

        public void AddMethodCallTranslators(NpgsqlCompositeMethodCallTranslator compositeMethodCallTranslator)
        {
            compositeMethodCallTranslator.AddTranslators(_methodCallTranslators);
        }
    }
}
