using System;

namespace view.model
{
    public class columnList
    {
        /// <summary>
        /// ID
        /// </summary>
        public Int32 cId { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public String cName { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        public String cType { get; set; }

        /// <summary>
        /// 字段精度
        /// </summary>
        public Int32 cLength { get; set; }

        /// <summary>
        /// 是否自增 1 是 0 否
        /// </summary>
        public Int16 cIdentity { get; set; }

        /// <summary>
        /// 是否主键 1 是 0 否
        /// </summary>
        public Int16 cPK { get; set; }

        /// <summary>
        /// 允许NULL  1 是 0 否
        /// </summary>
        public Int16 cNULL { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public String cDefault { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public String cRemark { get; set; }

        public String cIndexName { get; set; }
        public String cIndexSort { get; set; }
    }
}