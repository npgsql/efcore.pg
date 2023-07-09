namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlCubeTypeMapping : NpgsqlTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlCubeTypeMapping() : base("cube", typeof(NpgsqlCube), NpgsqlDbType.Cube) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlCubeTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Cube) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlCubeTypeMapping(parameters);

    /// <summary>
    ///     Generates the SQL representation of a non-null literal value.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>The generated string.</returns>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        if (!(value is NpgsqlCube cube))
            throw new InvalidOperationException($"Can't generate a cube SQL literal for CLR type {value.GetType()}");

        if (cube.Point)
            return $"'({string.Join(",", cube.LowerLeft)})'::cube";
        else
            return $"'({string.Join(",", cube.LowerLeft)}),({string.Join(",", cube.UpperRight)})'::cube";
    }
}
