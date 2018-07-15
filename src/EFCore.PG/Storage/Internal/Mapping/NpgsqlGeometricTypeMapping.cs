using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlPointTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPointTypeMapping() : base("point", typeof(NpgsqlPoint), NpgsqlDbType.Point) {}

        protected NpgsqlPointTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlPointTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlPointTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var point = (NpgsqlPoint)value;
            return $"POINT '({point.X.ToString("G17", CultureInfo.InvariantCulture)},{point.Y.ToString("G17", CultureInfo.InvariantCulture)})'";
        }
    }

    public class NpgsqlLineTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlLineTypeMapping() : base("line", typeof(NpgsqlLine), NpgsqlDbType.Line) {}

        protected NpgsqlLineTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlLineTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlLineTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var line = (NpgsqlLine)value;
            return $"LINE '{{{line.A.ToString("G17", CultureInfo.InvariantCulture)},{line.B.ToString("G17", CultureInfo.InvariantCulture)},{line.C.ToString("G17", CultureInfo.InvariantCulture)}}}'";
        }
    }

    public class NpgsqlLineSegmentTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlLineSegmentTypeMapping() : base("lseg", typeof(NpgsqlLSeg), NpgsqlDbType.LSeg) {}

        protected NpgsqlLineSegmentTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlLineSegmentTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlLineSegmentTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var lseg = (NpgsqlLSeg)value;
            var x1 = lseg.Start.X.ToString("G17", CultureInfo.InvariantCulture);
            var y1 = lseg.Start.Y.ToString("G17", CultureInfo.InvariantCulture);
            var x2 = lseg.End.X.ToString("G17", CultureInfo.InvariantCulture);
            var y2 = lseg.End.Y.ToString("G17", CultureInfo.InvariantCulture);
            return $"LSEG '[({x1},{y1}),({x2},{y2})]'";
        }
    }

    public class NpgsqlBoxTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlBoxTypeMapping() : base("box", typeof(NpgsqlBox), NpgsqlDbType.Box) {}

        protected NpgsqlBoxTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlBoxTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlBoxTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var box = (NpgsqlBox)value;
            var right  = box.Right.ToString("G17", CultureInfo.InvariantCulture);
            var top    = box.Top.ToString("G17", CultureInfo.InvariantCulture);
            var left   = box.Left.ToString("G17", CultureInfo.InvariantCulture);
            var bottom = box.Bottom.ToString("G17", CultureInfo.InvariantCulture);
            return $"BOX '(({right},{top}),({left},{bottom}))'";
        }
    }

    public class NpgsqlPathTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPathTypeMapping() : base("path", typeof(NpgsqlPath), NpgsqlDbType.Path) {}

        protected NpgsqlPathTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlPathTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlPathTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var path = (NpgsqlPath)value;
            var sb = new StringBuilder();
            sb.Append("PATH '");
            sb.Append(path.Open ? '[' : '(');
            for (var i = 0; i < path.Count; i++)
            {
                sb.Append('(');
                sb.Append(path[i].X.ToString("G17", CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(path[i].Y.ToString("G17", CultureInfo.InvariantCulture));
                sb.Append(')');
                if (i < path.Count - 1)
                    sb.Append(',');
            }
            sb.Append(path.Open ? ']' : ')');
            sb.Append('\'');
            return sb.ToString();
        }
    }

    public class NpgsqlPolygonTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPolygonTypeMapping() : base("polygon", typeof(NpgsqlPolygon), NpgsqlDbType.Polygon) {}

        protected NpgsqlPolygonTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlPolygonTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlPolygonTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var polygon = (NpgsqlPolygon)value;
            var sb = new StringBuilder();
            sb.Append("POLYGON '(");
            for (var i = 0; i < polygon.Count; i++)
            {
                sb.Append('(');
                sb.Append(polygon[i].X.ToString("G17", CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(polygon[i].Y.ToString("G17", CultureInfo.InvariantCulture));
                sb.Append(')');
                if (i < polygon.Count - 1)
                    sb.Append(',');
            }
            sb.Append(")'");
            return sb.ToString();
        }
    }

    public class NpgsqlCircleTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlCircleTypeMapping() : base("circle", typeof(NpgsqlCircle), NpgsqlDbType.Circle) {}

        protected NpgsqlCircleTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlCircleTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlCircleTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var circle = (NpgsqlCircle)value;
            var x = circle.X.ToString("G17", CultureInfo.InvariantCulture);
            var y = circle.Y.ToString("G17", CultureInfo.InvariantCulture);
            var radius = circle.Radius.ToString("G17", CultureInfo.InvariantCulture);
            return $"CIRCLE '<({x},{y}),{radius}>'";
        }
    }
}
