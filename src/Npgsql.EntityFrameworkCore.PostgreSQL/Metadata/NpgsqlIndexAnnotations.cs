using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class NpgsqlIndexAnnotations : RelationalIndexAnnotations, INpgsqlIndexAnnotations
    {
        public NpgsqlIndexAnnotations([NotNull] IIndex index)
            : base(index, NpgsqlFullAnnotationNames.Instance)
        {
        }

        protected NpgsqlIndexAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, NpgsqlFullAnnotationNames.Instance)
        {
        }

        /// <summary>
        /// The PostgreSQL index method to be used. Null selects the default (currently btree).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        public string Method
        {
            get { return (string) Annotations.GetAnnotation(NpgsqlFullAnnotationNames.Instance.IndexMethod, null); }
            set { Annotations.SetAnnotation(NpgsqlFullAnnotationNames.Instance.IndexMethod, null, value); }
        }
    }
}
