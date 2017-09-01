using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    public class DictionaryContainsKeyExpression : Expression
    {
        /// <summary>
        ///     Creates a new instance of expression that is used by EF translator to fetch 
        ///     HStore keys from database.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="dictionary"> The dictionary. </param>
        public DictionaryContainsKeyExpression(
            [NotNull] Expression key,
            [NotNull] Expression dictionary)
        {
            Check.NotNull(dictionary, nameof(dictionary));
            Check.NotNull(key, nameof(key));
            Debug.Assert(typeof(IDictionary<string, string>).IsAssignableFrom(dictionary.Type));

            Dictionary = dictionary;
            Key = key;
        }

        /// <summary>
        ///     Gets the dictionary.
        /// </summary>
        /// <value>
        ///     The dictionary.
        /// </value>
        public virtual Expression Dictionary { get; }

        /// <summary>
        ///     Gets the key.
        /// </summary>
        /// <value>
        ///     The key.
        /// </value>
        public virtual Expression Key { get; }

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type => typeof(bool);

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is NpgsqlQuerySqlGenerator npsgqlGenerator
                ? npsgqlGenerator.VisitDictionaryContainsKey(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(System.Linq.Expressions.Expression)" /> method passing the
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
        ///
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newDictionary = visitor.Visit(Dictionary);
            var newKey = visitor.Visit(Key);

            return newKey != Key || newDictionary != Dictionary
                ? new DictionaryContainsKeyExpression(newKey, newDictionary)
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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((DictionaryContainsKeyExpression)obj);
        }

        bool Equals(DictionaryContainsKeyExpression other)
            => Dictionary.Equals(other.Dictionary) && Key.Equals(other.Key);

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
                return (Key.GetHashCode() * 397) ^ Dictionary.GetHashCode();
            }
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString() => $"{Dictionary} ? {Key}";
    }
}
