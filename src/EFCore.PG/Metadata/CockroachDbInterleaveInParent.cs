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
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class CockroachDbInterleaveInParent
    {
        const string AnnotationName = CockroachDbAnnotationNames.InterleaveInParent;

        readonly IAnnotatable _annotatable;

        public Annotatable Annotatable => (Annotatable)_annotatable;

        public CockroachDbInterleaveInParent(IAnnotatable annotatable)
        {
            _annotatable = annotatable;
        }

        public string ParentTableSchema
        {
            get => GetData().ParentTableSchema;
            set
            {
                (var _, var parentTableName, var interleavePrefix) = GetData();
                SetData(value, parentTableName, interleavePrefix);
            }
        }

        public string ParentTableName
        {
            get => GetData().ParentTableName;
            set
            {
                (var parentTableSchema, var _, var interleavePrefix) = GetData();
                SetData(parentTableSchema, value, interleavePrefix);
            }
        }

        public List<string> InterleavePrefix
        {
            get => GetData().InterleavePrefix;
            set
            {
                (var parentTableSchema, var parentTableName, var _) = GetData();
                SetData(parentTableSchema, parentTableName, value);
            }
        }

        (string ParentTableSchema, string ParentTableName, List<string> InterleavePrefix) GetData()
        {
            var str = Annotatable[AnnotationName] as string;
            return str == null
                ? (null, null, null)
                : Deserialize(str);
        }

        void SetData(string parentTableSchema, string parentTableName, List<string> interleavePrefix)
            => Annotatable[AnnotationName] = Serialize(parentTableSchema, parentTableName, interleavePrefix);

        static string Serialize(string parentTableSchema, string parentTableName, List<string> interleavePrefix)
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
                        builder.Append(", ");
                }
            }
            return builder.ToString();
        }

        internal static (string ParentTableSchema, string ParentTableName, List<string> InterleavePrefix) Deserialize([NotNull] string value)
        {
            Check.NotEmpty(value, nameof(value));

            try
            {
                var position = 0;
                var parentTableSchema = ExtractValue(value, ref position);
                var parentTableName = ExtractValue(value, ref position);
                var interleavePrefix = new List<string>();
                while (position < value.Length - 1)
                    interleavePrefix.Add(ExtractValue(value, ref position));
                return (parentTableSchema, parentTableName, interleavePrefix);
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
