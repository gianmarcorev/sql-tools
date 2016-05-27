
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

        public const string LOCALDB_CONN_STR = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ArubaTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;User ID=arubadbuser;Password=ArubaUniPR2016";
        public const string BULK_INSERT_CSV = @"BULK INSERT {0} FROM '{1}' WITH ( FIELDTERMINATOR = '{2}', FIRSTROW = {3});";
    }
}
