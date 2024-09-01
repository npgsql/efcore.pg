using System.Net.NetworkInformation;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Json;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class JsonMacaddrReaderWriter : JsonValueReaderWriter<PhysicalAddress>
{
    private static readonly PropertyInfo InstanceProperty = typeof(JsonMacaddrReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static JsonMacaddrReaderWriter Instance { get; } = new();

    private JsonMacaddrReaderWriter()
    {
    }

    /// <inheritdoc />
    public override PhysicalAddress FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => PhysicalAddress.Parse(manager.CurrentReader.GetString()!);

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, PhysicalAddress value)
        => writer.WriteStringValue(value.ToString());

    /// <inheritdoc />
    public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
}
