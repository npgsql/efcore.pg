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
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlEnumTypeMapping : RelationalTypeMapping
    {
        [NotNull] static readonly NpgsqlSqlGenerationHelper SqlGenerationHelper =
            new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());

        [NotNull] readonly INpgsqlNameTranslator _nameTranslator;

        [CanBeNull] readonly string _storeTypeSchema;

        /// <summary>
        /// Translates the CLR member value to the PostgreSQL value label.
        /// </summary>
        [NotNull] readonly Dictionary<object, string> _members;

        [NotNull]
        public override string StoreType => SqlGenerationHelper.DelimitIdentifier(base.StoreType, _storeTypeSchema);

        public NpgsqlEnumTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] string storeTypeSchema,
            [NotNull] Type enumType,
            [CanBeNull] INpgsqlNameTranslator nameTranslator = null)
            : base(storeType, enumType)
        {
            if (!enumType.IsEnum || !enumType.IsValueType)
                throw new ArgumentException($"Enum type mappings require a CLR enum. {enumType.FullName} is not an enum.");

            if (nameTranslator == null)
                nameTranslator = NpgsqlConnection.GlobalTypeMapper.DefaultNameTranslator;

            _nameTranslator = nameTranslator;
            _storeTypeSchema = storeTypeSchema;
            _members = CreateValueMapping(enumType, nameTranslator);
        }

        protected NpgsqlEnumTypeMapping(
            RelationalTypeMappingParameters parameters,
            [CanBeNull] string storeTypeSchema,
            [NotNull] INpgsqlNameTranslator nameTranslator)
            : base(parameters)
        {
            _nameTranslator = nameTranslator;
            _storeTypeSchema = storeTypeSchema;
            _members = CreateValueMapping(parameters.CoreParameters.ClrType, nameTranslator);
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlEnumTypeMapping(parameters, _storeTypeSchema, _nameTranslator);

        protected override string GenerateNonNullSqlLiteral(object value) => $"'{_members[value]}'::{StoreType}";

        [NotNull]
        static Dictionary<object, string> CreateValueMapping([NotNull] Type enumType, [NotNull] INpgsqlNameTranslator nameTranslator)
            => Enum.GetValues(enumType)
                   .Cast<object>()
                   .ToDictionary(x => x, x => nameTranslator.TranslateMemberName(Enum.GetName(enumType, x)));
    }
}
