using System;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    internal static class VersionExtensions
    {
        // Note: a null version is interpreted as the latest version and will always return true.
        internal static bool AtLeast(this Version version, int major, int minor)
            => version is null || version >= new Version(major, minor);
    }
}
