using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;

/// <summary>
/// The default code generator for Npgsql.
/// </summary>
public class NpgsqlCodeGenerator : ProviderCodeGenerator
{
    private static readonly MethodInfo _useNpgsqlMethodInfo
        = typeof(NpgsqlDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlDbContextOptionsBuilderExtensions.UseNpgsql),
            typeof(DbContextOptionsBuilder),
            typeof(string),
            typeof(Action<NpgsqlDbContextOptionsBuilder>));

    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlCodeGenerator"/> class.
    /// </summary>
    /// <param name="dependencies">The dependencies.</param>
    public NpgsqlCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
        : base(dependencies) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override MethodCallCodeFragment GenerateUseProvider(
        string connectionString,
        MethodCallCodeFragment? providerOptions)
        => new(
            _useNpgsqlMethodInfo,
            providerOptions is null
                ? new object[] { connectionString }
                : new object[] { connectionString, new NestedClosureCodeFragment("x", providerOptions) });
}
