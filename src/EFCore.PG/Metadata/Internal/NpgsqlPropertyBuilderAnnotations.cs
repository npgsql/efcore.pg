// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NpgsqlPropertyBuilderAnnotations : NpgsqlPropertyAnnotations
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NpgsqlPropertyBuilderAnnotations(
            [NotNull] InternalPropertyBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

        private InternalPropertyBuilder PropertyBuilder => ((Property)Property).Builder;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected new virtual RelationalAnnotationsBuilder Annotations => (RelationalAnnotationsBuilder)base.Annotations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool ShouldThrowOnConflict => false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool ShouldThrowOnInvalidConfiguration => Annotations.ConfigurationSource == ConfigurationSource.Explicit;

#pragma warning disable 109
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual bool ColumnName([CanBeNull] string value) => SetColumnName(value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual bool ColumnType([CanBeNull] string value) => SetColumnType(value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual bool DefaultValueSql([CanBeNull] string value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
            return SetDefaultValueSql(value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual bool ComputedColumnSql([CanBeNull] string value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.Convention);
            return SetComputedColumnSql(value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual bool DefaultValue([CanBeNull] object value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
            return SetDefaultValue(value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual bool HiLoSequenceName([CanBeNull] string value) => SetHiLoSequenceName(value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual bool HiLoSequenceSchema([CanBeNull] string value) => SetHiLoSequenceSchema(value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual bool ValueGenerationStrategy(NpgsqlValueGenerationStrategy? value)
        {
            if (!SetValueGenerationStrategy(value))
            {
                return false;
            }

            switch (value)
            {
            case NpgsqlValueGenerationStrategy.SerialColumn:
            case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
            case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                PropertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
                HiLoSequenceName(null);
                HiLoSequenceSchema(null);
                break;
            case NpgsqlValueGenerationStrategy.SequenceHiLo:
                PropertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
                break;
            case null:
                PropertyBuilder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
                HiLoSequenceName(null);
                HiLoSequenceSchema(null);
                break;
            default:
                throw new ArgumentException("Unknown NpgsqlValueGenerationStrategy value: " + value);
            }

            return true;
        }
#pragma warning restore 109
    }
}
