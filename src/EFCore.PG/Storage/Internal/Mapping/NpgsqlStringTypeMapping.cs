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

using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Npgsql;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlStringTypeMapping : StringTypeMapping
    {
        readonly NpgsqlDbType _npgsqlDbType;

        public NpgsqlStringTypeMapping(
            [NotNull] string storeType,
            NpgsqlDbType npgsqlDbType,
            int? size = null)
            : this(storeType, null, npgsqlDbType, size)
        {}

        public NpgsqlStringTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            NpgsqlDbType npgsqlDbType,
            int? size = null)
            : base(storeType, converter, (DbType?)npgsqlDbType, false, size)
            => _npgsqlDbType = npgsqlDbType;

        public NpgsqlStringTypeMapping(string storeType, NpgsqlDbType npgsqlDbType)
            : base(storeType)
             => _npgsqlDbType = npgsqlDbType;

        protected override void ConfigureParameter([NotNull] DbParameter parameter)
            => ((NpgsqlParameter)parameter).NpgsqlDbType = (NpgsqlDbType)_npgsqlDbType;

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlStringTypeMapping(storeType, Converter, _npgsqlDbType, size);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlStringTypeMapping(StoreType, ComposeConverter(converter), _npgsqlDbType, Size);
    }
}
