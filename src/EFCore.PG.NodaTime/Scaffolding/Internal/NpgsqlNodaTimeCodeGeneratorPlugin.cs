using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Scaffolding.Internal;

public class NpgsqlNodaTimeCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
{
    private static readonly MethodInfo _useNodaTimeMethodInfo
        = typeof(NpgsqlNodaTimeDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlNodaTimeDbContextOptionsBuilderExtensions.UseNodaTime),
            typeof(NpgsqlDbContextOptionsBuilder));

    public override MethodCallCodeFragment GenerateProviderOptions()
        => new(_useNodaTimeMethodInfo);
}