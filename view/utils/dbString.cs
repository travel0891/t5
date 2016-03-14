
namespace view
{
    public class dbString
    {
        public static string getType(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "int": return "Int32";
                case "smallint": return "Int16";
                case "smalldatetime": return "DateTime";
                case "datetime": return "DateTime";
                case "char": return "String";
                case "varchar": return "String";
                case "text": return "String";
                case "nchar": return "String";
                case "nvarchar": return "String";
                case "ntext": return "String";
                case "uniqueidentifier": return "String";
                default: return "string";
            }
        }
    }
}