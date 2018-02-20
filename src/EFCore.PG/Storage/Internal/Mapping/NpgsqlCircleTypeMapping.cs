using System.Globalization;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlCircleTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlCircleTypeMapping() : base("circle", typeof(NpgsqlCircle), NpgsqlDbType.Circle) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var circle = (NpgsqlCircle)value;
            var x = circle.X.ToString("G17", CultureInfo.InvariantCulture);
            var y = circle.Y.ToString("G17", CultureInfo.InvariantCulture);
            var radius = circle.Radius.ToString("G17", CultureInfo.InvariantCulture);
            return $"(({x},{y}),{radius})";
        }
    }
}
