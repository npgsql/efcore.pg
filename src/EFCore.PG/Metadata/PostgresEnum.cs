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
    public class PostgresEnum
    {
        readonly IAnnotatable _annotatable;
        readonly string _annotationName;

        internal PostgresEnum(IAnnotatable annotatable, string annotationName)
        {
            _annotatable = annotatable;
            _annotationName = annotationName;
        }

        public static PostgresEnum GetOrAddPostgresEnum(
            [NotNull] IMutableAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string[] labels)
        {
            if (FindPostgresEnum(annotatable, schema, name) is PostgresEnum enumType)
                return enumType;

            enumType = new PostgresEnum(annotatable, BuildAnnotationName(schema, name));
            enumType.SetData(labels);
            return enumType;
        }

        public static PostgresEnum GetOrAddPostgresEnum(
            [NotNull] IMutableAnnotatable annotatable,
            [NotNull] string name,
            [NotNull] string[] labels)
            => GetOrAddPostgresEnum(annotatable, null, name, labels);

        public static PostgresEnum FindPostgresEnum(
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] string schema,
            [NotNull] string name)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotEmpty(name, nameof(name));

            var annotationName = BuildAnnotationName(schema, name);

            return annotatable[annotationName] == null ? null : new PostgresEnum(annotatable, annotationName);
        }

        static string BuildAnnotationName(string schema, string name)
            => NpgsqlAnnotationNames.EnumPrefix + (schema == null ? name : schema + '.' + name);

        public static IEnumerable<PostgresEnum> GetPostgresEnums([NotNull] IAnnotatable annotatable)
        {
            Check.NotNull(annotatable, nameof(annotatable));

            return annotatable.GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal))
                .Select(a => new PostgresEnum(annotatable, a.Name));
        }

        public Annotatable Annotatable => (Annotatable)_annotatable;

        public string Schema => GetData().Schema;

        public string Name => GetData().Name;

        public string[] Labels
        {
            get => GetData().Labels;
            set => SetData(value);
        }

        (string Schema, string Name, string[] Labels) GetData()
        {
            return !(Annotatable[_annotationName] is string annotationValue)
                ? (null, null, null)
                : Deserialize(_annotationName, annotationValue);
        }

        void SetData(string[] labels)
            => Annotatable[_annotationName] = string.Join(",", labels);

        static (string schema, string name, string[] labels) Deserialize(
            [NotNull] string annotationName,
            [NotNull] string annotationValue)
        {
            Check.NotEmpty(annotationValue, nameof(annotationValue));

            var labels = annotationValue.Split(',').ToArray();

            // Yes, this doesn't support dots in the schema/enum name, let somebody complain first.
            var schemaAndName = annotationName.Substring(NpgsqlAnnotationNames.EnumPrefix.Length).Split('.');
            switch (schemaAndName.Length)
            {
            case 1:
                return (null, schemaAndName[0], labels);
            case 2:
                return (schemaAndName[0], schemaAndName[1], labels);
            default:
                throw new ArgumentException("Cannot parse enum name from annotation: " + annotationName);
            }
        }
    }
}
