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
            => enumType.GetFields(BindingFlags.Static | BindingFlags.Public)
                       .ToDictionary(
                           x => x.GetValue(null),
                           x => x.GetCustomAttribute<PgNameAttribute>()?.PgName ?? nameTranslator.TranslateMemberName(x.Name));
    }
}
