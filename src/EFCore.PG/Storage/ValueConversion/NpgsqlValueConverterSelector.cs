using System.Collections.Concurrent;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlValueConverterSelector : ValueConverterSelector
{
    private readonly ConcurrentDictionary<(Type ModelElementClrType, Type ProviderElementClrType), ValueConverterInfo> _arrayConverters
        = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlValueConverterSelector(ValueConverterSelectorDependencies dependencies)
        : base(dependencies) {}

    /// <inheritdoc />
    public override IEnumerable<ValueConverterInfo> Select(Type modelClrType, Type? providerClrType = null)
    {
        var providerElementType = default(Type);

        if (modelClrType.TryGetElementType(out var modelElementType) &&
            (providerClrType is null || providerClrType.TryGetElementType(out providerElementType)))
        {
            // For each ValueConverterInfo selected by the superclass for the element type,
            // return a ValueConverterInfo for its array type

            // Note that the value converter system operates with the assumption that nullable CLR types have already been
            // unwrapped. This means that when looking for element type converters, we need to unwrap.
            var arrayConverters = base
                .Select(modelElementType.UnwrapNullableType(), providerElementType)
                .Select(elementConverterInfo => new
                {
                    ModelArrayType = modelClrType,
                    ProviderArrayType = providerClrType ?? (modelElementType.IsNullableType()
                        ? elementConverterInfo.ProviderClrType.MakeNullable().MakeArrayType()
                        : elementConverterInfo.ProviderClrType.MakeArrayType()),
                    ElementConverterInfo = elementConverterInfo
                })
                .Select(x => _arrayConverters.GetOrAdd(
                    (x.ModelArrayType, x.ProviderArrayType),
                    new ValueConverterInfo(
                        x.ModelArrayType,
                        x.ProviderArrayType,
                        _ => (ValueConverter)Activator.CreateInstance(
                            typeof(NpgsqlArrayConverter<,>).MakeGenericType(
                                modelClrType,
                                x.ProviderArrayType),
                            x.ElementConverterInfo.Create())!)));

            return arrayConverters.Concat(base.Select(modelClrType, providerClrType));
        }

        return base.Select(modelClrType, providerClrType);
    }
}
