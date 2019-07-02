using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Pipeline;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable ArgumentsStyleLiteral
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a SQL function call expression, supporting PostgreSQL's named parameter notation.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class PgFunctionExpression : SqlExpression, IEquatable<PgFunctionExpression>
    {
        /// <summary>
        /// An empty instance of <see cref="ReadOnlyDictionary{TKey,TValue}"/>.
        /// </summary>
        [NotNull] static readonly ReadOnlyDictionary<string, SqlExpression> EmptyNamedArguments =
            new ReadOnlyDictionary<string, SqlExpression>(new Dictionary<string, SqlExpression>());

        /// <summary>
        /// The backing field for <see cref="PositionalArguments"/>.
        /// </summary>
        [NotNull] [ItemNotNull] readonly ReadOnlyCollection<SqlExpression> _positionalArguments;

        /// <summary>
        /// The backing field for <see cref="NamedArguments"/>.
        /// </summary>
        [NotNull] readonly ReadOnlyDictionary<string, SqlExpression> _namedArguments;

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        /// <value>
        /// The name of the function.
        /// </value>
        [NotNull]
        public virtual string FunctionName { get; }

        /// <summary>
        /// Gets the name of the schema.
        /// </summary>
        /// <value>
        /// The name of the schema.
        /// </value>
        [CanBeNull]
        public virtual string Schema { get; }

        /// <summary>
        /// The instance.
        /// </summary>
        [CanBeNull]
        public virtual SqlExpression Instance { get; }

        /// <summary>
        /// The positional arguments.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<SqlExpression> PositionalArguments => _positionalArguments;

        /// <summary>
        /// The named arguments.
        /// </summary>
        [NotNull]
        public virtual IReadOnlyDictionary<string, SqlExpression> NamedArguments => _namedArguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="typeMapping">The type mapping corresponding to the return type, or null to allow inference.</param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType,
            RelationalTypeMapping typeMapping = null)
            : this(
                instance: null,
                Check.NotEmpty(functionName, nameof(functionName)),
                schema: null,
                Enumerable.Empty<SqlExpression>(),
                EmptyNamedArguments,
                Check.NotNull(returnType, nameof(returnType)),
                typeMapping) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="positionalArguments">The positional arguments.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="typeMapping">The type mapping corresponding to the return type, or null to allow inference.</param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [NotNull] IEnumerable<SqlExpression> positionalArguments,
            [NotNull] Type returnType,
            RelationalTypeMapping typeMapping = null)
            : this(
                instance: null,
                Check.NotEmpty(functionName, nameof(functionName)),
                schema: null,
                Check.NotNull(positionalArguments, nameof(positionalArguments)),
                EmptyNamedArguments,
                Check.NotNull(returnType, nameof(returnType)),
                typeMapping) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="namedArguments">The named arguments.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="typeMapping">The type mapping corresponding to the return type, or null to allow inference.</param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [NotNull] IDictionary<string, SqlExpression> namedArguments,
            [NotNull] Type returnType,
            RelationalTypeMapping typeMapping = null)
            : this(
                instance: null,
                Check.NotEmpty(functionName, nameof(functionName)),
                schema: null,
                Enumerable.Empty<SqlExpression>(),
                namedArguments,
                Check.NotNull(returnType, nameof(returnType)),
                typeMapping) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="schema">The schema this function exists in if any.</param>
        /// <param name="positionalArguments">The positional arguments.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="typeMapping">The type mapping corresponding to the return type, or null to allow inference.</param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [CanBeNull] string schema,
            [NotNull] IEnumerable<SqlExpression> positionalArguments,
            [NotNull] Type returnType,
            RelationalTypeMapping typeMapping = null)
            : this(
                instance: null,
                Check.NotEmpty(functionName, nameof(functionName)),
                schema,
                Check.NotNull(positionalArguments, nameof(positionalArguments)),
                EmptyNamedArguments,
                Check.NotNull(returnType, nameof(returnType)),
                typeMapping) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="instance">The instance on which the function is called.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="positionalArguments">The positional arguments.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="typeMapping">The type mapping corresponding to the return type, or null to allow inference.</param>
        public PgFunctionExpression(
            [NotNull] SqlExpression instance,
            [NotNull] string functionName,
            [NotNull] IEnumerable<SqlExpression> positionalArguments,
            [NotNull] Type returnType,
            RelationalTypeMapping typeMapping = null)
            : this(
                Check.NotNull(instance, nameof(instance)),
                Check.NotEmpty(functionName, nameof(functionName)),
                schema: null,
                Check.NotNull(positionalArguments, nameof(positionalArguments)),
                EmptyNamedArguments,
                Check.NotNull(returnType, nameof(returnType)),
                typeMapping) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="instance">The instance on which the function is called.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="positionalArguments">The positional arguments.</param>
        /// <param name="namedArguments">The named arguments.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="typeMapping">The type mapping corresponding to the return type, or null to allow inference.</param>
        public PgFunctionExpression(
            [NotNull] SqlExpression instance,
            [NotNull] string functionName,
            [NotNull] IEnumerable<SqlExpression> positionalArguments,
            [NotNull] IDictionary<string, SqlExpression> namedArguments,
            [NotNull] Type returnType,
            RelationalTypeMapping typeMapping = null)
            : this(
                Check.NotNull(instance, nameof(instance)),
                Check.NotEmpty(functionName, nameof(functionName)),
                schema: null,
                Check.NotNull(positionalArguments, nameof(positionalArguments)),
                namedArguments,
                Check.NotNull(returnType, nameof(returnType)),
                typeMapping) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="instance">The instance on which the function is called.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="schema">The schema this function exists in if any.</param>
        /// <param name="positionalArguments">The positional arguments.</param>
        /// <param name="namedArguments">The named arguments.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="typeMapping">The type mapping corresponding to the return type, or null to allow inference.</param>
        internal PgFunctionExpression(
            [CanBeNull] SqlExpression instance,
            [NotNull] string functionName,
            [CanBeNull] string schema,
            [NotNull] IEnumerable<SqlExpression> positionalArguments,
            [NotNull] IDictionary<string, SqlExpression> namedArguments,
            [NotNull] Type returnType,
            RelationalTypeMapping typeMapping = null)
            : base(returnType, typeMapping)
        {
            Instance = instance;
            FunctionName = functionName;
            Schema = schema;
            _positionalArguments = positionalArguments.ToList().AsReadOnly();
            _namedArguments = new ReadOnlyDictionary<string, SqlExpression>(namedArguments);
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitPgFunction(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var instance = (SqlExpression)visitor.Visit(Instance);
            var positionalArguments = visitor.VisitAndConvert(_positionalArguments, nameof(VisitChildren));

            // TODO: Inefficient, instantiate new dictionary only if there are changes
            var namedArguments = new Dictionary<string, SqlExpression>(_namedArguments.Count);
            foreach (var kv in _namedArguments)
                namedArguments[kv.Key] = (SqlExpression)visitor.Visit(kv.Value);

            return Update(instance, positionalArguments, namedArguments);
        }

        public PgFunctionExpression Update(SqlExpression instance, IEnumerable<SqlExpression> positionalArguments, IDictionary<string, SqlExpression> namedArguments)
            => instance == Instance && positionalArguments == PositionalArguments && namedArguments == NamedArguments
                ? this
                : new PgFunctionExpression(instance, FunctionName, Schema, positionalArguments, namedArguments, Type, TypeMapping);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is PgFunctionExpression pgFunction && Equals(pgFunction);

        /// <inheritdoc />
        public bool Equals(PgFunctionExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               base.Equals(other) &&
               Type == other.Type &&
               string.Equals(FunctionName, other.FunctionName) &&
               string.Equals(Schema, other.Schema) &&
               _positionalArguments.SequenceEqual(other._positionalArguments) &&
               _namedArguments.Count == other._namedArguments.Count &&
               _namedArguments.All(kv => other._namedArguments.TryGetValue(kv.Key, out var otherValue) && kv.Value?.Equals(otherValue) == true) &&
               (Instance == null && other.Instance == null || Instance?.Equals(other.Instance) == true);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            foreach (var a in _positionalArguments)
                hash.Add(a.GetHashCode());
            foreach (var kv in _namedArguments)
            {
                hash.Add(kv.Key.GetHashCode());
                hash.Add(kv.Value.GetHashCode());
            }

            hash.Add(Instance);
            hash.Add(FunctionName);
            hash.Add(Schema);
            hash.Add(Type);
            return hash.ToHashCode();
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            var sb = expressionPrinter.StringBuilder;

            if (!string.IsNullOrEmpty(Schema))
                sb.Append(Schema).Append(".");
            sb.Append(FunctionName);

            sb.Append("(");
            expressionPrinter.VisitList(PositionalArguments);

            var hasArguments = PositionalArguments.Count > 0 && NamedArguments.Count > 0;

            foreach (var kv in NamedArguments)
            {
                if (hasArguments)
                    sb.Append(", ");
                else
                    hasArguments = true;

                sb.Append(kv.Key).Append(" => ");

                expressionPrinter.Visit(kv.Value);
            }

            expressionPrinter.StringBuilder.Append(")");
        }

        /// <inheritdoc />
        public override string ToString()
            => (Instance != null ? Instance + "." : Schema != null ? Schema + "." : "") +
               $"{FunctionName}({string.Join("", "", PositionalArguments)}" +
               (NamedArguments.Count > 0 ? ", " + string.Join("", "", NamedArguments.Select(a => $"{a.Key} => {a.Value}")) : "") +
               ")";
    }
}
