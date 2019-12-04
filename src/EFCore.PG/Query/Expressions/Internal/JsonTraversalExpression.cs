using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL JSON operator traversing a JSON document with a path (i.e. x#>y or x#>>y)
    /// </summary>
    public class JsonTraversalExpression : SqlExpression, IEquatable<JsonTraversalExpression>
    {
        /// <summary>
        /// The match expression.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Expression { get; }

        /// <summary>
        /// The pattern to match.
        /// </summary>
        [NotNull]
        public virtual SqlExpression[] Path { get; }

        /// <summary>
        /// Whether the text-returning operator (x#>>y) or the object-returning operator (x#>y) is used.
        /// </summary>
        public virtual bool ReturnsText { get; }

        /// <summary>
        /// Constructs a <see cref="ILikeExpression"/>.
        /// </summary>
        public JsonTraversalExpression(
            [NotNull] SqlExpression expression,
            [NotNull] SqlExpression[] path,
            bool returnsText,
            [NotNull] Type type,
            RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            if (returnsText && type != typeof(string))
                throw new ArgumentException($"{nameof(type)} must be string", nameof(type));

            Expression = expression;
            Path = path;
            ReturnsText = returnsText;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitJsonPathTraversal(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update(
                (SqlExpression)visitor.Visit(Expression),
                Path.Select(p => (SqlExpression)visitor.Visit(p)).ToArray());

        public JsonTraversalExpression Update(SqlExpression expression, SqlExpression[] path)
            => expression == Expression &&
               path.Length == Path.Length &&
               path.Zip(Path, (x, y) => (x, y)).All(tup => tup.x == tup.y)
                ? this
                : new JsonTraversalExpression(expression, path, ReturnsText, Type, TypeMapping);

        public JsonTraversalExpression Append(SqlExpression pathComponent)
        {
            var oldPath = Path;
            var newPath = new SqlExpression[oldPath.Length + 1];
            Array.Copy(oldPath, newPath, oldPath.Length);
            newPath[newPath.Length - 1] = pathComponent;
            return new JsonTraversalExpression(Expression, newPath, ReturnsText, Type, TypeMapping);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as JsonTraversalExpression);

        /// <inheritdoc />
        public bool Equals(JsonTraversalExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               base.Equals(other) &&
               Equals(Expression, other.Expression) &&
               Path.Length == other.Path.Length &&
               Path.Zip(other.Path, (x, y) => (x, y)).All(tup => tup.x == tup.y);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Expression, Path);

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Expression);
            expressionPrinter.Append(ReturnsText ? "#>>" : "#>");
            expressionPrinter.Append("{");
            for (var i = 0; i < Path.Length; i++)
            {
                expressionPrinter.Visit(Path[i]);
                if (i < Path.Length - 1)
                    expressionPrinter.Append(",");
            }
            expressionPrinter.Append("}");
        }

        /// <inheritdoc />
        public override string ToString() => $"{Expression}{(ReturnsText ? "#>>" : "#>")}{Path}";
    }
}
