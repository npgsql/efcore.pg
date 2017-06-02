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
            get { return (string)Annotations.GetAnnotation(NpgsqlAnnotationNames.Comment); }
            [param: CanBeNull] set { SetComment(value); }
        }

        protected virtual bool SetComment([CanBeNull] string value)
            => Annotations.SetAnnotation(
                NpgsqlAnnotationNames.Comment,
                Check.NullButNotEmpty(value, nameof(value)));
    }
}
