#pragma warning disable 8632

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

/// <summary>
/// Represents a SQL function call expression, supporting PostgreSQL's named parameter notation
/// (e.g. make_interval(weeks => 2) and non-comma parameter separators (e.g. position(substring in string)).
/// </summary>
[DebuggerDisplay("{" + nameof(ToString) + "()}")]
public class PostgresFunctionExpression : SqlFunctionExpression, IEquatable<PostgresFunctionExpression>
{
    /// <inheritdoc />
    public new virtual IReadOnlyList<SqlExpression> Arguments
        => base.Arguments!;

    /// <inheritdoc />
    public new virtual IReadOnlyList<bool> ArgumentsPropagateNullability
        => base.ArgumentsPropagateNullability!;

    /// <summary>
    /// For aggregate methods, contains whether to apply distinct.
    /// </summary>
    public virtual bool IsAggregateDistinct { get; }

    /// <summary>
    /// For aggregate methods, contains the predicate to be applied (generated as the SQL FILTER clause).
    /// </summary>
    public virtual SqlExpression? AggregatePredicate { get; }

    /// <summary>
    /// For aggregate methods, contains the orderings to be applied.
    /// </summary>
    public virtual IReadOnlyList<OrderingExpression> AggregateOrderings { get; }

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

    /// <summary>
    ///     Creates an instance of <see cref="PostgresFunctionExpression" /> with named arguments.
    /// </summary>
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
            aggregateDistinct: false, aggregatePredicate: null, aggregateOrderings: Array.Empty<OrderingExpression>(),
            nullable: nullable, argumentsPropagateNullability: argumentsPropagateNullability, type: type, typeMapping: typeMapping);
    }

    /// <summary>
    ///     Creates an instance of <see cref="PostgresFunctionExpression" /> with argument separators.
    /// </summary>
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
            name, arguments, argumentNames: null, argumentSeparators: argumentSeparators,
            aggregateDistinct: false, aggregatePredicate: null, aggregateOrderings: Array.Empty<OrderingExpression>(),
            nullable: nullable, argumentsPropagateNullability: argumentsPropagateNullability, type: type, typeMapping: typeMapping);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="PostgresFunctionExpression" />.
    /// </summary>
    public PostgresFunctionExpression(
        string name,
        IEnumerable<SqlExpression> arguments,
        IEnumerable<string?>? argumentNames,
        IEnumerable<string?>? argumentSeparators,
        bool aggregateDistinct,
        SqlExpression? aggregatePredicate,
        IReadOnlyList<OrderingExpression> aggregateOrderings,
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

        IsAggregateDistinct = aggregateDistinct;
        AggregatePredicate = aggregatePredicate;
        AggregateOrderings = aggregateOrderings;
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        Check.NotNull(visitor, nameof(visitor));

        var changed = false;

        // Note that we don't have instance functions in PG

        SqlExpression[]? visitedArguments = null;

        if (!IsNiladic)
        {
            for (var i = 0; i < Arguments.Count; i++)
            {
                var visitedArgument = (SqlExpression)visitor.Visit(Arguments[i]);

                if (visitedArgument != Arguments[i] && visitedArguments is null)
                {
                    changed = true;
                    visitedArguments = new SqlExpression[Arguments.Count];

                    for (var j = 0; j < visitedArguments.Length; j++)
                    {
                        visitedArguments[j] = Arguments[j];
                    }
                }

                if (visitedArguments is not null)
                {
                    visitedArguments[i] = visitedArgument;
                }
            }
        }

        var visitedAggregatePredicate = (SqlExpression?)visitor.Visit(AggregatePredicate);
        changed |= visitedAggregatePredicate != AggregatePredicate;

        OrderingExpression[]? visitedAggregateOrderings = null;

        for (var i = 0; i < AggregateOrderings.Count; i++)
        {
            var visitedOrdering = (OrderingExpression)visitor.Visit(AggregateOrderings[i]);
            if (visitedOrdering != AggregateOrderings[i] && visitedAggregateOrderings is null)
            {
                changed = true;
                visitedAggregateOrderings = new OrderingExpression[AggregateOrderings.Count];

                for (var j = 0; j < visitedAggregateOrderings.Length; j++)
                {
                    visitedAggregateOrderings[j] = AggregateOrderings[j];
                }
            }

            if (visitedAggregateOrderings is not null)
            {
                visitedAggregateOrderings[i] = visitedOrdering;
            }
        }

        return changed
            ? new PostgresFunctionExpression(
                Name, visitedArguments ?? Arguments, ArgumentNames, ArgumentSeparators,
                IsAggregateDistinct,
                visitedAggregatePredicate ?? AggregatePredicate,
                visitedAggregateOrderings ?? AggregateOrderings,
                IsNullable, ArgumentsPropagateNullability!, Type, TypeMapping)
            : this;
    }

    /// <inheritdoc />
    public override SqlFunctionExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new PostgresFunctionExpression(
            Name,
            Arguments,
            ArgumentNames,
            ArgumentSeparators,
            IsAggregateDistinct,
            AggregatePredicate,
            AggregateOrderings,
            IsNullable,
            ArgumentsPropagateNullability, Type, typeMapping ?? TypeMapping);

    /// <inheritdoc />
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
                IsAggregateDistinct,
                AggregatePredicate,
                AggregateOrderings,
                IsNullable, ArgumentsPropagateNullability, Type, TypeMapping)
            : this;
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    public virtual PostgresFunctionExpression UpdateAggregateComponents(
        SqlExpression? predicate,
        IReadOnlyList<OrderingExpression> orderings)
    {
        return predicate != AggregatePredicate || orderings != AggregateOrderings
            ? new PostgresFunctionExpression(
                Name, Arguments, ArgumentNames, ArgumentSeparators,
                IsAggregateDistinct,
                predicate,
                orderings,
                IsNullable, ArgumentsPropagateNullability, Type, TypeMapping)
            : this;
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        if (!string.IsNullOrEmpty(Schema))
        {
            expressionPrinter.Append(Schema).Append(".").Append(Name);
        }
        else
        {
            Check.DebugAssert(Instance is null, "Instance is null");

            expressionPrinter.Append(Name);
        }

        if (!IsNiladic)
        {
            expressionPrinter.Append("(");

            if (IsAggregateDistinct)
            {
                expressionPrinter.Append("DISTINCT ");
            }

            expressionPrinter.VisitCollection(Arguments);

            if (AggregateOrderings.Count > 0)
            {
                expressionPrinter.Append(" ORDER BY ");
                expressionPrinter.VisitCollection(AggregateOrderings);
            }

            expressionPrinter.Append(")");

            if (AggregatePredicate is not null)
            {
                expressionPrinter.Append(" FILTER (WHERE ");
                expressionPrinter.Visit(AggregatePredicate);
                expressionPrinter.Append(")");
            }
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is PostgresFunctionExpression pgFunction && Equals(pgFunction);

    /// <inheritdoc />
    public virtual bool Equals(PostgresFunctionExpression? other)
        => ReferenceEquals(this, other)
            || other is not null
            && base.Equals(other)
            && ArgumentNames.SequenceEqual(other.ArgumentNames)
            && ArgumentSeparators.SequenceEqual(other.ArgumentSeparators)
            && AggregateOrderings.SequenceEqual(other.AggregateOrderings)
            && (AggregatePredicate is null && other.AggregatePredicate is null
                || AggregatePredicate != null && AggregatePredicate.Equals(other.AggregatePredicate));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(base.GetHashCode());

        foreach (var argumentName in ArgumentNames)
        {
            hash.Add(argumentName);
        }

        foreach (var argumentSeparator in ArgumentSeparators)
        {
            hash.Add(argumentSeparator);
        }

        foreach (var aggregateOrdering in AggregateOrderings)
        {
            hash.Add(aggregateOrdering);
        }

        hash.Add(AggregatePredicate);

        return hash.ToHashCode();
    }
}
