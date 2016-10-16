using EternityFramework.LinqToSql.SqlQueryBuilder.QueryBuilderStates;
using System;

namespace EternityFramework.LinqToSql
{
    public interface IQueryBuilder
    {
        void AddSelect(Type tableType, string properties);
        void AddWhere(Type tableType, string condition);
        void AddOrderBy(Type tableType, string property);
        void AddSkip(Type tableType, string count);
        void AddTake(Type tableType, string count);
        void AddBinaryOperation(string operation);
        void AddConstant(object constant);
        void AddMember(string member);
        void AddCurrentProperty();
        void AddAlias(string alias);
        void AddComma();
    }

    public interface ISqlQueryBuilderState : IQueryBuilder
    {
    }

    public interface ISqlQueryBuilder: IQueryBuilder
    {
        void SetState(ISqlQueryBuilderState state);
        ISqlQueryBuilder Clone();
        string GetSqlQuery();
    }
}
