using System;
using System.Data;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlTimeSpanTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTimeSpanTypeMapping()
            : base("interval", typeof(TimeSpan), NpgsqlTypes.NpgsqlDbType.Interval)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlTimeSpanTypeMapping();

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var ts = (TimeSpan)value;
            return $"INTERVAL '{ts.ToString($@"{(ts < TimeSpan.Zero ? "\\-" : "")}{(ts.Days == 0 ? "" : "d\\ ")}hh\:mm\:ss{(ts.Milliseconds == 0 ? "" : $"\\.FFF")}")}'";
        }
    }
}
