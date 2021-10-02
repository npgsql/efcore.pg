using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion
{
    public class NpgsqlValueConverterSelector : ValueConverterSelector
    {
        private readonly ConcurrentDictionary<(Type ModelElementClrType, Type ProviderElementClrType), ValueConverterInfo> _arrayConverters
            = new();

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
}
