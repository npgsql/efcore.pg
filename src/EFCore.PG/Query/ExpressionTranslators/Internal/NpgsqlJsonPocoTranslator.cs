using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlJsonPocoTranslator : IMemberTranslator
    {
        [NotNull]
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

        [NotNull]
        readonly RelationalTypeMapping _stringTypeMapping;

        public NpgsqlJsonPocoTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _stringTypeMapping = sqlExpressionFactory.FindMapping(typeof(string));
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
            => TranslateMemberAccess(instance, _sqlExpressionFactory.Constant(member.Name), returnType);

        public SqlExpression TranslateMemberAccess(SqlExpression instance, SqlExpression member, Type returnType)
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

            if (instance is JsonTraversalExpression prevPathTraversal)
            {
                return ConvertFromText(
                    prevPathTraversal.Append(_sqlExpressionFactory.ApplyDefaultTypeMapping(member)),
                    returnType);
            }

            return null;
        }

        public SqlExpression TranslateArrayLength(SqlExpression expression)
        {
            if (expression is ColumnExpression columnExpression &&
                columnExpression.TypeMapping is NpgsqlJsonTypeMapping mapping)
            {
                return _sqlExpressionFactory.Function(
                    mapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                    new[] { expression }, typeof(int));
            }

            if (expression is JsonTraversalExpression traversal)
            {
                // The traversal expression has ReturnsText=true (e.g. ->> not ->), so we recreate it to return
                // the JSON object instead.
                var lastPathComponent = traversal.Path.Last();
                var newTraversal = new JsonTraversalExpression(
                    traversal.Expression, traversal.Path,
                    returnsText: false,
                    lastPathComponent.Type,
                    _sqlExpressionFactory.FindMapping(lastPathComponent.Type));

                var jsonMapping = (NpgsqlJsonTypeMapping)traversal.Expression.TypeMapping;
                return _sqlExpressionFactory.Function(
                    jsonMapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                    new[] { newTraversal }, typeof(int));
            }

            return null;
        }

        // The PostgreSQL traversal operator always returns text, so we need to convert to int, bool, etc.
        SqlExpression ConvertFromText(SqlExpression expression, Type returnType)
        {
            switch (Type.GetTypeCode(returnType))
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
                return _sqlExpressionFactory.Convert(expression, returnType, _sqlExpressionFactory.FindMapping(returnType));
            default:
                return (returnType == typeof(Guid))
                    ? _sqlExpressionFactory.Convert(expression, returnType, _sqlExpressionFactory.FindMapping(returnType))
                    : expression;
            }
        }
    }
}
