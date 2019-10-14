using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lrtsDownload
{
    public class DownloadResult
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public int AllCount { get; set; }
    }
    public class lrtsModel
    {
        public string FileName { get; set; }
        public string Url { get; set; }
    }
    public class lrtsFileModel
    {
        /// <summary>
        /// 文件链接（可能为空，若为空，需要去http://www.lrts.me/ajax/path/{share_entityType}/{share_fatherEntityId}/{sectionid} 获取
        /// </summary>
        public string source { get; set; }
        public string sectionid { get; set; }
        public string player_r_name { get; set; }
        public string share_fatherEntityId { get; set; }
        public string share_entityType { get; set; }
    }
    public class lrtsBookInfo
    {
        public string booktype { get; set; }
        public string bookid { get; set; }
    }
}
