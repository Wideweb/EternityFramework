using EternityFramework.DataAccess;
using EternityFramework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace EternityFramework.LinqToSql
{
    public class DbQueryable<TData> : IOrderedQueryable<TData>
    {
        private readonly DbQuery dbQuery;

        #region Constructors
        /// <summary> 
        /// This constructor is called by the client to create the data source. 
        /// </summary> 
        public DbQueryable(DbQuery dbQuery)
        {
            this.dbQuery = dbQuery;
            Provider = new DbQueryProvider(dbQuery);
            Expression = Expression.Constant(this);
        }

        /// <summary> 
        /// This constructor is called by Provider.CreateQuery(). 
        /// </summary> 
        /// <param name="expression"></param>
        public DbQueryable(DbQueryProvider provider, Expression expression, DbQuery dbQuery)
        {
            this.dbQuery = dbQuery;

            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (!typeof(IQueryable<TData>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }

            Provider = provider;
            Expression = expression;
        }
        #endregion

            #region Properties

        public IQueryProvider Provider { get; private set; }
        public Expression Expression { get; private set; }

        public Type ElementType
        {
            get { return typeof(TData); }
        }

        #endregion

        #region Enumerators
        public IEnumerator<TData> GetEnumerator()
        {
            return (Provider.Execute<IEnumerable<TData>>(Expression)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (Provider.Execute<System.Collections.IEnumerable>(Expression)).GetEnumerator();
        }
        #endregion

        public async Task Update(TData entity)
        {
            var entityType = entity.GetType();

            await dbQuery.ExecuteCommandAsync(
                $@"UPDATE [dbo].[{DbEntityHelper.GetDbTableName(entityType)}] SET
                    {DbEntityHelper.GetUpdatePropertiesString(entity)}
                   WHERE Id = @Id
                ", DbEntityHelper.GetSqlParameters(entity, excludeId: false).ToArray());
        }

        public Task<long> Add(TData entity)
        {
            var entityType = entity.GetType();

            return dbQuery.ExecuteInsertCommandAsync(
                $@"INSERT INTO [dbo].[{DbEntityHelper.GetDbTableName(entityType)}]
                (
                    {DbEntityHelper.GetPropertiesString(entityType)}
                ) 
                VALUES
                (
                    {DbEntityHelper.GetSqlParametersString(entity)}
                )", DbEntityHelper.GetSqlParameters(entity).ToArray());
        }
    }
}
