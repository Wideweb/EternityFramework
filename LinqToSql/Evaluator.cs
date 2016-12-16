using System;
using System.Linq.Expressions;

namespace EternityFramework.LinqToSql
{
    public static class Evaluator
    {
        public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        public static Expression PartialEval(Expression expression)
        {
           return PartialEval(expression, Evaluator.CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType == ExpressionType.Constant 
                || expression.NodeType == ExpressionType.MemberAccess;
        }
    }
}
