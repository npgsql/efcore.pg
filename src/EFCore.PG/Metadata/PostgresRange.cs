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
            _annotatable = annotatable;
            _annotationName = annotationName;
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
            if (FindPostgresRange(annotatable, schema, name) is PostgresRange rangeType)
                return rangeType;

            rangeType = new PostgresRange(annotatable, BuildAnnotationName(schema, name));
            rangeType.SetData(subtype, canonicalFunction, subtypeOpClass, collation, subtypeDiff);
            return rangeType;
        }

        [CanBeNull]
        public static PostgresRange FindPostgresRange(
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotEmpty(name, nameof(name));

            var annotationName = BuildAnnotationName(schema, name);

            return annotatable[annotationName] == null ? null : new PostgresRange(annotatable, annotationName);
        }

        [NotNull]
        static string BuildAnnotationName(string schema, string name)
            => NpgsqlAnnotationNames.RangePrefix + (schema == null ? name : schema + '.' + name);

        [NotNull]
        public static IEnumerable<PostgresRange> GetPostgresRanges([NotNull] IAnnotatable annotatable)
            => Check.NotNull(annotatable, nameof(annotatable))
                .GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal))
                .Select(a => new PostgresRange(annotatable, a.Name));

        [NotNull]
        public Annotatable Annotatable => (Annotatable)_annotatable;

        [NotNull]
        public string Schema => GetData().Schema;

        [NotNull]
        public string Name => GetData().Name;

        [NotNull]
        public string Subtype
        {
            get => GetData().Subtype;
            set
            {
                var x = GetData();
                var (_, _, _, canonicalFunction, subtypeOpClass, collation, subtypeDiff) = GetData();
                SetData(value, canonicalFunction, subtypeOpClass, collation, subtypeDiff);
            }
        }

        [CanBeNull]
        public string CanonicalFunction
        {
            get => GetData().CanonicalFunction;
            set
            {
                var x = GetData();
                var (_, _, subtype, _, subtypeOpClass, collation, subtypeDiff) = GetData();
                SetData(subtype, value, subtypeOpClass, collation, subtypeDiff);
            }
        }

        [CanBeNull]
        public string SubtypeOpClass
        {
            get => GetData().SubtypeOpClass;
            set
            {
                var x = GetData();
                var (_, _, subtype, canonicalFunction, _, collation, subtypeDiff) = GetData();
                SetData(subtype, canonicalFunction, value, collation, subtypeDiff);
            }
        }

        [CanBeNull]
        public string Collation
        {
            get => GetData().Collation;
            set
            {
                var x = GetData();
                var (_, _, subtype, canonicalFunction, subtypeOpClass, _, subtypeDiff) = GetData();
                SetData(subtype, canonicalFunction, subtypeOpClass, value, subtypeDiff);
            }
        }

        [CanBeNull]
        public string SubtypeDiff
        {
            get => GetData().SubtypeDiff;
            set
            {
                var x = GetData();
                var (_, _, subtype, canonicalFunction, subtypeOpClass, collation, _) = GetData();
                SetData(subtype, canonicalFunction, subtypeOpClass, collation, value);
            }
        }

        (string Schema, string Name, string Subtype, string CanonicalFunction, string SubtypeOpClass, string Collation,
            string SubtypeDiff) GetData()
            => !(Annotatable[_annotationName] is string annotationValue)
                ? (null, null, null, null, null, null, null)
                : Deserialize(_annotationName, annotationValue);

        void SetData(string subtype, string canonicalFunction, string subtypeOpClass, string collation, string subtypeDiff)
            => Annotatable[_annotationName] = $"{subtype},{canonicalFunction},{subtypeOpClass},{collation},{subtypeDiff}";

        static (string Schema, string Name, string Subtype, string CanonicalFunction, string SubtypeOpClass, string collation, string SubtypeDiff)
            Deserialize([NotNull] string annotationName, [NotNull] string annotationValue)
        {
            Check.NotEmpty(annotationValue, nameof(annotationValue));

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
