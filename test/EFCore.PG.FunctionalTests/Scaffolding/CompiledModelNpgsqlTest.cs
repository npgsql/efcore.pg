// using System.Runtime.CompilerServices;
// using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;
// using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
// using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
//
// namespace Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding;
//
// public class CompiledModelNpgsqlTest : CompiledModelRelationalTestBase
// {
//     protected override TestHelpers TestHelpers => NpgsqlTestHelpers.Instance;
//     protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
//
//     // #3087
//     public override void BigModel()
//         => Assert.Throws<InvalidOperationException>(() => base.BigModel());
//
//     // #3087
//     public override void BigModel_with_JSON_columns()
//         => Assert.Throws<InvalidOperationException>(() => base.BigModel());
//
//     // #3087
//     public override void CheckConstraints()
//         => Assert.Throws<InvalidOperationException>(() => base.BigModel());
//
//     // #3087
//     public override void DbFunctions()
//         => Assert.Throws<InvalidOperationException>(() => base.BigModel());
//
//     // #3087
//     public override void Triggers()
//         => Assert.Throws<InvalidOperationException>(() => base.BigModel());
//
//     // https://github.com/dotnet/efcore/pull/32341/files#r1485603038
//     public override void Tpc()
//         => Assert.Throws<InvalidOperationException>(() => base.Tpc());
//
//     // https://github.com/dotnet/efcore/pull/32341/files#r1485603038
//     public override void ComplexTypes()
//         => Assert.Throws<InvalidOperationException>(() => base.ComplexTypes());
//
//     protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
//     {
//         new NpgsqlDbContextOptionsBuilder(builder).UseNetTopologySuite();
//         return builder;
//     }
//
//     protected override void AddDesignTimeServices(IServiceCollection services)
//         => new NpgsqlNetTopologySuiteDesignTimeServices().ConfigureDesignTimeServices(services);
//
//     protected override BuildSource AddReferences(BuildSource build, [CallerFilePath] string filePath = "")
//     {
//         base.AddReferences(build);
//         build.References.Add(BuildReference.ByName("Npgsql.EntityFrameworkCore.PostgreSQL"));
//         build.References.Add(BuildReference.ByName("Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite"));
//         build.References.Add(BuildReference.ByName("Npgsql"));
//         build.References.Add(BuildReference.ByName("NetTopologySuite"));
//         return build;
//     }
// }
