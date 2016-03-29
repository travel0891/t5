using System;

namespace model.utils
{
    /// <summary>
    /// 表基类
    /// </summary>
    public class baseTable
    {
        /// <summary>
        /// ID int IDENTITY(1,1)
        /// </summary>
        public Int32 intId { get; set; }

        /// <summary>
        /// GUID uniqueidentifier DEFAULT (newid())
        /// </summary>
        public String charId { get; set; }
    }
}