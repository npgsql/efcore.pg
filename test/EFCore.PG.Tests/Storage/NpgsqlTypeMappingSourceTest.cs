using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using NpgsqlTypes;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage
{
    public class NpgsqlTypeMappingSourceTest
    {
        [Theory]
        [InlineData("integer", typeof(int))]
        [InlineData("integer[]", typeof(int[]))]
        [InlineData("dummy", typeof(DummyType))]
        [InlineData("int4range", typeof(NpgsqlRange<int>))]
        [InlineData("floatrange", typeof(NpgsqlRange<float>))]
        [InlineData("dummyrange", typeof(NpgsqlRange<DummyType>))]
        public void By_StoreType(string storeType, Type expectedClrType)
            => Assert.Same(expectedClrType, Source.FindMapping(storeType).ClrType);

        [Theory]
        [InlineData(typeof(int), "integer")]
        [InlineData(typeof(int[]), "integer[]")]
        [InlineData(typeof(DummyType), "dummy")]
        [InlineData(typeof(NpgsqlRange<int>), "int4range")]
        [InlineData(typeof(NpgsqlRange<float>), "floatrange")]
        [InlineData(typeof(NpgsqlRange<DummyType>), "dummyrange")]
        public void By_ClrType(Type clrType, string expectedStoreType)
            => Assert.Equal(expectedStoreType, ((RelationalTypeMapping)Source.FindMapping(clrType)).StoreType);

        [Theory]
        [InlineData("integer", typeof(int))]
        [InlineData("integer[]", typeof(int[]))]
        [InlineData("dummy", typeof(DummyType))]
        [InlineData("int4range", typeof(NpgsqlRange<int>))]
        [InlineData("floatrange", typeof(NpgsqlRange<float>))]
        [InlineData("dummyrange", typeof(NpgsqlRange<DummyType>))]
        public void By_StoreType_with_ClrType(string storeType, Type clrType)
            => Assert.Equal(storeType, Source.FindMapping(clrType, storeType).StoreType);

        [Theory]
        [InlineData("integer", typeof(UnknownType))]
        //[InlineData("integer[]", typeof(UnknownType))] TODO Implement
        [InlineData("dummy", typeof(UnknownType))]
        [InlineData("int4range", typeof(UnknownType))]
        [InlineData("floatrange", typeof(UnknownType))]
        [InlineData("dummyrange", typeof(UnknownType))]
        public void By_StoreType_with_wrong_ClrType(string storeType, Type wrongClrType)
            => Assert.Null(Source.FindMapping(wrongClrType, storeType));

        // Happens when using domain/aliases: we don't know about the domain but continue with the mapping based on the ClrType
        [Fact]
        public void Unknown_StoreType_with_known_ClrType()
            => Assert.Equal("integer", Source.FindMapping(typeof(int), "some_domain").StoreType);

        #region Support

        public NpgsqlTypeMappingSourceTest()
        {
            var builder = new DbContextOptionsBuilder();
            new NpgsqlDbContextOptionsBuilder(builder).MapRange<float>("floatrange");
            new NpgsqlDbContextOptionsBuilder(builder).MapRange<DummyType>("dummyrange", subtypeName: "dummy");
            var options = new NpgsqlOptions();
            options.Initialize(builder.Options);

            Source = new NpgsqlTypeMappingSource(
                new TypeMappingSourceDependencies(
                    new ValueConverterSelector(new ValueConverterSelectorDependencies()),
                    Array.Empty<ITypeMappingSourcePlugin>()),
                new RelationalTypeMappingSourceDependencies(
                    new[] { new DummyTypeMappingSourcePlugin() }),
                new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                options);
        }

        NpgsqlTypeMappingSource Source { get; }

        class DummyTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
        {
            public RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
                => mappingInfo.StoreTypeName != null
                   ? mappingInfo.StoreTypeName == "dummy" && (mappingInfo.ClrType == null || mappingInfo.ClrType == typeof(DummyType))
                       ? _dummyMapping
                       : null
                   : mappingInfo.ClrType == typeof(DummyType)
                       ? _dummyMapping
                       : null;

            DummyMapping _dummyMapping = new DummyMapping();

            class DummyMapping : RelationalTypeMapping
            {
                // TODO: The DbType is a hack, we currently require of range subtype mapping that they other expose an NpgsqlDbType
                // or a DbType (from which NpgsqlDbType is computed), since RangeTypeMapping sends an NpgsqlDbType.
                // This means we currently don't support ranges over types without NpgsqlDbType, which are accessible via
                // NpgsqlParameter.DataTypeName
                public DummyMapping() : base("dummy", typeof(DummyType), System.Data.DbType.Guid) {}
            }
        }

        class DummyType {}

        class UnknownType {}

        #endregion Support
    }
}
