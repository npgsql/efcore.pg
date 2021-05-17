using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlObjectToStringTranslator : IMethodCallTranslator
    {
        private static readonly HashSet<Type> _typeMapping = new()
        {
            typeof(int),
            typeof(long),
            typeof(DateTime),
            typeof(Guid),
            typeof(bool),
            typeof(byte),
            //typeof(byte[])
            typeof(double),
            typeof(DateTimeOffset),
            typeof(char),
            typeof(short),
            typeof(float),
            typeof(decimal),
            typeof(TimeSpan),
            typeof(uint),
            typeof(ushort),
            typeof(ulong),
            typeof(sbyte),
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public NpgsqlObjectToStringTranslator(ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (instance == null || method.Name != nameof(ToString) || arguments.Count != 0)
            {
                return null;
            }

            if (instance.Type == typeof(bool))
            {
                return instance is ColumnExpression columnExpression && columnExpression.IsNullable
                    ? _sqlExpressionFactory.Case(
                        new[]
                        {
                            new CaseWhenClause(
                                _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(false)),
                                _sqlExpressionFactory.Constant(false.ToString())),
                            new CaseWhenClause(
                                _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(true)),
                                _sqlExpressionFactory.Constant(true.ToString()))
                        },
                        _sqlExpressionFactory.Constant(null))
                    : _sqlExpressionFactory.Case(
                        new[]
                        {
                            new CaseWhenClause(
                                _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(false)),
                                _sqlExpressionFactory.Constant(false.ToString()))
                        },
                        _sqlExpressionFactory.Constant(true.ToString()));
            }

            return _typeMapping.Contains(instance.Type)
                || instance.Type.UnwrapNullableType().IsEnum && instance.TypeMapping is NpgsqlEnumTypeMapping
                    ? _sqlExpressionFactory.Convert(instance, typeof(string))
                    : null;
        }
    }
}
