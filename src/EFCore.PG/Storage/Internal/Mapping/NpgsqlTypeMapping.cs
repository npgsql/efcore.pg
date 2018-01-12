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
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Npgsql;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlTypeMapping : RelationalTypeMapping
    {
        public NpgsqlDbType? NpgsqlDbType { get; protected set; }

        internal NpgsqlTypeMapping([NotNull] string storeType, [NotNull] Type clrType, NpgsqlDbType? npgsqlDbType = null)
            : base(storeType, clrType, unicode: false, size: null, dbType: null)
        {
            NpgsqlDbType = npgsqlDbType;
        }

        /// <param name="dbType"> The <see cref="DbType" /> to be used. </param>
        public NpgsqlTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            NpgsqlDbType? dbType = null)
            : base(storeType, typeof(object))
        {
            NpgsqlDbType = dbType;
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlTypeMapping(storeType, Converter, NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlTypeMapping(StoreType, ComposeConverter(converter), NpgsqlDbType);

        protected override void ConfigureParameter([NotNull] DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            if (NpgsqlDbType.HasValue)
                ((NpgsqlParameter)parameter).NpgsqlDbType = NpgsqlDbType.Value;
        }
    }
}
