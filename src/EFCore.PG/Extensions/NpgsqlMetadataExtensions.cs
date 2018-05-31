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

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlMetadataExtensions
    {
        public static INpgsqlEntityTypeAnnotations Npgsql([NotNull] this IEntityType entityType)
            => new NpgsqlEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        public static NpgsqlEntityTypeAnnotations Npgsql([NotNull] this IMutableEntityType entityType)
            => (NpgsqlEntityTypeAnnotations)Npgsql((IEntityType)entityType);

        public static IRelationalForeignKeyAnnotations Npgsql([NotNull] this IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)));

        public static RelationalForeignKeyAnnotations Npgsql([NotNull] this IMutableForeignKey foreignKey)
            => (RelationalForeignKeyAnnotations)Npgsql((IForeignKey)foreignKey);

        public static INpgsqlIndexAnnotations Npgsql([NotNull] this IIndex index)
            => new NpgsqlIndexAnnotations(Check.NotNull(index, nameof(index)));

        public static NpgsqlIndexAnnotations Npgsql([NotNull] this IMutableIndex index)
            => (NpgsqlIndexAnnotations)Npgsql((IIndex)index);

        public static IRelationalKeyAnnotations Npgsql([NotNull] this IKey key)
            => new RelationalKeyAnnotations(Check.NotNull(key, nameof(key)));

        public static RelationalKeyAnnotations Npgsql([NotNull] this IMutableKey key)
            => (RelationalKeyAnnotations)Npgsql((IKey)key);

        public static INpgsqlModelAnnotations Npgsql([NotNull] this IModel model)
            => new NpgsqlModelAnnotations(Check.NotNull(model, nameof(model)));

        public static NpgsqlModelAnnotations Npgsql([NotNull] this IMutableModel model)
            => (NpgsqlModelAnnotations)Npgsql((IModel)model);

        public static INpgsqlPropertyAnnotations Npgsql([NotNull] this IProperty property)
            => new NpgsqlPropertyAnnotations(Check.NotNull(property, nameof(property)));

        public static NpgsqlPropertyAnnotations Npgsql([NotNull] this IMutableProperty property)
            => (NpgsqlPropertyAnnotations)Npgsql((IProperty)property);
    }
}
