using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class NpgsqlModelAnnotations : RelationalModelAnnotations, INpgsqlModelAnnotations
    {
        public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";

        public NpgsqlModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        public NpgsqlModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        #region HiLo

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

        #endregion

        #region Value Generation Strategy

        public virtual NpgsqlValueGenerationStrategy? ValueGenerationStrategy
        {
            get => (NpgsqlValueGenerationStrategy?)Annotations.Metadata[NpgsqlAnnotationNames.ValueGenerationStrategy];
            set => SetValueGenerationStrategy(value);
        }

        protected virtual bool SetValueGenerationStrategy(NpgsqlValueGenerationStrategy? value)
            => Annotations.SetAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value);

        #endregion

        #region PostgreSQL Extensions

        public virtual PostgresExtension GetOrAddPostgresExtension([NotNull] string name)
            => PostgresExtension.GetOrAddPostgresExtension((IMutableModel)Model, name);

        public virtual IReadOnlyList<IPostgresExtension> PostgresExtensions
            => PostgresExtension.GetPostgresExtensions(Model).ToArray();

        #endregion

        #region Enum types

        public virtual PostgresEnum GetOrAddPostgresEnum(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string[] labels)
            => PostgresEnum.GetOrAddPostgresEnum((IMutableModel)Model, schema, name, labels);

        public virtual PostgresEnum GetOrAddPostgresEnum(
            [NotNull] string name,
            [NotNull] string[] labels)
            => GetOrAddPostgresEnum(null, name, labels);

        public virtual IReadOnlyList<PostgresEnum> PostgresEnums
            => PostgresEnum.GetPostgresEnums(Model).ToArray();

        #endregion Enum types

        #region Range types

        public virtual PostgresRange GetOrAddPostgresRange(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string subtype,
            string canonicalFunction = null,
            string subtypeOpClass = null,
            string collation = null,
            string subtypeDiff = null)
            => PostgresRange.GetOrAddPostgresRange(
                (IMutableModel)Model,
                schema,
                name,
                subtype,
                canonicalFunction,
                subtypeOpClass,
                collation,
                subtypeDiff);

        public virtual PostgresRange GetOrAddPostgresRange(
            [NotNull] string name,
            [NotNull] string subtype)
            => PostgresRange.GetOrAddPostgresRange((IMutableModel)Model, null, name, subtype);

        public virtual IReadOnlyList<PostgresRange> PostgresRanges
            => PostgresRange.GetPostgresRanges(Model).ToArray();

        #endregion Range types

        #region Database Template

        public virtual string DatabaseTemplate
        {
            get => (string)Annotations.Metadata[NpgsqlAnnotationNames.DatabaseTemplate];
            [param: CanBeNull]
            set => SetDatabaseTemplate(value);
        }

        protected virtual bool SetDatabaseTemplate([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.DatabaseTemplate,
                Check.NullButNotEmpty(value, nameof(value)));

        #endregion

        #region Tablespace

        public virtual string Tablespace
        {
            get => (string)Annotations.Metadata[NpgsqlAnnotationNames.Tablespace];
            [param: CanBeNull]
            set => SetTablespace(value);
        }

        protected virtual bool SetTablespace([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.Tablespace,
                Check.NullButNotEmpty(value, nameof(value)));

        #endregion
    }
}
