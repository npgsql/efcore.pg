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

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage
{
    public class NpgsqlStringRelationalTypeMapper : IStringRelationalTypeMapper
    {
        private readonly ConcurrentDictionary<int, RelationalTypeMapping> _boundedAnsiMappings
            = new ConcurrentDictionary<int, RelationalTypeMapping>();

        private static readonly RelationalTypeMapping _defaultStringMapping
            = new NpgsqlStringTypeMapping("text", NpgsqlDbType.Text);

        public RelationalTypeMapping FindMapping(bool unicode, bool keyOrIndex, int? maxLength)
            => maxLength.HasValue
                ? _boundedAnsiMappings.GetOrAdd(maxLength.Value,
                      ml => new NpgsqlStringTypeMapping($"varchar({maxLength})", NpgsqlDbType.Text)
                  )
                : _defaultStringMapping;
    }
}
