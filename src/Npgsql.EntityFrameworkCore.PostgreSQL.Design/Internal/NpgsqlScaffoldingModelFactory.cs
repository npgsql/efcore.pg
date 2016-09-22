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
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class NpgsqlScaffoldingModelFactory : RelationalScaffoldingModelFactory
    {
        public NpgsqlScaffoldingModelFactory(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IDatabaseModelFactory databaseModelFactory,
            [NotNull] CandidateNamingService candidateNamingService)
            : base(loggerFactory, typeMapper, databaseModelFactory, candidateNamingService)
        {
        }

        public override IModel Create(string connectionString, [CanBeNull] TableSelectionSet tableSelectionSet)
        {
            var model = base.Create(connectionString, tableSelectionSet);
            model.Scaffolding().UseProviderMethodName = nameof(NpgsqlDbContextOptionsExtensions.UseNpgsql);
            return model;
        }

        [CanBeNull]
        protected override KeyBuilder VisitPrimaryKey(EntityTypeBuilder builder, TableModel table)
        {
            var keyBuilder = base.VisitPrimaryKey(builder, table);

            if (keyBuilder == null)
            {
                return null;
            }

            // If this property is the single integer primary key on the EntityType then
            // KeyConvention assumes ValueGeneratedOnAdd(). If the underlying column does
            // not have Serial set then we need to set to ValueGeneratedNever() to
            // override this behavior.

            // TODO use KeyConvention directly to detect when it will be applied
            var pkColumns = table.Columns.Where(c => c.PrimaryKeyOrdinal.HasValue).ToList();
            if (pkColumns.Count != 1 || pkColumns[0].Npgsql().IsSerial)
            {
                return keyBuilder;
            }

            // TODO 
            var property = builder.Metadata.FindProperty(GetPropertyName(pkColumns[0]));
            var propertyType = property?.ClrType?.UnwrapNullableType();

            if ((propertyType?.IsIntegerForSerial() == true || propertyType == typeof(Guid)) &&
                !pkColumns[0].ValueGenerated.HasValue)
            {
                property.ValueGenerated = ValueGenerated.Never;
            }

            return keyBuilder;
        }

        [CanBeNull]
        protected override IndexBuilder VisitIndex(EntityTypeBuilder builder, IndexModel index)
        {
            var expression = index.Npgsql().Expression;
            if (expression != null)
            {
                Logger.LogWarning($"Ignoring unsupported index {index.Name} which contains an expression ({expression})");
                return null;
            }

            return base.VisitIndex(builder, index);
        }

        [CanBeNull]
        protected override RelationalTypeMapping GetTypeMapping(ColumnModel column)
        {
            Check.NotNull(column, nameof(column));

            var postgresType = column.Npgsql().PostgresTypeType;
            switch (postgresType)
            {
            case PostgresTypeType.Base:
                return base.GetTypeMapping(column);
            case PostgresTypeType.Array:
                return GetArrayTypeMapping(column);
            case PostgresTypeType.Range:
            case PostgresTypeType.Enum:
            case PostgresTypeType.Composite:
            default:
                Logger.LogWarning($"Can't scaffold PostgreSQL {postgresType} for column '{column.Name}' of type '{column.DataType}'");
                return null;
            }
        }

        NpgsqlArrayTypeMapping GetArrayTypeMapping(ColumnModel column)
        {
            Debug.Assert(column.Npgsql().ElementDataType != null);
            var elementMapping = TypeMapper.FindMapping(column.Npgsql().ElementDataType);

            var elementClrType = elementMapping?.ClrType;
            if (elementClrType == null)
            {
                Logger.LogWarning($"Could not find type mapping for array column '{column.Name}' with data type '{column.DataType}' - array of unknown element type {column.Npgsql().ElementDataType}. Skipping column.");
                return null;
            }

            return (NpgsqlArrayTypeMapping)TypeMapper.FindMapping(elementClrType.MakeArrayType());
        }
    }
}
