using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    /// <summary>
    /// Represents the metadata for a PostgreSQL enum.
    /// </summary>
    public class PostgresEnum
    {
        [NotNull] readonly IAnnotatable _annotatable;
        [NotNull] readonly string _annotationName;

        /// <summary>
        /// Creates a <see cref="PostgresEnum"/>.
        /// </summary>
        /// <param name="annotatable">The annotatable to search for the annotation.</param>
        /// <param name="annotationName">The annotation name to search for in the annotatable.</param>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="annotationName"/></exception>
        internal PostgresEnum([NotNull] IAnnotatable annotatable, [NotNull] string annotationName)
        {
            _annotatable = Check.NotNull(annotatable, nameof(annotatable));
            _annotationName = Check.NotNull(annotationName, nameof(annotationName));
        }

        /// <summary>
        /// Creates an <see cref="IAnnotation"/> that represents a PostgreSQL enum.
        /// </summary>
        /// <param name="schema">The enum schema or null to use the model's default schema.</param>
        /// <param name="name">The enum name.</param>
        /// <param name="labels">The enum labels.</param>
        /// <returns>
        /// An <see cref="IAnnotation"/> representing a PostgreSQL enum.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="schema"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="labels"/></exception>
        [NotNull]
        public static IAnnotation CreateAnnotation([CanBeNull] string schema, [NotNull] string name, [NotNull] string[] labels)
        {
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(name, nameof(name));
            Check.NotNull(labels, nameof(labels));

            var annotationName =
                schema != null
                    ? $"{NpgsqlAnnotationNames.EnumPrefix}{schema}.{name}"
                    : $"{NpgsqlAnnotationNames.EnumPrefix}{name}";

            var annotationValue = string.Join(",", labels);

            return new Annotation(annotationName, annotationValue);
        }

        /// <summary>
        /// Gets or adds a <see cref="PostgresEnum"/> from or to the <see cref="IMutableAnnotatable"/>.
        /// </summary>
        /// <param name="annotatable">The annotatable from which to get or add the enum.</param>
        /// <param name="schema">The enum schema or null to use the model's default schema.</param>
        /// <param name="name">The enum name.</param>
        /// <param name="labels">The enum labels.</param>
        /// <returns>
        /// The <see cref="PostgresEnum"/> from the <see cref="IMutableAnnotatable"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="schema"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="labels"/></exception>
        [NotNull]
        public static PostgresEnum GetOrAddPostgresEnum(
            [NotNull] IMutableAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string[] labels)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(name, nameof(name));
            Check.NotNull(labels, nameof(labels));

            var annotation = CreateAnnotation(schema, name, labels);

            var postgresEnum = new PostgresEnum(annotatable, annotation.Name);

            if (postgresEnum.Annotation == null && postgresEnum._annotatable is IMutableAnnotatable mutable)
                mutable.SetAnnotation(annotation.Name, annotation.Value);

            return postgresEnum;
        }

        /// <summary>
        /// Gets the collection of <see cref="PostgresEnum"/> stored in the <see cref="IAnnotatable"/>.
        /// </summary>
        /// <param name="annotatable">The annotatable to search for <see cref="PostgresEnum"/> annotations.</param>
        /// <returns>
        /// The collection of <see cref="PostgresEnum"/> stored in the <see cref="IAnnotatable"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<PostgresEnum> GetPostgresEnums([NotNull] IAnnotatable annotatable)
            => Check.NotNull(annotatable, nameof(annotatable))
                    .GetAnnotations()
                    .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal))
                    .Select(a => new PostgresEnum(annotatable, a.Name));

        [CanBeNull]
        IAnnotation Annotation => _annotatable.FindAnnotation(_annotationName);

        /// <summary>
        /// The enum schema or null to represent the default schema.
        /// </summary>
        [CanBeNull]
        public string Schema => Deserialize(Annotation).Schema ?? (string)_annotatable[RelationalAnnotationNames.DefaultSchema];

        /// <summary>
        /// The enum name.
        /// </summary>
        [NotNull]
        public string Name => Deserialize(Annotation).Name;

        /// <summary>
        /// The enum labels.
        /// </summary>
        [NotNull]
        public string[] Labels => Deserialize(Annotation).Labels;

        static (string Schema, string Name, string[] Labels) Deserialize([CanBeNull] IAnnotation annotation)
        {
            if (annotation == null || !(annotation.Value is string value) || string.IsNullOrEmpty(value))
                return (null, null, null);

            var labels = value.Split(',').ToArray();

            // Yes, this doesn't support dots in the schema/enum name, let somebody complain first.
            var schemaAndName = annotation.Name.Substring(NpgsqlAnnotationNames.EnumPrefix.Length).Split('.');
            switch (schemaAndName.Length)
            {
            case 1:
                return (null, schemaAndName[0], labels);
            case 2:
                return (schemaAndName[0], schemaAndName[1], labels);
            default:
                throw new ArgumentException($"Cannot parse enum name from annotation: {annotation.Name}");
            }
        }
    }
}
