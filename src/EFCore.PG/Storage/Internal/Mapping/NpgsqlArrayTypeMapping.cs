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

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public sealed class NpgsqlArrayTypeMapping : RelationalTypeMapping
    {
        public RelationalTypeMapping ElementMapping { get; }

        internal NpgsqlArrayTypeMapping(string storeType, RelationalTypeMapping elementMapping)
            : this(storeType, elementMapping, elementMapping.ClrType.MakeArrayType())
        {}

        internal NpgsqlArrayTypeMapping(RelationalTypeMapping elementMapping, Type arrayType)
            : this(elementMapping.StoreType + "[]", elementMapping, arrayType)
        {}

        NpgsqlArrayTypeMapping(string storeType, RelationalTypeMapping elementMapping, Type arrayType)
            : base(storeType, arrayType)
        {
            ElementMapping = elementMapping;
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlArrayTypeMapping(StoreType, ElementMapping);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            // Only support one-dimensional arrays (at least for now)
            var arr = (Array)value;

            if (arr.Rank != 1)
                throw new NotSupportedException("Multidimensional array literals aren't supported yet");

            var sb = new StringBuilder();
            sb.Append("ARRAY[");
            for (var i = 0; i < arr.Length; i++)
            {
                sb.Append(ElementMapping.GenerateSqlLiteral(arr.GetValue(i)));
                if (i < arr.Length - 1)
                    sb.Append(",");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
