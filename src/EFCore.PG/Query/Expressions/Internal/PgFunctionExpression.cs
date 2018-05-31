using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a SQL function call expression, supporting PostgreSQL's named parameter notation.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class PgFunctionExpression : Expression
    {
        readonly ReadOnlyCollection<Expression> _positionalArguments;
        readonly ReadOnlyDictionary<string, Expression> _namedArguments;

        static readonly ReadOnlyDictionary<string, Expression> EmptyNamedArguments =
            new ReadOnlyDictionary<string, Expression>(new Dictionary<string, Expression>());

            //new ReadOnlyDictionaryDictionary<string, Expression>();
        /// <summary>
        ///     Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName"> Name of the function. </param>
        /// <param name="returnType"> The return type. </param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType)
            : this(
                  Check.NotEmpty(functionName, nameof(functionName)),
                  Check.NotNull(returnType, nameof(returnType)),
                  Enumerable.Empty<Expression>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName"> Name of the function. </param>
        /// <param name="returnType"> The return type. </param>
        /// <param name="positionalArguments"> The positional arguments. </param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [NotNull] IEnumerable<Expression> positionalArguments)
            : this(
                  Check.NotEmpty(functionName, nameof(functionName)),
                  Check.NotNull(returnType, nameof(returnType)),
                  /*schema*/ null,
                  Check.NotNull(positionalArguments, nameof(positionalArguments)))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName"> Name of the function. </param>
        /// <param name="returnType"> The return type. </param>
        /// <param name="namedArguments"> The namedarguments. </param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [NotNull] IDictionary<string, Expression> namedArguments)
            : this(
                /*instance*/ null,
                Check.NotEmpty(functionName, nameof(functionName)),
                /*schema*/ null,
                Check.NotNull(returnType, nameof(returnType)),
                Enumerable.Empty<Expression>(),
                namedArguments)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName"> Name of the function. </param>
        /// <param name="schema"> The schema this function exists in if any. </param>
        /// <param name="returnType"> The return type. </param>
        /// <param name="positionalArguments"> The positional arguments. </param>
        public PgFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [CanBeNull] string schema,
            [NotNull] IEnumerable<Expression> positionalArguments)
            : this(
                /*instance*/ null,
                Check.NotEmpty(functionName, nameof(functionName)),
                schema,
                Check.NotNull(returnType, nameof(returnType)),
                Check.NotNull(positionalArguments, nameof(positionalArguments)),
                EmptyNamedArguments)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="instance"> The instance on which the function is called. </param>
        /// <param name="functionName"> Name of the function. </param>
        /// <param name="returnType"> The return type. </param>
        /// <param name="positionalArguments"> The positional arguments. </param>
        public PgFunctionExpression(
            [NotNull] Expression instance,
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [NotNull] IEnumerable<Expression> positionalArguments)
            : this(
                  Check.NotNull(instance, nameof(instance)),
                  Check.NotEmpty(functionName, nameof(functionName)),
                  /*schema*/ null,
                  Check.NotNull(returnType, nameof(returnType)),
                  Check.NotNull(positionalArguments, nameof(positionalArguments)),
                  EmptyNamedArguments)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PgFunctionExpression" /> class.
        /// </summary>
        /// <param name="instance"> The instance on which the function is called. </param>
        /// <param name="functionName"> Name of the function. </param>
        /// <param name="returnType"> The return type. </param>
        /// <param name="positionalArguments"> The positional arguments. </param>
        /// <param name="namedArguments"> The named arguments. </param>
        public PgFunctionExpression(
            [NotNull] Expression instance,
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [NotNull] IEnumerable<Expression> positionalArguments,
            [NotNull] IDictionary<string, Expression> namedArguments)
            : this(
                Check.NotNull(instance, nameof(instance)),
                Check.NotEmpty(functionName, nameof(functionName)),
                /*schema*/ null,
                Check.NotNull(returnType, nameof(returnType)),
                Check.NotNull(positionalArguments, nameof(positionalArguments)),
                namedArguments)
        {
        }

        PgFunctionExpression(
            [CanBeNull] Expression instance,
            [NotNull] string functionName,
            [CanBeNull] string schema,
            [NotNull] Type returnType,
            [NotNull] IEnumerable<Expression> positionalArguments,
            [NotNull] IDictionary<string, Expression> namedArguments)
        {
            Instance = instance;
            FunctionName = functionName;
            Type = returnType;
            Schema = schema;
            _positionalArguments = positionalArguments.ToList().AsReadOnly();
            _namedArguments = new ReadOnlyDictionary<string, Expression>(namedArguments);
        }

        /// <summary>
        ///     Gets the name of the function.
        /// </summary>
        /// <value>
        ///     The name of the function.
        /// </value>
        public virtual string FunctionName { get; }

        /// <summary>
        ///     Gets the name of the schema.
        /// </summary>
        /// <value>
        ///     The name of the schema.
        /// </value>
        public virtual string Schema { get; }

        /// <summary>
        ///     The instance.
        /// </summary>
        public virtual Expression Instance { get; }

        /// <summary>
        ///     The positional arguments.
        /// </summary>
        public virtual IReadOnlyList<Expression> PositionalArguments => _positionalArguments;

        /// <summary>
        ///     The named arguments.
        /// </summary>
        public virtual IReadOnlyDictionary<string, Expression> NamedArguments => _namedArguments;

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type { get; }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is NpgsqlQuerySqlGenerator specificVisitor
                ? specificVisitor.VisitPgFunction(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(Expression)" /> method passing the
        ///     reduced expression.
        ///     Throws an exception if the node isn't reducible.
        /// </summary>
        /// <param name="visitor"> An instance of <see cref="ExpressionVisitor" />. </param>
        /// <returns> The expression being visited, or an expression which should replace it in the tree. </returns>
        /// <remarks>
        ///     Override this method to provide logic to walk the node's children.
        ///     A typical implementation will call visitor.Visit on each of its
        ///     children, and if any of them change, should return a new copy of
        ///     itself with the modified children.
        /// </remarks>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newInstance = Instance != null ? visitor.Visit(Instance) : null;
            var newPositionalArguments = visitor.VisitAndConvert(_positionalArguments, nameof(VisitChildren));

            var newNamedArguments = new Dictionary<string, Expression>();
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

        /// <summary>
        ///     Tests if this object is considered equal to another.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns>
        ///     true if the objects are considered equal, false if they are not.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((PgFunctionExpression)obj);
        }

        bool Equals(PgFunctionExpression other)
            => Type == other.Type
               && string.Equals(FunctionName, other.FunctionName)
               && string.Equals(Schema, other.Schema)
               && _positionalArguments.SequenceEqual(other._positionalArguments)
               && _namedArguments.Count == other._namedArguments.Count
               && _namedArguments.All(kv => other._namedArguments.TryGetValue(kv.Key, out var otherValue) && kv.Value.Equals(otherValue))
               && (Instance == null && other.Instance == null
                    || Instance?.Equals(other.Instance) == true);



        /// <summary>
        ///     Returns a hash code for this object.
        /// </summary>
        /// <returns>
        ///     A hash code for this object.
        /// </returns>
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

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString()
            => (Instance != null ? Instance + "." : Schema != null ? Schema + "." : "") +
            $"{FunctionName}({string.Join("", "", PositionalArguments)}" +
            (NamedArguments.Count > 0 ? ", " + (string.Join("", "", NamedArguments.Select(a => $"{a.Key} => {a.Value}"))) : "") +
            ")";
    }
}
