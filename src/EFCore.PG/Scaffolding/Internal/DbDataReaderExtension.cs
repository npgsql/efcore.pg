using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;

internal static class DbDataReaderExtension
{
    [DebuggerStepThrough]
    [return: MaybeNull]
    internal static T GetValueOrDefault<T>(this DbDataReader reader, string name)
    {
        var idx = reader.GetOrdinal(name);
        return reader.IsDBNull(idx)
            ? default
            : reader.GetFieldValue<T>(idx);
    }

    [DebuggerStepThrough]
    [return: MaybeNull]
    internal static T GetValueOrDefault<T>(this DbDataRecord record, string name)
    {
        var idx = record.GetOrdinal(name);
        return record.IsDBNull(idx)
            ? default
            : (T)record.GetValue(idx);
    }

    [DebuggerStepThrough]
    internal static T GetFieldValue<T>(this DbDataRecord record, string name)
        => (T)record.GetValue(record.GetOrdinal(name));
}