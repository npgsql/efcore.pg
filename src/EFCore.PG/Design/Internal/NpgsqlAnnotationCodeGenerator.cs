using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal
{
    public class NpgsqlAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        #region MethodInfos

        private static readonly MethodInfo _modelHasPostgresExtensionMethodInfo1
            = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension), typeof(ModelBuilder), typeof(string));

        private static readonly MethodInfo _modelHasPostgresExtensionMethodInfo2
            = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension), typeof(ModelBuilder), typeof(string), typeof(string),
                typeof(string));

        private static readonly MethodInfo _modelHasPostgresEnumMethodInfo
            = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlModelBuilderExtensions.HasPostgresEnum), typeof(ModelBuilder), typeof(string), typeof(string[]));

        private static readonly MethodInfo _modelHasPostgresRangeMethodInfo
            = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlModelBuilderExtensions.HasPostgresRange), typeof(ModelBuilder), typeof(string), typeof(string));

        private static readonly MethodInfo _modelUseSerialColumnsMethodInfo
            = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlModelBuilderExtensions.UseSerialColumns), typeof(ModelBuilder));

        private static readonly MethodInfo _modelUseIdentityAlwaysColumnsMethodInfo
            = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlModelBuilderExtensions.UseIdentityAlwaysColumns), typeof(ModelBuilder));

        private static readonly MethodInfo _modelUseIdentityByDefaultColumnsMethodInfo
            = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns), typeof(ModelBuilder));

        private static readonly MethodInfo _modelUseHiLoMethodInfo
            = typeof(NpgsqlModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlModelBuilderExtensions.UseHiLo), typeof(ModelBuilder), typeof(string), typeof(string));

        private static readonly MethodInfo _modelHasAnnotationMethodInfo
            = typeof(ModelBuilder).GetRequiredRuntimeMethod(
                nameof(ModelBuilder.HasAnnotation), typeof(string), typeof(object));

        private static readonly MethodInfo _entityTypeIsUnloggedMethodInfo
            = typeof(NpgsqlEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlEntityTypeBuilderExtensions.IsUnlogged), typeof(EntityTypeBuilder), typeof(bool));

        private static readonly MethodInfo _propertyUseSerialColumnMethodInfo
            = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlPropertyBuilderExtensions.UseSerialColumn), typeof(PropertyBuilder));

        private static readonly MethodInfo _propertyUseIdentityAlwaysColumnMethodInfo
            = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn), typeof(PropertyBuilder));

        private static readonly MethodInfo _propertyUseIdentityByDefaultColumnMethodInfo
            = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn), typeof(PropertyBuilder));

        private static readonly MethodInfo _propertyUseHiLoMethodInfo
            = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlPropertyBuilderExtensions.UseHiLo), typeof(PropertyBuilder), typeof(string), typeof(string));

        private static readonly MethodInfo _propertyHasIdentityOptionsMethodInfo
            = typeof(NpgsqlPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlPropertyBuilderExtensions.HasIdentityOptions), typeof(PropertyBuilder), typeof(long?), typeof(long?),
                typeof(long?), typeof(long?), typeof(bool?), typeof(long?));

        private static readonly MethodInfo _indexUseCollationMethodInfo
            = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlIndexBuilderExtensions.UseCollation), typeof(IndexBuilder), typeof(string[]));

        private static readonly MethodInfo _indexHasMethodMethodInfo
            = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlIndexBuilderExtensions.HasMethod), typeof(IndexBuilder), typeof(string));

        private static readonly MethodInfo _indexHasOperatorsMethodInfo
            = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlIndexBuilderExtensions.HasOperators), typeof(IndexBuilder), typeof(string[]));

        private static readonly MethodInfo _indexHasSortOrderMethodInfo
            = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlIndexBuilderExtensions.HasSortOrder), typeof(IndexBuilder), typeof(SortOrder[]));

        private static readonly MethodInfo _indexHasNullSortOrderMethodInfo
            = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlIndexBuilderExtensions.HasNullSortOrder), typeof(IndexBuilder), typeof(NullSortOrder[]));

        private static readonly MethodInfo _indexIncludePropertiesMethodInfo
            = typeof(NpgsqlIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NpgsqlIndexBuilderExtensions.IncludeProperties), typeof(IndexBuilder), typeof(string[]));

        #endregion MethodInfos

        public NpgsqlAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies) {}

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

        public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IModel model,
            IDictionary<string, IAnnotation> annotations)
            => base.GenerateFluentApiCalls(model, annotations)
                .Concat(GenerateValueGenerationStrategy(annotations, onModel: true))
                .ToList();

        protected override MethodCallCodeFragment? GenerateFluentApi(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal))
            {
                var extension = new PostgresExtension(model, annotation.Name);

                return extension.Schema is "public" or null
                    ? new MethodCallCodeFragment(_modelHasPostgresExtensionMethodInfo1, extension.Name)
                    : new MethodCallCodeFragment(_modelHasPostgresExtensionMethodInfo2, extension.Schema, extension.Name);
            }

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal))
            {
                var enumTypeDef = new PostgresEnum(model, annotation.Name);

                return enumTypeDef.Schema == "public"
                    ? new MethodCallCodeFragment(_modelHasPostgresEnumMethodInfo, enumTypeDef.Name, enumTypeDef.Labels)
                    : new MethodCallCodeFragment(_modelHasPostgresEnumMethodInfo, enumTypeDef.Schema, enumTypeDef.Name, enumTypeDef.Labels);
            }

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal))
            {
                var rangeTypeDef = new PostgresRange(model, annotation.Name);

                if (rangeTypeDef.CanonicalFunction == null &&
                    rangeTypeDef.SubtypeOpClass == null &&
                    rangeTypeDef.Collation == null &&
                    rangeTypeDef.SubtypeDiff == null)
                {
                    return new MethodCallCodeFragment(_modelHasPostgresRangeMethodInfo,
                        rangeTypeDef.Schema == "public" ? null : rangeTypeDef.Schema,
                        rangeTypeDef.Name,
                        rangeTypeDef.Subtype);
                }

                return new MethodCallCodeFragment(_modelHasPostgresRangeMethodInfo,
                    rangeTypeDef.Schema == "public" ? null : rangeTypeDef.Schema,
                    rangeTypeDef.Name,
                    rangeTypeDef.Subtype,
                    rangeTypeDef.CanonicalFunction,
                    rangeTypeDef.SubtypeOpClass,
                    rangeTypeDef.Collation,
                    rangeTypeDef.SubtypeDiff);
            }

            return null;
        }

        protected override MethodCallCodeFragment? GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == NpgsqlAnnotationNames.UnloggedTable)
            {
                return new MethodCallCodeFragment(_entityTypeIsUnloggedMethodInfo, annotation.Value);
            }

            return null;
        }

        public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IProperty property,
            IDictionary<string, IAnnotation> annotations)
            => base.GenerateFluentApiCalls(property, annotations)
                .Concat(GenerateValueGenerationStrategy(annotations, onModel: false))
                .Concat(GenerateIdentityOptions(annotations))
                .ToList();

        private IReadOnlyList<MethodCallCodeFragment> GenerateValueGenerationStrategy(
            IDictionary<string, IAnnotation> annotations,
            bool onModel)
        {
            if (!TryGetAndRemove(annotations, NpgsqlAnnotationNames.ValueGenerationStrategy,
                out NpgsqlValueGenerationStrategy strategy))
            {
                return Array.Empty<MethodCallCodeFragment>();
            }

            switch (strategy)
            {
            case NpgsqlValueGenerationStrategy.SerialColumn:
                return new List<MethodCallCodeFragment>
                {
                    new(onModel ? _modelUseSerialColumnsMethodInfo : _propertyUseSerialColumnMethodInfo)
                };

            case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                return new List<MethodCallCodeFragment>
                {
                    new(onModel ? _modelUseIdentityAlwaysColumnsMethodInfo : _propertyUseIdentityAlwaysColumnMethodInfo)
                };

            case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                return new List<MethodCallCodeFragment>
                {
                    new(onModel ? _modelUseIdentityByDefaultColumnsMethodInfo : _propertyUseIdentityByDefaultColumnMethodInfo)
                };

            case NpgsqlValueGenerationStrategy.SequenceHiLo:
                var name = GetAndRemove<string>(NpgsqlAnnotationNames.HiLoSequenceName)!;
                var schema = GetAndRemove<string>(NpgsqlAnnotationNames.HiLoSequenceSchema);
                return new List<MethodCallCodeFragment>
                {
                    new(
                        onModel ? _modelUseHiLoMethodInfo : _propertyUseHiLoMethodInfo,
                        (name, schema) switch
                        {
                            (null, null) => Array.Empty<object>(),
                            (_, null) => new object[] { name },
                            _ => new object?[] { name!, schema }
                        })
                };

            case NpgsqlValueGenerationStrategy.None:
                return new List<MethodCallCodeFragment>
                {
                    new(_modelHasAnnotationMethodInfo, NpgsqlAnnotationNames.ValueGenerationStrategy, NpgsqlValueGenerationStrategy.None)
                };

            default:
                throw new ArgumentOutOfRangeException(strategy.ToString());
            }

            T? GetAndRemove<T>(string annotationName)
                => TryGetAndRemove(annotations, annotationName, out T? annotationValue)
                    ? annotationValue
                    : default;
        }

        private IReadOnlyList<MethodCallCodeFragment> GenerateIdentityOptions(IDictionary<string, IAnnotation> annotations)
        {
            if (!TryGetAndRemove(annotations, NpgsqlAnnotationNames.IdentityOptions,
                out string? annotationValue))
            {
                return Array.Empty<MethodCallCodeFragment>();
            }

            var identityOptions = IdentitySequenceOptionsData.Deserialize(annotationValue);
            return new List<MethodCallCodeFragment>
            {
                new(
                    _propertyHasIdentityOptionsMethodInfo,
                    identityOptions.StartValue,
                    identityOptions.IncrementBy == 1 ? null : (long?) identityOptions.IncrementBy,
                    identityOptions.MinValue,
                    identityOptions.MaxValue,
                    identityOptions.IsCyclic ? true : (bool?) null,
                    identityOptions.NumbersToCache == 1 ? null : (long?) identityOptions.NumbersToCache)
            };
        }

        protected override MethodCallCodeFragment? GenerateFluentApi(IIndex index, IAnnotation annotation)
            => annotation.Name switch
            {
                RelationalAnnotationNames.Collation
                    => new MethodCallCodeFragment(_indexUseCollationMethodInfo, annotation.Value),

                NpgsqlAnnotationNames.IndexMethod
                    => new MethodCallCodeFragment(_indexHasMethodMethodInfo, annotation.Value),
                NpgsqlAnnotationNames.IndexOperators
                    => new MethodCallCodeFragment(_indexHasOperatorsMethodInfo, annotation.Value),
                NpgsqlAnnotationNames.IndexSortOrder
                    => new MethodCallCodeFragment(_indexHasSortOrderMethodInfo, annotation.Value),
                NpgsqlAnnotationNames.IndexNullSortOrder
                    => new MethodCallCodeFragment(_indexHasNullSortOrderMethodInfo, annotation.Value),
                NpgsqlAnnotationNames.IndexInclude
                    => new MethodCallCodeFragment(_indexIncludePropertiesMethodInfo, annotation.Value),
                _ => null
            };

        private static bool TryGetAndRemove<T>(
            IDictionary<string, IAnnotation> annotations,
            string annotationName,
            [NotNullWhen(true)] out T? annotationValue)
        {
            if (annotations.TryGetValue(annotationName, out var annotation)
                && annotation.Value != null)
            {
                annotations.Remove(annotationName);
                annotationValue = (T)annotation.Value;
                return true;
            }

            annotationValue = default;
            return false;
        }
    }
}
