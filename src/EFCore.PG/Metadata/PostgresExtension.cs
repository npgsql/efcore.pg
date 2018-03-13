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
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class PostgresExtension : IPostgresExtension
    {
        readonly IAnnotatable _annotatable;
        readonly string _annotationName;

        internal PostgresExtension(IAnnotatable annotatable, string annotationName)
        {
            _annotatable = annotatable;
            _annotationName = annotationName;
        }

        public static PostgresExtension GetOrAddPostgresExtension(
            [NotNull] IMutableAnnotatable annotatable,
            [NotNull] string extensionName)
        {
            var extension = (PostgresExtension)FindPostgresExtension(annotatable, extensionName);
            if (extension != null)
                return extension;

            extension = new PostgresExtension(annotatable, BuildAnnotationName(extensionName));
            extension.SetData(new PostgresExtensionData { Name = extensionName });
            return extension;
        }

        public static IPostgresExtension FindPostgresExtension(
            [NotNull] IAnnotatable annotatable,
            [NotNull] string name)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotEmpty(name, nameof(name));

            var annotationName = BuildAnnotationName(name);

            return annotatable[annotationName] == null ? null : new PostgresExtension(annotatable, annotationName);
        }

        static string BuildAnnotationName(string name)
            => NpgsqlAnnotationNames.PostgresExtensionPrefix + name;

        public static IEnumerable<IPostgresExtension> GetPostgresExtensions([NotNull] IAnnotatable annotatable)
        {
            Check.NotNull(annotatable, nameof(annotatable));

            return annotatable.GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal))
                .Select(a => new PostgresExtension(annotatable, a.Name));
        }

        public Annotatable Annotatable => (Annotatable)_annotatable;

        public string Name => GetData().Name;

        [CanBeNull]
        public string Schema
        {
            get => GetData().Schema;
            set
            {
                var data = GetData();
                data.Schema = value;
                SetData(data);
            }
        }

        [CanBeNull]
        public string Version
        {
            get => GetData().Version;
            set
            {
                var data = GetData();
                data.Version = value;
                SetData(data);
            }
        }

        PostgresExtensionData GetData() => PostgresExtensionData.Deserialize((string)Annotatable[_annotationName]);

        void SetData(PostgresExtensionData data)
            => Annotatable[_annotationName] = data.Serialize();

        IAnnotatable IPostgresExtension.Annotatable => _annotatable;

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Schema != null)
            {
                sb.Append(Schema);
                sb.Append('.');
            }
            sb.Append(Name);
            if (Version != null)
            {
                sb.Append("' (");
                sb.Append(Version);
                sb.Append(')');
            }
            return sb.ToString();
        }

        class PostgresExtensionData
        {
            public string Name { get; set; }
            public string Schema { get; set; }
            public string Version { get; set; }

            public string Serialize()
            {
                var builder = new StringBuilder();

                EscapeAndQuote(builder, Name);
                builder.Append(", ");
                EscapeAndQuote(builder, Schema);
                builder.Append(", ");
                EscapeAndQuote(builder, Version);

                return builder.ToString();
            }

            public static PostgresExtensionData Deserialize([NotNull] string value)
            {
                Check.NotEmpty(value, nameof(value));

                try
                {
                    var data = new PostgresExtensionData();

                    // ReSharper disable PossibleInvalidOperationException
                    var position = 0;
                    data.Name = ExtractValue(value, ref position);
                    data.Schema = ExtractValue(value, ref position);
                    data.Version = ExtractValue(value, ref position);
                    // ReSharper restore PossibleInvalidOperationException

                    return data;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(RelationalStrings.BadSequenceString, ex);
                }
            }

            private static string ExtractValue(string value, ref int position)
            {
                position = value.IndexOf('\'', position) + 1;

                var end = value.IndexOf('\'', position);

                while ((end + 1 < value.Length)
                       && (value[end + 1] == '\''))
                {
                    end = value.IndexOf('\'', end + 2);
                }

                var extracted = value.Substring(position, end - position).Replace("''", "'");
                position = end + 1;

                return extracted.Length == 0 ? null : extracted;
            }

            private static void EscapeAndQuote(StringBuilder builder, object value)
            {
                builder.Append("'");

                if (value != null)
                {
                    builder.Append(value.ToString().Replace("'", "''"));
                }

                builder.Append("'");
            }
        }
    }
}
