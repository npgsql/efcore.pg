using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlEnumTypeMapping : RelationalTypeMapping
    {
        [NotNull] readonly ISqlGenerationHelper _sqlGenerationHelper;
        [NotNull] readonly INpgsqlNameTranslator _nameTranslator;

        /// <summary>
        /// Translates the CLR member value to the PostgreSQL value label.
        /// </summary>
        [NotNull] readonly Dictionary<object, string> _members;

        public NpgsqlEnumTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] string storeTypeSchema,
            [NotNull] Type enumType,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [CanBeNull] INpgsqlNameTranslator nameTranslator = null)
            : base(sqlGenerationHelper.DelimitIdentifier(storeType, storeTypeSchema), enumType)
        {
            if (!enumType.IsEnum || !enumType.IsValueType)
                throw new ArgumentException($"Enum type mappings require a CLR enum. {enumType.FullName} is not an enum.");

            if (nameTranslator == null)
                nameTranslator = NpgsqlConnection.GlobalTypeMapper.DefaultNameTranslator;

            _nameTranslator = nameTranslator;
            _sqlGenerationHelper = sqlGenerationHelper;
            _members = CreateValueMapping(enumType, nameTranslator);
        }

        protected NpgsqlEnumTypeMapping(
            RelationalTypeMappingParameters parameters,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] INpgsqlNameTranslator nameTranslator)
            : base(parameters)
        {
            _nameTranslator = nameTranslator;
            _sqlGenerationHelper = sqlGenerationHelper;
            _members = CreateValueMapping(parameters.CoreParameters.ClrType, nameTranslator);
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlEnumTypeMapping(parameters, _sqlGenerationHelper, _nameTranslator);

        protected override string GenerateNonNullSqlLiteral(object value) => $"'{_members[value]}'::{StoreType}";

        [NotNull]
        static Dictionary<object, string> CreateValueMapping([NotNull] Type enumType, [NotNull] INpgsqlNameTranslator nameTranslator)
            => enumType.GetFields(BindingFlags.Static | BindingFlags.Public)
                       .ToDictionary(
                           x => x.GetValue(null),
                           x => x.GetCustomAttribute<PgNameAttribute>()?.PgName ?? nameTranslator.TranslateMemberName(x.Name));
    }
}
