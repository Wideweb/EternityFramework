using EternityFramework.DataAccess;
using EternityFramework.LinqToSql.SqlQueryBuilder;
using EternityFramework.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            Type elementType = TypeSystem.GetElementType(expression.Type);
            var sqlQuery = new LinqToSqlInterpretator().Visit(new MSSqlQueryBuilder(), expression);
            var queryResult = dbQuery.ExecuteReader(sqlQuery, sdr => {
                var data = new List<object>();
                while (sdr.Read())
                {
                    data.Add(Load(sdr, elementType));
                }
                return data;
            }).AsQueryable();

            return queryResult.Provider.Execute(queryResult.Expression);
        }

        // Queryable's "single value" standard query operators call this method.
        // It is also called from QueryableTerraServerData.GetEnumerator(). 
        public TResult Execute<TResult>(Expression expression)
        {
            bool IsEnumerable = (typeof(TResult).Name == "IEnumerable`1");
            Type elementType = TypeSystem.GetElementType(expression.Type);
            var sqlQuery = new LinqToSqlInterpretator().Visit(new MSSqlQueryBuilder(), expression);
            var queryResult = dbQuery.ExecuteReader(sqlQuery, sdr => {
                var data = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                while (sdr.Read())
                {
                    data.Add(Load(sdr, elementType));
                }
                return data;
            }).AsQueryable();

            var exp = queryResult.Expression;
            if (expression.NodeType == ExpressionType.Call)
            {
                var node = expression as MethodCallExpression;
                if(node.Method.Name == "First" || node.Method.Name == "FirstOrDefault" ||
                    node.Method.Name == "Single" || node.Method.Name == "SingleOrDefault")
                {
                    exp = Expression.Call(node.Method, Expression.Constant(queryResult));
                }
            }
            
            if (IsEnumerable)
                return (TResult)queryResult.Provider.CreateQuery(exp);
            else
                return (TResult)queryResult.Provider.Execute(exp);
        }

        public object Load(SqlDataReader sqlDataReader, Type tableType)
        {
            try
            {
                var instance = Activator.CreateInstance(tableType);
                var properties = tableType
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(it => it.CanWrite);

                foreach (var property in properties)
                {
                    property.SetValue(instance, sqlDataReader[property.Name]);
                }
                return instance;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
