using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion
{
    public class NpgsqlValueConverterSelector : ValueConverterSelector
    {
        readonly ConcurrentDictionary<(Type ModelElementClrType, Type ProviderElementClrType), ValueConverterInfo> _arrayConverters
            = new ConcurrentDictionary<(Type, Type), ValueConverterInfo>();

        public NpgsqlValueConverterSelector([NotNull] ValueConverterSelectorDependencies dependencies)
            : base(dependencies) {}

        /// <inheritdoc />
        public override IEnumerable<ValueConverterInfo> Select(Type modelClrType, Type providerClrType = null)
        {
            var providerElementType = default(Type);

            if (modelClrType.TryGetElementType(out var modelElementType) &&
                (providerClrType == null || providerClrType.TryGetElementType(out providerElementType)))
            {
                // For each ValueConverterInfo selected by the superclass for the element type,
                // return a ValueConverterInfo for its array type
                return base
                    .Select(modelElementType, providerElementType)
                    .Select(elementConverterInfo => new
                    {
                        ModelArrayType = modelClrType,
                        ProviderArrayType = providerClrType ?? elementConverterInfo.ProviderClrType.MakeArrayType(),
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
                                x.ElementConverterInfo.Create()))))
                    .Concat(base.Select(modelClrType, providerClrType));
            }

            return base.Select(modelClrType, providerClrType);
        }
    }
}
