using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace model.utils
{
    public class helper
    {
        #region setParameter

        public static readonly String connectionString = ConfigurationManager.ConnectionStrings["mssqlConnection"].ConnectionString;

        private Boolean setParameter(IDbCommand command, IDbConnection connection, CommandType commandType, String commandText, IDbDataParameter[] dataParameter)
        {
            Boolean success = true;
            if (connection.State != ConnectionState.Open)
            {
                try
                {
                    connection.Open();
                }
                catch (SqlException sqlException)
                {
                    success = false;
                    throw sqlException;
                }
            }
            if (success)
            {
                command.Connection = connection;
                command.CommandText = commandText;
                command.CommandType = commandType;
                if (dataParameter != null)
                {
                    foreach (IDbDataParameter parameter in dataParameter)
                    {
                        command.Parameters.Add(parameter);
                    }
                }
            }
            return success;
        }

        #endregion

        #region executeReader

        public IDataReader executeReader(String connectionString, String commandText, IDbDataParameter[] dataParameter, CommandType commandType)
        {
            IDataReader dataReader = null;
            IDbCommand command = new SqlCommand();
            IDbConnection connection = new SqlConnection(connectionString);
            if (setParameter(command, connection, commandType, commandText, dataParameter))
            {
                dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                command.Parameters.Clear();
            }
            return dataReader;
        }

        #endregion

        #region executeDataSet

        public DataSet executeDataSet(String connectionString, String commandText, IDbDataParameter[] dataParameter, CommandType commandType)
        {
            IDataReader dataReader = null;
            IDbCommand command = new SqlCommand();
            IDbConnection connection = new SqlConnection(connectionString);
            if (setParameter(command, connection, commandType, commandText, dataParameter))
            {
                dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                command.Parameters.Clear();
            }
            DataSet dataSet = new DataSet();

            DataTable schemaTable = dataReader.GetSchemaTable();
            DataTable dataTable = new DataTable();

            if (schemaTable != null)
            {
                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    DataRow dataRow = schemaTable.Rows[i];
                    string columnName = (string)dataRow["columnName"];
                    DataColumn column = new DataColumn(columnName, (Type)dataRow["dataType"]);
                    dataTable.Columns.Add(column);
                }

                dataSet.Tables.Add(dataTable);

                while (dataReader.Read())
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        dataRow[i] = dataReader.GetValue(i);
                    }
                    dataTable.Rows.Add(dataRow);
                }
                dataReader.Close();
            }
            else
            {
                DataColumn column = new DataColumn("rowsAffected");
                dataTable.Columns.Add(column);
                dataSet.Tables.Add(dataTable);
                DataRow dataRow = dataTable.NewRow();
                dataRow[0] = dataReader.RecordsAffected;
                dataTable.Rows.Add(dataRow);
            }
            return dataSet;
        }

        #endregion

        #region executeNonQuery

        public Int32 executeNonQuery(String connectionString, String commandText, IDbDataParameter[] dataParameters, CommandType commandType)
        {
            Int32 effect = 0;
            IDbCommand command = new SqlCommand();
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                if (setParameter(command, connection, commandType, commandText, dataParameters))
                {
                    effect = command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                return effect;
            }
        }

        #endregion

        #region executeScalar

        public String executeScalarToString(String connectionString, String commandText, IDbDataParameter[] dataParameter, CommandType commandType)
        {
            String valString = null;
            IDbCommand command = new SqlCommand();
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                if (setParameter(command, connection, commandType, commandText, dataParameter))
                {
                    Object valObj = command.ExecuteScalar();
                    command.Parameters.Clear();
                    valString = (valObj == null ? null : valObj.ToString());
                }
                return valString;
            }
        }

        public Int32 executeScalarToInt(String connectionString, String commandText, IDbDataParameter[] dataParameter, CommandType commandType)
        {
            Int32 valInt = 0;
            IDbCommand command = new SqlCommand();
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                if (setParameter(command, connection, commandType, commandText, dataParameter))
                {
                    Object valObj = command.ExecuteScalar();
                    command.Parameters.Clear();
                    valInt = (valObj == null ? 0 : (Int32)valObj);
                }
                return valInt;
            }
        }

        #endregion

        private static helper h = null;

        public static helper instance()
        {
            return h == null ? new helper() : h;
        }
    }
}