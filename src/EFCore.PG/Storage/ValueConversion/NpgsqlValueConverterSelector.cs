using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public override IEnumerable<ValueConverterInfo> Select(Type modelArrayClrType, Type providerArrayClrType = null)
        {
            var converters = default(IEnumerable<ValueConverterInfo>);
            if (modelArrayClrType.IsArray && (providerArrayClrType == null || providerArrayClrType.IsArray))
            {
                var modelElementType = modelArrayClrType.GetElementType();
                Debug.Assert(modelElementType != null);

                var providerElementType = default(Type);
                if (providerArrayClrType != null)
                {
                    providerElementType = providerArrayClrType.GetElementType();
                    Debug.Assert(providerElementType != null);
                }

                // For each ValueConverterInfo selected by the superclass for the element type, return a ValueConverterInfo for its array type
                converters = base
                    .Select(modelElementType, providerElementType)
                    .Select(elementConverterInfo => _arrayConverters.GetOrAdd(
                        (elementConverterInfo.ModelClrType, elementConverterInfo.ProviderClrType),
                        new ValueConverterInfo(
                            modelArrayClrType,
                            elementConverterInfo.ProviderClrType.MakeArrayType(),
                            ci => (ValueConverter)Activator.CreateInstance(
                                typeof(NpgsqlArrayConverter<,>).MakeGenericType(modelArrayClrType, elementConverterInfo.ProviderClrType.MakeArrayType()),
                                    elementConverterInfo.Create()))));
            }

            var baseConverters = base.Select(modelArrayClrType, providerArrayClrType);
            return converters == null ? baseConverters : converters.Concat(baseConverters);
        }
    }
}
