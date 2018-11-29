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
    public class PostgresRange
    {
        [NotNull] readonly IAnnotatable _annotatable;
        [NotNull] readonly string _annotationName;

        internal PostgresRange([NotNull] IAnnotatable annotatable, [NotNull] string annotationName)
        {
            _annotatable = Check.NotNull(annotatable, nameof(annotatable));
            _annotationName = Check.NotNull(annotationName, nameof(annotationName));
        }

        public static PostgresRange GetOrAddPostgresRange(
            [NotNull] IMutableAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string subtype,
            string canonicalFunction = null,
            string subtypeOpClass = null,
            string collation = null,
            string subtypeDiff = null)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(name, nameof(name));
            Check.NotNull(subtype, nameof(subtype));

            var annotationName =
                schema != null
                    ? $"{NpgsqlAnnotationNames.RangePrefix}{schema}.{name}"
                    : $"{NpgsqlAnnotationNames.RangePrefix}{name}";

            return annotatable[annotationName] != null
                ? new PostgresRange(annotatable, annotationName)
                : new PostgresRange(annotatable, annotationName)
                {
                    CanonicalFunction = canonicalFunction,
                    Collation = collation,
                    Subtype = subtype,
                    SubtypeDiff = subtypeDiff,
                    SubtypeOpClass = subtypeOpClass
                };
        }

        [NotNull]
        public static IEnumerable<PostgresRange> GetPostgresRanges([NotNull] IAnnotatable annotatable)
            => Check.NotNull(annotatable, nameof(annotatable))
                    .GetAnnotations()
                    .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal))
                    .Select(a => new PostgresRange(annotatable, a.Name));

        [CanBeNull]
        public string Schema => GetData().Schema;

        [NotNull]
        public string Name => GetData().Name;

        [NotNull]
        public string Subtype
        {
            get => GetData().Subtype;
            private set => SetData(subtype: value);
        }

        [CanBeNull]
        public string CanonicalFunction
        {
            get => GetData().CanonicalFunction;
            private set => SetData(canonicalFunction: value);
        }

        [CanBeNull]
        public string SubtypeOpClass
        {
            get => GetData().SubtypeOpClass;
            private set => SetData(subtypeOpClass: value);
        }

        [CanBeNull]
        public string Collation
        {
            get => GetData().Collation;
            private set => SetData(collation: value);
        }

        [CanBeNull]
        public string SubtypeDiff
        {
            get => GetData().SubtypeDiff;
            private set => SetData(subtypeDiff: value);
        }

        (string Schema, string Name, string Subtype, string CanonicalFunction, string SubtypeOpClass, string Collation, string SubtypeDiff) GetData()
            => Deserialize(_annotationName, _annotatable[_annotationName] as string);

        void SetData(string subtype = null, string canonicalFunction = null, string subtypeOpClass = null, string collation = null, string subtypeDiff = null)
        {
            ((Annotatable)_annotatable)[_annotationName] =
                string.Join(",",
                    subtype ?? Subtype,
                    canonicalFunction ?? CanonicalFunction,
                    subtypeOpClass ?? SubtypeOpClass,
                    collation ?? Collation,
                    subtypeDiff ?? SubtypeDiff);
        }

        static (string Schema, string Name, string Subtype, string CanonicalFunction, string SubtypeOpClass, string Collation, string SubtypeDiff)
            Deserialize([NotNull] string annotationName, [CanBeNull] string annotationValue)
        {
            if (annotationValue == null)
                return (null, null, null, null, null, null, null);

            var elements = annotationValue.Split(',').ToArray();
            if (elements.Length != 5)
                throw new ArgumentException("Cannot parse range annotation value: " + annotationValue);
            for (var i = 0; i < 5; i++)
                if (elements[i] == "")
                    elements[i] = null;

            // Yes, this doesn't support dots in the schema/range name, let somebody complain first.
            var schemaAndName = annotationName.Substring(NpgsqlAnnotationNames.RangePrefix.Length).Split('.');
            switch (schemaAndName.Length)
            {
            case 1:
                return (null, schemaAndName[0], elements[0], elements[1], elements[2], elements[3], elements[4]);
            case 2:
                return (schemaAndName[0], schemaAndName[1], elements[0], elements[1], elements[2], elements[3], elements[4]);
            default:
                throw new ArgumentException("Cannot parse enum name from annotation: " + annotationName);
            }
        }
    }
}
