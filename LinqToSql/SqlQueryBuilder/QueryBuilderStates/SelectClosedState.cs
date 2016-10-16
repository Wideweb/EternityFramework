using EternityFramework.Utils;
using System;

namespace EternityFramework.LinqToSql.SqlQueryBuilder.QueryBuilderStates
{
    public class SelectClosedState : ISqlQueryBuilderState
    {
        private readonly ISqlQueryBuilder sqlQueryBuilder;
        private readonly QeuryStateContext context;

        public SelectClosedState(ISqlQueryBuilder sqlQueryBuilder, QeuryStateContext context)
        {
            this.sqlQueryBuilder = sqlQueryBuilder;
            this.context = context;
        }

        public void AddBinaryOperation(string sign)
        {
            context.SqlQuery += $" {sign} ";
        }

        public void AddConstant(object constant)
        {
            var constantType = TypeSystem.GetElementType(constant.GetType());
            if (constantType.Equals(context.TableType))
            {
                context.SqlQuery += " * ";
            }

            context.CurrentProperty = constant.ToString();
            context.SqlQuery += WrapSqlConstant(constant);
        }

        private string WrapSqlConstant(object constant)
        {
            if (constant.GetType().Name == typeof(string).Name)
            {
                return $"'{constant}'";
            }

            return $"{constant}";
        }

        public void AddMember(string member)
        {
            context.SqlQuery += $" {context.CurrentTableAlias}.{member} ";
        }

        public void AddCurrentProperty()
        {
            context.SqlQuery += $" {context.CurrentProperty} ";
        }

        public void AddOrderBy(Type tableType, string property)
        {
            context.TableType = tableType;
            context.SqlQuery = $"SELECT TOP 100 PERCENT * FROM ({context.SqlQuery}) AS {context.CurrentTableAlias} ORDER BY {property}";
            context.GenerateNextTableAlias();
            context.OrderedByProperty = property;
        }

        public void AddSelect(Type tableType, string properties)
        {
            var propertiesToSelect = string.IsNullOrEmpty(properties) ? "*" : properties;
            context.TableType = tableType;
            context.SqlQuery = $" SELECT {propertiesToSelect} FROM ({context.SqlQuery}) AS {context.CurrentTableAlias}";
            context.GenerateNextTableAlias();
        }

        public void AddWhere(Type tableType, string condition)
        {
            context.TableType = tableType;
            context.SqlQuery = $"SELECT * FROM ({context.SqlQuery}) AS {context.CurrentTableAlias} WHERE {condition}";
            context.GenerateNextTableAlias();
        }

        public void AddAlias(string alias)
        {
            context.SqlQuery += $" AS [{alias}]";
        }

        public void AddComma()
        {
            context.SqlQuery += ",";
        }

        public void AddSkip(Type tableType, string count)
        {
            context.TableType = tableType;
            context.SqlQuery = $"SELECT * FROM ({context.SqlQuery}) AS {context.CurrentTableAlias} ORDER BY {context.OrderedByProperty} OFFSET {count} ROWS";
            context.GenerateNextTableAlias();
        }

        public void AddTake(Type tableType, string count)
        {
            context.TableType = tableType;
            context.SqlQuery = $"SELECT * FROM ({context.SqlQuery}) AS {context.CurrentTableAlias} ORDER BY {context.OrderedByProperty} OFFSET 0 ROWS FETCH NEXT {count} ROWS ONLY ";
            context.GenerateNextTableAlias();
        }
    }
}
