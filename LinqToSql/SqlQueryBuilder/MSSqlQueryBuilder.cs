using System;
using EternityFramework.LinqToSql.SqlQueryBuilder.QueryBuilderStates;

namespace EternityFramework.LinqToSql.SqlQueryBuilder
{
    public class MSSqlQueryBuilder : ISqlQueryBuilder
    {
        private QeuryStateContext context;
        private ISqlQueryBuilderState state;

        public MSSqlQueryBuilder() : this(new QeuryStateContext())
        { }

        public MSSqlQueryBuilder(QeuryStateContext context)
        {
            this.context = context;
            state = new InitQueryState(this, context);
        }

        public MSSqlQueryBuilder(ISqlQueryBuilderState state)
        {
            this.state = state;
        }

        public ISqlQueryBuilder Clone()
        {
            return new MSSqlQueryBuilder(context.Clone());
        }

        public void AddBinaryOperation(string sign)
        {
            state.AddBinaryOperation(sign);
        }

        public void AddConstant(object constant)
        {
            state.AddConstant(constant);
        }

        public void AddMember(string member)
        {
            state.AddMember(member);
        }

        public void AddCurrentProperty()
        {
            state.AddCurrentProperty();
        }

        public void AddOrderBy(Type tableType, string property)
        {
            state.AddOrderBy(tableType, property);
        }

        public void AddSelect(Type tableType, string properties)
        {
            state.AddSelect(tableType, properties);
        }

        public void AddWhere(Type tableType, string condition)
        {
            state.AddWhere(tableType, condition);
        }

        public void AddAlias(string alias)
        {
            state.AddAlias(alias);
        }

        public string GetSqlQuery()
        {
            return context.SqlQuery;
        }

        public void SetState(ISqlQueryBuilderState state)
        {
            this.state = state;
        }

        public void AddSkip(Type tableType, string count)
        {
            state.AddSkip(tableType, count);
        }

        public void AddTake(Type tableType, string count)
        {
            state.AddTake(tableType, count);
        }

        public void AddComma()
        {
            state.AddComma();
        }
    }
}