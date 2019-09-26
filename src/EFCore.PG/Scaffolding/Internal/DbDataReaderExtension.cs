using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal
{
    public static class DbDataReaderExtension
    {
        [DebuggerStepThrough]
        public static T GetValueOrDefault<T>([NotNull] this DbDataReader reader, [NotNull] string name)
        {
            var idx = reader.GetOrdinal(name);
            return reader.IsDBNull(idx)
                ? default(T)
                : reader.GetFieldValue<T>(idx);
        }

        [DebuggerStepThrough]
        public static T GetValueOrDefault<T>([NotNull] this DbDataRecord record, [NotNull] string name)
        {
            var idx = record.GetOrdinal(name);
            return record.IsDBNull(idx)
                ? default
                : (T)record.GetValue(idx);
        }
    }
}
