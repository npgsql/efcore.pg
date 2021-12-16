#pragma warning disable 8632

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
/// Represents a SQL function call expression, supporting PostgreSQL's named parameter notation
/// (e.g. make_interval(weeks => 2) and non-comma parameter separators (e.g. position(substring in string)).
/// </summary>
[DebuggerDisplay("{" + nameof(ToString) + "()}")]
public class PostgresFunctionExpression : SqlFunctionExpression, IEquatable<PostgresFunctionExpression>
{
    public new virtual IReadOnlyList<SqlExpression> Arguments
        => base.Arguments!;

    public new virtual IReadOnlyList<bool> ArgumentsPropagateNullability
        => base.ArgumentsPropagateNullability!;

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
        string name,
        IEnumerable<SqlExpression> arguments,
        IEnumerable<string?> argumentNames,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        bool builtIn,
        Type type,
        RelationalTypeMapping? typeMapping)
    {
        Check.NotNull(arguments, nameof(arguments));
        Check.NotNull(argumentNames, nameof(argumentNames));

        return new PostgresFunctionExpression(
            name, arguments, argumentNames, argumentSeparators: null,
            nullable, argumentsPropagateNullability, type, typeMapping);
    }

    public static PostgresFunctionExpression CreateWithArgumentSeparators(
        string name,
        IEnumerable<SqlExpression> arguments,
        IEnumerable<string?> argumentSeparators,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        bool builtIn,
        Type type,
        RelationalTypeMapping? typeMapping)
    {
        Check.NotNull(arguments, nameof(arguments));
        Check.NotNull(argumentSeparators, nameof(argumentSeparators));

        return new PostgresFunctionExpression(
            name, arguments, argumentNames: null, argumentSeparators,
            nullable, argumentsPropagateNullability, type, typeMapping);
    }

    private PostgresFunctionExpression(
        string name,
        IEnumerable<SqlExpression> arguments,
        IEnumerable<string?>? argumentNames,
        IEnumerable<string?>? argumentSeparators,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(name, arguments, nullable, argumentsPropagateNullability, type, typeMapping)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(type, nameof(type));

        ArgumentNames = (argumentNames ?? Array.Empty<string>()).ToList();
        ArgumentSeparators = (argumentSeparators ?? Array.Empty<string>()).ToList();

        if (ArgumentNames.SkipWhile(a => a is null).Contains(null))
        {
            throw new ArgumentException($"{nameof(argumentNames)} must contain nulls followed by non-nulls", nameof(argumentNames));
        }
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Check.NotNull(visitor, nameof(visitor));

        var visited = base.VisitChildren(visitor);
        return visited != this && visited is PostgresFunctionExpression e
            ? new PostgresFunctionExpression(
                e.Name, e.Arguments, ArgumentNames, ArgumentSeparators,
                IsNullable, ArgumentsPropagateNullability!, Type, TypeMapping)
            : visited;
    }

    public override SqlFunctionExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new PostgresFunctionExpression(
            Name,
            Arguments,
            ArgumentNames,
            ArgumentSeparators,
            IsNullable,
            ArgumentsPropagateNullability,
            Type,
            typeMapping ?? TypeMapping);

    public override SqlFunctionExpression Update(SqlExpression? instance, IReadOnlyList<SqlExpression>? arguments)
    {
        Check.NotNull(arguments, nameof(arguments));
        if (instance is not null)
        {
            throw new ArgumentException("Must be null", nameof(instance));
        }

        return !arguments.SequenceEqual(Arguments)
            ? new PostgresFunctionExpression(
                Name, arguments, ArgumentNames, ArgumentSeparators,
                IsNullable, ArgumentsPropagateNullability, Type, TypeMapping)
            : this;
    }

    public override bool Equals(object? obj) => obj is PostgresFunctionExpression pgFunction && Equals(pgFunction);

    public virtual bool Equals(PostgresFunctionExpression? other)
        => ReferenceEquals(this, other) ||
            other is not null &&
            base.Equals(other) &&
            ArgumentNames.SequenceEqual(other.ArgumentNames) &&
            ArgumentSeparators.SequenceEqual(other.ArgumentSeparators);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        foreach (var argumentName in ArgumentNames)
        {
            hash.Add(argumentName?.GetHashCode());
        }

        foreach (var argumentSeparator in ArgumentSeparators)
        {
            hash.Add(argumentSeparator?.GetHashCode() ?? 0);
        }

        return hash.ToHashCode();
    }
}