using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlJsonDbFunctionsTranslator : IMethodCallTranslator
    {
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        readonly RelationalTypeMapping _boolTypeMapping;
        readonly RelationalTypeMapping _stringTypeMapping;
        readonly RelationalTypeMapping _jsonbTypeMapping;

        public NpgsqlJsonDbFunctionsTranslator(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] NpgsqlSqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool));
            _stringTypeMapping = typeMappingSource.FindMapping(typeof(string));
            _jsonbTypeMapping = typeMappingSource.FindMapping("jsonb");
        }

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (method.DeclaringType != typeof(NpgsqlJsonDbFunctionsExtensions))
                return null;

            var args = arguments
                // Skip useless DbFunctions instance
                .Skip(1)
                // JSON extensions accept object parameters for JSON, since they must be able to handle POCOs, strings or DOM types.
                // This means they come wrapped in a convert node, which we need to remove.
                // Convert nodes may also come from wrapping JsonTraversalExpressions generated through POCO traversal.
                .Select(RemoveConvert)
                // If a function is invoked over a JSON traversal expression, that expression may come with
                // returnText: true (i.e. operator ->> and not ->). Since the functions below require a json object and
                // not text, we transform it.
                .Select(a => a is PostgresJsonTraversalExpression traversal ? WithReturnsText(traversal, false) : a)
                .ToArray();

            if (!args.Any(a => a.TypeMapping is NpgsqlJsonTypeMapping || a is PostgresJsonTraversalExpression))
                throw new InvalidOperationException("The EF JSON methods require a JSON parameter and none was found.");

            if (method.Name == nameof(NpgsqlJsonDbFunctionsExtensions.JsonTypeof))
            {
                return _sqlExpressionFactory.Function(
                    ((NpgsqlJsonTypeMapping)args[0].TypeMapping).IsJsonb ? "jsonb_typeof" : "json_typeof",
                    new[] { args[0] },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(string));
            }

            // The following are jsonb-only, not support on json
            if (args.Any(a => a.TypeMapping is NpgsqlJsonTypeMapping jsonMapping && !jsonMapping.IsJsonb))
                throw new InvalidOperationException("JSON methods on EF.Functions only support the jsonb type, not json.");

            return method.Name switch
            {
                nameof(NpgsqlJsonDbFunctionsExtensions.JsonContains)
                => _sqlExpressionFactory.Contains(Jsonb(args[0]), Jsonb(args[1])),
                nameof(NpgsqlJsonDbFunctionsExtensions.JsonContained)
                => _sqlExpressionFactory.ContainedBy(Jsonb(args[0]), Jsonb(args[1])),
                nameof(NpgsqlJsonDbFunctionsExtensions.JsonExists)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.JsonExists, Jsonb(args[0]), args[1]),
                nameof(NpgsqlJsonDbFunctionsExtensions.JsonExistAny)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.JsonExistsAny, Jsonb(args[0]), args[1]),
                nameof(NpgsqlJsonDbFunctionsExtensions.JsonExistAll)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.JsonExistsAll, Jsonb(args[0]), args[1]),

                _ => null
            };

            SqlExpression Jsonb(SqlExpression e) => _sqlExpressionFactory.ApplyTypeMapping(e, _jsonbTypeMapping);

            static SqlExpression RemoveConvert(SqlExpression e)
            {
                while (e is SqlUnaryExpression unary &&
                       (unary.OperatorType == ExpressionType.Convert || unary.OperatorType == ExpressionType.ConvertChecked))
                {
                    e = unary.Operand;
                }

                return e;
            }

            PostgresJsonTraversalExpression WithReturnsText(PostgresJsonTraversalExpression traversal, bool returnsText)
                => traversal.ReturnsText == returnsText
                    ? traversal
                    : returnsText
                        ? new PostgresJsonTraversalExpression(traversal.Expression, traversal.Path, true, typeof(string), _stringTypeMapping)
                        : new PostgresJsonTraversalExpression(traversal.Expression, traversal.Path, false, traversal.Type, traversal.Expression.TypeMapping);
        }
    }
}
