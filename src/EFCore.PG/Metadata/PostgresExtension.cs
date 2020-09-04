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
    /// Represents the metadata for a PostgreSQL extension.
    /// </summary>
    [PublicAPI]
    public class PostgresExtension
    {
        [NotNull] readonly IAnnotatable _annotatable;
        [NotNull] readonly string _annotationName;

        /// <summary>
        /// Creates a <see cref="PostgresExtension"/>.
        /// </summary>
        /// <param name="annotatable">The annotatable to search for the annotation.</param>
        /// <param name="annotationName">The annotation name to search for in the annotatable.</param>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="annotationName"/></exception>
        internal PostgresExtension([NotNull] IAnnotatable annotatable, [NotNull] string annotationName)
        {
            _annotatable = Check.NotNull(annotatable, nameof(annotatable));
            _annotationName = Check.NotNull(annotationName, nameof(annotationName));
        }

        /// <summary>
        /// Gets or adds a <see cref="PostgresExtension"/> from or to the <see cref="IMutableAnnotatable"/>.
        /// </summary>
        /// <param name="annotatable">The annotatable from which to get or add the extension.</param>
        /// <param name="schema">The extension schema or null to use the model's default schema.</param>
        /// <param name="name">The extension name.</param>
        /// <param name="version">The extension version.</param>
        /// <returns>
        /// The <see cref="PostgresExtension"/> from the <see cref="IMutableAnnotatable"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="schema"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
        [NotNull]
        public static PostgresExtension GetOrAddPostgresExtension(
            [NotNull] IMutableAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string version)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(name, nameof(name));

            if (FindPostgresExtension(annotatable, schema, name) is PostgresExtension postgresExtension)
                return postgresExtension;

            var annotationName = BuildAnnotationName(schema, name);

            return new PostgresExtension(annotatable, annotationName) { Version = version };
        }

        /// <summary>
        /// Gets or adds a <see cref="PostgresExtension"/> from or to the <see cref="IMutableAnnotatable"/>.
        /// </summary>
        /// <param name="annotatable">The annotatable from which to get or add the extension.</param>
        /// <param name="name">The extension name.</param>
        /// <param name="version">The extension version.</param>
        /// <returns>
        /// The <see cref="PostgresExtension"/> from the <see cref="IMutableAnnotatable"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
        [NotNull]
        public static PostgresExtension GetOrAddPostgresExtension(
            [NotNull] IMutableAnnotatable annotatable,
            [NotNull] string name,
            [CanBeNull] string version)
            => GetOrAddPostgresExtension(annotatable, null, name, version);

        /// <summary>
        /// Finds a <see cref="PostgresExtension"/> in the <see cref="IAnnotatable"/>, or returns null if not found.
        /// </summary>
        /// <param name="annotatable">The annotatable to search for the extension.</param>
        /// <param name="schema">The extension schema. The default schema is never used.</param>
        /// <param name="name">The extension name.</param>
        /// <returns>
        /// The <see cref="PostgresExtension"/> from the <see cref="IAnnotatable"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="schema"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
        [CanBeNull]
        public static PostgresExtension FindPostgresExtension(
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotEmpty(name, nameof(name));

            var annotationName = BuildAnnotationName(schema, name);

            return annotatable[annotationName] == null ? null : new PostgresExtension(annotatable, annotationName);
        }

        [NotNull]
        static string BuildAnnotationName(string schema, string name)
            => schema != null
                ? $"{NpgsqlAnnotationNames.PostgresExtensionPrefix}{schema}.{name}"
                : $"{NpgsqlAnnotationNames.PostgresExtensionPrefix}{name}";

        /// <summary>
        /// Gets the collection of <see cref="PostgresExtension"/> stored in the <see cref="IAnnotatable"/>.
        /// </summary>
        /// <param name="annotatable">The annotatable to search for <see cref="PostgresExtension"/> annotations.</param>
        /// <returns>
        /// The collection of <see cref="PostgresExtension"/> stored in the <see cref="IAnnotatable"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<PostgresExtension> GetPostgresExtensions([NotNull] IAnnotatable annotatable)
            => Check.NotNull(annotatable, nameof(annotatable))
                    .GetAnnotations()
                    .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal))
                    .Select(a => new PostgresExtension(annotatable, a.Name));

        /// <summary>
        /// The <see cref="Annotatable"/> that stores the extension.
        /// </summary>
        [NotNull]
        public virtual Annotatable Annotatable => (Annotatable)_annotatable;

        /// <summary>
        /// The extension schema or null to represent the default schema.
        /// </summary>
        [CanBeNull]
        public virtual string Schema => GetData().Schema;

        /// <summary>
        /// The extension name.
        /// </summary>
        [NotNull]
        public virtual string Name => GetData().Name;

        /// <summary>
        /// The extension version.
        /// </summary>
        public virtual string Version
        {
            get => GetData().Version;
            [param: CanBeNull] set => SetData(value);
        }

        (string Schema, string Name, string Version) GetData()
            => Deserialize(Annotatable.FindAnnotation(_annotationName));

        void SetData([CanBeNull] string version)
        {
            var data = GetData();
            Annotatable[_annotationName] = $"{data.Schema},{data.Name},{version}";
        }

        static (string Schema, string Name, string Version) Deserialize([CanBeNull] IAnnotation annotation)
        {
            if (annotation == null || !(annotation.Value is string value) || string.IsNullOrEmpty(value))
                return (null, null, null);

            // TODO: Can't actually use schema and name...they might not be set when this is first called.
            var schemaNameValue = value.Split(',').Select(x => x.Trim()).Select(x => x == "" || x == "''" ? null : x).ToArray();
            var schemaAndName = annotation.Name.Substring(NpgsqlAnnotationNames.PostgresExtensionPrefix.Length).Split('.');
            switch (schemaAndName.Length)
            {
            case 1:
                return (null, schemaAndName[0], schemaNameValue[2]);
            case 2:
                return (schemaAndName[0], schemaAndName[1], schemaNameValue[2]);
            default:
                throw new ArgumentException($"Cannot parse extension name from annotation: {annotation.Name}");
            }
        }
    }
}
