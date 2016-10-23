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

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class NpgsqlFullAnnotationNames : RelationalFullAnnotationNames
    {
        protected NpgsqlFullAnnotationNames(string prefix)
            : base(prefix)
        {
            ValueGenerationStrategy = prefix + NpgsqlAnnotationNames.ValueGenerationStrategy;
            HiLoSequenceName = prefix + NpgsqlAnnotationNames.HiLoSequenceName;
            HiLoSequenceSchema = prefix + NpgsqlAnnotationNames.HiLoSequenceSchema;
            IndexMethod = prefix + NpgsqlAnnotationNames.IndexMethod;
            PostgresExtensionPrefix = prefix + NpgsqlAnnotationNames.PostgresExtensionPrefix;
            DatabaseTemplate = prefix + NpgsqlAnnotationNames.DatabaseTemplate;
#pragma warning disable 618
            ValueGeneratedOnAdd = prefix + NpgsqlAnnotationNames.ValueGeneratedOnAdd;
#pragma warning restore 618
        }

        public new static NpgsqlFullAnnotationNames Instance { get; } = new NpgsqlFullAnnotationNames(NpgsqlAnnotationNames.Prefix);

        public readonly string ValueGenerationStrategy;
        public readonly string HiLoSequenceName;
        public readonly string HiLoSequenceSchema;
        public readonly string IndexMethod;
        public readonly string PostgresExtensionPrefix;
        public readonly string DatabaseTemplate;

        [Obsolete("Replaced by ValueGenerationStrategy.SerialColumn")]
        public readonly string ValueGeneratedOnAdd;
    }
}
