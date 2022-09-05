using System.Diagnostics.CodeAnalysis;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlAnnotationCodeGenerator : AnnotationCodeGenerator
{
    #region MethodInfos

    private static readonly MethodInfo ModelHasPostgresExtensionMethodInfo1
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension), typeof(ModelBuilder), typeof(string));

    private static readonly MethodInfo ModelHasPostgresExtensionMethodInfo2
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension), typeof(ModelBuilder), typeof(string), typeof(string), typeof(string));

    private static readonly MethodInfo ModelHasPostgresEnumMethodInfo1
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.HasPostgresEnum), typeof(ModelBuilder), typeof(string), typeof(string[]));

    private static readonly MethodInfo ModelHasPostgresEnumMethodInfo2
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.HasPostgresEnum), typeof(ModelBuilder), typeof(string), typeof(string), typeof(string[]));

    private static readonly MethodInfo ModelHasPostgresRangeMethodInfo1
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.HasPostgresRange), typeof(ModelBuilder), typeof(string), typeof(string));

    private static readonly MethodInfo ModelHasPostgresRangeMethodInfo2
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.HasPostgresRange), typeof(ModelBuilder), typeof(string), typeof(string),typeof(string), typeof(string),typeof(string), typeof(string),typeof(string));

    private static readonly MethodInfo ModelUseSerialColumnsMethodInfo
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.UseSerialColumns), typeof(ModelBuilder));

    private static readonly MethodInfo ModelUseIdentityAlwaysColumnsMethodInfo
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.UseIdentityAlwaysColumns), typeof(ModelBuilder));

    private static readonly MethodInfo ModelUseIdentityByDefaultColumnsMethodInfo
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns), typeof(ModelBuilder));

    private static readonly MethodInfo ModelUseHiLoMethodInfo
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.UseHiLo), typeof(ModelBuilder), typeof(string), typeof(string));

    private static readonly MethodInfo ModelHasAnnotationMethodInfo
        = typeof(ModelBuilder).GetRequiredRuntimeMethod(
            nameof(ModelBuilder.HasAnnotation), typeof(string), typeof(object));

    private static readonly MethodInfo ModelUseKeySequencesMethodInfo
        = typeof(NpgsqlModelBuilderExtensions).GetRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.UseKeySequences), new[] { typeof(ModelBuilder), typeof(string), typeof(string) })!;

    private static readonly MethodInfo EntityTypeIsUnloggedMethodInfo
        = typeof(NpgsqlEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlEntityTypeBuilderExtensions.IsUnlogged), typeof(EntityTypeBuilder), typeof(bool));

    private static readonly MethodInfo PropertyUseSerialColumnMethodInfo
        = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.UseSerialColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo PropertyUseIdentityAlwaysColumnMethodInfo
        = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo PropertyUseIdentityByDefaultColumnMethodInfo
        = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo PropertyUseHiLoMethodInfo
        = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.UseHiLo), typeof(PropertyBuilder), typeof(string), typeof(string));

    private static readonly MethodInfo PropertyHasIdentityOptionsMethodInfo
        = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.HasIdentityOptions), typeof(PropertyBuilder), typeof(long?), typeof(long?),
            typeof(long?), typeof(long?), typeof(bool?), typeof(long?));

    private static readonly MethodInfo PropertyUseSequenceMethodInfo
        = typeof(NpgsqlPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.UseSequence), new[] { typeof(PropertyBuilder), typeof(string), typeof(string) })!;

    private static readonly MethodInfo IndexUseCollationMethodInfo
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.UseCollation), typeof(IndexBuilder), typeof(string[]));

    private static readonly MethodInfo IndexHasMethodMethodInfo
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.HasMethod), typeof(IndexBuilder), typeof(string));

    private static readonly MethodInfo IndexHasOperatorsMethodInfo
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.HasOperators), typeof(IndexBuilder), typeof(string[]));

    private static readonly MethodInfo IndexHasSortOrderMethodInfo
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.HasSortOrder), typeof(IndexBuilder), typeof(SortOrder[]));

    private static readonly MethodInfo IndexHasNullSortOrderMethodInfo
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.HasNullSortOrder), typeof(IndexBuilder), typeof(NullSortOrder[]));

    private static readonly MethodInfo IndexIncludePropertiesMethodInfo
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.IncludeProperties), typeof(IndexBuilder), typeof(string[]));

    #endregion MethodInfos

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
        : base(dependencies) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsHandledByConvention(IModel model, IAnnotation annotation)
    {
        Check.NotNull(model, nameof(model));
        Check.NotNull(annotation, nameof(annotation));

        if (annotation.Name == RelationalAnnotationNames.DefaultSchema
            && (string?)annotation.Value == "public")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsHandledByConvention(IIndex index, IAnnotation annotation)
    {
        Check.NotNull(index, nameof(index));
        Check.NotNull(annotation, nameof(annotation));

        if (annotation.Name == NpgsqlAnnotationNames.IndexMethod
            && (string?)annotation.Value == "btree")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsHandledByConvention(IProperty property, IAnnotation annotation)
    {
        Check.NotNull(property, nameof(property));
        Check.NotNull(annotation, nameof(annotation));

        // The default by-convention value generation strategy is serial in pre-10 PostgreSQL,
        // and IdentityByDefault otherwise.
        if (annotation.Name == NpgsqlAnnotationNames.ValueGenerationStrategy)
        {
            // Note: both serial and identity-by-default columns are considered by-convention - we don't want
            // to assume that the PostgreSQL version of the scaffolded database necessarily determines the
            // version of the database that the scaffolded model will target. This makes life difficult for
            // models with mixed strategies but that's an edge case.
            return (NpgsqlValueGenerationStrategy?)annotation.Value switch
            {
                NpgsqlValueGenerationStrategy.SerialColumn => true,
                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn => true,
                _ => false
            };
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IModel model,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(model, annotations));

        if (GenerateValueGenerationStrategy(annotations, onModel: true) is { } valueGenerationStrategy)
        {
            fragments.Add(valueGenerationStrategy);
        }

        return fragments;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MethodCallCodeFragment? GenerateFluentApi(IModel model, IAnnotation annotation)
    {
        Check.NotNull(model, nameof(model));
        Check.NotNull(annotation, nameof(annotation));

        if (annotation.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal))
        {
            var extension = new PostgresExtension(model, annotation.Name);

            return extension.Schema is "public" or null
                ? new MethodCallCodeFragment(ModelHasPostgresExtensionMethodInfo1, extension.Name)
                : new MethodCallCodeFragment(ModelHasPostgresExtensionMethodInfo2, extension.Schema, extension.Name);
        }

        if (annotation.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal))
        {
            var enumTypeDef = new PostgresEnum(model, annotation.Name);

            return enumTypeDef.Schema is null
                ? new MethodCallCodeFragment(ModelHasPostgresEnumMethodInfo1, enumTypeDef.Name, enumTypeDef.Labels)
                : new MethodCallCodeFragment(ModelHasPostgresEnumMethodInfo2, enumTypeDef.Schema, enumTypeDef.Name, enumTypeDef.Labels);
        }

        if (annotation.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal))
        {
            var rangeTypeDef = new PostgresRange(model, annotation.Name);

            if (rangeTypeDef.Schema is null &&
                rangeTypeDef.CanonicalFunction is null &&
                rangeTypeDef.SubtypeOpClass is null &&
                rangeTypeDef.Collation is null &&
                rangeTypeDef.SubtypeDiff is null)
            {
                return new MethodCallCodeFragment(ModelHasPostgresRangeMethodInfo1, rangeTypeDef.Name, rangeTypeDef.Subtype);
            }

            return new MethodCallCodeFragment(ModelHasPostgresRangeMethodInfo2,
                rangeTypeDef.Schema,
                rangeTypeDef.Name,
                rangeTypeDef.Subtype,
                rangeTypeDef.CanonicalFunction,
                rangeTypeDef.SubtypeOpClass,
                rangeTypeDef.Collation,
                rangeTypeDef.SubtypeDiff);
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MethodCallCodeFragment? GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
    {
        Check.NotNull(entityType, nameof(entityType));
        Check.NotNull(annotation, nameof(annotation));

        if (annotation.Name == NpgsqlAnnotationNames.UnloggedTable)
        {
            return new MethodCallCodeFragment(EntityTypeIsUnloggedMethodInfo, annotation.Value);
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(property, annotations));

        if (GenerateValueGenerationStrategy(annotations, onModel: false) is { } valueGenerationStrategy)
        {
            fragments.Add(valueGenerationStrategy);
        }

        if (GenerateIdentityOptions(annotations) is { } identityOptionsFragment)
        {
            fragments.Add(identityOptionsFragment);
        }

        return fragments;
    }

    private MethodCallCodeFragment? GenerateValueGenerationStrategy(IDictionary<string, IAnnotation> annotations, bool onModel)
    {
        if (!TryGetAndRemove(annotations, NpgsqlAnnotationNames.ValueGenerationStrategy, out NpgsqlValueGenerationStrategy strategy))
        {
            return null;
        }

        switch (strategy)
        {
            case NpgsqlValueGenerationStrategy.SerialColumn:
                return new(onModel ? ModelUseSerialColumnsMethodInfo : PropertyUseSerialColumnMethodInfo);

            case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                return new(onModel ? ModelUseIdentityAlwaysColumnsMethodInfo : PropertyUseIdentityAlwaysColumnMethodInfo);

            case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                return new(onModel ? ModelUseIdentityByDefaultColumnsMethodInfo : PropertyUseIdentityByDefaultColumnMethodInfo);

            case NpgsqlValueGenerationStrategy.SequenceHiLo:
            {
                var name = GetAndRemove<string>(NpgsqlAnnotationNames.HiLoSequenceName)!;
                var schema = GetAndRemove<string>(NpgsqlAnnotationNames.HiLoSequenceSchema);
                return new(
                    onModel ? ModelUseHiLoMethodInfo : PropertyUseHiLoMethodInfo,
                    (name, schema) switch
                    {
                        (null, null) => Array.Empty<object>(),
                        (_, null) => new object[] { name },
                        _ => new object?[] { name!, schema }
                    });
            }

            case NpgsqlValueGenerationStrategy.Sequence:
            {
                var nameOrSuffix = GetAndRemove<string>(
                    onModel ? NpgsqlAnnotationNames.SequenceNameSuffix : NpgsqlAnnotationNames.SequenceName);

                var schema = GetAndRemove<string>(NpgsqlAnnotationNames.SequenceSchema);
                return new MethodCallCodeFragment(
                    onModel ? ModelUseKeySequencesMethodInfo : PropertyUseSequenceMethodInfo,
                    (name: nameOrSuffix, schema) switch
                    {
                        (null, null) => Array.Empty<object>(),
                        (_, null) => new object[] { nameOrSuffix },
                        _ => new object[] { nameOrSuffix!, schema }
                    });
            }
            case NpgsqlValueGenerationStrategy.None:
                return new(ModelHasAnnotationMethodInfo, NpgsqlAnnotationNames.ValueGenerationStrategy, NpgsqlValueGenerationStrategy.None);

            default:
                throw new ArgumentOutOfRangeException(strategy.ToString());
        }

        T? GetAndRemove<T>(string annotationName)
            => TryGetAndRemove(annotations, annotationName, out T? annotationValue)
                ? annotationValue
                : default;
    }

    private MethodCallCodeFragment? GenerateIdentityOptions(IDictionary<string, IAnnotation> annotations)
    {
        if (!TryGetAndRemove(annotations, NpgsqlAnnotationNames.IdentityOptions,
                out string? annotationValue))
        {
            return null;
        }

        var identityOptions = IdentitySequenceOptionsData.Deserialize(annotationValue);
        return new(
            PropertyHasIdentityOptionsMethodInfo,
            identityOptions.StartValue,
            identityOptions.IncrementBy == 1 ? null : (long?)identityOptions.IncrementBy,
            identityOptions.MinValue,
            identityOptions.MaxValue,
            identityOptions.IsCyclic ? true : null,
            identityOptions.NumbersToCache == 1 ? null : (long?)identityOptions.NumbersToCache);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MethodCallCodeFragment? GenerateFluentApi(IIndex index, IAnnotation annotation)
        => annotation.Name switch
        {
            RelationalAnnotationNames.Collation
                => new MethodCallCodeFragment(IndexUseCollationMethodInfo, annotation.Value),

            NpgsqlAnnotationNames.IndexMethod
                => new MethodCallCodeFragment(IndexHasMethodMethodInfo, annotation.Value),
            NpgsqlAnnotationNames.IndexOperators
                => new MethodCallCodeFragment(IndexHasOperatorsMethodInfo, annotation.Value),
            NpgsqlAnnotationNames.IndexSortOrder
                => new MethodCallCodeFragment(IndexHasSortOrderMethodInfo, annotation.Value),
            NpgsqlAnnotationNames.IndexNullSortOrder
                => new MethodCallCodeFragment(IndexHasNullSortOrderMethodInfo, annotation.Value),
            NpgsqlAnnotationNames.IndexInclude
                => new MethodCallCodeFragment(IndexIncludePropertiesMethodInfo, annotation.Value),
            _ => null
        };

    private static bool TryGetAndRemove<T>(
        IDictionary<string, IAnnotation> annotations,
        string annotationName,
        [NotNullWhen(true)] out T? annotationValue)
    {
        if (annotations.TryGetValue(annotationName, out var annotation)
            && annotation.Value is not null)
        {
            annotations.Remove(annotationName);
            annotationValue = (T)annotation.Value;
            return true;
        }

        annotationValue = default;
        return false;
    }
}
