using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Qiniu.Samples.Qiniu
{
    public interface IFileUploadHelper
    {
        #region 文件上传
        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileName"><文件名称/param>
        /// <param name="block">上传目录</param>
        /// <param name="fileExtension">扩展名</param>
        string UploadstreamFile(Stream stream, string fileName = null, string block = null, string fileExtension = "png");
        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileName"><文件名称/param>
        /// <param name="block">上传目录</param>
        public string UploadBigFile(Stream stream, string fileName, string block = null);
        #endregion
        #region 网络文件上传
        /// <summary>
        /// 网络文件上传
        /// </summary>
        /// <param name="url">文件地址</param>
        /// <returns></returns>
        string DownloadFile(string fileUrl);
        #endregion
    }
}
