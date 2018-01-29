using System.Globalization;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlPointTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPointTypeMapping() : base("point", typeof(NpgsqlPoint), NpgsqlDbType.Point) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var point = (NpgsqlPoint)value;
            return $"({point.X.ToString("G17", CultureInfo.InvariantCulture)},{point.Y.ToString("G17", CultureInfo.InvariantCulture)})";
        }
    }
}
