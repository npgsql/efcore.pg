using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Translates method and property calls on arrays/lists into their corresponding PostgreSQL operations.
/// </summary>
/// <remarks>
///     https://www.postgresql.org/docs/current/static/functions-array.html
/// </remarks>
public class NpgsqlArrayMethodTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory, NpgsqlJsonPocoTranslator jsonPocoTranslator)
    : IMethodCallTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory = sqlExpressionFactory;
    private readonly NpgsqlJsonPocoTranslator _jsonPocoTranslator = jsonPocoTranslator;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // During preprocessing, ArrayIndex and List[] get normalized to ElementAt; so we handle indexing into array/list here
        if (method is { IsGenericMethod: true, Name: nameof(Enumerable.ElementAt) }
            && method.DeclaringType == typeof(Enumerable)
            && arguments is [var source, var index]
            && index.Type == typeof(int))
        {
            return source.TypeMapping switch
            {
                // Indexing over bytea is special, we have to use function rather than subscript
                NpgsqlByteArrayTypeMapping
                    => _sqlExpressionFactory.Function(
                        "get_byte",
                        [source, index],
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        typeof(byte)),

                NpgsqlArrayTypeMapping typeMapping
                    => _sqlExpressionFactory.ArrayIndex(
                        source,
                        _sqlExpressionFactory.GenerateOneBasedIndexExpression(index),
                        nullable: true),

                // Try translating indexing inside JSON column
                // Note that Length over PG arrays (not within JSON) gets translated by QueryableMethodTranslatingEV, since arrays are primitive
                // collections
                _ => _jsonPocoTranslator.TranslateMemberAccess(source, index, method.ReturnType)
            };
        }

        if (method is { IsGenericMethod: true, Name: nameof(Enumerable.SequenceEqual) }
            && method.DeclaringType == typeof(Enumerable)
            && arguments is [var first, var second]
            && first.Type.IsArrayOrGenericList()
            && !IsMappedToNonArray(first)
            && second.Type.IsArrayOrGenericList()
            && !IsMappedToNonArray(second))
        {
            return _sqlExpressionFactory.Equal(first, second);
        }

        // Translate instance methods on List
        if (instance is not null && instance.Type.IsGenericList() && !IsMappedToNonArray(instance))
        {
            return TranslateCommon(instance, arguments);
        }

        // Translate extension methods over array or List
        if (instance is null && arguments.Count > 0 && arguments[0].Type.IsArrayOrGenericList() && !IsMappedToNonArray(arguments[0]))
        {
            return TranslateCommon(arguments[0], arguments.Slice(1));
        }

        return null;

        // The array/list CLR type may be mapped to a non-array database type (e.g. byte[] to bytea, or just
        // value converters) - we don't want to translate for those cases.
        static bool IsMappedToNonArray(SqlExpression arrayOrList)
            => arrayOrList.TypeMapping is { } and not (NpgsqlArrayTypeMapping or NpgsqlJsonTypeMapping);

#pragma warning disable CS8321
        SqlExpression? TranslateCommon(SqlExpression arrayOrList, IReadOnlyList<SqlExpression> arguments)
#pragma warning restore CS8321
        {
            if (arguments is [var searchItem]
                && (method is { IsGenericMethod: true, Name: nameof(Array.IndexOf) } && method.DeclaringType == typeof(Array)
                    || method.Name == nameof(List<int>.IndexOf) && method.DeclaringType.IsGenericList()))
            {
                var (item, array) = _sqlExpressionFactory.ApplyTypeMappingsOnItemAndArray(searchItem, arrayOrList);

                return _sqlExpressionFactory.Coalesce(
                    _sqlExpressionFactory.Subtract(
                        _sqlExpressionFactory.Function(
                            "array_position",
                            [array, item],
                            nullable: true,
                            // array_position can return NULL even if both its arguments are non-nullable;
                            // this is currently the way to express that (see
                            // https://github.com/dotnet/efcore/pull/33814#issuecomment-2687857927).
                            FalseArrays[2],
                            arrayOrList.Type),
                        _sqlExpressionFactory.Constant(1)),
                    _sqlExpressionFactory.Constant(-1));
            }

            if (arguments is [var searchItem2, var startIndexArg]
                && (method is { IsGenericMethod: true, Name: nameof(Array.IndexOf) } && method.DeclaringType == typeof(Array)
                    || method.Name == nameof(List<>.IndexOf) && method.DeclaringType.IsGenericList()))
            {
                var (item, array) = _sqlExpressionFactory.ApplyTypeMappingsOnItemAndArray(searchItem2, arrayOrList);
                var startIndex = _sqlExpressionFactory.GenerateOneBasedIndexExpression(startIndexArg);

                return _sqlExpressionFactory.Coalesce(
                    _sqlExpressionFactory.Subtract(
                        _sqlExpressionFactory.Function(
                            "array_position",
                            [array, item, startIndex],
                            nullable: true,
                            // array_position can return NULL even if both its arguments are non-nullable;
                            // this is currently the way to express that (see
                            // https://github.com/dotnet/efcore/pull/33814#issuecomment-2687857927).
                            FalseArrays[3],
                            arrayOrList.Type),
                        _sqlExpressionFactory.Constant(1)),
                    _sqlExpressionFactory.Constant(-1));
            }

            // TODO: Enumerable Append and Concat are only here because primitive collections aren't handled in ExecuteUpdate,
            // https://github.com/dotnet/efcore/issues/32494
            if (method is { IsGenericMethod: true, Name: nameof(Enumerable.Append) }
                && method.DeclaringType == typeof(Enumerable)
                && arguments is [var element])
            {
                var (item, array) = _sqlExpressionFactory.ApplyTypeMappingsOnItemAndArray(element, arrayOrList);

                return _sqlExpressionFactory.Function(
                    "array_append",
                    [array, item],
                    nullable: true,
                    TrueArrays[2],
                    arrayOrList.Type,
                    arrayOrList.TypeMapping);
            }

            if (method is { IsGenericMethod: true, Name: nameof(Enumerable.Concat) }
                && method.DeclaringType == typeof(Enumerable)
                && arguments is [var otherArray])
            {
                var inferredMapping = ExpressionExtensions.InferTypeMapping(arrayOrList, otherArray);

                return _sqlExpressionFactory.Function(
                    "array_cat",
                    [
                        _sqlExpressionFactory.ApplyTypeMapping(arrayOrList, inferredMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(otherArray, inferredMapping)
                    ],
                    nullable: true,
                    TrueArrays[2],
                    arrayOrList.Type,
                    inferredMapping);
            }

            return null;
        }
    }
}
