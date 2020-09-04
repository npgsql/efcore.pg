using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents creating a new PostgreSQL array.
    /// </summary>
    public class PostgresNewArrayExpression : SqlExpression
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PostgresNewArrayExpression" /> class.
        /// </summary>
        /// <param name="initializers">The initializers for the array.</param>
        /// <param name="type">The <see cref="Type"/> of the expression.</param>
        /// <param name="typeMapping">The <see cref="RelationalTypeMapping"/> associated with the expression.</param>
        public PostgresNewArrayExpression(
            [NotNull] IReadOnlyList<SqlExpression> initializers,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Check.NotNull(initializers, nameof(initializers));

            if (!type.IsArrayOrGenericList())
                throw new ArgumentException($"{nameof(PostgresNewArrayExpression)} must have an array type");

            Initializers = initializers;
        }

        /// <summary>
        ///     The operator of this PostgreSQL binary operation.
        /// </summary>
        public virtual IReadOnlyList<SqlExpression> Initializers { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            List<SqlExpression> newInitializers = null;
            for (var i = 0; i < Initializers.Count; i++)
            {
                var initializer = Initializers[i];
                var newInitializer = (SqlExpression)visitor.Visit(initializer);
                if (newInitializer != initializer && newInitializers is null)
                {
                    newInitializers = new List<SqlExpression>();
                    for (var j = 0; j < i; j++)
                        newInitializers.Add(newInitializer);
                }

                if (newInitializers != null)
                    newInitializers.Add(newInitializer);
            }

            return newInitializers is null
                ? this
                : new PostgresNewArrayExpression(newInitializers, Type, TypeMapping);
        }

        /// <summary>
        /// Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        /// return this expression.
        /// </summary>
        /// <param name="initializers">The initializers for the array.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public virtual PostgresNewArrayExpression Update([NotNull] IReadOnlyList<SqlExpression> initializers)
        {
            Check.NotNull(initializers, nameof(initializers));

            return initializers == Initializers
                ? this
                : new PostgresNewArrayExpression(initializers, Type, TypeMapping);
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("ARRAY[");

            var first = true;
            foreach (var initializer in Initializers)
            {
                if (!first)
                {
                    expressionPrinter.Append(", ");
                }

                first = false;

                expressionPrinter.Visit(initializer);
            }

            expressionPrinter.Append("]");
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                   || obj is PostgresNewArrayExpression sqlBinaryExpression
                   && Equals(sqlBinaryExpression));

        bool Equals(PostgresNewArrayExpression postgresNewArrayExpression)
            => base.Equals(postgresNewArrayExpression)
               && Initializers.SequenceEqual(postgresNewArrayExpression.Initializers);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(base.GetHashCode());
            for (var i = 0; i < Initializers.Count; i++)
                hash.Add(Initializers[i]);

            return hash.ToHashCode();
        }
    }
}

