using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlEnumTypeMapping(storeType, ClrType, _nameTranslator);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlEnumTypeMapping(Parameters.WithComposedConverter(converter), _nameTranslator);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"'{_nameTranslator.TranslateMemberName(value.ToString())}'::{StoreType}";
    }
}
