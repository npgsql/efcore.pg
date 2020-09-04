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
    /// Represents the metadata for a PostgreSQL range.
    /// </summary>
    [PublicAPI]
    public class PostgresRange
    {
        [NotNull] readonly IAnnotatable _annotatable;
        [NotNull] readonly string _annotationName;

        /// <summary>
        /// Creates a <see cref="PostgresRange"/>.
        /// </summary>
        /// <param name="annotatable">The annotatable to search for the annotation.</param>
        /// <param name="annotationName">The annotation name to search for in the annotatable.</param>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="annotationName"/></exception>
        internal PostgresRange([NotNull] IAnnotatable annotatable, [NotNull] string annotationName)
        {
            _annotatable = Check.NotNull(annotatable, nameof(annotatable));
            _annotationName = Check.NotNull(annotationName, nameof(annotationName));
        }

        /// <summary>
        /// Gets or adds a <see cref="PostgresRange"/> from or to the <see cref="IMutableAnnotatable"/>.
        /// </summary>
        /// <param name="annotatable">The annotatable from which to get or add the range.</param>
        /// <param name="schema">The range schema or null to use the model's default schema.</param>
        /// <param name="name">The range name.</param>
        /// <param name="subtype">The range subtype.</param>
        /// <param name="canonicalFunction"></param>
        /// <param name="subtypeOpClass"></param>
        /// <param name="collation"></param>
        /// <param name="subtypeDiff"></param>
        /// <returns>
        /// The <see cref="PostgresRange"/> from the <see cref="IMutableAnnotatable"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="schema"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="subtype"/></exception>
        [NotNull]
        public static PostgresRange GetOrAddPostgresRange(
            [NotNull] IMutableAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string subtype,
            [CanBeNull] string canonicalFunction = null,
            [CanBeNull] string subtypeOpClass = null,
            [CanBeNull] string collation = null,
            [CanBeNull] string subtypeDiff = null)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(subtype, nameof(subtype));

            if (FindPostgresRange(annotatable, schema, name) is PostgresRange postgresRange)
                return postgresRange;

            var annotationName = BuildAnnotationName(schema, name);

            return new PostgresRange(annotatable, annotationName)
            {
                Subtype = subtype,
                CanonicalFunction = canonicalFunction,
                SubtypeOpClass = subtypeOpClass,
                Collation = collation,
                SubtypeDiff = subtypeDiff,
            };
        }

        /// <summary>
        /// Finds a <see cref="PostgresRange"/> in the <see cref="IAnnotatable"/>, or returns null if not found.
        /// </summary>
        /// <param name="annotatable">The annotatable to search for the range.</param>
        /// <param name="schema">The range schema or null to use the model's default schema.</param>
        /// <param name="name">The range name.</param>
        /// <returns>
        /// The <see cref="PostgresRange"/> from the <see cref="IAnnotatable"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="schema"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
        [CanBeNull]
        public static PostgresRange FindPostgresRange(
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotEmpty(name, nameof(name));

            var annotationName = BuildAnnotationName(schema, name);

            return annotatable[annotationName] == null ? null : new PostgresRange(annotatable, annotationName);
        }

        [NotNull]
        static string BuildAnnotationName(string schema, string name)
            => schema != null
                ? $"{NpgsqlAnnotationNames.RangePrefix}{schema}.{name}"
                : $"{NpgsqlAnnotationNames.RangePrefix}{name}";

        /// <summary>
        /// Gets the collection of <see cref="PostgresRange"/> stored in the <see cref="IAnnotatable"/>.
        /// </summary>
        /// <param name="annotatable">The annotatable to search for <see cref="PostgresRange"/> annotations.</param>
        /// <returns>
        /// The collection of <see cref="PostgresRange"/> stored in the <see cref="IAnnotatable"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="annotatable"/></exception>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<PostgresRange> GetPostgresRanges([NotNull] IAnnotatable annotatable)
            => Check.NotNull(annotatable, nameof(annotatable))
                    .GetAnnotations()
                    .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal))
                    .Select(a => new PostgresRange(annotatable, a.Name));

        /// <summary>
        /// The <see cref="Annotatable"/> that stores the range.
        /// </summary>
        [NotNull]
        public virtual Annotatable Annotatable => (Annotatable)_annotatable;

        /// <summary>
        /// The range schema or null to represent the default schema.
        /// </summary>
        [CanBeNull]
        public virtual string Schema => GetData().Schema;

        /// <summary>
        /// The range name.
        /// </summary>
        [NotNull]
        public virtual string Name => GetData().Name;

        /// <summary>
        /// The subtype of the range.
        /// </summary>
        public virtual string Subtype
        {
            get => GetData().Subtype;
            [param: NotNull] set => SetData(subtype: value);
        }

        /// <summary>
        /// The function defining a "step" in a discrete range.
        /// </summary>
        public virtual string CanonicalFunction
        {
            get => GetData().CanonicalFunction;
            [param: CanBeNull] set => SetData(canonicalFunction: value);
        }

        /// <summary>
        /// The operator class to use.
        /// </summary>
        public virtual string SubtypeOpClass
        {
            get => GetData().SubtypeOpClass;
            [param: CanBeNull] set => SetData(subtypeOpClass: value);
        }

        /// <summary>
        /// The collation to use.
        /// </summary>
        public virtual string Collation
        {
            get => GetData().Collation;
            [param: CanBeNull] set => SetData(collation: value);
        }

        /// <summary>
        /// The function defining a difference in subtype values.
        /// </summary>
        public virtual string SubtypeDiff
        {
            get => GetData().SubtypeDiff;
            [param: CanBeNull] set => SetData(subtypeDiff: value);
        }

        (string Schema, string Name, string Subtype, string CanonicalFunction, string SubtypeOpClass, string Collation, string SubtypeDiff) GetData()
            => Deserialize(Annotatable.FindAnnotation(_annotationName));

        void SetData(string subtype = null, string canonicalFunction = null, string subtypeOpClass = null, string collation = null, string subtypeDiff = null)
            => Annotatable[_annotationName] =
                $"{subtype ?? Subtype},{canonicalFunction ?? CanonicalFunction},{subtypeOpClass ?? SubtypeOpClass},{collation ?? Collation},{subtypeDiff ?? SubtypeDiff}";

        static (string Schema, string Name, string Subtype, string CanonicalFunction, string SubtypeOpClass, string Collation, string SubtypeDiff)
            Deserialize([CanBeNull] IAnnotation annotation)
        {
            if (annotation == null || !(annotation.Value is string value) || string.IsNullOrEmpty(value))
                return (null, null, null, null, null, null, null);

            var elements = value.Split(',');
            if (elements.Length != 5)
                throw new ArgumentException($"Cannot parse range annotation value: {value}");

            for (var i = 0; i < 5; i++)
                if (elements[i] == "")
                    elements[i] = null;

            // TODO: This would be a safer operation if we stored schema and name in the annotation value (see Sequence.cs).
            // Yes, this doesn't support dots in the schema/range name, let somebody complain first.
            var schemaAndName = annotation.Name.Substring(NpgsqlAnnotationNames.RangePrefix.Length).Split('.');
            switch (schemaAndName.Length)
            {
            case 1:
                return (null, schemaAndName[0], elements[0], elements[1], elements[2], elements[3], elements[4]);
            case 2:
                return (schemaAndName[0], schemaAndName[1], elements[0], elements[1], elements[2], elements[3], elements[4]);
            default:
                throw new ArgumentException($"Cannot parse range name from annotation: {annotation.Name}");
            }
        }
    }
}
