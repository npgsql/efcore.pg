using System.Diagnostics.CodeAnalysis;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Translations.NodaTime;

/// <summary>
///     A <see cref="NodaTimeQueryNpgsqlFixture" /> which additionally verifies, before generating SQL, that every SQL expression involving
///     a NodaTime type has a type mapping for that same type. A mismatch (e.g. an expression typed as <c>ZonedDateTime</c> but carrying
///     the <c>Instant</c> type mapping) means that the expression can't be projected or further composed correctly.
///     It also verifies that date_part() (and trunc() of its result) are typed as <see langword="double" />,
///     i.e. double precision, which is what PostgreSQL returns, e.g. for the intermediate results of <c>Milliseconds</c>.
///     Finally, it rejects queries whose result is (partly) evaluated on the client, i.e. whose shaper does more than read the
///     projected value (or materialize an entity).
/// </summary>
public class NodaTimeParityFixture : NodaTimeQueryNpgsqlFixture
{
    protected override string StoreName
        => "NodaTimeParityTest";

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(
            serviceCollection
                .AddSingleton<IQuerySqlGeneratorFactory, TypeCheckingQuerySqlGeneratorFactory>()
                .AddScoped<IQueryTranslationPostprocessorFactory, ServerEvaluationCheckingPostprocessorFactory>());

#pragma warning disable EF1001 // Internal EF Core API usage.
    private sealed class ServerEvaluationCheckingPostprocessorFactory(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies)
        : NpgsqlQueryTranslationPostprocessorFactory(dependencies, relationalDependencies)
    {
        public override QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
            => new ServerEvaluationCheckingPostprocessor(
                Dependencies, RelationalDependencies, (RelationalQueryCompilationContext)queryCompilationContext);
    }

    private sealed class ServerEvaluationCheckingPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        RelationalQueryCompilationContext queryCompilationContext)
        : NpgsqlQueryTranslationPostprocessor(dependencies, relationalDependencies, queryCompilationContext)
    {
        public override Expression Process(Expression query)
        {
            var result = base.Process(query);

            if (result is ShapedQueryExpression { ShaperExpression: var shaper } && !ReadsServerResult(shaper))
            {
                throw new InvalidOperationException(
                    $"The query result would be evaluated on the client, rather than translated: {shaper.Print()}");
            }

            return result;

            static bool ReadsServerResult(Expression shaper)
                => shaper switch
                {
                    UnaryExpression { NodeType: ExpressionType.Convert, Operand: var operand } => ReadsServerResult(operand),
                    ProjectionBindingExpression or StructuralTypeShaperExpression => true,
                    _ => false
                };
        }
    }

    private sealed class TypeCheckingQuerySqlGeneratorFactory : NpgsqlQuerySqlGeneratorFactory
    {
        private readonly QuerySqlGeneratorDependencies _dependencies;
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly INpgsqlSingletonOptions _npgsqlSingletonOptions;

        public TypeCheckingQuerySqlGeneratorFactory(
            QuerySqlGeneratorDependencies dependencies,
            IRelationalTypeMappingSource typeMappingSource,
            INpgsqlSingletonOptions npgsqlSingletonOptions)
            : base(dependencies, typeMappingSource, npgsqlSingletonOptions)
        {
            _dependencies = dependencies;
            _typeMappingSource = typeMappingSource;
            _npgsqlSingletonOptions = npgsqlSingletonOptions;
        }

        public override QuerySqlGenerator Create()
            => new TypeCheckingQuerySqlGenerator(
                _dependencies,
                _typeMappingSource,
                _npgsqlSingletonOptions.ReverseNullOrderingEnabled,
                _npgsqlSingletonOptions.PostgresVersion);
    }

    private sealed class TypeCheckingQuerySqlGenerator(
        QuerySqlGeneratorDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource,
        bool reverseNullOrderingEnabled,
        Version postgresVersion)
        : NpgsqlQuerySqlGenerator(dependencies, typeMappingSource, reverseNullOrderingEnabled, postgresVersion)
    {
        public override IRelationalCommand GetCommand(Expression queryExpression)
        {
            new TypeMappingVerifier().Visit(queryExpression);

            return base.GetCommand(queryExpression);
        }
    }
#pragma warning restore EF1001

    private sealed class TypeMappingVerifier : ExpressionVisitor
    {
        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            // date_part() returns double precision, and so does trunc(double precision)
            if (node is SqlFunctionExpression { Name: "date_part" or "trunc" } function
                && (function.Name == "date_part" || function.Arguments is [{ Type: var argumentType }] && argumentType == typeof(double))
                && (function.Type != typeof(double) || function.TypeMapping?.StoreType != "double precision"))
            {
                throw new InvalidOperationException(
                    $"SQL expression '{function.Print()}' has .NET type '{function.Type.Name}' and store type "
                    + $"'{function.TypeMapping?.StoreType}', but should be double precision.");
            }

            if (node is SqlExpression { TypeMapping: { } typeMapping } sqlExpression)
            {
                var type = Nullable.GetUnderlyingType(sqlExpression.Type) ?? sqlExpression.Type;
                var mappedType = Nullable.GetUnderlyingType(typeMapping.ClrType) ?? typeMapping.ClrType;

                if (type != mappedType && (type.Namespace == "NodaTime" || mappedType.Namespace == "NodaTime"))
                {
                    throw new InvalidOperationException(
                        $"SQL expression '{sqlExpression.Print()}' has .NET type '{type.Name}', but a type mapping for '{mappedType.Name}' "
                        + $"('{typeMapping.StoreType}').");
                }
            }

            return base.Visit(node);
        }
    }
}
