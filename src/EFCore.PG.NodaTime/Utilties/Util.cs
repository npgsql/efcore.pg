namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Utilties;

internal static class Util
{
    internal static NewExpression ConstantNew(ConstructorInfo constructor, params object[] parameters)
        => Expression.New(constructor, parameters.Select(p => Expression.Constant(p)).ToArray());

    internal static MethodCallExpression ConstantCall(MethodInfo method, params object[] parameters)
        => Expression.Call(method, parameters.Select(p => Expression.Constant(p)).ToArray());
}