using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Qiniu.Samples.Qiniu
{
    public class QiniuOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public string AccessKey { get; set; }

        public string SecretKey { get; set; }
        /// <summary>
        /// 文件上传空间
        /// </summary>
        public string Bucket { get; set; }
        /// <summary>
        /// 文件上传目录
        /// </summary>
        public string Catalog { get; set; }
        /// <summary>
        /// 文件上传域名
        /// </summary>
        public string Domain { get; set; }
    }
}
