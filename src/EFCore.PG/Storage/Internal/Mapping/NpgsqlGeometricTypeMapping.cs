using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlPointTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPointTypeMapping() : base("point", typeof(NpgsqlPoint), NpgsqlDbType.Point) {}

        protected NpgsqlPointTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Point) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlPointTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var point = (NpgsqlPoint)value;
            return FormattableString.Invariant($"POINT '({point.X:G17},{point.Y:G17})'");
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var point = (NpgsqlPoint)value;
            return Expression.New(Constructor, Expression.Constant(point.X), Expression.Constant(point.Y));
        }

        static readonly ConstructorInfo Constructor =
            typeof(NpgsqlPoint).GetConstructor(new[] { typeof(double), typeof(double) });
    }

    public class NpgsqlLineTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlLineTypeMapping() : base("line", typeof(NpgsqlLine), NpgsqlDbType.Line) {}

        protected NpgsqlLineTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Line) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlLineTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var line = (NpgsqlLine)value;
            var a = line.A.ToString("G17", CultureInfo.InvariantCulture);
            var b = line.B.ToString("G17", CultureInfo.InvariantCulture);
            var c = line.C.ToString("G17", CultureInfo.InvariantCulture);
            return $"LINE '{{{a},{b},{c}}}'";
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var line = (NpgsqlLine)value;
            return Expression.New(
                Constructor,
                Expression.Constant(line.A), Expression.Constant(line.B), Expression.Constant(line.C));
        }

        static readonly ConstructorInfo Constructor =
            typeof(NpgsqlLine).GetConstructor(new[] { typeof(double), typeof(double), typeof(double) });
    }

    public class NpgsqlLineSegmentTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlLineSegmentTypeMapping() : base("lseg", typeof(NpgsqlLSeg), NpgsqlDbType.LSeg) {}

        protected NpgsqlLineSegmentTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.LSeg) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlLineSegmentTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var lseg = (NpgsqlLSeg)value;
            return FormattableString.Invariant($"LSEG '[({lseg.Start.X:G17},{lseg.Start.Y:G17}),({lseg.End.X:G17},{lseg.End.Y:G17})]'");
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var lseg = (NpgsqlLSeg)value;
            return Expression.New(
                Constructor,
                Expression.Constant(lseg.Start.X), Expression.Constant(lseg.Start.Y),
                Expression.Constant(lseg.End.X), Expression.Constant(lseg.End.Y));
        }

        static readonly ConstructorInfo Constructor =
            typeof(NpgsqlLSeg).GetConstructor(new[] { typeof(double), typeof(double), typeof(double), typeof(double) });
    }

    public class NpgsqlBoxTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlBoxTypeMapping() : base("box", typeof(NpgsqlBox), NpgsqlDbType.Box) {}

        protected NpgsqlBoxTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Box) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlBoxTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var box = (NpgsqlBox)value;
            return FormattableString.Invariant($"BOX '(({box.Right:G17},{box.Top:G17}),({box.Left:G17},{box.Bottom:G17}))'");
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var box = (NpgsqlBox)value;
            return Expression.New(
                Constructor,
                Expression.Constant(box.Top), Expression.Constant(box.Right),
                Expression.Constant(box.Bottom), Expression.Constant(box.Left));
        }

        static readonly ConstructorInfo Constructor =
            typeof(NpgsqlBox).GetConstructor(new[] { typeof(double), typeof(double), typeof(double), typeof(double) });
    }

    public class NpgsqlPathTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPathTypeMapping() : base("path", typeof(NpgsqlPath), NpgsqlDbType.Path) {}

        protected NpgsqlPathTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Path) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlPathTypeMapping(parameters);

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

        public override Expression GenerateCodeLiteral(object value)
        {
            var path = (NpgsqlPath)value;
            return Expression.New(
                Constructor,
                Expression.NewArrayInit(typeof(NpgsqlPoint),
                    path.Select(p => Expression.New(
                        PointConstructor,
                        Expression.Constant(p.X), Expression.Constant(p.Y)))),
                Expression.Constant(path.Open));
        }

        static readonly ConstructorInfo Constructor =
            typeof(NpgsqlPath).GetConstructor(new[] { typeof(IEnumerable<NpgsqlPoint>), typeof(bool) });

        static readonly ConstructorInfo PointConstructor =
            typeof(NpgsqlPoint).GetConstructor(new[] { typeof(double), typeof(double) });
    }

    public class NpgsqlPolygonTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPolygonTypeMapping() : base("polygon", typeof(NpgsqlPolygon), NpgsqlDbType.Polygon) {}

        protected NpgsqlPolygonTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Polygon) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlPolygonTypeMapping(parameters);

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

        public override Expression GenerateCodeLiteral(object value)
        {
            var polygon = (NpgsqlPolygon)value;
            return Expression.New(
                Constructor,
                Expression.NewArrayInit(typeof(NpgsqlPoint),
                    polygon.Select(p => Expression.New(
                        PointConstructor,
                        Expression.Constant(p.X), Expression.Constant(p.Y)))));
        }

        static readonly ConstructorInfo Constructor =
            typeof(NpgsqlPolygon).GetConstructor(new[] { typeof(NpgsqlPoint[]) });

        static readonly ConstructorInfo PointConstructor =
            typeof(NpgsqlPoint).GetConstructor(new[] { typeof(double), typeof(double) });
    }

    public class NpgsqlCircleTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlCircleTypeMapping() : base("circle", typeof(NpgsqlCircle), NpgsqlDbType.Circle) {}

        protected NpgsqlCircleTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Circle) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlCircleTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var circle = (NpgsqlCircle)value;
            return FormattableString.Invariant($"CIRCLE '<({circle.X:G17},{circle.Y:G17}),{circle.Radius:G17}>'");
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var circle = (NpgsqlCircle)value;
            return Expression.New(
                Constructor,
                Expression.Constant(circle.X), Expression.Constant(circle.Y), Expression.Constant(circle.Radius));
        }

        static readonly ConstructorInfo Constructor =
            typeof(NpgsqlCircle).GetConstructor(new[] { typeof(double), typeof(double), typeof(double) });
    }
}
