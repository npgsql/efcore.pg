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
    public class PostgresCollation
    {
        [NotNull] readonly IAnnotatable _annotatable;
        [NotNull] readonly string _annotationName;

        internal PostgresCollation([NotNull] IAnnotatable annotatable, [NotNull] string annotationName)
        {
            _annotatable = Check.NotNull(annotatable, nameof(annotatable));
            _annotationName = Check.NotNull(annotationName, nameof(annotationName));
        }

        [NotNull]
        public static PostgresCollation GetOrAddCollation(
            [NotNull] IMutableAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string lcCollate = null,
            [CanBeNull] string lcCtype = null,
            [CanBeNull] string provider = null,
            [CanBeNull] bool? deterministic = null)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotEmpty(name, nameof(name));

            if (FindCollation(annotatable, schema, name) is PostgresCollation collation)
                return collation;

            var annotationName = BuildAnnotationName(schema, name);

            return new PostgresCollation(annotatable, annotationName)
            {
                LcCollate = lcCollate,
                LcCtype = lcCtype,
                Provider = provider,
                IsDeterministic = deterministic
            };
        }

        [CanBeNull]
        public static PostgresCollation FindCollation(
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotEmpty(name, nameof(name));

            var annotationName = BuildAnnotationName(schema, name);

            return annotatable[annotationName] == null ? null : new PostgresCollation(annotatable, annotationName);
        }

        [NotNull]
        static string BuildAnnotationName(string schema, string name)
            => schema != null
                ? $"{NpgsqlAnnotationNames.CollationDefinitionPrefix}{schema}.{name}"
                : $"{NpgsqlAnnotationNames.CollationDefinitionPrefix}{name}";

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<PostgresCollation> GetCollations([NotNull] IAnnotatable annotatable)
            => Check.NotNull(annotatable, nameof(annotatable))
                    .GetAnnotations()
                    .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.CollationDefinitionPrefix, StringComparison.Ordinal))
                    .Select(a => new PostgresCollation(annotatable, a.Name));

        [NotNull]
        public Annotatable Annotatable => (Annotatable)_annotatable;

        [CanBeNull]
        public string Schema => GetData().Schema;

        [NotNull]
        public string Name => GetData().Name;

        [NotNull]
        public string LcCollate
        {
            get => GetData().LcCollate;
            set => SetData(lcCollate: value);
        }

        [NotNull]
        public string LcCtype
        {
            get => GetData().LcCtype;
            set => SetData(lcCtype: value);
        }

        [CanBeNull]
        public string Provider
        {
            get => GetData().Provider;
            set => SetData(provider: value);
        }

        public bool? IsDeterministic
        {
            get => GetData().IsDeterministic;
            set => SetData(deterministic: value);
        }

        (string Schema, string Name, string LcCollate, string LcCtype, string Provider, bool? IsDeterministic) GetData()
            => Deserialize(Annotatable.FindAnnotation(_annotationName));

        void SetData(string lcCollate = null, string lcCtype = null, string provider = null, bool? deterministic = null)
            => Annotatable[_annotationName] =
                $"{lcCollate ?? LcCollate},{lcCtype ?? LcCtype},{provider ?? Provider},{deterministic ?? IsDeterministic}";

        static (string Schemaa, string Name, string LcCollate, string LcCtype, string Provider, bool? IsDeterministic)
            Deserialize([CanBeNull] IAnnotation annotation)
        {
            if (annotation == null || !(annotation.Value is string value) || string.IsNullOrEmpty(value))
                return (null, null, null, null, null, null);

            var elements = value.Split(',');
            if (elements.Length != 4)
                throw new ArgumentException($"Cannot parse collation annotation value: {value}");

            for (var i = 0; i < 4; i++)
                if (elements[i] == "")
                    elements[i] = null;

            var isDeterministic = elements[3] is null
                ? (bool?)null
                : bool.Parse(elements[3]);

            // TODO: This would be a safer operation if we stored schema and name in the annotation value (see Sequence.cs).
            // Yes, this doesn't support dots in the schema/collation name, let somebody complain first.
            var schemaAndName = annotation.Name.Substring(NpgsqlAnnotationNames.CollationDefinitionPrefix.Length).Split('.');
            switch (schemaAndName.Length)
            {
            case 1:
                return (null, schemaAndName[0], elements[0], elements[1], elements[2], isDeterministic);
            case 2:
                return (schemaAndName[0], schemaAndName[1], elements[0], elements[1], elements[2], isDeterministic);
            default:
                throw new ArgumentException($"Cannot parse collation name from annotation: {annotation.Name}");
            }
        }
    }
}
