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

        public override MethodCallCodeFragment GenerateFluentApi(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix))
            {
                var extension = new PostgresExtension(model, annotation.Name);

                return (bool)annotation.Value == false
                    ? new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension), extension)
                    : new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension));
            }

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == NpgsqlAnnotationNames.Comment)
            {
                return (bool)annotation.Value == false
                    ? new MethodCallCodeFragment(nameof(NpgsqlEntityTypeBuilderExtensions.ForNpgsqlHasComment), false)
                    : new MethodCallCodeFragment(nameof(NpgsqlEntityTypeBuilderExtensions.ForNpgsqlHasComment));
            }

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == NpgsqlAnnotationNames.Comment)
            {
                return (bool)annotation.Value == false
                    ? new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.ForNpgsqlHasComment), false)
                    : new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.ForNpgsqlHasComment));
            }

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IIndex index, IAnnotation annotation)
        {
            if (annotation.Name == NpgsqlAnnotationNames.IndexMethod)
            {
                return (bool)annotation.Value == false
                    ? new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.ForNpgsqlHasMethod), false)
                    : new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.ForNpgsqlHasMethod));
            }

            return null;
        }
    }
}
