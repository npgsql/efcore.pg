#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
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
            [NotNull] IReadOnlyList<string> labels)
        {
            var extension = FindPostgresEnum(annotatable, schema, name);
            if (extension != null)
                return extension;

            extension = new PostgresEnum(annotatable, BuildAnnotationName(schema, name));
            extension.SetData(labels);
            return extension;
        }

        public static PostgresEnum GetOrAddPostgresEnum(
            [NotNull] IMutableAnnotatable annotatable,
            [NotNull] string name,
            [NotNull] IReadOnlyList<string> labels)
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

        public IReadOnlyList<string> Labels
        {
            get => GetData().Labels;
            set => SetData(value);
        }

        (string Schema, string Name, List<string> Labels) GetData()
        {
            return !(Annotatable[_annotationName] is string annotationValue)
                ? (null, null, null)
                : Deserialize(_annotationName, annotationValue);
        }

        void SetData(IReadOnlyList<string> labels)
            => Annotatable[_annotationName] = string.Join(",", labels);

        static (string schema, string name, List<string> labels) Deserialize(
            [NotNull] string annotationName,
            [NotNull] string annotationValue)
        {
            Check.NotEmpty(annotationValue, nameof(annotationValue));

            var labels = annotationValue.Split(',').ToList();

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
