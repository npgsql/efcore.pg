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
    public class PostgresFunctionExpression : SqlFunctionExpression, IEquatable<PostgresFunctionExpression>
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

        public static PostgresFunctionExpression CreateWithNamedArguments(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] IEnumerable<string?> argumentNames,
            bool nullable,
            [CanBeNull] IEnumerable<bool> argumentsPropagateNullability,
            bool builtIn,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(argumentNames, nameof(argumentNames));

            return new PostgresFunctionExpression(
                name, arguments, argumentNames, argumentSeparators: null,
                nullable, argumentsPropagateNullability, type, typeMapping);
        }

        public static PostgresFunctionExpression CreateWithArgumentSeparators(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] IEnumerable<string?> argumentSeparators,
            bool nullable,
            [CanBeNull] IEnumerable<bool> argumentsPropagateNullability,
            bool builtIn,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        {
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(argumentSeparators, nameof(argumentSeparators));

            return new PostgresFunctionExpression(
                name, arguments, argumentNames: null, argumentSeparators,
                nullable, argumentsPropagateNullability, type, typeMapping);
        }

        PostgresFunctionExpression(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [CanBeNull] IEnumerable<string?>? argumentNames,
            [CanBeNull] IEnumerable<string?>? argumentSeparators,
            bool nullable,
            [CanBeNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
        : base(name, arguments, nullable, argumentsPropagateNullability, type, typeMapping)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));

            ArgumentNames = (argumentNames ?? Array.Empty<string>()).ToList();
            ArgumentSeparators = (argumentSeparators ?? Array.Empty<string>()).ToList();

            if (ArgumentNames.SkipWhile(a => a == null).Contains(null))
                throw new ArgumentException($"{nameof(argumentNames)} must contain nulls followed by non-nulls", nameof(argumentNames));
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var visited = base.VisitChildren(visitor);
            return visited != this && visited is SqlFunctionExpression e
                ? new PostgresFunctionExpression(
                    e.Name, e.Arguments, ArgumentNames, ArgumentSeparators,
                    IsNullable, ArgumentsPropagateNullability, Type, TypeMapping)
                : visited;
        }

        public override SqlFunctionExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
            => new PostgresFunctionExpression(
                Name,
                Arguments,
                ArgumentNames,
                ArgumentSeparators,
                IsNullable,
                ArgumentsPropagateNullability,
                Type,
                typeMapping ?? TypeMapping);

        public override SqlFunctionExpression Update(SqlExpression instance, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(arguments, nameof(arguments));
            if (instance != null)
                throw new ArgumentException($"Must be null", nameof(instance));

            return !arguments.SequenceEqual(Arguments)
                ? new PostgresFunctionExpression(
                    Name, arguments, ArgumentNames, ArgumentSeparators,
                    IsNullable, ArgumentsPropagateNullability, Type, TypeMapping)
                : this;
        }

        public override bool Equals(object obj) => obj is PostgresFunctionExpression pgFunction && Equals(pgFunction);

        public virtual bool Equals(PostgresFunctionExpression other)
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
