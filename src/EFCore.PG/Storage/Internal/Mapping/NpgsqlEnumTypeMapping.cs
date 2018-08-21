using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlEnumTypeMapping : RelationalTypeMapping
    {
        readonly NpgsqlSqlGenerationHelper _sqlGenerationHelper;
        readonly INpgsqlNameTranslator _nameTranslator;

        public NpgsqlEnumTypeMapping(string storeType, Type enumType, INpgsqlNameTranslator nameTranslator)
            : base(storeType, enumType)
        {
            _nameTranslator = nameTranslator;
            _sqlGenerationHelper = new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
        }

        protected NpgsqlEnumTypeMapping(RelationalTypeMappingParameters parameters, INpgsqlNameTranslator nameTranslator)
            : base(parameters)
        {
            _nameTranslator = nameTranslator;
            _sqlGenerationHelper = new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlEnumTypeMapping(parameters, _nameTranslator);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"'{_nameTranslator.TranslateMemberName(value.ToString())}'::{_sqlGenerationHelper.DelimitIdentifier(StoreType)}";
    }
}
