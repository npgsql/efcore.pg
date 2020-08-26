using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
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
    public class NpgsqlJsonPocoTranslator : IMemberTranslator
    {
        readonly IRelationalTypeMappingSource _typeMappingSource;
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        readonly RelationalTypeMapping _stringTypeMapping;

        public NpgsqlJsonPocoTranslator(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] NpgsqlSqlExpressionFactory sqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
            _stringTypeMapping = typeMappingSource.FindMapping(typeof(string));
        }

        public virtual SqlExpression Translate(SqlExpression instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => instance?.TypeMapping is NpgsqlJsonTypeMapping || instance is PostgresJsonTraversalExpression
                ? TranslateMemberAccess(
                    instance,
                    _sqlExpressionFactory.Constant(
                        member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? member.Name),
                    returnType)
                : null;

        public virtual SqlExpression TranslateMemberAccess(
            [NotNull] SqlExpression instance, [NotNull] SqlExpression member, [NotNull] Type returnType)
        {
            // The first time we see a JSON traversal it's on a column - create a JsonTraversalExpression.
            // Traversals on top of that get appended into the same expression.

            if (instance is ColumnExpression columnExpression &&
                columnExpression.TypeMapping is NpgsqlJsonTypeMapping)
            {
                return ConvertFromText(
                    _sqlExpressionFactory.JsonTraversal(
                        columnExpression,
                        new[] { member },
                        returnsText: true,
                        typeof(string),
                        _stringTypeMapping),
                    returnType);
            }

            if (instance is PostgresJsonTraversalExpression prevPathTraversal)
            {
                return ConvertFromText(
                    prevPathTraversal.Append(_sqlExpressionFactory.ApplyDefaultTypeMapping(member)),
                    returnType);
            }

            return null;
        }

        public virtual SqlExpression TranslateArrayLength([NotNull] SqlExpression expression)
        {
            if (expression is ColumnExpression columnExpression &&
                columnExpression.TypeMapping is NpgsqlJsonTypeMapping mapping)
            {
                return _sqlExpressionFactory.Function(
                    mapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                    new[] { expression },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(int));
            }

            if (expression is PostgresJsonTraversalExpression traversal)
            {
                // The traversal expression has ReturnsText=true (e.g. ->> not ->), so we recreate it to return
                // the JSON object instead.
                var lastPathComponent = traversal.Path.Last();
                var newTraversal = new PostgresJsonTraversalExpression(
                    traversal.Expression, traversal.Path,
                    returnsText: false,
                    lastPathComponent.Type,
                    _typeMappingSource.FindMapping(lastPathComponent.Type));

                var jsonMapping = (NpgsqlJsonTypeMapping)traversal.Expression.TypeMapping;
                return _sqlExpressionFactory.Function(
                    jsonMapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                    new[] { newTraversal },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(int));
            }

            return null;
        }

        // The PostgreSQL traversal operator always returns text, so we need to convert to int, bool, etc.
        SqlExpression ConvertFromText(SqlExpression expression, Type returnType)
        {
            var unwrappedReturnType = returnType.UnwrapNullableType();

            switch (Type.GetTypeCode(unwrappedReturnType))
            {
            case TypeCode.Boolean:
            case TypeCode.Byte:
            case TypeCode.DateTime:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.SByte:
            case TypeCode.Single:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return _sqlExpressionFactory.Convert(expression, returnType, _typeMappingSource.FindMapping(returnType));
            default:
                return unwrappedReturnType == typeof(Guid)
                    ? _sqlExpressionFactory.Convert(expression, returnType, _typeMappingSource.FindMapping(returnType))
                    : expression;
            }
        }
    }
}
