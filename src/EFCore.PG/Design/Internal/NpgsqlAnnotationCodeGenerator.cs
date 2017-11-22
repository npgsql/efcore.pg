using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class NpgsqlAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        public NpgsqlAnnotationCodeGenerator([NotNull] AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == RelationalAnnotationNames.DefaultSchema
                && string.Equals("public", (string)annotation.Value))
            {
                return true;
            }

            return false;
        }

        public override bool IsHandledByConvention(IIndex index, IAnnotation annotation)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == NpgsqlAnnotationNames.IndexMethod
                && string.Equals("btree", (string)annotation.Value))
            {
                return true;
            }

            return false;
        }

        public override string GenerateFluentApi(IModel model, IAnnotation annotation, string language)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            if (language != "CSharp")
                return null;

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix))
            {
                var extension = new PostgresExtension(model, annotation.Name);
                return $".{nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension)}(\"{extension.Name}\")";
            }
            return null;
        }

        public override string GenerateFluentApi(IEntityType entityType, IAnnotation annotation, string language)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            if (language != "CSharp")
                return null;

            return annotation.Name == NpgsqlAnnotationNames.Comment
                ? $".{nameof(NpgsqlEntityTypeBuilderExtensions.ForNpgsqlHasComment)}(\"{annotation.Value}\")"
                : null;
        }

        public override string GenerateFluentApi(IProperty property, IAnnotation annotation, string language)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            if (language != "CSharp")
                return null;

            return annotation.Name == NpgsqlAnnotationNames.Comment
                ? $".{nameof(NpgsqlPropertyBuilderExtensions.ForNpgsqlHasComment)}(\"{annotation.Value}\")"
                : null;
        }

        public override string GenerateFluentApi(IIndex index, IAnnotation annotation, string language)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            if (language != "CSharp")
                return null;

            if (annotation.Name == NpgsqlAnnotationNames.IndexMethod) {
                return $".{nameof(NpgsqlIndexBuilderExtensions.ForNpgsqlHasMethod)}(\"{annotation.Value}\")";
            }

            if (annotation.Name == NpgsqlAnnotationNames.IndexOperators) {
                var value = (string)annotation.Value;
                var operatorList = value.Split(' ').Select(o => $"\"{o}\"").Join(", ");
                return $".{nameof(NpgsqlIndexBuilderExtensions.ForNpgsqlHasOperators)}({operatorList})";
            }

            return null;
        }
    }
}
