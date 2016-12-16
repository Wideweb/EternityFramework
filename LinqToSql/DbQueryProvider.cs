using EternityFramework.DataAccess;
using EternityFramework.LinqToSql.SqlQueryBuilder;
using EternityFramework.Utils;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EternityFramework.LinqToSql
{
    public class DbQueryProvider : IQueryProvider
    {
        private readonly DbQuery dbQuery;

        public DbQueryProvider(DbQuery dbQuery)
        {
            this.dbQuery = dbQuery;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(DbQueryable<>).MakeGenericType(elementType), new object[] { this, expression, dbQuery });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        // Queryable's collection-returning standard query operators call this method. 
        public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
        {
            return new DbQueryable<TResult>(this, expression, dbQuery);
        }

        public object Execute(Expression expression)
        {
            var interpretationResult = new LinqToSqlInterpretator().Visit(new MSSqlQueryBuilder(), Evaluator.PartialEval(expression));
            return dbQuery.Get(interpretationResult.SqlQuery, expression.Type);
        }

        // Queryable's "single value" standard query operators call this method.
        // It is also called from QueryableTerraServerData.GetEnumerator(). 
        public TResult Execute<TResult>(Expression expression)
        {
            var interpretationResult = new LinqToSqlInterpretator().Visit(new MSSqlQueryBuilder(), Evaluator.PartialEval(expression));
            var queryResult = dbQuery.Get(interpretationResult.SqlQuery, expression.Type);
            
            return (TResult)queryResult;
        }
    }
}
