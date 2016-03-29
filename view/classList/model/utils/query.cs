using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace model.utils
{
    internal class query
    {
        private static readonly String IDENTITY1 = "intId", IDENTITY2 = "charId";

        #region string

        public String singleInsertString(baseTable table, out IDbDataParameter[] parameters)
        {
            String commandText = String.Empty;
            StringBuilder tableField = new StringBuilder(), parameterField = new StringBuilder();

            List<IDbDataParameter> listParameter = new List<IDbDataParameter>();

            Type type = table.GetType();

            foreach (PropertyInfo field in type.GetProperties())
            {
                if (field.GetValue(table, null) != null && field.Name != IDENTITY1 && field.Name != IDENTITY2)
                {
                    tableField.Append(field.Name + ",");
                    parameterField.Append("@" + field.Name + ",");
                    listParameter.Add(new SqlParameter("@" + field.Name, field.GetValue(table, null)));
                }
            }

            commandText = String.Format("insert into {0}({1}) values ({2});", type.Name, tableField.ToString().Trim(','), parameterField.ToString().Trim(','));
            parameters = listParameter.ToArray();

            return commandText;
        }

        public String singleUpdateString(baseTable table, out IDbDataParameter[] parameters)
        {
            String commandText = String.Empty;
            StringBuilder tableField = new StringBuilder();

            List<IDbDataParameter> listParameter = new List<IDbDataParameter>();

            Type type = table.GetType();

            foreach (PropertyInfo field in type.GetProperties())
            {
                if (field.GetValue(table, null) != null)
                {
                    if (field.Name != IDENTITY1 && field.Name != IDENTITY2)
                    {
                        tableField.Append(field.Name + " = @" + field.Name + ",");
                    }
                    listParameter.Add(new SqlParameter("@" + field.Name, field.GetValue(table, null)));
                }
            }

            commandText = String.Format(" update {0} set {1} where {2} = @{2} ", type.Name, tableField.ToString().Trim(','), IDENTITY2);
            parameters = listParameter.ToArray();
            return commandText;
        }

        public String singleDeletString(baseTable table, out IDbDataParameter[] parameters)
        {
            String commandText = String.Empty;
            Type type = table.GetType();

            IDbDataParameter[] parameter = { new SqlParameter("@" + IDENTITY2, table.charId) };

            commandText = String.Format(" delete from {0} where {1} = @{1} ", type.Name, IDENTITY2);
            parameters = parameter;
            return commandText;
        }

        public String batchInsertString(out IDbDataParameter[] parameters, params baseTable[] tables)
        {
            String tableName = String.Empty, tranCommandText = String.Empty;
            StringBuilder commandText = new StringBuilder();
            List<IDbDataParameter> listParameter = new List<IDbDataParameter>();
            foreach (baseTable table in tables)
            {
                StringBuilder tableField = new StringBuilder(), parameterField = new StringBuilder();
                Type type = table.GetType();
                tableName = type.Name;
                foreach (PropertyInfo field in type.GetProperties())
                {
                    if (field.GetValue(table, null) != null && field.Name != IDENTITY1)
                    {
                        String tempGUID = Guid.NewGuid().ToString("N");
                        tableField.Append(field.Name + ",");
                        parameterField.Append("@" + field.Name + tempGUID + ",");
                        listParameter.Add(new SqlParameter("@" + field.Name + tempGUID, field.GetValue(table, null)));
                    }
                }
                commandText.AppendFormat(" insert into {0}({1}) values ({2}); ", tableName, tableField.ToString().Trim(','), parameterField.ToString().Trim(','));
            }
            tranCommandText = string.Format("{0} {1} {2}", "begin tran", commandText, "if @@error<>0 begin rollback tran end else begin commit tran end");
            parameters = listParameter.ToArray();

            return tranCommandText;
        }

        public String batchUpdateString(out IDbDataParameter[] parameters, params baseTable[] tables)
        {
            String tableName = String.Empty, tranCommandText = String.Empty;
            StringBuilder commandText = new StringBuilder(), tranRowCount = new StringBuilder();
            List<IDbDataParameter> listParameter = new List<IDbDataParameter>();
            foreach (baseTable table in tables)
            {
                String tempGUID = Guid.NewGuid().ToString("N");
                StringBuilder temp1 = new StringBuilder();
                Type type = table.GetType();
                foreach (PropertyInfo field in type.GetProperties())
                {
                    if (field.GetValue(table, null) != null)
                    {
                        if (field.Name != IDENTITY1 && field.Name != IDENTITY2)
                        {
                            temp1.Append(field.Name + " = @" + field.Name + tempGUID + ",");
                        }
                        listParameter.Add(new SqlParameter("@" + field.Name + tempGUID, field.GetValue(table, null)));
                    }
                }
                commandText.AppendFormat(" declare @row{3} int;update {0} set {1} where {2} = @{2}{3}; set @row{3}=@@rowcount; ", type.Name, temp1.ToString().Trim(','), IDENTITY2, tempGUID);
                tranRowCount.AppendFormat(" or @row{0} = 0 ", tempGUID);
            }
            tranCommandText = string.Format("{0} {1} {2} {3} {4}", "begin tran", commandText, "if @@error<>0", tranRowCount, "begin rollback tran end else begin commit tran end");
            parameters = listParameter.ToArray();
            return tranCommandText;
        }

        public String batchDeletString(out IDbDataParameter[] parameters, params baseTable[] tables)
        {
            String tranCommandText = String.Empty;
            StringBuilder commandText = new StringBuilder(), tranRowCount = new StringBuilder();
            List<IDbDataParameter> listParameter = new List<IDbDataParameter>();
            foreach (baseTable table in tables)
            {
                Type type = table.GetType();
                String tempGUID = Guid.NewGuid().ToString("N");

                listParameter.Add(new SqlParameter("@" + IDENTITY2 + tempGUID, table.charId));
                commandText.AppendFormat(" declare @row{2} int; delete from {0} where {1} = @{1}{2}; set @row{2}=@@rowcount;", type.Name, IDENTITY2, tempGUID);
                tranRowCount.AppendFormat(" or @row{0} = 0 ", tempGUID);
            }

            tranCommandText = string.Format("{0} {1} {2} {3} {4}", "begin tran", commandText, "if @@error<>0", tranRowCount, "begin rollback tran end else begin commit tran end");
            parameters = listParameter.ToArray();
            return tranCommandText.ToString();
        }

        public String batchBlendString(out IDbDataParameter[] parameters, params Object[] objects)
        {
            parameters = null;
            return null;
        }

        #endregion

        #region execute

        public Int32 insert(params baseTable[] tables)
        {
            String commandText = String.Empty;
            IDbDataParameter[] parameters = null;
            if (tables != null && tables.Length > 0)
            {
                if (tables.Length == 1)
                {
                    commandText = singleInsertString(tables[0], out parameters);
                }
                else
                {
                    commandText = batchInsertString(out parameters, tables);
                }
            }
            return helper.instance().executeNonQuery(helper.connectionString, commandText, parameters, CommandType.Text);
        }

        public Int32 update(params baseTable[] tables)
        {
            String commandText = String.Empty;
            IDbDataParameter[] parameters = null;
            if (tables != null && tables.Length > 0)
            {
                if (tables.Length == 1)
                {
                    commandText = singleUpdateString(tables[0], out parameters);
                }
                else
                {
                    commandText = batchUpdateString(out parameters, tables);
                }
            }
            return helper.instance().executeNonQuery(helper.connectionString, commandText, parameters, CommandType.Text);
        }

        public Int32 delete(params baseTable[] tables)
        {
            String commandText = String.Empty;
            IDbDataParameter[] parameters = null;
            if (tables != null && tables.Length > 0)
            {
                if (tables.Length == 1)
                {
                    commandText = singleDeletString(tables[0], out parameters);
                }
                else
                {
                    commandText = batchDeletString(out parameters, tables);
                }
            }
            return helper.instance().executeNonQuery(helper.connectionString, commandText, parameters, CommandType.Text);
        }

        public Int32 blend(params Object[] tables)
        {
            String commandText = String.Empty;
            IDbDataParameter[] parameters = null;

            commandText = batchBlendString(out parameters, tables);

            return helper.instance().executeNonQuery(helper.connectionString, commandText, parameters, CommandType.Text);
        }

        #endregion

        #region data

        public IDbDataParameter[] builderParameter(out String whereSQL, params Object[] parameters)
        {
            StringBuilder sbSQL = new StringBuilder();
            List<IDbDataParameter> listParameter = new List<IDbDataParameter>();
            if (parameters != null && parameters.Length > 0 && parameters.Length % 4 == 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    sbSQL.Append((i > 0 ? parameters[i] : "where") + " " + parameters[i + 1] + " " + parameters[i + 2] + " @" + parameters[i + 1] + " ");
                    listParameter.Add(new SqlParameter(parameters[i + 1].ToString(), parameters[i + 3]));
                    i += 3;
                }
            }
            whereSQL = sbSQL.ToString();
            return listParameter.ToArray();
        }

        public IDataReader dataReader(String commandText, IDbDataParameter[] dataParameter)
        {
            return helper.instance().executeReader(helper.connectionString, commandText, dataParameter, CommandType.Text);
        }

        public DataSet dataSet(String commandText, IDbDataParameter[] dataParameter)
        {
            return helper.instance().executeDataSet(helper.connectionString, commandText, dataParameter, CommandType.Text);
        }

        public String scalarString(String commandText, IDbDataParameter[] dataParameter)
        {
            return helper.instance().executeScalarToString(helper.connectionString, commandText, dataParameter, CommandType.Text);
        }

        public Int32 scalarInt(String commandText, IDbDataParameter[] dataParameter)
        {
            return helper.instance().executeScalarToInt(helper.connectionString, commandText, dataParameter, CommandType.Text);
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