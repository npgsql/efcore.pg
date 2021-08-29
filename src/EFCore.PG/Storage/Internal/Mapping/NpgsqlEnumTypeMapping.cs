using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlEnumTypeMapping : RelationalTypeMapping
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private readonly INpgsqlNameTranslator _nameTranslator;

        /// <summary>
        /// Translates the CLR member value to the PostgreSQL value label.
        /// </summary>
        private readonly Dictionary<object, string> _members;

        public NpgsqlEnumTypeMapping(
            string storeType,
            string? storeTypeSchema,
            Type enumType,
            ISqlGenerationHelper sqlGenerationHelper,
            INpgsqlNameTranslator? nameTranslator = null)
            : base(sqlGenerationHelper.DelimitIdentifier(storeType, storeTypeSchema), enumType)
        {
            if (!enumType.IsEnum || !enumType.IsValueType)
                throw new ArgumentException($"Enum type mappings require a CLR enum. {enumType.FullName} is not an enum.");

            nameTranslator ??= NpgsqlConnection.GlobalTypeMapper.DefaultNameTranslator;

            _nameTranslator = nameTranslator;
            _sqlGenerationHelper = sqlGenerationHelper;
            _members = CreateValueMapping(enumType, nameTranslator);
        }

        protected NpgsqlEnumTypeMapping(
            RelationalTypeMappingParameters parameters,
            ISqlGenerationHelper sqlGenerationHelper,
            INpgsqlNameTranslator nameTranslator)
            : base(parameters)
        {
            _nameTranslator = nameTranslator;
            _sqlGenerationHelper = sqlGenerationHelper;
            _members = CreateValueMapping(parameters.CoreParameters.ClrType, nameTranslator);
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlEnumTypeMapping(parameters, _sqlGenerationHelper, _nameTranslator);

        protected override string GenerateNonNullSqlLiteral(object value) => $"'{_members[value]}'::{StoreType}";

        private static Dictionary<object, string> CreateValueMapping(Type enumType, INpgsqlNameTranslator nameTranslator)
            => enumType.GetFields(BindingFlags.Static | BindingFlags.Public)
                .ToDictionary(
                    x => x.GetValue(null)!,
                    x => x.GetCustomAttribute<PgNameAttribute>()?.PgName ?? nameTranslator.TranslateMemberName(x.Name));
    }
}
