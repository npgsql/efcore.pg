using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL JSON operator traversing a JSON document with a path (i.e. x#>y or x#>>y)
    /// </summary>
    public class PostgresJsonTraversalExpression : SqlExpression, IEquatable<PostgresJsonTraversalExpression>
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
        public virtual IReadOnlyList<SqlExpression> Path { get; }

        /// <summary>
        /// Whether the text-returning operator (x#>>y) or the object-returning operator (x#>y) is used.
        /// </summary>
        public virtual bool ReturnsText { get; }

        /// <summary>
        /// Constructs a <see cref="PostgresJsonTraversalExpression"/>.
        /// </summary>
        public PostgresJsonTraversalExpression(
            [NotNull] SqlExpression expression,
            [NotNull] IReadOnlyList<SqlExpression> path,
            bool returnsText,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            if (returnsText && type != typeof(string))
                throw new ArgumentException($"{nameof(type)} must be string", nameof(type));

            Expression = expression;
            Path = path;
            ReturnsText = returnsText;
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update(
                (SqlExpression)visitor.Visit(Expression),
                Path.Select(p => (SqlExpression)visitor.Visit(p)).ToArray());

        public virtual PostgresJsonTraversalExpression Update(
            [NotNull] SqlExpression expression,
            [NotNull] IReadOnlyList<SqlExpression> path)
            => expression == Expression &&
               path.Count == Path.Count &&
               path.Zip(Path, (x, y) => (x, y)).All(tup => tup.x == tup.y)
                ? this
                : new PostgresJsonTraversalExpression(expression, path, ReturnsText, Type, TypeMapping);

        public virtual PostgresJsonTraversalExpression Append([NotNull] SqlExpression pathComponent)
        {
            var newPath = new SqlExpression[Path.Count + 1];
            for (var i = 0; i < Path.Count(); i++)
                newPath[i] = Path[i];
            newPath[newPath.Length - 1] = pathComponent;
            return new PostgresJsonTraversalExpression(Expression, newPath, ReturnsText, Type, TypeMapping);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as PostgresJsonTraversalExpression);

        /// <inheritdoc />
        public virtual bool Equals(PostgresJsonTraversalExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               base.Equals(other) &&
               Equals(Expression, other.Expression) &&
               Path.Count == other.Path.Count &&
               Path.Zip(other.Path, (x, y) => (x, y)).All(tup => tup.x == tup.y);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Expression, Path);

        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Expression);
            expressionPrinter.Append(ReturnsText ? "#>>" : "#>");
            expressionPrinter.Append("{");
            for (var i = 0; i < Path.Count; i++)
            {
                expressionPrinter.Visit(Path[i]);
                if (i < Path.Count - 1)
                    expressionPrinter.Append(",");
            }
            expressionPrinter.Append("}");
        }

        /// <inheritdoc />
        public override string ToString() => $"{Expression}{(ReturnsText ? "#>>" : "#>")}{Path}";
    }
}
