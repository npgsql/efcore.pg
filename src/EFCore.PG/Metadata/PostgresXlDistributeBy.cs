// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class PostgresXlDistributeBy
    {
        private const string AnnotationName = PostgresXlDistributeByAnnotationNames.DistributeBy;

        readonly IAnnotatable _annotatable;

        public virtual Annotatable Annotatable
            => (Annotatable)_annotatable;


        public PostgresXlDistributeBy(IAnnotatable annotatable)
            => _annotatable = annotatable;

        public virtual PostgresXlDistributeByStrategy DistributionStrategy
        {
            get => GetData().DistributionStrategy;
            [param: NotNull] set
            {
                (_, var distributeByColumnFunction, var distributionStyle, var columnName) = GetData();
                SetData(value, distributeByColumnFunction, distributionStyle, columnName);
            }
        }

        public virtual PostgresXlDistributeByColumnFunction DistributeByColumnFunction
        {
            get => GetData().DistributeByColumnFunction;
            [param: NotNull] set
            {
                (var distributionStrategy, _, var distributionStyle, var columnName) = GetData();
                SetData(distributionStrategy, value, distributionStyle, columnName);
            }
        }

        public virtual PostgresXlDistributionStyle DistributionStyle
        {
            get => GetData().DistributionStyle;
            [param: NotNull] set
            {
                (var distributionStrategy, var distributeByColumnFunction, _, var columnName) = GetData();
                SetData(distributionStrategy, distributeByColumnFunction, value, columnName);
            }
        }

        public virtual string DistributeByColumnName
        {
            get => GetData().ColumnName;
            [param: NotNull] set
            {
                (var distributionStrategy, var distributeByColumnFunction, var distributionStyle, _) = GetData();
                SetData(distributionStrategy, distributeByColumnFunction, distributionStyle, value);
            }
        }

        private (PostgresXlDistributeByStrategy DistributionStrategy, PostgresXlDistributeByColumnFunction DistributeByColumnFunction, PostgresXlDistributionStyle DistributionStyle, string ColumnName) GetData()
        {
            var str = Annotatable[AnnotationName] as string;
            return str == null
                ? (0, 0, 0, null)
                : Deserialize(str);
        }

        private (PostgresXlDistributeByStrategy DistributionStrategy, PostgresXlDistributeByColumnFunction DistributeByColumnFunction, PostgresXlDistributionStyle DistributionStyle, string ColumnName) Deserialize(string str)
        {
            throw new System.NotImplementedException();
        }

        private void SetData(
            PostgresXlDistributeByStrategy distributionStrategy,
            PostgresXlDistributeByColumnFunction distributeByColumnFunction,
            PostgresXlDistributionStyle postgresXlDistributionStyle,
            string distributeByColumnName)
        {
            Annotatable[AnnotationName] = Serialize(distributionStrategy, distributeByColumnFunction, distributeByColumnName);
        }

        private string Serialize(PostgresXlDistributeByStrategy distributionStrategy, PostgresXlDistributeByColumnFunction distributeByColumnFunction, string distributeByColumnName)
        {
            throw new System.NotImplementedException();
        }
    }
}
