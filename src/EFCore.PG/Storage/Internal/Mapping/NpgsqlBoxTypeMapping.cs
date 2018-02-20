using System.Globalization;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlBoxTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlBoxTypeMapping() : base("box", typeof(NpgsqlBox), NpgsqlDbType.Box) {}

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var box = (NpgsqlBox)value;
            var right  = box.Right.ToString("G17", CultureInfo.InvariantCulture);
            var top    = box.Top.ToString("G17", CultureInfo.InvariantCulture);
            var left   = box.Left.ToString("G17", CultureInfo.InvariantCulture);
            var bottom = box.Bottom.ToString("G17", CultureInfo.InvariantCulture);
            return $"({right},{top},{left},{bottom})";
        }
    }
}
