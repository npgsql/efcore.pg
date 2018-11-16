using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlCompositeTypeMapping : RelationalTypeMapping
    {
        [NotNull] static readonly NpgsqlSqlGenerationHelper SqlGenerationHelper =
            new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());

        [CanBeNull] readonly string _storeTypeSchema;

        [NotNull] public INpgsqlNameTranslator NameTranslator { get; }

        [NotNull]
        public override string StoreType => SqlGenerationHelper.DelimitIdentifier(base.StoreType, _storeTypeSchema);

        public NpgsqlCompositeTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] string storeTypeSchema,
            [NotNull] Type clrType,
            [CanBeNull] INpgsqlNameTranslator nameTranslator = null)
            : base(storeType, clrType)
        {
            if (nameTranslator == null)
                nameTranslator = NpgsqlConnection.GlobalTypeMapper.DefaultNameTranslator;

            NameTranslator = nameTranslator;
            _storeTypeSchema = storeTypeSchema;
            //_members = CreateValueMapping(enumType, nameTranslator);
        }

        protected NpgsqlCompositeTypeMapping(
            RelationalTypeMappingParameters parameters,
            [CanBeNull] string storeTypeSchema,
            [NotNull] INpgsqlNameTranslator nameTranslator)
            : base(parameters)
        {
            NameTranslator = nameTranslator;
            _storeTypeSchema = storeTypeSchema;
            //_members = CreateValueMapping(parameters.CoreParameters.ClrType, nameTranslator);
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlCompositeTypeMapping(parameters, _storeTypeSchema, NameTranslator);

        // TODO: Requires PostgreSQL composite fields definition from the ADO layer, which we don't have.
        // This is currently problematic since the EF Core mapping is constructed based on the ADO global mapping,
        // but composite fields are only loaded once a specific connection is made to a database.
        // protected override string GenerateNonNullSqlLiteral(object value) => $"'{_members[value]}'::{StoreType}";

        // TODO: Look for a constructor on ClrType that has parameters matching in name and type all the
        // type's members? Or should we make use of the PostgreSQL composite fields definition from
        // the ADO layer, once we get it here somehow (necessary anyway for SQL generation)?
        // public override Expression GenerateCodeLiteral(object value) {}
    }
}
