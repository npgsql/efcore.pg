using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class NpgsqlIndexAnnotations : RelationalIndexAnnotations, INpgsqlIndexAnnotations
    {
        public NpgsqlIndexAnnotations([NotNull] IIndex index)
            : base(index)
        {
        }

        protected NpgsqlIndexAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
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
            get => (string) Annotations.Metadata[NpgsqlAnnotationNames.IndexMethod];
            set => SetMethod(value);
        }

        /// <summary>
        /// The PostgreSQL index operators to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        public string[] Operators
        {
            get => (string[]) Annotations.Metadata[NpgsqlAnnotationNames.IndexOperators];
            set => SetOperators(value);
        }

        IReadOnlyList<string> INpgsqlIndexAnnotations.Operators => Operators;

        public string[] IncludeProperties
        {
            get => (string[]) Annotations.Metadata[NpgsqlAnnotationNames.IndexInclude];
            set => SetIncludeProperties(value);
        }

        IReadOnlyList<string> INpgsqlIndexAnnotations.IncludeProperties => IncludeProperties;

        protected virtual bool SetMethod(string value)
            => Annotations.SetAnnotation(NpgsqlAnnotationNames.IndexMethod, value);

        protected virtual bool SetOperators(string[] value)
            => Annotations.SetAnnotation(NpgsqlAnnotationNames.IndexOperators, value);

        protected virtual bool SetIncludeProperties(string[] value)
            => Annotations.SetAnnotation(NpgsqlAnnotationNames.IndexInclude, value);
    }
}
