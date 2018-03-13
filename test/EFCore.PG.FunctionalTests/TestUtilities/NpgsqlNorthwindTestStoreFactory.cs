using System;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class NpgsqlNorthwindTestStoreFactory : NpgsqlTestStoreFactory
    {
        public const string Name = "Northwind";
        public static readonly string NorthwindConnectionString = NpgsqlTestStore.CreateConnectionString(Name);
        public new static NpgsqlNorthwindTestStoreFactory Instance { get; } = new NpgsqlNorthwindTestStoreFactory();

        protected NpgsqlNorthwindTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => NpgsqlTestStore.GetOrCreate(Name, "Northwind.sql");
    }
}
