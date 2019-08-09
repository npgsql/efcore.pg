using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal
{
    public class IdentitySequenceOptionsData : IEquatable<IdentitySequenceOptionsData>
    {
        public static readonly IdentitySequenceOptionsData Empty = new IdentitySequenceOptionsData();

        public long? StartValue { get; set; }
        public long IncrementBy { get; set; } = 1;
        public long? MinValue { get; set; }
        public long? MaxValue { get; set; }
        public bool IsCyclic { get; set; }
        public long NumbersToCache { get; set; } = 1;

        public string Serialize()
        {
            var builder = new StringBuilder();

            EscapeAndQuote(builder, StartValue);
            builder.Append(", ");
            EscapeAndQuote(builder, IncrementBy);
            builder.Append(", ");
            EscapeAndQuote(builder, MinValue);
            builder.Append(", ");
            EscapeAndQuote(builder, MaxValue);
            builder.Append(", ");
            EscapeAndQuote(builder, IsCyclic);
            builder.Append(", ");
            EscapeAndQuote(builder, NumbersToCache);

            return builder.ToString();
        }

        public static IdentitySequenceOptionsData Get([NotNull] IAnnotatable annotatable)
            => Deserialize((string)annotatable[NpgsqlAnnotationNames.IdentityOptions]);

        public static IdentitySequenceOptionsData Deserialize([CanBeNull] string value)
        {
            var data = new IdentitySequenceOptionsData();

            if (value == null)
                return data;

            try
            {
                // ReSharper disable PossibleInvalidOperationException
                var position = 0;
                data.StartValue = AsLong(ExtractValue(value, ref position));
                data.IncrementBy = (int)AsLong(ExtractValue(value, ref position));
                data.MinValue = AsLong(ExtractValue(value, ref position));
                data.MaxValue = AsLong(ExtractValue(value, ref position));
                data.IsCyclic = AsBool(ExtractValue(value, ref position));
                data.NumbersToCache = (int)AsLong(ExtractValue(value, ref position));
                // ReSharper restore PossibleInvalidOperationException

                return data;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(RelationalStrings.BadSequenceString, ex);
            }
        }

        static string ExtractValue(string value, ref int position)
        {
            position = value.IndexOf('\'', position) + 1;

            var end = value.IndexOf('\'', position);

            while (end + 1 < value.Length
                   && value[end + 1] == '\'')
            {
                end = value.IndexOf('\'', end + 2);
            }

            var extracted = value[position..end].Replace("''", "'");
            position = end + 1;

            return extracted.Length == 0 ? null : extracted;
        }

        static long? AsLong(string value)
            => value == null ? null : (long?)long.Parse(value, CultureInfo.InvariantCulture);

        static bool AsBool(string value)
            => value != null && bool.Parse(value);

        static void EscapeAndQuote(StringBuilder builder, object value)
        {
            builder.Append("'");

            if (value != null)
            {
                builder.Append(value.ToString().Replace("'", "''"));
            }

            builder.Append("'");
        }

        public bool Equals(IdentitySequenceOptionsData other)
            => !(other is null) && (
                   ReferenceEquals(this, other) ||
                   StartValue == other.StartValue &&
                   IncrementBy == other.IncrementBy &&
                   MinValue == other.MinValue &&
                   MaxValue == other.MaxValue &&
                   IsCyclic == other.IsCyclic &&
                   NumbersToCache == other.NumbersToCache
               );

        public override bool Equals(object obj)
            => obj is IdentitySequenceOptionsData other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(StartValue, IncrementBy, MinValue, MaxValue, IsCyclic, NumbersToCache);
    }
}
