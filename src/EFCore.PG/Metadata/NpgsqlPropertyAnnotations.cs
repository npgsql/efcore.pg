// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class NpgsqlPropertyAnnotations : RelationalPropertyAnnotations, INpgsqlPropertyAnnotations
    {
        public NpgsqlPropertyAnnotations([NotNull] IProperty property)
            : base(property)
        {
        }

        protected NpgsqlPropertyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual string HiLoSequenceName
        {
            get => (string)Annotations.Metadata[NpgsqlAnnotationNames.HiLoSequenceName];
            [param: CanBeNull]
            set => SetHiLoSequenceName(value);
        }

        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string HiLoSequenceSchema
        {
            get => (string)Annotations.Metadata[NpgsqlAnnotationNames.HiLoSequenceSchema];
            [param: CanBeNull]
            set => SetHiLoSequenceSchema(value);
        }

        protected virtual bool SetHiLoSequenceSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual ISequence FindHiLoSequence()
        {
            var modelExtensions = Property.DeclaringEntityType.Model.Npgsql();

            if (ValueGenerationStrategy != NpgsqlValueGenerationStrategy.SequenceHiLo)
            {
                return null;
            }

            var sequenceName = HiLoSequenceName
                               ?? modelExtensions.HiLoSequenceName
                               ?? NpgsqlModelAnnotations.DefaultHiLoSequenceName;

            var sequenceSchema = HiLoSequenceSchema
                                 ?? modelExtensions.HiLoSequenceSchema;

            return modelExtensions.FindSequence(sequenceName, sequenceSchema);
        }

        public virtual NpgsqlValueGenerationStrategy? ValueGenerationStrategy
        {
            get { return GetNpgsqlValueGenerationStrategy(fallbackToModel: true); }
            [param: CanBeNull]
            set { SetValueGenerationStrategy(value); }
        }

        private NpgsqlValueGenerationStrategy? GetNpgsqlValueGenerationStrategy(bool fallbackToModel)
        {
            if (GetDefaultValue(false) != null
                || GetDefaultValueSql(false) != null
                || GetComputedColumnSql(false) != null)
            {
                return null;
            }

            var value = (NpgsqlValueGenerationStrategy?)Annotations.Metadata[NpgsqlAnnotationNames.ValueGenerationStrategy];
            if (value != null)
            {
                return value;
            }

            var relationalProperty = Property.Relational();
            if (!fallbackToModel
                || Property.ValueGenerated != ValueGenerated.OnAdd
                || relationalProperty.DefaultValue != null
                || relationalProperty.DefaultValueSql != null
                || relationalProperty.ComputedColumnSql != null)
            {
                return null;
            }

            var modelStrategy = Property.DeclaringEntityType.Model.Npgsql().ValueGenerationStrategy;

            if (modelStrategy == NpgsqlValueGenerationStrategy.SequenceHiLo && Property.ClrType.IsInteger())
                return NpgsqlValueGenerationStrategy.SequenceHiLo;

            if (modelStrategy == NpgsqlValueGenerationStrategy.SerialColumn && Property.ClrType.IsIntegerForIdentityOrSerial())
                return NpgsqlValueGenerationStrategy.SerialColumn;

            if (modelStrategy == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn && Property.ClrType.IsIntegerForIdentityOrSerial())
                return NpgsqlValueGenerationStrategy.IdentityAlwaysColumn;

            if (modelStrategy == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn && Property.ClrType.IsIntegerForIdentityOrSerial())
                return NpgsqlValueGenerationStrategy.IdentityByDefaultColumn;

            return null;
        }

        protected virtual bool SetValueGenerationStrategy(NpgsqlValueGenerationStrategy? value)
        {
            if (value != null)
            {
                var propertyType = Property.ClrType;

                if (value == NpgsqlValueGenerationStrategy.SerialColumn && !propertyType.IsIntegerForIdentityOrSerial())
                {
                    if (ShouldThrowOnInvalidConfiguration)
                        throw new ArgumentException($"Serial value generation cannot be used for the property '{Property.Name}' on entity type '{Property.DeclaringEntityType.DisplayName()}' because the property type is '{propertyType.ShortDisplayName()}'. Serial columns can only be of type short, int or long.");
                    return false;
                }

                if ((value == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn || value == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    && !propertyType.IsIntegerForIdentityOrSerial())
                {
                    if (ShouldThrowOnInvalidConfiguration)
                        throw new ArgumentException($"Identity value generation cannot be used for the property '{Property.Name}' on entity type '{Property.DeclaringEntityType.DisplayName()}' because the property type is '{propertyType.ShortDisplayName()}'. Identity columns can only be of type short, int or long.");
                    return false;
                }

                if (value == NpgsqlValueGenerationStrategy.SequenceHiLo && !propertyType.IsInteger())
                {
                    if (ShouldThrowOnInvalidConfiguration)
                        throw new ArgumentException($"PostgreSQL sequences cannot be used to generate values for the property '{Property.Name}' on entity type '{Property.DeclaringEntityType.DisplayName()}' because the property type is '{propertyType.ShortDisplayName()}'. Sequences can only be used with integer properties.");
                    return false;
                }
            }

            if (!CanSetValueGenerationStrategy(value))
            {
                return false;
            }

            if (!ShouldThrowOnConflict
                && ValueGenerationStrategy != value
                && value != null)
            {
                ClearAllServerGeneratedValues();
            }

            return Annotations.SetAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value);
        }

        protected virtual bool CanSetValueGenerationStrategy(NpgsqlValueGenerationStrategy? value)
        {
            if (GetNpgsqlValueGenerationStrategy(fallbackToModel: false) == value)
            {
                return true;
            }

            if (!Annotations.CanSetAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value))
            {
                return false;
            }

            if (ShouldThrowOnConflict)
            {
                if (GetDefaultValue(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(DefaultValue)));
                }
                if (GetDefaultValueSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(DefaultValueSql)));
                }
                if (GetComputedColumnSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(ComputedColumnSql)));
                }
            }
            else if (value != null
                     && (!CanSetDefaultValue(null)
                         || !CanSetDefaultValueSql(null)
                         || !CanSetComputedColumnSql(null)))
            {
                return false;
            }

            return true;
        }

        protected override object GetDefaultValue(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetDefaultValue(fallback);
        }

        protected override bool CanSetDefaultValue(object value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValue), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetDefaultValue(value);
        }

        protected override string GetDefaultValueSql(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetDefaultValueSql(fallback);
        }

        protected override bool CanSetDefaultValueSql(string value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValueSql), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetDefaultValueSql(value);
        }

        protected override string GetComputedColumnSql(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetComputedColumnSql(fallback);
        }

        protected override bool CanSetComputedColumnSql(string value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ComputedColumnSql), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetComputedColumnSql(value);
        }

        protected override void ClearAllServerGeneratedValues()
        {
            SetValueGenerationStrategy(null);

            base.ClearAllServerGeneratedValues();
        }

        public virtual string Comment
        {
            get => (string)Annotations.Metadata[NpgsqlAnnotationNames.Comment];
            [param: CanBeNull]
            set => SetComment(value);
        }

        protected virtual bool SetComment([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.Comment,
                Check.NullButNotEmpty(value, nameof(value)));
    }
}
