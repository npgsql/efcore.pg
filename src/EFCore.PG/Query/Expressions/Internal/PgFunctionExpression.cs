#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a SQL function call expression, supporting PostgreSQL's named parameter notation.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class PgFunctionExpression : Expression, IEquatable<PgFunctionExpression>
    {
        /// <summary>
        /// An empty instance of <see cref="ReadOnlyDictionary{TKey,TValue}"/>.
        /// </summary>
        [NotNull] static readonly ReadOnlyDictionary<string, Expression> EmptyNamedArguments =
            new ReadOnlyDictionary<string, Expression>(new Dictionary<string, Expression>());

        /// <summary>
        /// The backing field for <see cref="PositionalArguments"/>.
        /// </summary>
        [NotNull] [ItemNotNull] readonly ReadOnlyCollection<Expression> _positionalArguments;

        /// <summary>
        /// The backing field for <see cref="NamedArguments"/>.
        /// </summary>
        [NotNull] readonly ReadOnlyDictionary<string, Expression> _namedArguments;

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
        public virtual Expression Instance { get; }

        /// <summary>
        /// The positional arguments.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public virtual IReadOnlyList<Expression> PositionalArguments => _positionalArguments;

        /// <summary>
        /// The named arguments.
        /// </summary>
        [NotNull]
        public virtual IReadOnlyDictionary<string, Expression> NamedArguments => _namedArguments;

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="returnType">The return type.</param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType)
            : this(
                instance: null,
                Check.NotEmpty(functionName, nameof(functionName)),
                schema: null,
                Check.NotNull(returnType, nameof(returnType)),
                Enumerable.Empty<Expression>(),
                EmptyNamedArguments) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="positionalArguments">The positional arguments.</param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [NotNull] IEnumerable<Expression> positionalArguments)
            : this(
                instance: null,
                Check.NotEmpty(functionName, nameof(functionName)),
                schema: null,
                Check.NotNull(returnType, nameof(returnType)),
                Check.NotNull(positionalArguments, nameof(positionalArguments)),
                EmptyNamedArguments) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="namedArguments">The namedarguments.</param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [NotNull] IDictionary<string, Expression> namedArguments)
            : this(
                instance: null,
                Check.NotEmpty(functionName, nameof(functionName)),
                schema: null,
                Check.NotNull(returnType, nameof(returnType)),
                Enumerable.Empty<Expression>(),
                namedArguments) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="schema">The schema this function exists in if any.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="positionalArguments">The positional arguments.</param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [CanBeNull] string schema,
            [NotNull] IEnumerable<Expression> positionalArguments)
            : this(
                instance: null,
                Check.NotEmpty(functionName, nameof(functionName)),
                schema,
                Check.NotNull(returnType, nameof(returnType)),
                Check.NotNull(positionalArguments, nameof(positionalArguments)),
                EmptyNamedArguments) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="instance">The instance on which the function is called.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="positionalArguments">The positional arguments.</param>
        public PgFunctionExpression(
            [NotNull] Expression instance,
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [NotNull] IEnumerable<Expression> positionalArguments)
            : this(
                Check.NotNull(instance, nameof(instance)),
                Check.NotEmpty(functionName, nameof(functionName)),
                schema: null,
                Check.NotNull(returnType, nameof(returnType)),
                Check.NotNull(positionalArguments, nameof(positionalArguments)),
                EmptyNamedArguments) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="instance">The instance on which the function is called.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="positionalArguments">The positional arguments.</param>
        /// <param name="namedArguments">The named arguments.</param>
        public PgFunctionExpression(
            [NotNull] Expression instance,
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [NotNull] IEnumerable<Expression> positionalArguments,
            [NotNull] IDictionary<string, Expression> namedArguments)
            : this(
                Check.NotNull(instance, nameof(instance)),
                Check.NotEmpty(functionName, nameof(functionName)),
                schema: null,
                Check.NotNull(returnType, nameof(returnType)),
                Check.NotNull(positionalArguments, nameof(positionalArguments)),
                namedArguments) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="instance">The instance on which the function is called.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="schema">The schema this function exists in if any.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="positionalArguments">The positional arguments.</param>
        /// <param name="namedArguments">The named arguments.</param>
        internal PgFunctionExpression(
            [CanBeNull] Expression instance,
            [NotNull] string functionName,
            [CanBeNull] string schema,
            [NotNull] Type returnType,
            [NotNull] IEnumerable<Expression> positionalArguments,
            [NotNull] IDictionary<string, Expression> namedArguments)
        {
            Instance = instance;
            FunctionName = functionName;
            Type = returnType;
            Schema = schema;
            _positionalArguments = positionalArguments.ToList().AsReadOnly();
            _namedArguments = new ReadOnlyDictionary<string, Expression>(namedArguments);
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is NpgsqlQuerySqlGenerator specificVisitor
                ? specificVisitor.VisitPgFunction(this)
                : base.Accept(visitor);
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newInstance = visitor.Visit(Instance);
            var newPositionalArguments = visitor.VisitAndConvert(_positionalArguments, nameof(VisitChildren));

            var newNamedArguments = new Dictionary<string, Expression>(_namedArguments.Count);
            var namedArgumentsChanged = false;
            foreach (var kv in _namedArguments)
            {
                var newExpression = visitor.Visit(kv.Value);
                if (newExpression != kv.Value)
                    namedArgumentsChanged = true;
                newNamedArguments[kv.Key] = newExpression;
            }

            return newInstance != Instance ||
                   newPositionalArguments != _positionalArguments ||
                   namedArgumentsChanged
                ? new PgFunctionExpression(newInstance, FunctionName, Schema, Type, newPositionalArguments, newNamedArguments)
                : this;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is PgFunctionExpression pgFunction && Equals(pgFunction);

        /// <inheritdoc />
        public bool Equals(PgFunctionExpression other)
            => other != null
               && Type == other.Type
               && string.Equals(FunctionName, other.FunctionName)
               && string.Equals(Schema, other.Schema)
               && _positionalArguments.SequenceEqual(other._positionalArguments)
               && _namedArguments.Count == other._namedArguments.Count
               && _namedArguments.All(kv => other._namedArguments.TryGetValue(kv.Key, out var otherValue) && kv.Value?.Equals(otherValue) == true)
               && (Instance == null && other.Instance == null || Instance?.Equals(other.Instance) == true);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _positionalArguments.Aggregate(0, (current, argument) => current + ((current * 397) ^ argument.GetHashCode()));
                hashCode = (hashCode * 397) ^ _namedArguments.Aggregate(0, (current, argument) =>
                               current + ((current * 397) ^ argument.Key.GetHashCode()) + ((current * 397) ^ argument.Value.GetHashCode()));
                hashCode = (hashCode * 397) ^ (Instance?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ FunctionName.GetHashCode();
                hashCode = (hashCode * 397) ^ (Schema?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Type.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
            => (Instance != null ? Instance + "." : Schema != null ? Schema + "." : "") +
               $"{FunctionName}({string.Join("", "", PositionalArguments)}" +
               (NamedArguments.Count > 0 ? ", " + string.Join("", "", NamedArguments.Select(a => $"{a.Key} => {a.Value}")) : "") +
               ")";
    }
}
