using System;
using System.IO;
using System.Text;
using System.Web;

namespace view
{
    public class classString
    {
        /// <summary>
        /// 创建类文件
        /// </summary>
        /// <param name="classFile">类目录</param>
        /// <param name="className">类文件名</param>
        /// <param name="classContent">类内容</param>
        public static void doFile(String classFile, String className, String classContent)
        {
            String rootPath = HttpContext.Current.Server.MapPath("classList") + "\\" + classFile;

            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            using (FileStream fs = File.Create(rootPath + "\\" + className + ".cs"))
            {
                byte[] fileInfo = new UTF8Encoding(true).GetBytes(classContent);
                fs.Write(fileInfo, 0, fileInfo.Length);
            }
        }
    }
}