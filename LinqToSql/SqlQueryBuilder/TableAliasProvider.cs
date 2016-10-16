using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EternityFramework.LinqToSql.SqlQueryBuilder
{
    public class TableAliasProvider
    {
        private const string TableAliasFormat = "[table_{0}]";
        private const string MemberNameRegex = @"\[table_\d+\]\.\[(.+?)\]";

        private long currentTableAliasNumber = 0;

        public string GetCurrentTableAlias()
        {
            return string.Format(TableAliasFormat, currentTableAliasNumber);
        }

        public string GenerateNextTableAlias()
        {
            currentTableAliasNumber++;
            return GetCurrentTableAlias();
        }

        public string GetTableMemberName(string tableMember)
        {
            return new Regex(MemberNameRegex).Match(tableMember).Groups[1].Value;
        }
    }
}
