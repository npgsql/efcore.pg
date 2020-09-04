using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
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
    public class NpgsqlJsonDomTranslator : IMemberTranslator, IMethodCallTranslator
    {
        static readonly MemberInfo RootElement = typeof(JsonDocument).GetProperty(nameof(JsonDocument.RootElement));
        static readonly MethodInfo GetProperty = typeof(JsonElement).GetRuntimeMethod(nameof(JsonElement.GetProperty), new[] { typeof(string) });
        static readonly MethodInfo GetArrayLength = typeof(JsonElement).GetRuntimeMethod(nameof(JsonElement.GetArrayLength), Type.EmptyTypes);

        static readonly MethodInfo ArrayIndexer = typeof(JsonElement).GetProperties()
            .Single(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(int))
            .GetMethod;

        static readonly string[] GetMethods =
        {
            nameof(JsonElement.GetBoolean),
            nameof(JsonElement.GetDateTime),
            nameof(JsonElement.GetDateTimeOffset),
            nameof(JsonElement.GetDecimal),
            nameof(JsonElement.GetDouble),
            nameof(JsonElement.GetGuid),
            nameof(JsonElement.GetInt16),
            nameof(JsonElement.GetInt32),
            nameof(JsonElement.GetInt64),
            nameof(JsonElement.GetSingle),
            nameof(JsonElement.GetString)
        };

        readonly IRelationalTypeMappingSource _typeMappingSource;
        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        readonly RelationalTypeMapping _stringTypeMapping;

        public NpgsqlJsonDomTranslator(
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
        {
            if (member.DeclaringType != typeof(JsonDocument))
                return null;

            if (member == RootElement &&
                instance is ColumnExpression column &&
                column.TypeMapping is NpgsqlJsonTypeMapping)
            {
                // Simply get rid of the RootElement member access
                return column;
            }

            return null;
        }

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (method.DeclaringType != typeof(JsonElement) ||
                !(instance.TypeMapping is NpgsqlJsonTypeMapping mapping))
            {
                return null;
            }

            // The root of the JSON expression is a ColumnExpression. We wrap that with an empty traversal
            // expression (col #>> '{}'); subsequent traversals will gradually append the path into that.
            // Note that it's possible to call methods such as GetString() directly on the root, and the
            // empty traversal is necessary to properly convert it to a text.
            instance = instance is ColumnExpression columnExpression
                ? _sqlExpressionFactory.JsonTraversal(
                    columnExpression, returnsText: false, typeof(string), mapping)
                : instance;

            if (method == GetProperty || method == ArrayIndexer)
            {
                return instance is PostgresJsonTraversalExpression prevPathTraversal
                    ? prevPathTraversal.Append(_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]))
                    : null;
            }

            if (GetMethods.Contains(method.Name) &&
                arguments.Count == 0 &&
                instance is PostgresJsonTraversalExpression traversal)
            {
                var traversalToText = new PostgresJsonTraversalExpression(
                    traversal.Expression,
                    traversal.Path,
                    returnsText: true,
                    typeof(string),
                    _stringTypeMapping);

                return method.Name == nameof(JsonElement.GetString)
                    ? traversalToText
                    : ConvertFromText(traversalToText, method.ReturnType);
            }

            if (method == GetArrayLength)
            {
                return _sqlExpressionFactory.Function(
                    mapping.IsJsonb ? "jsonb_array_length" : "json_array_length",
                    new[] { instance },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(int));
            }

            if (method.Name.StartsWith("TryGet") && arguments.Count == 0)
                throw new InvalidOperationException($"The TryGet* methods on {nameof(JsonElement)} aren't translated yet, use Get* instead.'");

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
                return _sqlExpressionFactory.Convert(expression, returnType, _typeMappingSource.FindMapping(returnType));
            default:
                return returnType == typeof(Guid)
                    ? _sqlExpressionFactory.Convert(expression, returnType, _typeMappingSource.FindMapping(returnType))
                    : expression;
            }
        }
    }
}
