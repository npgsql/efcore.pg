// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class InheritanceRelationshipsQueryNpgsqlTest : InheritanceRelationshipsQueryTestBase<NpgsqlTestStore, InheritanceRelationshipsQueryNpgsqlFixture>
    {
        public InheritanceRelationshipsQueryNpgsqlTest(InheritanceRelationshipsQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public override void Include_reference_with_inheritance1()
        {
            base.Include_reference_with_inheritance1();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseReferenceOnBase"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) AS ""b"" ON ""b"".""BaseParentId"" = ""e"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')",
                Sql);
        }

        public override void Include_reference_with_inheritance2()
        {
            base.Include_reference_with_inheritance2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_reference_with_inheritance_reverse()
        {
            base.Include_reference_with_inheritance_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""BaseParentId"", ""e"".""Discriminator"", ""e"".""Name"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""BaseReferenceOnBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b"" ON ""e"".""BaseParentId"" = ""b"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')",
                Sql);
        }

        public override void Include_reference_with_inheritance_with_filter1()
        {
            base.Include_reference_with_inheritance_with_filter1();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseReferenceOnBase"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) AS ""b"" ON ""b"".""BaseParentId"" = ""e"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND ((""e"".""Name"" <> 'Bar') OR ""e"".""Name"" IS NULL)",
                Sql);
        }

        public override void Include_reference_with_inheritance_with_filter2()
        {
            base.Include_reference_with_inheritance_with_filter2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_reference_with_inheritance_with_filter_reverse()
        {
            base.Include_reference_with_inheritance_with_filter_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""BaseParentId"", ""e"".""Discriminator"", ""e"".""Name"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""BaseReferenceOnBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b"" ON ""e"".""BaseParentId"" = ""b"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase') AND ((""e"".""Name"" <> 'Bar') OR ""e"".""Name"" IS NULL)",
                Sql);
        }

        public override void Include_reference_without_inheritance()
        {
            base.Include_reference_without_inheritance();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""r"".""Id"", ""r"".""Name"", ""r"".""ParentId""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN ""ReferenceOnBase"" AS ""r"" ON ""r"".""ParentId"" = ""e"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')",
                Sql);
        }

        public override void Include_reference_without_inheritance_reverse()
        {
            base.Include_reference_without_inheritance_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Name"", ""e"".""ParentId"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""ReferenceOnBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b"" ON ""e"".""ParentId"" = ""b"".""Id""",
                Sql);
        }

        public override void Include_reference_without_inheritance_with_filter()
        {
            base.Include_reference_without_inheritance_with_filter();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""r"".""Id"", ""r"".""Name"", ""r"".""ParentId""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN ""ReferenceOnBase"" AS ""r"" ON ""r"".""ParentId"" = ""e"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND ((""e"".""Name"" <> 'Bar') OR ""e"".""Name"" IS NULL)",
                Sql);
        }

        public override void Include_reference_without_inheritance_with_filter_reverse()
        {
            base.Include_reference_without_inheritance_with_filter_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Name"", ""e"".""ParentId"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""ReferenceOnBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b"" ON ""e"".""ParentId"" = ""b"".""Id""
WHERE (""e"".""Name"" <> 'Bar') OR ""e"".""Name"" IS NULL",
                Sql);
        }

        public override void Include_collection_with_inheritance1()
        {
            base.Include_collection_with_inheritance1();

            // TODO: Assert on SQL
        }

        public override void Include_collection_with_inheritance2()
        {
            base.Include_collection_with_inheritance2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_collection_with_inheritance_reverse()
        {
            base.Include_collection_with_inheritance_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""BaseParentId"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""DerivedProperty"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""BaseCollectionOnBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b"" ON ""e"".""BaseParentId"" = ""b"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase')",
                Sql);
        }

        public override void Include_collection_with_inheritance_with_filter1()
        {
            base.Include_collection_with_inheritance_with_filter1();

            // TODO: Assert on SQL
        }

        public override void Include_collection_with_inheritance_with_filter2()
        {
            base.Include_collection_with_inheritance_with_filter2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_collection_with_inheritance_with_filter_reverse()
        {
            base.Include_collection_with_inheritance_with_filter_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""BaseParentId"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""DerivedProperty"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""BaseCollectionOnBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b"" ON ""e"".""BaseParentId"" = ""b"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase') AND ((""e"".""Name"" <> 'Bar') OR ""e"".""Name"" IS NULL)",
                Sql);
        }

        public override void Include_collection_without_inheritance()
        {
            base.Include_collection_without_inheritance();

            // TODO: Assert on SQL
        }

        public override void Include_collection_without_inheritance_reverse()
        {
            base.Include_collection_without_inheritance_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Name"", ""e"".""ParentId"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""CollectionOnBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b"" ON ""e"".""ParentId"" = ""b"".""Id""",
                Sql);
        }

        public override void Include_collection_without_inheritance_with_filter()
        {
            base.Include_collection_without_inheritance_with_filter();

            // TODO: Assert on SQL
        }

        public override void Include_collection_without_inheritance_with_filter_reverse()
        {
            base.Include_collection_without_inheritance_with_filter_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Name"", ""e"".""ParentId"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""CollectionOnBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b"" ON ""e"".""ParentId"" = ""b"".""Id""
WHERE (""e"".""Name"" <> 'Bar') OR ""e"".""Name"" IS NULL",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived1()
        {
            base.Include_reference_with_inheritance_on_derived1();

            // TODO: Assert on SQL
        }

        public override void Include_reference_with_inheritance_on_derived2()
        {
            base.Include_reference_with_inheritance_on_derived2();

            // TODO: Assert on SQL
        }

        public override void Include_reference_with_inheritance_on_derived3()
        {
            base.Include_reference_with_inheritance_on_derived3();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived4()
        {
            base.Include_reference_with_inheritance_on_derived4();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""DerivedInheritanceRelationshipEntityId""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseReferenceOnDerived"" AS ""b""
    WHERE ""b"".""Discriminator"" = 'DerivedReferenceOnDerived'
) AS ""b"" ON ""b"".""DerivedInheritanceRelationshipEntityId"" = ""e"".""Id""
WHERE ""e"".""Discriminator"" = 'DerivedInheritanceRelationshipEntity'",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_reverse()
        {
            base.Include_reference_with_inheritance_on_derived_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""BaseParentId"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""DerivedInheritanceRelationshipEntityId"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""BaseReferenceOnDerived"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" = 'DerivedInheritanceRelationshipEntity'
) AS ""b"" ON ""e"".""BaseParentId"" = ""b"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedReferenceOnDerived', 'BaseReferenceOnDerived')",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter1()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter1();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseReferenceOnBase"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) AS ""b"" ON ""b"".""BaseParentId"" = ""e"".""Id""
WHERE (""e"".""Discriminator"" = 'DerivedInheritanceRelationshipEntity') AND ((""e"".""Name"" <> 'Bar') OR ""e"".""Name"" IS NULL)",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter2()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter2();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""DerivedInheritanceRelationshipEntityId""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseReferenceOnDerived"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedReferenceOnDerived', 'BaseReferenceOnDerived')
) AS ""b"" ON ""b"".""BaseParentId"" = ""e"".""Id""
WHERE (""e"".""Discriminator"" = 'DerivedInheritanceRelationshipEntity') AND ((""e"".""Name"" <> 'Bar') OR ""e"".""Name"" IS NULL)",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter3()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter3();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter4()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter4();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""DerivedInheritanceRelationshipEntityId""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseReferenceOnDerived"" AS ""b""
    WHERE ""b"".""Discriminator"" = 'DerivedReferenceOnDerived'
) AS ""b"" ON ""b"".""DerivedInheritanceRelationshipEntityId"" = ""e"".""Id""
WHERE (""e"".""Discriminator"" = 'DerivedInheritanceRelationshipEntity') AND ((""e"".""Name"" <> 'Bar') OR ""e"".""Name"" IS NULL)",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter_reverse()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""BaseParentId"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""DerivedInheritanceRelationshipEntityId"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""BaseReferenceOnDerived"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" = 'DerivedInheritanceRelationshipEntity'
) AS ""b"" ON ""e"".""BaseParentId"" = ""b"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedReferenceOnDerived', 'BaseReferenceOnDerived') AND ((""e"".""Name"" <> 'Bar') OR ""e"".""Name"" IS NULL)",
                Sql);
        }

        public override void Include_reference_without_inheritance_on_derived1()
        {
            base.Include_reference_without_inheritance_on_derived1();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""r"".""Id"", ""r"".""Name"", ""r"".""ParentId""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN ""ReferenceOnBase"" AS ""r"" ON ""r"".""ParentId"" = ""e"".""Id""
WHERE ""e"".""Discriminator"" = 'DerivedInheritanceRelationshipEntity'",
                Sql);
        }

        public override void Include_reference_without_inheritance_on_derived2()
        {
            base.Include_reference_without_inheritance_on_derived2();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""r"".""Id"", ""r"".""Name"", ""r"".""ParentId""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN ""ReferenceOnDerived"" AS ""r"" ON ""r"".""ParentId"" = ""e"".""Id""
WHERE ""e"".""Discriminator"" = 'DerivedInheritanceRelationshipEntity'",
                Sql);
        }

        public override void Include_reference_without_inheritance_on_derived_reverse()
        {
            base.Include_reference_without_inheritance_on_derived_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Name"", ""e"".""ParentId"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""ReferenceOnDerived"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" = 'DerivedInheritanceRelationshipEntity'
) AS ""b"" ON ""e"".""ParentId"" = ""b"".""Id""",
                Sql);
        }

        public override void Include_collection_with_inheritance_on_derived1()
        {
            base.Include_collection_with_inheritance_on_derived1();

            // TODO: Assert on SQL
        }

        public override void Include_collection_with_inheritance_on_derived2()
        {
            base.Include_collection_with_inheritance_on_derived2();

            // TODO: Assert on SQL
        }

        public override void Include_collection_with_inheritance_on_derived3()
        {
            base.Include_collection_with_inheritance_on_derived3();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_collection_with_inheritance_on_derived4()
        {
            base.Include_collection_with_inheritance_on_derived4();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_collection_with_inheritance_on_derived_reverse()
        {
            base.Include_collection_with_inheritance_on_derived_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""ParentId"", ""e"".""DerivedInheritanceRelationshipEntityId"", ""b"".""Id"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""BaseId""
FROM ""BaseCollectionOnDerived"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b""
    WHERE ""b"".""Discriminator"" = 'DerivedInheritanceRelationshipEntity'
) AS ""b"" ON ""e"".""ParentId"" = ""b"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedCollectionOnDerived', 'BaseCollectionOnDerived')",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_reference1()
        {
            base.Nested_include_with_inheritance_reference_reference1();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name"", ""n"".""Id"", ""n"".""Discriminator"", ""n"".""Name"", ""n"".""ParentCollectionId"", ""n"".""ParentReferenceId""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseReferenceOnBase"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) AS ""b"" ON ""b"".""BaseParentId"" = ""e"".""Id""
LEFT JOIN (
    SELECT ""n"".*
    FROM ""NestedReferenceBase"" AS ""n""
    WHERE ""n"".""Discriminator"" IN ('NestedReferenceDerived', 'NestedReferenceBase')
) AS ""n"" ON ""n"".""ParentReferenceId"" = ""b"".""Id""
WHERE ""e"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_reference2()
        {
            base.Nested_include_with_inheritance_reference_reference2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_reference3()
        {
            base.Nested_include_with_inheritance_reference_reference3();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""BaseId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name"", ""n"".""Id"", ""n"".""Discriminator"", ""n"".""Name"", ""n"".""ParentCollectionId"", ""n"".""ParentReferenceId""
FROM ""BaseInheritanceRelationshipEntity"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseReferenceOnBase"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) AS ""b"" ON ""b"".""BaseParentId"" = ""e"".""Id""
LEFT JOIN (
    SELECT ""n"".*
    FROM ""NestedReferenceBase"" AS ""n""
    WHERE ""n"".""Discriminator"" IN ('NestedReferenceDerived', 'NestedReferenceBase')
) AS ""n"" ON ""n"".""ParentReferenceId"" = ""b"".""Id""
WHERE ""e"".""Discriminator"" = 'DerivedInheritanceRelationshipEntity'",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_reference4()
        {
            base.Nested_include_with_inheritance_reference_reference4();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_reference_reverse()
        {
            base.Nested_include_with_inheritance_reference_reference_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""ParentCollectionId"", ""e"".""ParentReferenceId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name"", ""b0"".""Id"", ""b0"".""Discriminator"", ""b0"".""Name"", ""b0"".""BaseId""
FROM ""NestedReferenceBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseReferenceOnBase"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) AS ""b"" ON ""e"".""ParentReferenceId"" = ""b"".""Id""
LEFT JOIN (
    SELECT ""b0"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b0""
    WHERE ""b0"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b0"" ON ""b"".""BaseParentId"" = ""b0"".""Id""
WHERE ""e"".""Discriminator"" IN ('NestedReferenceDerived', 'NestedReferenceBase')",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_collection1()
        {
            base.Nested_include_with_inheritance_reference_collection1();

            // TODO: Assert on SQL
        }

        public override void Nested_include_with_inheritance_reference_collection2()
        {
            base.Nested_include_with_inheritance_reference_collection2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_collection3()
        {
            base.Nested_include_with_inheritance_reference_collection3();

            // TODO: Assert on SQL
        }

        public override void Nested_include_with_inheritance_reference_collection4()
        {
            base.Nested_include_with_inheritance_reference_collection4();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_collection_reverse()
        {
            base.Nested_include_with_inheritance_reference_collection_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""ParentCollectionId"", ""e"".""ParentReferenceId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name"", ""b0"".""Id"", ""b0"".""Discriminator"", ""b0"".""Name"", ""b0"".""BaseId""
FROM ""NestedCollectionBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseReferenceOnBase"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) AS ""b"" ON ""e"".""ParentReferenceId"" = ""b"".""Id""
LEFT JOIN (
    SELECT ""b0"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b0""
    WHERE ""b0"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b0"" ON ""b"".""BaseParentId"" = ""b0"".""Id""
WHERE ""e"".""Discriminator"" IN ('NestedCollectionDerived', 'NestedCollectionBase')",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_reference1()
        {
            base.Nested_include_with_inheritance_collection_reference1();

            // TODO: Assert on SQL
        }

        public override void Nested_include_with_inheritance_collection_reference2()
        {
            base.Nested_include_with_inheritance_collection_reference2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_reference3()
        {
            base.Nested_include_with_inheritance_collection_reference3();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_reference4()
        {
            base.Nested_include_with_inheritance_collection_reference4();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_reference_reverse()
        {
            base.Nested_include_with_inheritance_collection_reference_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""ParentCollectionId"", ""e"".""ParentReferenceId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""DerivedProperty"", ""b0"".""Id"", ""b0"".""Discriminator"", ""b0"".""Name"", ""b0"".""BaseId""
FROM ""NestedReferenceBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseCollectionOnBase"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase')
) AS ""b"" ON ""e"".""ParentCollectionId"" = ""b"".""Id""
LEFT JOIN (
    SELECT ""b0"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b0""
    WHERE ""b0"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b0"" ON ""b"".""BaseParentId"" = ""b0"".""Id""
WHERE ""e"".""Discriminator"" IN ('NestedReferenceDerived', 'NestedReferenceBase')",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_collection1()
        {
            base.Nested_include_with_inheritance_collection_collection1();

            // TODO: Assert on SQL
        }

        public override void Nested_include_with_inheritance_collection_collection2()
        {
            base.Nested_include_with_inheritance_collection_collection2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_collection3()
        {
            base.Nested_include_with_inheritance_collection_collection3();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_collection4()
        {
            base.Nested_include_with_inheritance_collection_collection4();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_collection_reverse()
        {
            base.Nested_include_with_inheritance_collection_collection_reverse();

            Assert.Equal(
                @"SELECT ""e"".""Id"", ""e"".""Discriminator"", ""e"".""Name"", ""e"".""ParentCollectionId"", ""e"".""ParentReferenceId"", ""b"".""Id"", ""b"".""BaseParentId"", ""b"".""Discriminator"", ""b"".""Name"", ""b"".""DerivedProperty"", ""b0"".""Id"", ""b0"".""Discriminator"", ""b0"".""Name"", ""b0"".""BaseId""
FROM ""NestedCollectionBase"" AS ""e""
LEFT JOIN (
    SELECT ""b"".*
    FROM ""BaseCollectionOnBase"" AS ""b""
    WHERE ""b"".""Discriminator"" IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase')
) AS ""b"" ON ""e"".""ParentCollectionId"" = ""b"".""Id""
LEFT JOIN (
    SELECT ""b0"".*
    FROM ""BaseInheritanceRelationshipEntity"" AS ""b0""
    WHERE ""b0"".""Discriminator"" IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) AS ""b0"" ON ""b"".""BaseParentId"" = ""b0"".""Id""
WHERE ""e"".""Discriminator"" IN ('NestedCollectionDerived', 'NestedCollectionBase')",
                Sql);
        }

        protected override void ClearLog()
        {
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
