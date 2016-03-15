
namespace view
{
    public class dbString
    {
        public static string getCType(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "int":
                    return "Int32";
                case "smallint":
                    return "Int16";

                case "smalldatetime":
                    return "DateTime";
                case "datetime":
                    return "DateTime";

                case "char":
                    return "String";
                case "varchar":
                    return "String";
                case "text":
                    return "String";

                case "nchar":
                    return "String";
                case "nvarchar":
                    return "String";
                case "ntext":
                    return "String";

                case "uniqueidentifier":
                    return "String";
                default:
                    return "doCheck";
            }
        }

        public static string getDrType(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "int":
                    return "GetInt32";
                case "smallint":
                    return "GetInt16";

                case "smalldatetime":
                    return "GetDateTime";
                case "datetime":
                    return "GetDateTime";

                case "char":
                    return "GetString";
                case "varchar":
                    return "GetString";
                case "text":
                    return "GetString";

                case "nchar":
                    return "GetString";
                case "nvarchar":
                    return "GetString";
                case "ntext":
                    return "GetString";

                case "uniqueidentifier":
                    return "GetGuid";
                default:
                    return "doCheck";
            }
        }

        public static string changeChar(string inChar)
        {
            return inChar.Substring(0, 1).ToUpper() + inChar.Substring(1);
        }
    }
}