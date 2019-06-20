using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.EvaluatableExpressionFilters.Internal
{
    // TODO: This is a hack until https://github.com/aspnet/EntityFrameworkCore/issues/13454 is done
    public class NpgsqlPGroongaEvaluatableExpressionFilter : IEvaluatableExpressionFilter
    {
        /// <inheritdoc />
        public bool IsEvaluatableMethodCall(MethodCallExpression node)
            => node.Method.DeclaringType?.FullName != "Microsoft.EntityFrameworkCore.PGroongaDbFunctionsExtensions";

        bool IEvaluatableExpressionFilter.IsEvaluatableMember(MemberExpression node)
            => node.Member.DeclaringType?.FullName != "Microsoft.EntityFrameworkCore.PGroongaDbFunctionsExtensions";

        #region unused interface methods

        bool IEvaluatableExpressionFilter.IsEvaluatableBinary(BinaryExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableConditional(ConditionalExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableConstant(ConstantExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableElementInit(ElementInit node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableInvocation(InvocationExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableLambda(LambdaExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableListInit(ListInitExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableMemberAssignment(MemberAssignment node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableMemberInit(MemberInitExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableMemberListBinding(MemberListBinding node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableMemberMemberBinding(MemberMemberBinding node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableNew(NewExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableNewArray(NewArrayExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableTypeBinary(TypeBinaryExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableUnary(UnaryExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableBlock(BlockExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableCatchBlock(CatchBlock node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableDebugInfo(DebugInfoExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableDefault(DefaultExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableGoto(GotoExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableIndex(IndexExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableLabel(LabelExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableLabelTarget(LabelTarget node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableLoop(LoopExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableSwitch(SwitchExpression node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableSwitchCase(SwitchCase node) => true;
        bool IEvaluatableExpressionFilter.IsEvaluatableTry(TryExpression node) => true;

        #endregion
    }
}
