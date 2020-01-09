using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

#nullable enable

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a SQL function call expression, supporting PostgreSQL's named parameter notation
    /// (e.g. make_interval(weeks => 2) and non-comma parameter separators (e.g. position(substring in string)).
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class PgFunctionExpression : SqlFunctionExpression, IEquatable<PgFunctionExpression>
    {
        /// <summary>
        /// List of argument names, corresponding position-wise to arguments in <see cref="SqlFunctionExpression.Arguments"/>.
        /// Unnamed (positional) arguments must come first, so this list must contain possible nulls, followed by
        /// non-nulls.
        /// </summary>
        public virtual IReadOnlyList<string?> ArgumentNames { get; }

        /// <summary>
        /// List of non-comma separators between argument separators, in the order in which they appear between
        /// the arguments. <c>null</c> as well as positions beyond the end of the list mean regular commas.
        /// </summary>
        public virtual IReadOnlyList<string?> ArgumentSeparators { get; }

        public static PgFunctionExpression CreateWithNamedArguments(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] IEnumerable<string?> argumentNames,
            bool builtIn,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(argumentNames, nameof(argumentNames));

            return new PgFunctionExpression(name, arguments, argumentNames, argumentSeparators: null, builtIn, type, typeMapping);
        }

        public static PgFunctionExpression CreateWithArgumentSeparators(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] IEnumerable<string?> argumentSeparators,
            bool builtIn,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(argumentSeparators, nameof(argumentSeparators));

            return new PgFunctionExpression(name, arguments, argumentNames: null, argumentSeparators, builtIn, type, typeMapping);
        }

        public PgFunctionExpression(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [CanBeNull] IEnumerable<string?>? argumentNames,
            [CanBeNull] IEnumerable<string?>? argumentSeparators,
            bool builtIn,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        : base(
            instance: null, schema: null, name, niladic: false, arguments,
            nullResultAllowed: true, instancPropagatesNullability: null, argumentsPropagateNullability: arguments.Select(a => false),
            builtIn, type, typeMapping)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));

            ArgumentNames = (argumentNames ?? Array.Empty<string>()).ToList();
            ArgumentSeparators = (argumentSeparators ?? Array.Empty<string>()).ToList();

            if (ArgumentNames.SkipWhile(a => a == null).Contains(null))
                throw new ArgumentException($"{nameof(argumentNames)} must contain nulls followed by non-nulls", nameof(argumentNames));
        }

        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitPgFunction(this)
                : base.Accept(visitor);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var visited = base.VisitChildren(visitor);
            return visited != this && visited is SqlFunctionExpression e
                ? new PgFunctionExpression(e.Name, e.Arguments, ArgumentNames, ArgumentSeparators, IsBuiltIn, Type, TypeMapping)
                : visited;
        }

        public override SqlFunctionExpression ApplyTypeMapping([CanBeNull] RelationalTypeMapping typeMapping)
            => new PgFunctionExpression(
                Name,
                Arguments,
                ArgumentNames,
                ArgumentSeparators,
                IsBuiltIn,
                Type,
                typeMapping ?? TypeMapping);

        public override SqlFunctionExpression Update(
            [CanBeNull] SqlExpression instance, [CanBeNull] IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(arguments, nameof(arguments));
            if (instance != null)
                throw new ArgumentException($"Must be null", nameof(instance));

            return !arguments.SequenceEqual(Arguments)
                ? new PgFunctionExpression(Name, arguments, ArgumentNames, ArgumentSeparators, IsBuiltIn, Type, TypeMapping)
                : this;
        }

        public override bool Equals(object obj) => obj is PgFunctionExpression pgFunction && Equals(pgFunction);

        public bool Equals(PgFunctionExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               base.Equals(other) &&
               ArgumentNames.SequenceEqual(other.ArgumentNames) &&
               ArgumentSeparators.SequenceEqual(other.ArgumentSeparators);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            foreach (var argumentName in ArgumentNames)
                hash.Add(argumentName?.GetHashCode());
            foreach (var argumentSeparator in ArgumentSeparators)
                hash.Add(argumentSeparator?.GetHashCode() ?? 0);
            return hash.ToHashCode();
        }
    }
}
