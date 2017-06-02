using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
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
            get { return (string)Annotations.GetAnnotation(NpgsqlAnnotationNames.HiLoSequenceName); }
            [param: CanBeNull]
            set { SetHiLoSequenceName(value); }
        }

        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string HiLoSequenceSchema
        {
            get { return (string)Annotations.GetAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema); }
            [param: CanBeNull]
            set { SetHiLoSequenceSchema(value); }
        }

        protected virtual bool SetHiLoSequenceSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(value, nameof(value)));

        #endregion

        #region Value Generation Strategy

        public virtual NpgsqlValueGenerationStrategy? ValueGenerationStrategy
        {
            get
            {
                return (NpgsqlValueGenerationStrategy?)Annotations.GetAnnotation(
                    NpgsqlAnnotationNames.ValueGenerationStrategy);
            }
            set { SetValueGenerationStrategy(value); }
        }

        protected virtual bool SetValueGenerationStrategy(NpgsqlValueGenerationStrategy? value)
            => Annotations.SetAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value);

        #endregion

        #region PostgreSQL Extensions

        public virtual PostgresExtension GetOrAddPostgresExtension([NotNull] string name)
            => PostgresExtension.GetOrAddPostgresExtension((IMutableModel)Model, name);

        public virtual IReadOnlyList<IPostgresExtension> PostgresExtensions
            => PostgresExtension.GetPostgresExtensions(Model).ToList();

        #endregion

        #region Database Template

        public virtual string DatabaseTemplate
        {
            get { return (string)Annotations.GetAnnotation(NpgsqlAnnotationNames.DatabaseTemplate); }
            [param: CanBeNull]
            set { SetDatabaseTemplate(value); }
        }

        protected virtual bool SetDatabaseTemplate([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.DatabaseTemplate,
                Check.NullButNotEmpty(value, nameof(value)));

        #endregion
    }
}
