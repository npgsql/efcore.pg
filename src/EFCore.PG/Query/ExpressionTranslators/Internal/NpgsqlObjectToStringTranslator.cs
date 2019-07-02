using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlObjectToStringTranslator : IMethodCallTranslator
    {
        static readonly List<Type> SupportedTypes = new List<Type>
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

        readonly ISqlExpressionFactory _sqlExpressionFactory;

        public NpgsqlObjectToStringTranslator(ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
            => method.Name == nameof(ToString)
               && arguments.Count == 0
               && instance != null
               && SupportedTypes.Contains(instance.Type.UnwrapNullableType())
               ? _sqlExpressionFactory.Convert(instance, typeof(string))
               : null;
    }
}
