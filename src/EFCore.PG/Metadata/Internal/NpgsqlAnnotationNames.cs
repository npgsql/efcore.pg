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

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal
{
    public static class NpgsqlAnnotationNames
    {
        public const string Prefix = "Npgsql:";

        public const string ValueGenerationStrategy = Prefix + "ValueGenerationStrategy";
        public const string HiLoSequenceName = Prefix + "HiLoSequenceName";
        public const string HiLoSequenceSchema = Prefix + "HiLoSequenceSchema";
        public const string IndexMethod = Prefix + "IndexMethod";
        public const string PostgresExtensionPrefix = Prefix + "PostgresExtension:";
        public const string DatabaseTemplate = Prefix + "DatabaseTemplate";
        public const string Tablespace = Prefix + "Tablespace";
        public const string StorageParameterPrefix = Prefix + "StorageParameter:";
        public const string Comment = Prefix + "Comment";

        // Database model annotations

        /// <summary>
        /// Identifies the type of the PostgreSQL type of this column (e.g. array, range, base).
        /// </summary>
        public const string PostgresTypeType = Prefix + "PostgresTypeType";

        /// <summary>
        /// If this column's data type is an array, contains the data type of its elements.
        /// Otherwise null.
        /// </summary>
        public const string ElementDataType = Prefix + "ElementDataType";

        /// <summary>
        /// If the index contains an expression (rather than simple column references), the expression is contained here.
        /// This is currently unsupported and will be ignored.
        /// </summary>
        public const string IndexExpression = Prefix + "IndexExpression";

        [Obsolete("Replaced by ValueGenerationStrategy.SerialColumn")]
        public const string ValueGeneratedOnAdd = Prefix + "ValueGeneratedOnAdd";
    }
}
