using EternityFramework.Utils;
using System;
using System.Linq.Expressions;

namespace EternityFramework.LinqToSql
{
    public class LinqToSqlInterpretator : ExpressionVisitor
    {
        private ISqlQueryBuilder sqlQueryBuilder;
        
        public string Visit(ISqlQueryBuilder sqlQueryBuilder, Expression node)
        {
            this.sqlQueryBuilder = sqlQueryBuilder;
            Visit(node);
            return this.sqlQueryBuilder.GetSqlQuery();
        }
        
        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;
            
            Console.WriteLine($"{node.NodeType}");
            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Arguments[0].NodeType == ExpressionType.Call)
            {
                this.Visit(node.Arguments[0]);
            }

            if (node.Method.Name.Equals("FirstOrDefault", StringComparison.OrdinalIgnoreCase) ||
                node.Method.Name.Equals("First", StringComparison.OrdinalIgnoreCase) ||
                node.Method.Name.Equals("SingleOrDefault", StringComparison.OrdinalIgnoreCase) ||
                node.Method.Name.Equals("Single", StringComparison.OrdinalIgnoreCase))
            {
                sqlQueryBuilder.AddTake(TypeSystem.GetElementType(node.Arguments[0].Type), "1");
            }

            string arg = null;
            if(node.Arguments.Count == 2)
            {
                arg = new LinqToSqlInterpretator().Visit(sqlQueryBuilder.Clone(), node.Arguments[1]);
            }
            
            if (node.Method.Name.Equals("Where", StringComparison.OrdinalIgnoreCase))
            {
                sqlQueryBuilder.AddWhere(TypeSystem.GetElementType(node.Arguments[0].Type), arg);
            }

            if (node.Method.Name.Equals("OrderBy", StringComparison.OrdinalIgnoreCase))
            {
                sqlQueryBuilder.AddOrderBy(TypeSystem.GetElementType(node.Arguments[0].Type), arg);
            }

            if (node.Method.Name.Equals("Skip", StringComparison.OrdinalIgnoreCase))
            {
                sqlQueryBuilder.AddSkip(TypeSystem.GetElementType(node.Arguments[0].Type), arg);
            }

            if (node.Method.Name.Equals("Take", StringComparison.OrdinalIgnoreCase))
            {
                sqlQueryBuilder.AddTake(TypeSystem.GetElementType(node.Arguments[0].Type), arg);
            }

            if (node.Method.Name.Equals("Select", StringComparison.OrdinalIgnoreCase))
            {
                sqlQueryBuilder.AddSelect(TypeSystem.GetElementType(node.Arguments[0].Type), arg);
            }

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            sqlQueryBuilder.AddMember(node.Member.Name);
            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (TypeSystem.IsTypeAssignable(node.Value.GetType(), typeof(DbQueryable<>)))
            {
                sqlQueryBuilder.AddSelect(TypeSystem.GetElementType(node.Value.GetType()), null);
            }else
            {
                sqlQueryBuilder.AddConstant(node.Value);
            }

            return base.VisitConstant(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var sign = string.Empty;
            switch (node.NodeType) {
                case ExpressionType.Equal:
                    sign = "=";
                    break;
                case ExpressionType.GreaterThan:
                    sign = ">";
                    break;
                case ExpressionType.LessThan:
                    sign = "<";
                    break;
                case ExpressionType.AndAlso:
                    sign = "AND";
                    break;
                case ExpressionType.OrElse:
                    sign = "OR";
                    break;
            }


            if(node.Left.NodeType == ExpressionType.Parameter)
            {
                sqlQueryBuilder.AddCurrentProperty();
            }else
            {
                Visit(sqlQueryBuilder, node.Left);
            }

            sqlQueryBuilder.AddBinaryOperation(sign);
            Visit(sqlQueryBuilder, node.Right);
            
            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var isFirstParameter = true;
            foreach(MemberAssignment b in node.Bindings)
            {
                if (!isFirstParameter)
                {
                    sqlQueryBuilder.AddComma();
                }

                Visit(b.Expression);
                sqlQueryBuilder.AddAlias(b.Member.Name);

                isFirstParameter = false;
            }

            return node;
        }
    }
}
