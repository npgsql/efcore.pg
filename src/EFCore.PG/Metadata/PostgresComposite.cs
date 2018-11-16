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
    public class PostgresComposite
    {
        readonly IAnnotatable _annotatable;
        readonly string _annotationName;

        internal PostgresComposite(IAnnotatable annotatable, string annotationName)
        {
            _annotatable = annotatable;
            _annotationName = annotationName;
        }

        public static PostgresComposite GetOrAddPostgresComposite(
            [NotNull] IMutableAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] params (string Name, string StoreType)[] fields)
        {
            if (FindPostgresComposite(annotatable, schema, name) is PostgresComposite CompositeType)
                return CompositeType;

            // Adding a new composite definition.
            // Each composite annotation has an ordering number in the annotation value: composite CREATE TYPE
            // migrations need to be generated in a specific order, since they may depend on each other (composite
            // types nested within other composite types).
            // Find the next free ordering number
            var ordering = GetPostgresComposites(annotatable).Any()
                ? GetPostgresComposites(annotatable).Select(a => a.Ordering).Max() + 1
                : 0;

            CompositeType = new PostgresComposite(annotatable, BuildAnnotationName(schema, name));
            CompositeType.SetData(ordering, fields);
            return CompositeType;
        }

        public static PostgresComposite GetOrAddPostgresComposite(
            [NotNull] IMutableAnnotatable annotatable,
            [NotNull] string name,
            [NotNull] params (string Name, string StoreType)[] fields)
            => GetOrAddPostgresComposite(annotatable, null, name, fields);

        public static PostgresComposite FindPostgresComposite(
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotEmpty(name, nameof(name));

            var annotationName = BuildAnnotationName(schema, name);

            return annotatable[annotationName] == null ? null : new PostgresComposite(annotatable, annotationName);
        }

        static string BuildAnnotationName(string schema, string name)
            => NpgsqlAnnotationNames.CompositePrefix + (schema == null ? name : schema + '.' + name);

        public static IEnumerable<PostgresComposite> GetPostgresComposites([NotNull] IAnnotatable annotatable)
        {
            Check.NotNull(annotatable, nameof(annotatable));

            return annotatable.GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.CompositePrefix, StringComparison.Ordinal))
                .Select(a => new PostgresComposite(annotatable, a.Name));
        }

        public Annotatable Annotatable => (Annotatable)_annotatable;

        public string Schema => GetData().Schema;

        public string Name => GetData().Name;

        public int Ordering => GetData().Ordering;

        public (string Name, string StoreType)[] Fields => GetData().Fields;

        (string Schema, string Name, int Ordering, (string Name, string StoreType)[] Fields) GetData()
        {
            return !(Annotatable[_annotationName] is string annotationValue)
                ? (null, null, 0, null)
                : Deserialize(_annotationName, annotationValue);
        }

        void SetData(int ordering, (string Name, string StoreType)[] fields)
            => Annotatable[_annotationName] = ordering + ";" + string.Join(";", fields.Select(f => $"{f.Name},{f.StoreType}"));

        static (string schema, string name, int ordering, (string Name, string StoreType)[]) Deserialize(
            [NotNull] string annotationName,
            [NotNull] string annotationValue)
        {
            Check.NotEmpty(annotationValue, nameof(annotationValue));

            if (!int.TryParse(annotationValue.Split(';')[0], out var ordering))
                throw new ArgumentException("Cannot parse composite ordering from annotation: " + annotationName);

            var fields = annotationValue.Split(';')
                .Skip(1)
                .Select(s => (s.Split(',')[0], s.Split(',')[1])).ToArray();

            var schemaAndName = annotationName.Substring(NpgsqlAnnotationNames.CompositePrefix.Length).Split('.');
            switch (schemaAndName.Length)
            {
            case 1:
                return (null, schemaAndName[0], ordering, fields);
            case 2:
                return (schemaAndName[0], schemaAndName[1], ordering, fields);
            default:
                throw new ArgumentException("Cannot parse composite name from annotation: " + annotationName);
            }
        }
    }
}
