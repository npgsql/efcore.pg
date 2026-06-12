// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
public class NpgsqlCSharpRuntimeAnnotationCodeGenerator
    : RelationalCSharpRuntimeAnnotationCodeGenerator, ICSharpRuntimeAnnotationCodeGenerator
{
    private int _typeMappingNestingCount;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlCSharpRuntimeAnnotationCodeGenerator(
        CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies,
        RelationalCSharpRuntimeAnnotationCodeGeneratorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Create(
        CoreTypeMapping typeMapping,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ValueComparer? valueComparer = null,
        ValueComparer? keyValueComparer = null,
        ValueComparer? providerValueComparer = null)
    {
        _typeMappingNestingCount++;

        try
        {
            var result = base.Create(typeMapping, parameters, valueComparer, keyValueComparer, providerValueComparer);
            AddNpgsqlTypeMappingTweaks(typeMapping, parameters);
            return result;
        }
        finally
        {
            _typeMappingNestingCount--;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual void AddNpgsqlTypeMappingTweaks(
        CoreTypeMapping typeMapping,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        var mainBuilder = parameters.MainBuilder;

        var npgsqlDbTypeBasedDefaultInstance = typeMapping switch
        {
            NpgsqlStringTypeMapping => NpgsqlStringTypeMapping.Default,
            NpgsqlUIntTypeMapping => NpgsqlUIntTypeMapping.Default,
            NpgsqlULongTypeMapping => NpgsqlULongTypeMapping.Default,
            // NpgsqlMultirangeTypeMapping => NpgsqlMultirangeTypeMapping.Default,
            _ => (INpgsqlTypeMapping?)null
        };

        if (npgsqlDbTypeBasedDefaultInstance is not null)
        {
            CheckElementTypeMapping();

            var npgsqlDbType = ((INpgsqlTypeMapping)typeMapping).NpgsqlDbType;

            if (npgsqlDbType != npgsqlDbTypeBasedDefaultInstance.NpgsqlDbType)
            {
                mainBuilder.AppendLine(";");

                mainBuilder.Append(
                    $"{parameters.TargetName}.TypeMapping = (({typeMapping.GetType().Name}){parameters.TargetName}.TypeMapping).Clone(npgsqlDbType: ");

                mainBuilder
                    .Append(nameof(NpgsqlTypes))
                    .Append(".")
                    .Append(nameof(NpgsqlDbType))
                    .Append(".")
                    .Append(npgsqlDbType.ToString());

                mainBuilder
                    .Append(")")
                    .DecrementIndent();
            }

        }

        switch (typeMapping)
        {
            case NpgsqlEnumTypeMapping enumTypeMapping:
                CheckElementTypeMapping();

                var code = Dependencies.CSharpHelper;
                mainBuilder.AppendLine(";");

                mainBuilder.AppendLine(
                        $"{parameters.TargetName}.TypeMapping = ((NpgsqlEnumTypeMapping){parameters.TargetName}.TypeMapping).Clone(")
                    .IncrementIndent();

                mainBuilder
                    .Append("unquotedStoreType: ")
                    .Append(code.Literal(enumTypeMapping.UnquotedStoreType))
                    .AppendLine(",")
                    .AppendLine("labels: new Dictionary<object, string>()")
                    .AppendLine("{")
                    .IncrementIndent();

                foreach (var (enumValue, label) in enumTypeMapping.Labels)
                {
                    mainBuilder
                        .Append('[')
                        .Append(code.UnknownLiteral(enumValue))
                        .Append(']')
                        .Append(" = ")
                        .Append(code.Literal(label))
                        .AppendLine(",");
                }

                mainBuilder
                    .Append("}")
                    .DecrementIndent()
                    .Append(")")
                    .DecrementIndent();

                break;

            case NpgsqlRangeTypeMapping rangeTypeMapping:
            {
                CheckElementTypeMapping();

                var defaultInstance = NpgsqlRangeTypeMapping.Default;

                var npgsqlDbTypeDifferent = rangeTypeMapping.NpgsqlDbType != defaultInstance.NpgsqlDbType;
                var subtypeTypeMappingIsDifferent = rangeTypeMapping.SubtypeMapping != defaultInstance.SubtypeMapping;

                if (npgsqlDbTypeDifferent || subtypeTypeMappingIsDifferent)
                {
                    mainBuilder.AppendLine(";");

                    mainBuilder.AppendLine(
                        $"{parameters.TargetName}.TypeMapping = ((NpgsqlRangeTypeMapping){parameters.TargetName}.TypeMapping).Clone(")
                        .IncrementIndent();

                    mainBuilder
                        .Append("npgsqlDbType: ")
                        .Append(nameof(NpgsqlTypes))
                        .Append(".")
                        .Append(nameof(NpgsqlDbType))
                        .Append(".")
                        .Append(rangeTypeMapping.NpgsqlDbType.ToString())
                        .AppendLine(",");

                    mainBuilder.Append("subtypeTypeMapping: ");

                    Create(rangeTypeMapping.SubtypeMapping, parameters);

                    mainBuilder
                        .Append(")")
                        .DecrementIndent();
                }

                break;
            }
        }

        void CheckElementTypeMapping()
        {
            if (_typeMappingNestingCount > 1)
            {
                throw new NotSupportedException(
                    $"Non-default Npgsql type mappings ('{typeMapping.GetType().Name}' with store type '{(typeMapping as RelationalTypeMapping)?.StoreType}') aren't currently supported as element type mappings, see https://github.com/npgsql/efcore.pg/issues/3366.");
            }
        }
    }

    /// <inheritdoc />
    public override void Generate(IModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            annotations.Remove(NpgsqlAnnotationNames.DatabaseTemplate);
            annotations.Remove(NpgsqlAnnotationNames.Tablespace);
            annotations.Remove(NpgsqlAnnotationNames.CollationDefinitionPrefix);

#pragma warning disable CS0618
            annotations.Remove(NpgsqlAnnotationNames.DefaultColumnCollation);
#pragma warning restore CS0618

            foreach (var annotationName in annotations.Keys.Where(
                         k =>
                             k.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal)
                             || k.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal)
                             || k.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }

        base.Generate(model, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IRelationalModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            annotations.Remove(NpgsqlAnnotationNames.DatabaseTemplate);
            annotations.Remove(NpgsqlAnnotationNames.Tablespace);
            annotations.Remove(NpgsqlAnnotationNames.CollationDefinitionPrefix);

#pragma warning disable CS0618
            annotations.Remove(NpgsqlAnnotationNames.DefaultColumnCollation);
#pragma warning restore CS0618

            foreach (var annotationName in annotations.Keys.Where(
                         k =>
                             k.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal)
                             || k.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal)
                             || k.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }

        base.Generate(model, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            annotations.Remove(NpgsqlAnnotationNames.IdentityOptions);
            annotations.Remove(NpgsqlAnnotationNames.TsVectorConfig);
            annotations.Remove(NpgsqlAnnotationNames.TsVectorProperties);
            annotations.Remove(NpgsqlAnnotationNames.CompressionMethod);

            if (!annotations.ContainsKey(NpgsqlAnnotationNames.ValueGenerationStrategy))
            {
                annotations[NpgsqlAnnotationNames.ValueGenerationStrategy] = property.GetValueGenerationStrategy();
            }
        }

        base.Generate(property, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IColumn column, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(NpgsqlAnnotationNames.IdentityOptions);
            annotations.Remove(NpgsqlAnnotationNames.TsVectorConfig);
            annotations.Remove(NpgsqlAnnotationNames.TsVectorProperties);
            annotations.Remove(NpgsqlAnnotationNames.CompressionMethod);
        }

        base.Generate(column, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(NpgsqlAnnotationNames.IndexMethod);
            annotations.Remove(NpgsqlAnnotationNames.IndexOperators);
            annotations.Remove(NpgsqlAnnotationNames.IndexSortOrder);
            annotations.Remove(NpgsqlAnnotationNames.IndexNullSortOrder);
            annotations.Remove(NpgsqlAnnotationNames.IndexInclude);
            annotations.Remove(NpgsqlAnnotationNames.CreatedConcurrently);
            annotations.Remove(NpgsqlAnnotationNames.NullsDistinct);

            foreach (var annotationName in annotations.Keys.Where(
                         k => k.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }

        base.Generate(index, parameters);
    }

    /// <inheritdoc />
    public override void Generate(ITableIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(NpgsqlAnnotationNames.IndexMethod);
            annotations.Remove(NpgsqlAnnotationNames.IndexOperators);
            annotations.Remove(NpgsqlAnnotationNames.IndexSortOrder);
            annotations.Remove(NpgsqlAnnotationNames.IndexNullSortOrder);
            annotations.Remove(NpgsqlAnnotationNames.IndexInclude);
            annotations.Remove(NpgsqlAnnotationNames.CreatedConcurrently);
            annotations.Remove(NpgsqlAnnotationNames.NullsDistinct);

            foreach (var annotationName in annotations.Keys.Where(
                         k => k.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }

        base.Generate(index, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(NpgsqlAnnotationNames.UnloggedTable);
            annotations.Remove(CockroachDbAnnotationNames.InterleaveInParent);

            foreach (var annotationName in annotations.Keys.Where(
                         k => k.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }

        base.Generate(entityType, parameters);
    }

    /// <inheritdoc />
    public override void Generate(ITable table, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(NpgsqlAnnotationNames.UnloggedTable);
            annotations.Remove(CockroachDbAnnotationNames.InterleaveInParent);

            foreach (var annotationName in annotations.Keys.Where(
                         k => k.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal)))
            {
                annotations.Remove(annotationName);
            }
        }

        base.Generate(table, parameters);
    }
}
