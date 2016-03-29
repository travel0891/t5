using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace model.utils
{
    public class query
    {
        private static readonly String IDENTITY1 = "intId", IDENTITY2 = "charId";

        public Int32 insert(baseTable table)
        {
            String commandText = String.Empty;
            StringBuilder temp1 = new StringBuilder(), temp2 = new StringBuilder();
            List<IDbDataParameter> lsParameter = new List<IDbDataParameter>();
            Type type = table.GetType();
            foreach (PropertyInfo field in type.GetProperties())
            {
                if (field.GetValue(table, null) != null && field.Name != IDENTITY1 && field.Name != IDENTITY2)
                {
                    temp1.Append(field.Name + ",");
                    temp2.Append("@" + field.Name + ",");
                    lsParameter.Add(new SqlParameter("@" + field.Name, field.GetValue(table, null)));
                }
            }
            IDbDataParameter[] parameter = lsParameter.ToArray();
            commandText = String.Format("insert into {0}({1}) values ({2});", type.Name, temp1.ToString().Trim(','), temp2.ToString().Trim(','));
            return helper.instance().executeNonQuery(helper.connectionString, commandText, parameter, CommandType.Text);
        }

        public Int32 insert(params baseTable[] parameters)
        {
            String tableName = String.Empty, tranCommandText = String.Empty;
            StringBuilder commandText = new StringBuilder();
            List<IDbDataParameter> listParameter = new List<IDbDataParameter>();
            foreach (baseTable table in parameters)
            {
                StringBuilder temp1 = new StringBuilder(), temp2 = new StringBuilder();
                Type type = table.GetType();
                tableName = type.Name;
                foreach (PropertyInfo field in type.GetProperties())
                {
                    if (field.GetValue(table, null) != null && field.Name != IDENTITY1 && field.Name != IDENTITY2)
                    {
                        String tempGUID = Guid.NewGuid().ToString("N");
                        temp1.Append(field.Name + ",");
                        temp2.Append("@" + field.Name + tempGUID + ",");
                        listParameter.Add(new SqlParameter("@" + field.Name + tempGUID, field.GetValue(table, null)));
                    }
                }
                commandText.AppendFormat(" insert into {0}({1}) values ({2}); ", tableName, temp1.ToString().Trim(','), temp2.ToString().Trim(','));
            }
            tranCommandText = string.Format("{0} {1} {2}", "begin tran", commandText, "if @@error<>0 begin rollback tran end else begin commit  tran end");
            IDbDataParameter[] parameter = listParameter.ToArray();
            return helper.instance().executeNonQuery(helper.connectionString, tranCommandText, parameter, CommandType.Text);
        }

        public Int32 update(baseTable table)
        {
            String commandText = String.Empty;
            StringBuilder temp1 = new StringBuilder();
            List<IDbDataParameter> lsParameter = new List<IDbDataParameter>();
            Type type = table.GetType();

            foreach (PropertyInfo field in type.GetProperties())
            {
                if (field.GetValue(table, null) != null)
                {
                    if (field.Name != IDENTITY1 && field.Name != IDENTITY2)
                    {
                        temp1.Append(field.Name + " = @" + field.Name + ",");
                    }
                    lsParameter.Add(new SqlParameter("@" + field.Name, field.GetValue(table, null)));
                }
            }
            IDbDataParameter[] parameter = lsParameter.ToArray();
            commandText = String.Format(" update {0} set {1} where {2} = @{2} ", type.Name, temp1.ToString().Trim(','), IDENTITY2);
            return helper.instance().executeNonQuery(helper.connectionString, commandText, parameter, CommandType.Text);
        }

        public Int32 update(params baseTable[] parameters)
        {
            String tableName = String.Empty, tranCommandText = String.Empty;
            StringBuilder commandText = new StringBuilder(), tranRowCount = new StringBuilder();
            List<IDbDataParameter> listParameter = new List<IDbDataParameter>();
            foreach (baseTable table in parameters)
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
            IDbDataParameter[] parameter = listParameter.ToArray();
            return helper.instance().executeNonQuery(helper.connectionString, tranCommandText, parameter, CommandType.Text);
        }

        public Int32 delete(baseTable table)
        {
            String commandText = String.Empty;
            Type type = table.GetType();
            IDbDataParameter[] parameter = { new SqlParameter("@" + IDENTITY2, table.charId) };
            commandText = String.Format(" delete from {0} where {1} = @{1} ", type.Name, IDENTITY2);
            return helper.instance().executeNonQuery(helper.connectionString, commandText, parameter, CommandType.Text);
        }

        public Int32 delete(params baseTable[] parameters)
        {
            String tranCommandText = String.Empty;
            StringBuilder commandText = new StringBuilder(), tranRowCount = new StringBuilder();
            List<IDbDataParameter> listParameter = new List<IDbDataParameter>();
            foreach (baseTable table in parameters)
            {
                Type type = table.GetType();
                String tempGUID = Guid.NewGuid().ToString("N");

                listParameter.Add(new SqlParameter("@" + IDENTITY2 + tempGUID, table.charId));
                commandText.AppendFormat(" declare @row{2} int; delete from {0} where {1} = @{1}{2}; set @row{2}=@@rowcount;", type.Name, IDENTITY2, tempGUID);
                tranRowCount.AppendFormat(" or @row{0} = 0 ", tempGUID);
            }

            tranCommandText = string.Format("{0} {1} {2} {3} {4}", "begin tran", commandText, "if @@error<>0", tranRowCount, "begin rollback tran end else begin commit tran end");
            IDbDataParameter[] parameter = listParameter.ToArray();
            return helper.instance().executeNonQuery(helper.connectionString, tranCommandText.ToString(), parameter, CommandType.Text);
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

        private static query q = null;

        public static query instance()
        {
            return q == null ? new query() : q;
        }
    }
}