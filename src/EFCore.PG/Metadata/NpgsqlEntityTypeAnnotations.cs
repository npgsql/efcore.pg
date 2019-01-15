using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class NpgsqlEntityTypeAnnotations : RelationalEntityTypeAnnotations, INpgsqlEntityTypeAnnotations
    {
        public NpgsqlEntityTypeAnnotations([NotNull] IEntityType entityType)
            : base(entityType)
        {
        }

        public NpgsqlEntityTypeAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public bool SetStorageParameter(string parameterName, object parameterValue)
            => Annotations.SetAnnotation(NpgsqlAnnotationNames.StorageParameterPrefix + parameterName, parameterValue);

        public Dictionary<string, object> GetStorageParameters()
            => Annotations.Metadata.GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix))
                .ToDictionary(
                    a => a.Name.Substring(NpgsqlAnnotationNames.StorageParameterPrefix.Length),
                    a => a.Value
                );

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

        public virtual bool IsUnlogged
        {
            get => Annotations.Metadata[NpgsqlAnnotationNames.UnloggedTable] is bool unlogged && unlogged;
            set => SetIsUnlogged(value);
        }

        protected virtual bool SetIsUnlogged(bool value)
            => value
                ? Annotations.SetAnnotation(NpgsqlAnnotationNames.UnloggedTable, true)
                : Annotations.RemoveAnnotation(NpgsqlAnnotationNames.UnloggedTable);

        public virtual CockroachDbInterleaveInParent CockroachDbInterleaveInParent
            => new CockroachDbInterleaveInParent(EntityType);
    }
}
