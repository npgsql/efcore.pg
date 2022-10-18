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

    // ReSharper disable InconsistentNaming
    private static readonly MethodInfo Model_HasPostgresExtension
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension), typeof(ModelBuilder), typeof(string), typeof(string), typeof(string));

    private static readonly MethodInfo Model_HasPostgresEnum1
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.HasPostgresEnum), typeof(ModelBuilder), typeof(string), typeof(string[]));

    private static readonly MethodInfo Model_HasPostgresEnum2
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.HasPostgresEnum), typeof(ModelBuilder), typeof(string), typeof(string), typeof(string[]));

    private static readonly MethodInfo Model_HasPostgresRange1
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.HasPostgresRange), typeof(ModelBuilder), typeof(string), typeof(string));

    private static readonly MethodInfo Model_HasPostgresRange2
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.HasPostgresRange), typeof(ModelBuilder), typeof(string), typeof(string),typeof(string), typeof(string),typeof(string), typeof(string),typeof(string));

    private static readonly MethodInfo Model_UseSerialColumns
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.UseSerialColumns), typeof(ModelBuilder));

    private static readonly MethodInfo Model_UseIdentityAlwaysColumns
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.UseIdentityAlwaysColumns), typeof(ModelBuilder));

    private static readonly MethodInfo Model_UseIdentityByDefaultColumns
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns), typeof(ModelBuilder));

    private static readonly MethodInfo Model_UseHiLo
        = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.UseHiLo), typeof(ModelBuilder), typeof(string), typeof(string));

    private static readonly MethodInfo Model_HasAnnotation
        = typeof(ModelBuilder).GetRequiredRuntimeMethod(
            nameof(ModelBuilder.HasAnnotation), typeof(string), typeof(object));

    private static readonly MethodInfo Model_UseKeySequences
        = typeof(NpgsqlModelBuilderExtensions).GetRuntimeMethod(
            nameof(NpgsqlModelBuilderExtensions.UseKeySequences), new[] { typeof(ModelBuilder), typeof(string), typeof(string) })!;

    private static readonly MethodInfo EntityType_IsUnlogged
        = typeof(NpgsqlEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlEntityTypeBuilderExtensions.IsUnlogged), typeof(EntityTypeBuilder), typeof(bool));

    private static readonly MethodInfo Property_UseSerialColumn
        = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.UseSerialColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo Property_UseIdentityAlwaysColumn
        = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo Property_UseIdentityByDefaultColumn
        = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn), typeof(PropertyBuilder));

    private static readonly MethodInfo Property_UseHiLo
        = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.UseHiLo), typeof(PropertyBuilder), typeof(string), typeof(string));

    private static readonly MethodInfo Property_HasIdentityOptions
        = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.HasIdentityOptions), typeof(PropertyBuilder), typeof(long?), typeof(long?),
            typeof(long?), typeof(long?), typeof(bool?), typeof(long?));

    private static readonly MethodInfo Property_UseSequence
        = typeof(NpgsqlPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(NpgsqlPropertyBuilderExtensions.UseSequence), new[] { typeof(PropertyBuilder), typeof(string), typeof(string) })!;

    private static readonly MethodInfo Index_UseCollation
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.UseCollation), typeof(IndexBuilder), typeof(string[]));

    private static readonly MethodInfo Index_HasMethod
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.HasMethod), typeof(IndexBuilder), typeof(string));

    private static readonly MethodInfo Index_HasOperators
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.HasOperators), typeof(IndexBuilder), typeof(string[]));

    private static readonly MethodInfo Index_HasSortOrder
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.HasSortOrder), typeof(IndexBuilder), typeof(SortOrder[]));

    private static readonly MethodInfo Index_HasNullSortOrder
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.HasNullSortOrder), typeof(IndexBuilder), typeof(NullSortOrder[]));

    private static readonly MethodInfo Index_IncludeProperties
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.IncludeProperties), typeof(IndexBuilder), typeof(string[]));

    private static readonly MethodInfo Index_AreNullsDistinct
        = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(NpgsqlIndexBuilderExtensions.AreNullsDistinct), typeof(IndexBuilder), typeof(bool));
    // ReSharper restore InconsistentNaming

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

        if (TryGetAndRemove(annotations, NpgsqlAnnotationNames.PostgresExtensions, out SortedDictionary<(string, string?), IPostgresExtension>? postgresExtensions))
        {
            foreach (var postgresExtension in postgresExtensions.Values)
            {
                var schema = postgresExtension.Schema is "public" ? null : postgresExtension.Schema;

                fragments.Add(postgresExtension.Version is not null
                    ? new MethodCallCodeFragment(Model_HasPostgresExtension, schema, postgresExtension.Name, postgresExtension.Version)
                    : schema is not null
                        ? new MethodCallCodeFragment(Model_HasPostgresExtension, schema, postgresExtension.Name)
                        : new MethodCallCodeFragment(Model_HasPostgresExtension, postgresExtension.Name));
            }
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

        if (annotation.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal))
        {
            var enumTypeDef = new PostgresEnum(model, annotation.Name);

            return enumTypeDef.Schema is null
                ? new MethodCallCodeFragment(Model_HasPostgresEnum1, enumTypeDef.Name, enumTypeDef.Labels)
                : new MethodCallCodeFragment(Model_HasPostgresEnum2, enumTypeDef.Schema, enumTypeDef.Name, enumTypeDef.Labels);
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
                return new MethodCallCodeFragment(Model_HasPostgresRange1, rangeTypeDef.Name, rangeTypeDef.Subtype);
            }

            return new MethodCallCodeFragment(Model_HasPostgresRange2,
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
            return new MethodCallCodeFragment(EntityType_IsUnlogged, annotation.Value);
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
                return new(onModel ? Model_UseSerialColumns : Property_UseSerialColumn);

            case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                return new(onModel ? Model_UseIdentityAlwaysColumns : Property_UseIdentityAlwaysColumn);

            case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                return new(onModel ? Model_UseIdentityByDefaultColumns : Property_UseIdentityByDefaultColumn);

            case NpgsqlValueGenerationStrategy.SequenceHiLo:
            {
                var name = GetAndRemove<string>(NpgsqlAnnotationNames.HiLoSequenceName)!;
                var schema = GetAndRemove<string>(NpgsqlAnnotationNames.HiLoSequenceSchema);
                return new(
                    onModel ? Model_UseHiLo : Property_UseHiLo,
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
                    onModel ? Model_UseKeySequences : Property_UseSequence,
                    (name: nameOrSuffix, schema) switch
                    {
                        (null, null) => Array.Empty<object>(),
                        (_, null) => new object[] { nameOrSuffix },
                        _ => new object[] { nameOrSuffix!, schema }
                    });
            }
            case NpgsqlValueGenerationStrategy.None:
                return new(Model_HasAnnotation, NpgsqlAnnotationNames.ValueGenerationStrategy, NpgsqlValueGenerationStrategy.None);

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
            Property_HasIdentityOptions,
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
                => new MethodCallCodeFragment(Index_UseCollation, annotation.Value),

            NpgsqlAnnotationNames.IndexMethod
                => new MethodCallCodeFragment(Index_HasMethod, annotation.Value),
            NpgsqlAnnotationNames.IndexOperators
                => new MethodCallCodeFragment(Index_HasOperators, annotation.Value),
            NpgsqlAnnotationNames.IndexSortOrder
                => new MethodCallCodeFragment(Index_HasSortOrder, annotation.Value),
            NpgsqlAnnotationNames.IndexNullSortOrder
                => new MethodCallCodeFragment(Index_HasNullSortOrder, annotation.Value),
            NpgsqlAnnotationNames.IndexInclude
                => new MethodCallCodeFragment(Index_IncludeProperties, annotation.Value),
            NpgsqlAnnotationNames.NullsDistinct
                => new MethodCallCodeFragment(Index_AreNullsDistinct, annotation.Value),
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
