using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sql_tools
{
    static class Constants
    {
        public const string CHECK_TABLE = @"select case when exists((select * from information_schema.tables where table_name = '{0}')) then 1 else 0 end";
        public const string CREATE_TABLE_BODY = @"CREATE TABLE {0}.{1} ({2})";

        const int SQL_VARCHAR_LENGTH = 250;
        public static string getSqlType(string type) {
            switch (type.ToLower()) {
                case "string": return "varchar(" + SQL_VARCHAR_LENGTH + ")";
                case "int32": return "int";
                case "decimal": return "decimal"; 
                default: return null;
            }
        }
    }
}
