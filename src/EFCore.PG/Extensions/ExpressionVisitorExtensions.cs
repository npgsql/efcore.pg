using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Linq.Expressions;

[DebuggerStepThrough]
internal static class ExpressionVisitorExtensions
{
    /// <summary>
    ///     Dispatches the list of expressions to one of the more specialized visit methods in this class.
    /// </summary>
    /// <param name="visitor">The expression visitor.</param>
    /// <param name="nodes">The expressions to visit.</param>
    /// <returns>
    ///     The modified expression list, if any of the elements were modified; otherwise, returns the original expression list.
    /// </returns>
    public static IReadOnlyList<T> Visit<T>(this ExpressionVisitor visitor, IReadOnlyList<T> nodes)
        where T : Expression
    {
        T[]? newNodes = null;
        for (int i = 0, n = nodes.Count; i < n; i++)
        {
            var node = (T)visitor.Visit(nodes[i]);

            if (newNodes is not null)
            {
                newNodes[i] = node;
            }
            else if (!ReferenceEquals(node, nodes[i]))
            {
                newNodes = new T[n];
                for (var j = 0; j < i; j++)
                {
                    newNodes[j] = nodes[j];
                }

                newNodes[i] = node;
            }
        }

        return newNodes ?? nodes;
    }

    /// <summary>
    ///     Visits an expression, casting the result back to the original expression type.
    /// </summary>
    /// <typeparam name="T">The type of the expression.</typeparam>
    /// <param name="visitor">The expression visitor.</param>
    /// <param name="nodes">The expression to visit.</param>
    /// <param name="callerName">The name of the calling method; used to report to report a better error message.</param>
    /// <returns>
    ///     The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
    /// </returns>
    /// <exception cref="InvalidOperationException">The visit method for this node returned a different type.</exception>
    public static IReadOnlyList<T> VisitAndConvert<T>(
        this ExpressionVisitor visitor,
        IReadOnlyList<T> nodes,
        [CallerMemberName] string? callerName = null)
        where T : Expression
    {
        T[]? newNodes = null;
        for (int i = 0, n = nodes.Count; i < n; i++)
        {
            if (visitor.Visit(nodes[i]) is not T node)
            {
                throw new InvalidOperationException(CoreStrings.MustRewriteToSameNode(callerName, typeof(T).Name));
            }

            if (newNodes is not null)
            {
                newNodes[i] = node;
            }
            else if (!ReferenceEquals(node, nodes[i]))
            {
                newNodes = new T[n];
                for (var j = 0; j < i; j++)
                {
                    newNodes[j] = nodes[j];
                }

                newNodes[i] = node;
            }
        }

        return newNodes ?? nodes;
    }

    /// <summary>
    ///     Visits all nodes in the collection using a specified element visitor.
    /// </summary>
    /// <typeparam name="T">The type of the nodes.</typeparam>
    /// <param name="visitor">The expression visitor.</param>
    /// <param name="nodes">The nodes to visit.</param>
    /// <param name="elementVisitor">
    ///     A delegate that visits a single element,
    ///     optionally replacing it with a new element.
    /// </param>
    /// <returns>
    ///     The modified node list, if any of the elements were modified;
    ///     otherwise, returns the original node list.
    /// </returns>
    public static IReadOnlyList<T> Visit<T>(this ExpressionVisitor visitor, IReadOnlyList<T> nodes, Func<T, T> elementVisitor)
    {
        T[]? newNodes = null;
        for (int i = 0, n = nodes.Count; i < n; i++)
        {
            var node = elementVisitor(nodes[i]);
            if (newNodes is not null)
            {
                newNodes[i] = node;
            }
            else if (!ReferenceEquals(node, nodes[i]))
            {
                newNodes = new T[n];
                for (var j = 0; j < i; j++)
                {
                    newNodes[j] = nodes[j];
                }

                newNodes[i] = node;
            }
        }

        return newNodes ?? nodes;
    }
}
