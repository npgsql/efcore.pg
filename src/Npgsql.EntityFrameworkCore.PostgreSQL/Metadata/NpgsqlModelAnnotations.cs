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
        public NpgsqlModelAnnotations([NotNull] IModel model)
            : base(model, NpgsqlFullAnnotationNames.Instance)
        {
        }

        public NpgsqlModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, NpgsqlFullAnnotationNames.Instance)
        {
        }

        public virtual PostgresExtension GetOrAddPostgresExtension([CanBeNull] string name, [CanBeNull] string schema = null)
            => PostgresExtension.GetOrAddPostgresExtension((IMutableModel)Model,
                NpgsqlFullAnnotationNames.Instance.PostgresExtensionPrefix,
                name,
                schema);

        public virtual IReadOnlyList<IPostgresExtension> PostgresExtensions
            => PostgresExtension.GetPostgresExtensions(Model, NpgsqlFullAnnotationNames.Instance.PostgresExtensionPrefix).ToList();

        public virtual string DatabaseTemplate
        {
            get { return (string)Annotations.GetAnnotation(NpgsqlFullAnnotationNames.Instance.DatabaseTemplate, null); }
            [param: CanBeNull]
            set { SetDatabaseTemplate(value); }
        }

        protected virtual bool SetDatabaseTemplate([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlFullAnnotationNames.Instance.DatabaseTemplate,
                null,
                Check.NullButNotEmpty(value, nameof(value)));
    }
}
