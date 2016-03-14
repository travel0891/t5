using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;

using utils;
using view.model;

namespace view
{
    public partial class entrance : Page
    {
        protected void Page_Load(object sender, EventArgs e) { table(); }

        /// <summary>
        /// 生成表结构实体 基类 baseTable 默认目录 model\table\xxx.cs
        /// </summary>
        private void table()
        {
            String tempClass = String.Empty;
            Int32 tempIndex = 1;
            StringBuilder sbHTML = new StringBuilder();
            ctrl c = new ctrl();

            List<tableList> lsTable = c.selectTable("U");
            foreach (tableList table in lsTable)
            {
                tempClass = table.tName;
                sbHTML.AppendLine("using System;");
                sbHTML.AppendLine("");
                sbHTML.AppendLine("namespace model.table");
                sbHTML.AppendLine("{");
                sbHTML.AppendLine("    public class " + tempClass + " : baseTable");
                sbHTML.AppendLine("    {");

                List<columnList> lsColumn = c.selectColumn(tempClass);
                foreach (columnList column in lsColumn)
                {
                    if (column.cRemark.Length > 0)
                    {
                        sbHTML.AppendLine("        /// <summary> ");
                        sbHTML.AppendLine("        /// " + column.cRemark + " " + column.cType + " " + column.cLength + (column.cIdentity == 1 ? "主键" : null) + " " + column.cDefault);
                        sbHTML.AppendLine("        /// </summary> ");
                    }
                    sbHTML.AppendLine("        public " + dbString.getType(column.cType) + (column.cNULL == 1 ? "?" : null) + " " + column.cName + " { get; set; }");

                    if (lsColumn.Count > tempIndex)
                    {
                        sbHTML.AppendLine("");
                    }
                    tempIndex++;
                }
                sbHTML.AppendLine("    }");
                sbHTML.AppendLine("}");
            }

            classString.doFile("model\\table", tempClass, sbHTML.ToString());
        }
    }

    public class ctrl
    {
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
                sbSQL.Append(" where xtype = '" + tType + "' ");
            }

            IDataReader dr = query.GetInstance().dataReader(sbSQL.ToString(), null);
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
        /// <returns></returns>
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
            sbSQL.Append(" where o.name=N'" + tableName + "' ");
            sbSQL.Append(" and c.name<>'intId' and c.name<>'charId' ");
            sbSQL.Append(" order by o.name,c.column_id ");

            IDataReader dr = query.GetInstance().dataReader(sbSQL.ToString(), null);
            columnList model = null;
            while (dr.Read())
            {
                model = new columnList();
                model.cId = Convert.ToInt32(dr["cId"]);
                model.cName = dr["cName"].ToString();
                model.cType = dr["cType"].ToString();
                model.cLength = Convert.ToInt32(dr["cLength"]);
                model.cIdentity = Convert.ToInt16(dr["cIdentity"]);
                model.cPK = Convert.ToInt16(dr["cPK"]);
                model.cNULL = Convert.ToInt16(dr["cNULL"]);
                model.cDefault = dr["cDefault"].ToString();
                model.cRemark = dr["cRemark"].ToString();
                lsModel.Add(model);
            }
            dr.Close();

            return lsModel;
        }
    }
}