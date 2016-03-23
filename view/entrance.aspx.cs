using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;

namespace view
{
    using view.model;

    public partial class entrance : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            table();
            entity("entityProvider");
        }

        /// <summary>
        /// 生成表结构实体 基类 baseTable 默认目录 model\table\xxx.cs
        /// </summary>
        private void table()
        {
            String tempClass = String.Empty;

            StringBuilder sbHTML = null;
            ctrl c = new ctrl();

            List<tableList> lsTable = c.selectTable("U");
            foreach (tableList table in lsTable)
            {
                Int32 tempIndex = 1;

                sbHTML = new StringBuilder();
                tempClass = table.tName;
                sbHTML.AppendLine("using System;");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("namespace model.table");
                sbHTML.AppendLine("{");
                sbHTML.AppendLine("    using model.utils;");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("    public class " + tempClass + " : baseTable");
                sbHTML.AppendLine("    {");

                List<columnList> lsColumn = c.selectColumn(tempClass);
                foreach (columnList column in lsColumn)
                {
                    // if (column.cRemark.Length > 0)
                    // {
                    sbHTML.AppendLine("        /// <summary> ");
                    sbHTML.AppendLine("        /// " + column.cRemark + " " + column.cType + " " + column.cLength + (column.cIdentity == 1 ? " 主键" : null) + " " + column.cDefault + " " + column.cIndexName + " " + column.cIndexSort + " " + (column.cNULL == 1 ? "允许NULL" : null));
                    sbHTML.AppendLine("        /// </summary> ");
                    // }
                    sbHTML.AppendLine("        public " + dbString.getCType(column.cType) + ((column.cNULL == 1 || column.cType == "DATETIME") && dbString.getCType(column.cType) != "String" ? "?" : null) + " " + column.cName + " { get; set; }");

                    if (lsColumn.Count > tempIndex)
                    {
                        sbHTML.AppendLine("");
                    }
                    tempIndex++;
                }
                sbHTML.AppendLine("    }");
                sbHTML.AppendLine("}");

                classString.doFile("model\\table", tempClass, sbHTML.ToString());
            }
        }

        /// <summary>
        /// 生成基本CURD 默认目录 model\entity\xxx.cs
        /// </summary>
        /// <param name="instanceName">实例名称</param>
        private void entity(String instanceName)
        {
            String tempClass = String.Empty;
            Int32 instanceIndex = 1;
            StringBuilder sbHTML = null, sbSQL = null;
            ctrl c = new ctrl();

            List<tableList> lsTable = c.selectTable("U");
            foreach (tableList table in lsTable)
            {
                sbHTML = new StringBuilder();

                tempClass = table.tName;

                #region using

                sbHTML.AppendLine("using System;");
                sbHTML.AppendLine("using System.Collections.Generic;");
                sbHTML.AppendLine("using System.Data;");
                sbHTML.AppendLine("using System.Data.SqlClient;");
                sbHTML.AppendLine("using System.Text;");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("namespace model.entity");
                sbHTML.AppendLine("{");
                sbHTML.AppendLine("    using model.table;");
                sbHTML.AppendLine("    using model.utils;");
                sbHTML.AppendLine("");

                #endregion

                #region begin

                sbHTML.AppendLine("    public partial class " + instanceName + " ");
                sbHTML.AppendLine("    {");

                #endregion

                List<columnList> lsColumn = c.selectColumn(tempClass);

                #region list Object

                sbHTML.AppendLine("        public List<" + tempClass + "> select" + dbString.changeChar(tempClass) + "(" + tempClass + " whereModel, Int32 pageSize, Int32 pageIndex, out Int32 dataCount, out Int32 pageCount, String orderString, params Object[] param)");
                sbHTML.AppendLine("        {");
                sbHTML.AppendLine("            String dataCountSQL = \" select count(1) from " + tempClass + " \";");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("            StringBuilder sbSQL = new StringBuilder();");
                sbHTML.AppendLine("            sbSQL.Append(\" select \");");
                sbHTML.AppendLine("            sbSQL.AppendFormat(\" {0} \", pageSize > 0 ? \" top \" + pageSize : null);");
                sbHTML.AppendLine("            sbSQL.Append(\" intId, charId \");");
                sbSQL = new StringBuilder();
                foreach (columnList itemColum in lsColumn)
                {
                    sbSQL.Append("," + itemColum.cName + " ");
                }
                sbHTML.AppendLine("            sbSQL.Append(\" " + sbSQL.ToString() + "\");");
                sbHTML.AppendLine("            sbSQL.Append(\" from " + tempClass + " \");");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("            String whereSQL = String.Empty;");
                sbHTML.AppendLine("            IDbDataParameter[] parameter = query.instance().builderParameter(out whereSQL, param);");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("            StringBuilder pageSQL = new StringBuilder();");
                sbHTML.AppendLine("            pageIndex = pageIndex > 0 ? pageIndex - 1 : 0;");
                sbHTML.AppendLine("            if (pageIndex > 0)");
                sbHTML.AppendLine("            {");
                sbHTML.AppendLine("                pageSQL.Append(\" and intId > \");");
                sbHTML.AppendLine("                pageSQL.AppendFormat(\" ( select max(intId) from (select top {0} intId from area {1} order by intId ) as dataList ) \", pageIndex * pageSize, whereSQL);");
                sbHTML.AppendLine("            }");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("            StringBuilder orderSQL = new StringBuilder();");
                sbHTML.AppendLine("            orderSQL.AppendFormat(\" order by {0} \", String.IsNullOrEmpty(orderString) ? \"intId asc\" : orderString);");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("            List<" + tempClass + "> list" + dbString.changeChar(tempClass) + "Model = new List<" + tempClass + ">();");
                sbHTML.AppendLine("            " + tempClass + " " + tempClass + "Model = null;");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("            dataCount = query.instance().scalarInt(dataCountSQL + whereSQL, parameter);");
                sbHTML.AppendLine("            pageCount = (Int32)Math.Ceiling((Double)dataCount / (Double)pageSize);");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("            IDataReader dr = query.instance().dataReader(sbSQL.ToString() + whereSQL + pageSQL.ToString() + orderSQL.ToString(), parameter);");
                sbHTML.AppendLine("            while (dr.Read())");
                sbHTML.AppendLine("            {");
                sbHTML.AppendLine("                " + tempClass + "Model = new " + tempClass + "();");
                sbHTML.AppendLine("                " + tempClass + "Model.intId = dr.GetInt32(0);");
                sbHTML.AppendLine("                " + tempClass + "Model.charId = dr.GetGuid(1).ToString();");
                Int32 lsIndex = 2;
                foreach (columnList itemColum in lsColumn)
                {
                    sbHTML.AppendLine("                " + tempClass + "Model." + itemColum.cName + " = dr." + dbString.getDrType(itemColum.cType) + "(" + lsIndex + ")" + (itemColum.cType.ToLower() == "uniqueidentifier" ? ".ToString()" : null) + ";");
                    lsIndex++;
                }
                sbHTML.AppendLine("                list" + dbString.changeChar(tempClass) + "Model.Add(" + tempClass + "Model);");
                sbHTML.AppendLine("            }");
                sbHTML.AppendLine("            dr.Close();");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("            return list" + dbString.changeChar(tempClass) + "Model;");
                sbHTML.AppendLine("        }");
                sbHTML.AppendLine("");

                #endregion

                #region single Object

                sbHTML.AppendLine("        public " + tempClass + " select" + dbString.changeChar(tempClass) + "ByCharId(String charId)");
                sbHTML.AppendLine("        {");
                sbHTML.AppendLine("            " + tempClass + " " + tempClass + "Model = null;");
                sbHTML.AppendLine("            StringBuilder sbSQL = new StringBuilder();");
                sbHTML.AppendLine("            sbSQL.Append(\" select intId, charId \");");
                sbSQL = new StringBuilder();
                foreach (columnList itemColum in lsColumn)
                {
                    sbSQL.Append("," + itemColum.cName + " ");
                }
                sbHTML.AppendLine("            sbSQL.Append(\" " + sbSQL.ToString() + "\");");
                sbHTML.AppendLine("            sbSQL.Append(\" from " + tempClass + " \");");
                sbHTML.AppendLine("            sbSQL.Append(\" where charId = @charId \");");
                sbHTML.AppendLine("            IDbDataParameter[] parameter = { new SqlParameter(\"charId\", charId) }; ");
                sbHTML.AppendLine("            IDataReader dr = query.instance().dataReader(sbSQL.ToString(), parameter);");
                sbHTML.AppendLine("            if (dr.Read())");
                sbHTML.AppendLine("            {");
                sbHTML.AppendLine("                " + tempClass + "Model = new " + tempClass + "();");
                sbHTML.AppendLine("                " + tempClass + "Model.intId = dr.GetInt32(0);");
                sbHTML.AppendLine("                " + tempClass + "Model.charId = dr.GetGuid(1).ToString();");
                Int32 drIndex = 2;
                foreach (columnList itemColum in lsColumn)
                {
                    sbHTML.AppendLine("                " + tempClass + "Model." + itemColum.cName + " = dr." + dbString.getDrType(itemColum.cType) + "(" + drIndex + ")" + (itemColum.cType.ToLower() == "uniqueidentifier" ? ".ToString()" : null) + ";");
                    drIndex++;
                }
                sbHTML.AppendLine("            }");
                sbHTML.AppendLine("            dr.Close();");
                sbHTML.AppendLine("            return " + tempClass + "Model;");
                sbHTML.AppendLine("        }");
                sbHTML.AppendLine("");

                #endregion

                #region insert update delete

                sbHTML.AppendLine("        public Int32 insert" + dbString.changeChar(tempClass) + "(" + tempClass + " " + tempClass + "Model)");
                sbHTML.AppendLine("        {");
                sbHTML.AppendLine("            return query.instance().insert(" + tempClass + "Model);");
                sbHTML.AppendLine("        }");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("        public Int32 update" + dbString.changeChar(tempClass) + "(" + tempClass + " " + tempClass + "Model)");
                sbHTML.AppendLine("        {");
                sbHTML.AppendLine("            return query.instance().update(" + tempClass + "Model);");
                sbHTML.AppendLine("        }");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("        public Int32 delete" + dbString.changeChar(tempClass) + "(" + tempClass + " " + tempClass + "Model)");
                sbHTML.AppendLine("        {");
                sbHTML.AppendLine("            return query.instance().delete(" + tempClass + "Model);");
                sbHTML.AppendLine("        }");

                #endregion

                #region instance

                if (instanceIndex == 1)
                {
                    sbHTML.AppendLine("");
                    sbHTML.AppendLine("        private static " + instanceName + " entity = null;");
                    sbHTML.AppendLine("");
                    sbHTML.AppendLine("        private " + instanceName + "() { }");
                    sbHTML.AppendLine("");
                    sbHTML.AppendLine("        public static " + instanceName + " instance()");
                    sbHTML.AppendLine("        {");
                    sbHTML.AppendLine("            return entity == null ? new " + instanceName + "() : entity;");
                    sbHTML.AppendLine("        }");
                }

                #endregion

                #region end

                sbHTML.AppendLine("    }");
                sbHTML.AppendLine("}");

                #endregion

                instanceIndex++;

                classString.doFile("model\\entity", tempClass, sbHTML.ToString());
            }
        }
    }

    public class ctrl
    {
        #region ctrl

        /// <summary>
        /// 获取数据库中对象
        /// </summary>
        /// <param name="tType">U：数据表 V：视图，P：存储过程</param>
        /// <returns>List<tableList></returns>
        public List<tableList> selectTable(String tType)
        {
            List<tableList> lsModel = new List<tableList>();
            StringBuilder sbSQL = new StringBuilder();

            sbSQL.Append(" select id,name,xtype from sysobjects ");
            if (!String.IsNullOrEmpty(tType))
            {
                sbSQL.AppendFormat(" where xtype = '{0}' ", tType);
            }

            IDataReader dr = query.instance().dataReader(sbSQL.ToString(), null);
            tableList model = null;
            while (dr.Read())
            {
                model = new tableList();
                model.tid = Convert.ToInt32(dr["id"]);
                model.tName = dr["name"].ToString();
                model.tType = dr["xtype"].ToString();
                lsModel.Add(model);
            }
            dr.Close();

            return lsModel;
        }

        /// <summary>
        /// 获取对应表的字段
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>List</returns>
        public List<columnList> selectColumn(String tableName)
        {
            List<columnList> lsModel = new List<columnList>();
            StringBuilder sbSQL = new StringBuilder();

            sbSQL.Append(" select c.column_id as cId ");
            sbSQL.Append(" ,c.name as cName ");
            sbSQL.Append(" ,t.name as cType ");
            sbSQL.Append(" ,case when t.name = 'nchar' then c.max_length/2 when t.name='nvarchar' then c.max_length/2 else c.max_length end as cLength ");
            sbSQL.Append(" ,case when C.is_identity = 1 then N'1'else N'0' end as cIdentity ");
            sbSQL.Append(" ,isnull(idx.PrimaryKey,N'0') as cPK ");
            sbSQL.Append(" ,case when c.is_nullable = 1 then N'1'else N'0' end as cNULL ");
            sbSQL.Append(" ,isnull(d.definition,N'') as cDefault ");
            sbSQL.Append(" ,isnull(pfd.value,N'') as cRemark ");
            sbSQL.Append(",isnull(idx.IndexName,N'') as cIndexName ");
            sbSQL.Append(",isnull(idx.Sort,N'') as cIndexSort ");
            sbSQL.Append(" from sys.columns as c ");
            sbSQL.Append(" inner join sys.objects as o on c.object_id = o.object_id and (o.type = 'U' or o.type = 'V') and o.is_ms_shipped = 0 ");
            sbSQL.Append(" inner join sys.types as t on c.user_type_id = t.user_type_id ");
            sbSQL.Append(" left join sys.default_constraints as d on c.object_id = d.parent_object_id and c.column_id = d.parent_column_id and c.default_object_id = d.object_id ");
            sbSQL.Append(" left join sys.extended_properties as pfd on pfd.class = 1 and c.object_id = pfd.major_id and c.column_id = pfd.minor_id ");
            sbSQL.Append(" left join sys.extended_properties as ptb on ptb.class = 1 and ptb.minor_id = 0 and c.object_id = ptb.major_id  ");
            sbSQL.Append(" left join ( ");
            sbSQL.Append(" select idxc.object_id, idxc.column_id ");
            sbSQL.Append(" ,Sort=case INDEXKEY_PROPERTY(idxc.object_id,idxc.index_id,idxc.index_column_id,'IsDescending') when 1 then 'desc' when 0 then 'asc' else '' end ");
            sbSQL.Append(" ,PrimaryKey = case when idx.is_primary_key = 1 then N'1' else N'0' end ");
            sbSQL.Append(" ,IndexName=idx.Name from sys.indexes as idx ");
            sbSQL.Append(" inner join sys.index_columns as idxc on idx.object_id = idxc.object_id and idx.index_id = idxc.index_id ");
            sbSQL.Append(" left join sys.key_constraints as kc on idx.object_id = kc.parent_object_id and idx.index_id = kc.unique_index_id  ");
            sbSQL.Append(" inner join ( ");
            sbSQL.Append(" select object_id, Column_id, index_id = min(index_id) ");
            sbSQL.Append(" from sys.index_columns ");
            sbSQL.Append(" group by object_id,Column_id ) as idxcuq ");
            sbSQL.Append(" on idxc.object_id = idxcuq.object_id and idxc.Column_id = idxcuq.Column_id and idxc.index_id = idxcuq.index_id ) as idx ");
            sbSQL.Append(" on c.object_id = idx.object_id and c.column_id = idx.column_id   ");
            sbSQL.AppendFormat(" where o.name=N'{0}' ", tableName);
            sbSQL.Append(" and c.name<>'intId' and c.name<>'charId' ");
            sbSQL.Append(" order by o.name,c.column_id ");

            IDataReader dr = query.instance().dataReader(sbSQL.ToString(), null);
            columnList model = null;
            while (dr.Read())
            {
                model = new columnList();
                model.cId = Convert.ToInt32(dr["cId"]);
                model.cName = dr["cName"].ToString();
                model.cType = dr["cType"].ToString().ToUpper();
                model.cLength = Convert.ToInt32(dr["cLength"]);
                model.cIdentity = Convert.ToInt16(dr["cIdentity"]);
                model.cPK = Convert.ToInt16(dr["cPK"]);
                model.cNULL = Convert.ToInt16(dr["cNULL"]);
                model.cDefault = dr["cDefault"].ToString();
                model.cRemark = dr["cRemark"].ToString();
                model.cIndexName = dr["cIndexName"].ToString().ToUpper();
                model.cIndexSort = dr["cIndexSort"].ToString().ToUpper();

                lsModel.Add(model);
            }
            dr.Close();

            return lsModel;
        }

        /// <summary>
        /// 获取表主外键关系
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>List</returns>
        public List<pkList> selectPk(String tableName)
        {
            List<pkList> lsModel = new List<pkList>();
            StringBuilder sbSQL = new StringBuilder();

            sbSQL.Append(" select obj1.name as key1, obj2.name as key2, obj3.name as key3 ");
            sbSQL.Append(" from sysforeignkeys as fk ");
            sbSQL.Append(" inner join sysobjects as obj1 on fk.constid = obj1.id ");
            sbSQL.Append(" inner join sysobjects as obj2 on fk.fkeyid = obj2.id ");
            sbSQL.Append(" inner join sysobjects as obj3 on fk.rkeyid = obj3.id ");
            sbSQL.AppendFormat(" where obj2.name = '{0}' ", tableName);
            sbSQL.AppendFormat(" order by {0} desc ", "constid");

            IDataReader dr = query.instance().dataReader(sbSQL.ToString(), null);
            pkList model = null;
            while (dr.Read())
            {
                model = new pkList();
                model.key1 = dr["key1"].ToString().ToUpper();
                model.key2 = dr["key2"].ToString();
                model.key3 = dr["key3"].ToString();

                lsModel.Add(model);
            }
            dr.Close();

            return lsModel;
        }

        #endregion
    }
}