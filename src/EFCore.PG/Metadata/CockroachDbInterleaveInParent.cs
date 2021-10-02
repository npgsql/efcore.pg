using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class CockroachDbInterleaveInParent
    {
        private const string AnnotationName = CockroachDbAnnotationNames.InterleaveInParent;

        private readonly IReadOnlyAnnotatable _annotatable;

        public virtual Annotatable Annotatable => (Annotatable)_annotatable;

        public CockroachDbInterleaveInParent(IReadOnlyAnnotatable annotatable)
            => _annotatable = annotatable;

        public virtual string? ParentTableSchema
        {
            get => GetData().ParentTableSchema;
            set
            {
                (var _, var parentTableName, var interleavePrefix) = GetData();
                SetData(value, parentTableName, interleavePrefix);
            }
        }

        public virtual string ParentTableName
        {
            get => GetData().ParentTableName;
            set
            {
                (var parentTableSchema, var _, var interleavePrefix) = GetData();
                SetData(parentTableSchema, value, interleavePrefix);
            }
        }

        public virtual List<string> InterleavePrefix
        {
            get => GetData().InterleavePrefix;
            set
            {
                (var parentTableSchema, var parentTableName, var _) = GetData();
                SetData(parentTableSchema, parentTableName, value);
            }
        }

        private (string? ParentTableSchema, string ParentTableName, List<string> InterleavePrefix) GetData()
        {
            var str = Annotatable[AnnotationName] as string;
            return str == null
                ? (null, null!, null!)
                : Deserialize(str);
        }

        private void SetData(string? parentTableSchema, string parentTableName, List<string> interleavePrefix)
            => Annotatable[AnnotationName] = Serialize(parentTableSchema, parentTableName, interleavePrefix);

        private static string Serialize(string? parentTableSchema, string parentTableName, List<string> interleavePrefix)
        {
            var builder = new StringBuilder();

            EscapeAndQuote(builder, parentTableSchema);
            builder.Append(", ");
            EscapeAndQuote(builder, parentTableName);
            if (interleavePrefix != null && interleavePrefix.Count > 0)
            {
                builder.Append(", ");
                for (var i = 0; i < interleavePrefix.Count; i++)
                {
                    EscapeAndQuote(builder, interleavePrefix[i]);
                    if (i < interleavePrefix.Count - 1)
                    {
                        builder.Append(", ");
                    }
                }
            }
            return builder.ToString();
        }

        internal static (string? ParentTableSchema, string ParentTableName, List<string> InterleavePrefix) Deserialize(string value)
        {
            Check.NotEmpty(value, nameof(value));

            try
            {
                var position = 0;
                var parentTableSchema = ExtractValue(value, ref position);
                var parentTableName = ExtractValue(value, ref position)!;
                var interleavePrefix = new List<string>();
                while (position < value.Length - 1)
                {
                    interleavePrefix.Add(ExtractValue(value, ref position)!);
                }

                return (parentTableSchema, parentTableName, interleavePrefix);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Couldn't deserialize {nameof(CockroachDbInterleaveInParent)} from annotation", ex);
            }
        }

        private static string? ExtractValue(string value, ref int position)
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

        private static void EscapeAndQuote(StringBuilder builder, object? value)
        {
            builder.Append("'");

            if (value != null)
            {
                builder.Append(value.ToString()!.Replace("'", "''"));
            }

            builder.Append("'");
        }
    }
}
