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
        /// The method to be used, or <c>null</c> if it hasn't been specified. <c>null</c> selects the default (currently <c>btree</c>).
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
        /// The column operators to be used, or <c>null</c> if they have not been specified.
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

        /// <summary>
        /// The column collations to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        public string[] Collation
        {
            get => (string[])Annotations.Metadata[NpgsqlAnnotationNames.IndexCollation];
            set => SetCollation(value);
        }

        IReadOnlyList<string> INpgsqlIndexAnnotations.Collation => Collation;

        /// <summary>
        /// The column sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public SortOrder[] SortOrder
        {
            get => (SortOrder[])Annotations.Metadata[NpgsqlAnnotationNames.IndexSortOrder];
            set => SetSortOrder(value);
        }

        IReadOnlyList<SortOrder> INpgsqlIndexAnnotations.SortOrder => SortOrder;

        /// <summary>
        /// The column NULL sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        public NullSortOrder[] NullSortOrder
        {
            get => (NullSortOrder[])Annotations.Metadata[NpgsqlAnnotationNames.IndexNullSortOrder];
            set => SetNullSortOrder(value);
        }

        IReadOnlyList<NullSortOrder> INpgsqlIndexAnnotations.NullSortOrder => NullSortOrder;

        /// <summary>
        /// The included property names, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
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

        protected virtual bool SetCollation(string[] value)
            => Annotations.SetAnnotation(NpgsqlAnnotationNames.IndexCollation, value);

        protected virtual bool SetSortOrder(SortOrder[] value)
            => Annotations.SetAnnotation(NpgsqlAnnotationNames.IndexSortOrder, value);

        protected virtual bool SetNullSortOrder(NullSortOrder[] value)
            => Annotations.SetAnnotation(NpgsqlAnnotationNames.IndexNullSortOrder, value);

        protected virtual bool SetIncludeProperties(string[] value)
            => Annotations.SetAnnotation(NpgsqlAnnotationNames.IndexInclude, value);
    }
}
