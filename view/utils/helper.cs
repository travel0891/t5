using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace utils
{
    class helper
    {
        #region method

        #region SetParameter

        public static readonly String connectionString = ConfigurationManager.ConnectionStrings["mssqlConnection"].ConnectionString;

        private Boolean SetParameter(IDbCommand command, IDbConnection connection, IDbTransaction transaction, CommandType commandType, String commandText, IDbDataParameter[] dataParameters)
        {
            Boolean isSuccess = true;
            if (connection.State != ConnectionState.Open)
            {
                try
                {
                    connection.Open();
                }
                catch (SqlException sqlException)
                {
                    isSuccess = false;
                    throw sqlException;
                }
            }
            if (isSuccess)
            {
                command.Connection = connection;
                command.CommandText = commandText;
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }
                command.CommandType = commandType;
                if (dataParameters != null)
                {
                    foreach (IDbDataParameter dataParameter in dataParameters)
                    {
                        command.Parameters.Add(dataParameter);
                    }
                }
            }
            return isSuccess;
        }

        #endregion

        #region ExecuteReader

        public IDataReader ExecuteReader(String connectionString, String commandText, IDbDataParameter[] dataParameter, CommandType commandType)
        {
            IDataReader dataReader = null;
            IDbCommand command = new SqlCommand();
            IDbConnection connection = new SqlConnection(connectionString);
            if (SetParameter(command, connection, null, commandType, commandText, dataParameter))
            {
                dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                command.Parameters.Clear();
            }
            return dataReader;
        }

        #endregion

        #region ExecuteScalar

        public String ExecuteScalarToString(String connectionString, String commandText, IDbDataParameter[] dataParameter, CommandType commandType)
        {
            String valString = null;
            IDbCommand command = new SqlCommand();
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                if (SetParameter(command, connection, null, commandType, commandText, dataParameter))
                {
                    Object valObj = command.ExecuteScalar();
                    command.Parameters.Clear();
                    valString = (valObj == null ? null : valObj.ToString());
                }
                return valString;
            }
        }

        public Int32 ExecuteScalarToInt(String connectionString, String commandText, IDbDataParameter[] dataParameter, CommandType commandType)
        {
            Int32 valInt = 0;
            IDbCommand command = new SqlCommand();
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                if (SetParameter(command, connection, null, commandType, commandText, dataParameter))
                {
                    Object valObj = command.ExecuteScalar();
                    command.Parameters.Clear();
                    valInt = (valObj == null ? 0 : (Int32)valObj);
                }
                return valInt;
            }
        }

        #endregion

        #endregion

        private static helper h = null;

        private helper() { }

        public static helper GetInstance()
        {
            return h == null ? new helper() : h;
        }
    }
}