using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class NpgsqlEntityTypeAnnotations : RelationalEntityTypeAnnotations, INpgsqlEntityTypeAnnotations
    {
        public NpgsqlEntityTypeAnnotations([NotNull] IEntityType entityType)
            : base(entityType, NpgsqlFullAnnotationNames.Instance)
        {
        }

        public NpgsqlEntityTypeAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, NpgsqlFullAnnotationNames.Instance)
        {
        }

        public bool SetStorageParameter(string parameterName, object parameterValue)
            => Annotations.SetAnnotation(NpgsqlFullAnnotationNames.Instance.StorageParameterPrefix + parameterName, null, parameterValue);

        public Dictionary<string, object> GetStorageParameters()
            => Annotations.Metadata.GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlFullAnnotationNames.Instance.StorageParameterPrefix))
                .ToDictionary(
                    a => a.Name.Substring(NpgsqlFullAnnotationNames.Instance.StorageParameterPrefix.Length),
                    a => a.Value
                );

        public virtual string Comment
        {
            get { return (string)Annotations.GetAnnotation(NpgsqlFullAnnotationNames.Instance.Comment, null); }
            [param: CanBeNull] set { SetComment(value); }
        }

        protected virtual bool SetComment([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlFullAnnotationNames.Instance.Comment,
                null,
                Check.NullButNotEmpty(value, nameof(value)));
    }
}
