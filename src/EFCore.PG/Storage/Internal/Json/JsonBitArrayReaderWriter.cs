using System.Collections;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Json;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class JsonBitArrayReaderWriter : JsonValueReaderWriter<BitArray>
{
    private static readonly PropertyInfo InstanceProperty = typeof(JsonBitArrayReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static JsonBitArrayReaderWriter Instance { get; } = new();

    /// <inheritdoc />
    public override BitArray FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
    {
        var s = manager.CurrentReader.GetString()!;

        var bitArray = new BitArray(s.Length);

        for (var i = 0; i < s.Length; i++)
        {
            bitArray[i] = s[i] switch
            {
                '1' => true,
                '0' => false,
                _ => throw new InvalidOperationException($"Invalid character '{s[i]}' in BitArray string representation in JSON")
            };
        }

        return bitArray;
    }

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, BitArray value)
        => writer.WriteStringValue(
            string.Create(
                value.Length, value, (s, a) =>
                {
                    for (var i = 0; i < s.Length; i++)
                    {
                        s[i] = a[i] ? '1' : '0';
                    }
                }));

    /// <inheritdoc />
    public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
}
