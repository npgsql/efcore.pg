using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.ReverseEngineering;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

#if NETCOREAPP1_1
using System.Reflection;
using Microsoft.CodeAnalysis;
#endif

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.FunctionalTests.ReverseEngineering
{
    [FrameworkSkipCondition(RuntimeFrameworks.CoreCLR, SkipReason = "https://github.com/xunit/xunit/issues/978")]
    public class NpgsqlE2ETests : E2ETestBase, IClassFixture<NpgsqlE2EFixture>
    {
        protected override string ProviderName => "Npgsql.EntityFrameworkCore.PostgreSQL.Design";

        protected override void ConfigureDesignTimeServices(IServiceCollection services)
            => new NpgsqlDesignTimeServices().ConfigureDesignTimeServices(services);

        public virtual string TestNamespace => "E2ETest.Namespace";
        public virtual string TestProjectDir => Path.Combine("E2ETest", "Output");
        public virtual string TestSubDir => "SubDir";
        public virtual string CustomizedTemplateDir => Path.Combine("E2ETest", "CustomizedTemplate", "Dir");

        public static TableSelectionSet Filter
            => new TableSelectionSet(new List<string>{
                "AllDataTypes",
                "PropertyConfiguration",
                "Test Spaces Keywords Table",
                "OneToManyDependent",
                "OneToManyPrincipal",
                "OneToOneDependent",
                "OneToOnePrincipal",
                "OneToOneSeparateFKDependent",
                "OneToOneSeparateFKPrincipal",
                "OneToOneFKToUniqueKeyDependent",
                "OneToOneFKToUniqueKeyPrincipal",
                /*
                "ReferredToByTableWithUnmappablePrimaryKeyColumn",
                "TableWithUnmappablePrimaryKeyColumn",
                */
                "SelfReferencing",
                "SpecialTypes"
            });

        public NpgsqlE2ETests(NpgsqlE2EFixture fixture, ITestOutputHelper output)
            : base(output)
        {
        }

        readonly string _connectionString =
            new NpgsqlConnectionStringBuilder(TestEnvironment.DefaultConnection) {
                Database = "NpgsqlReverseEngineerTestE2E"
            }.ConnectionString;

        static readonly List<string> _expectedEntityTypeFiles = new List<string>
            {
                "AllDataTypes.expected",
                "OneToManyDependent.expected",
                "OneToManyPrincipal.expected",
                "OneToOneDependent.expected",
                "OneToOneFKToUniqueKeyDependent.expected",
                "OneToOneFKToUniqueKeyPrincipal.expected",
                "OneToOnePrincipal.expected",
                "OneToOneSeparateFKDependent.expected",
                "OneToOneSeparateFKPrincipal.expected",
                "PropertyConfiguration.expected",
                /*
                "ReferredToByTableWithUnmappablePrimaryKeyColumn.expected",
                */
                "SelfReferencing.expected",
                "SpecialTypes.expected",
                "Test_Spaces_Keywords_Table.expected",
           };

        [Fact]
        [UseCulture("en-US")]
        public void E2ETest_UseAttributesInsteadOfFluentApi()
        {
            var configuration = new ReverseEngineeringConfiguration
            {
                ConnectionString = _connectionString,
                ContextClassName = "AttributesContext",
                ProjectPath = TestProjectDir + Path.DirectorySeparatorChar, // tests that ending DirectorySeparatorChar does not affect namespace
                ProjectRootNamespace = TestNamespace,
                OutputPath = TestSubDir,
                TableSelectionSet = Filter,
            };

            var filePaths = Generator.GenerateAsync(configuration).GetAwaiter().GetResult();

            var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(Path.Combine(TestProjectDir, TestSubDir)))
            {
                Files = Enumerable.Repeat(filePaths.ContextFile, 1).Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
            };

            var expectedFileSet = new FileSet(new FileSystemFileService(),
                Path.Combine("ReverseEngineering", "Expected", "Attributes"),
                contents => contents.Replace("namespace " + TestNamespace, "namespace " + TestNamespace + "." + TestSubDir)
                    .Replace("{{connectionString}}", _connectionString))
            {
                Files = (new List<string> { "AttributesContext.expected" })
                    .Concat(_expectedEntityTypeFiles).ToList()
            };

            /*
            AssertLog(new LoggerMessages
            {
                Warn =
                        {
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.geographyColumn", "geography"),
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.geometryColumn", "geometry"),
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.hierarchyidColumn", "hierarchyid"),
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.sql_variantColumn", "sql_variant"),
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.xmlColumn", "xml"),
                            NpgsqlDesignStrings.DataTypeDoesNotAllowNpgsqlIdentityStrategy("dbo.PropertyConfiguration.PropertyConfigurationID","tinyint"),
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.TableWithUnmappablePrimaryKeyColumn.TableWithUnmappablePrimaryKeyColumnID", "hierarchyid"),
                            RelationalDesignStrings.PrimaryKeyErrorPropertyNotFound("dbo.TableWithUnmappablePrimaryKeyColumn"),
                            RelationalDesignStrings.UnableToGenerateEntityType("dbo.TableWithUnmappablePrimaryKeyColumn"),
                        }
            });
            */

            AssertEqualFileContents(expectedFileSet, actualFileSet);
            AssertCompile(actualFileSet);
        }

        [Fact]
        [UseCulture("en-US")]
        public void E2ETest_AllFluentApi()
        {
            var configuration = new ReverseEngineeringConfiguration
            {
                ConnectionString = _connectionString,
                ProjectPath = TestProjectDir,
                ProjectRootNamespace = TestNamespace,
                OutputPath = null, // not used for this test
                UseFluentApiOnly = true,
                TableSelectionSet = Filter,
            };

            var filePaths = Generator.GenerateAsync(configuration).GetAwaiter().GetResult();

            var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(TestProjectDir))
            {
                Files = Enumerable.Repeat(filePaths.ContextFile, 1).Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
            };

             var expectedFileSet = new FileSet(new FileSystemFileService(),
                Path.Combine("ReverseEngineering", "Expected", "AllFluentApi"),
                inputFile => inputFile.Replace("{{connectionString}}", _connectionString))
            {
                Files = (new List<string> { "NpgsqlReverseEngineerTestE2EContext.expected" })
                    .Concat(_expectedEntityTypeFiles).ToList()
            };

            /*
            AssertLog(new LoggerMessages
            {
                Warn =
                        {
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.geographyColumn", "geography"),
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.geometryColumn", "geometry"),
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.hierarchyidColumn", "hierarchyid"),
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.sql_variantColumn", "sql_variant"),
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.xmlColumn", "xml"),
                            NpgsqlDesignStrings.DataTypeDoesNotAllowNpgsqlIdentityStrategy("dbo.PropertyConfiguration.PropertyConfigurationID","tinyint"),
                            RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.TableWithUnmappablePrimaryKeyColumn.TableWithUnmappablePrimaryKeyColumnID", "hierarchyid"),
                            RelationalDesignStrings.PrimaryKeyErrorPropertyNotFound("dbo.TableWithUnmappablePrimaryKeyColumn"),
                            RelationalDesignStrings.UnableToGenerateEntityType("dbo.TableWithUnmappablePrimaryKeyColumn"),
                        }
            });
            */

            AssertEqualFileContents(expectedFileSet, actualFileSet);
            AssertCompile(actualFileSet);
        }

        [Fact]
        public void Sequences()
        {
            using (var scratch = NpgsqlTestStore.CreateScratch())
            {
                scratch.ExecuteNonQuery(@"
CREATE SEQUENCE ""CountByTwo""
    START WITH 1
    INCREMENT BY 2;

CREATE SEQUENCE ""CyclicalCountByThree""
    START WITH 6
    INCREMENT BY 3
    MAXVALUE 27
    MINVALUE 0
    CYCLE;");

                var configuration = new ReverseEngineeringConfiguration
                {
                    ConnectionString = scratch.ConnectionString,
                    ProjectPath = TestProjectDir + Path.DirectorySeparatorChar,
                    ProjectRootNamespace = TestNamespace,
                    ContextClassName = "SequenceContext",
                };
                var expectedFileSet = new FileSet(new FileSystemFileService(),
                    Path.Combine("ReverseEngineering", "Expected"),
                    contents => contents.Replace("{{connectionString}}", scratch.ConnectionString))
                {
                    Files = new List<string> { "SequenceContext.expected" }
                };

                var filePaths = Generator.GenerateAsync(configuration).GetAwaiter().GetResult();

                var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(TestProjectDir))
                {
                    Files = new[] { filePaths.ContextFile }.Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };

                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public void ColumnsWithSequences()
        {
            using (var scratch = NpgsqlTestStore.CreateScratch())
            {
                scratch.ExecuteNonQuery(@"
DROP TABLE IF EXISTS ""IdSerialSequence"";
CREATE TABLE ""IdSerialSequence"" (
  ""Id"" SERIAL PRIMARY KEY
);

DROP TABLE IF EXISTS ""IdNonSerialSequence"";
DROP SEQUENCE IF EXISTS ""IdSomeSequence"";
CREATE SEQUENCE ""IdSomeSequence"";
CREATE TABLE ""IdNonSerialSequence"" (
  ""Id"" INTEGER PRIMARY KEY DEFAULT nextval('""IdSomeSequence""')
);

DROP TABLE IF EXISTS ""SerialSequence"";
CREATE TABLE ""SerialSequence"" (
  ""Id"" INTEGER PRIMARY KEY,
  ""SomeField"" SERIAL
);

DROP TABLE IF EXISTS ""NonSerialSequence"";
DROP SEQUENCE IF EXISTS ""SomeSequence"";
CREATE SEQUENCE ""SomeSequence"";
CREATE TABLE ""NonSerialSequence"" (
  ""Id"" INTEGER PRIMARY KEY,
  ""SomeField"" INTEGER DEFAULT nextval('""SomeSequence""')
);");

                var configuration = new ReverseEngineeringConfiguration
                {
                    ConnectionString = scratch.ConnectionString,
                    ProjectPath = TestProjectDir + Path.DirectorySeparatorChar,
                    ProjectRootNamespace = TestNamespace,
                    ContextClassName = "ColumnsWithSequencesContext",
                };
                var expectedFileSet = new FileSet(new FileSystemFileService(),
                    Path.Combine("ReverseEngineering", "Expected", "ColumnsWithSequences"),
                    contents => contents.Replace("{{connectionString}}", scratch.ConnectionString))
                {
                    Files = new List<string>
                    {
                        "ColumnsWithSequencesContext.expected",
                        "IdNonSerialSequence.expected",
                        "IdSerialSequence.expected",
                        "NonSerialSequence.expected",
                        "SerialSequence.expected"
                    }
                };

                var filePaths = Generator.GenerateAsync(configuration).GetAwaiter().GetResult();

                var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(TestProjectDir))
                {
                    Files = new[] { filePaths.ContextFile }.Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };

                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        protected override ICollection<BuildReference> References { get; } = new List<BuildReference>
        {
#if NET46
            BuildReference.ByName("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
            BuildReference.ByName("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
            BuildReference.ByName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
            BuildReference.ByName("System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"),
#elif NETCOREAPP2_0
            BuildReference.ByName("netstandard"),
            BuildReference.ByName("System.Collections"),
            BuildReference.ByName("System.Data.Common"),
            BuildReference.ByName("System.Linq.Expressions"),
            BuildReference.ByName("System.Reflection"),
            BuildReference.ByName("System.ComponentModel.Annotations"),
            BuildReference.ByName("System.Net.Primitives"),
            BuildReference.ByName("System.Net.NetworkInformation"),
#else
#error target frameworks need to be updated.
#endif
            BuildReference.ByName("Microsoft.EntityFrameworkCore"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
            BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions"),
            BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions"),
            BuildReference.ByName("Npgsql.EntityFrameworkCore.PostgreSQL")
        };
    }
}
