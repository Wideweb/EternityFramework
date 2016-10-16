using System;

namespace EternityFramework.LinqToSql.SqlQueryBuilder.QueryBuilderStates
{
    public class QeuryStateContext
    {
        private string orderedByProperty;
        private TableAliasProvider tableAliasProvider;

        public Type TableType { get; set; }
        public string CurrentTableAlias => tableAliasProvider.GetCurrentTableAlias();
        public string CurrentProperty { get; set; }
        public string SqlQuery { get; set; }
        public string OrderedByProperty
        {
            get { return $"{CurrentTableAlias}.{orderedByProperty}"; }
            set { orderedByProperty = tableAliasProvider.GetTableMemberName(value); }
        }

        public QeuryStateContext()
        {
            tableAliasProvider = new TableAliasProvider();
            orderedByProperty = "Id";
        }

        public string GenerateNextTableAlias()
        {
            return tableAliasProvider.GenerateNextTableAlias();
        }

        public QeuryStateContext Clone()
        {
            return new QeuryStateContext
            {
                CurrentProperty = this.CurrentProperty,
                tableAliasProvider = this.tableAliasProvider,
                TableType = this.TableType
            };
        }
    }
}
