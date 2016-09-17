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

        #region PostgreSQL Extensions

        public virtual PostgresExtension GetOrAddPostgresExtension([NotNull] string name)
            => PostgresExtension.GetOrAddPostgresExtension((IMutableModel)Model, name);

        public virtual IReadOnlyList<IPostgresExtension> PostgresExtensions
            => PostgresExtension.GetPostgresExtensions(Model).ToList();

        #endregion

        #region Database Template

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

        #endregion
    }
}
