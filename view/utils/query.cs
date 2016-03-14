using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace view
{
    public class query
    {
        #region method

        private static readonly String
            IDENTITY1 = "intId"
            , IDENTITY2 = "charId"
            , AUTOFIEID = "createTime";

        public IDataReader dataReader(String commandText, IDbDataParameter[] dataParameter)
        {
            return helper.instance().ExecuteReader(helper.connectionString, commandText, dataParameter, CommandType.Text);
        }

        public String scalarString(String commandText, IDbDataParameter[] dataParameter)
        {
            return helper.instance().ExecuteScalarToString(helper.connectionString, commandText, dataParameter, CommandType.Text);
        }

        public Int32 scalarInt(String commandText, IDbDataParameter[] dataParameter)
        {
            return helper.instance().ExecuteScalarToInt(helper.connectionString, commandText, dataParameter, CommandType.Text);
        }

        #endregion

        private static query q = null;

        private query() { }

        public static query instance()
        {
            return q == null ? new query() : q;
        }
    }
}