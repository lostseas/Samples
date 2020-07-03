using Microsoft.Extensions.Options;
using Qiniu.Common;
using Qiniu.Http;
using Qiniu.IO;
using Qiniu.IO.Model;
using Qiniu.Util;
using System;
using System.IO;
using System.Net;

namespace Qiniu.Samples.Qiniu
{
    public class FileUploadHelper : IFileUploadHelper
    {
        private readonly QiniuOptions _qiniuOptions;


        // 内部变量，仅作演示
        private static bool paused = false;

        public FileUploadHelper(IOptions<QiniuOptions> qiniuOptionsAccessor)
        {
            _qiniuOptions = qiniuOptionsAccessor.Value;
        }



        #region 文件上传
        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileName"><文件名称/param>
        /// <param name="block">上传目录</param>
        /// <param name="fileExtension">扩展名</param>
        /// <returns></returns>
        public string UploadstreamFile(Stream stream, string fileName = null, string block = null, string fileExtension = "png")
        {
            try
            {
                if (string.IsNullOrEmpty(block))
                {
                    block = _qiniuOptions.Catalog;
                }
                block = block + "/";
                //var fileExt = Path.GetExtension("").ToLower();
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = Guid.NewGuid().ToString().Replace("-", "").ToLower() + "." + fileExtension;
                }
                Mac mac = new Mac(_qiniuOptions.AccessKey, _qiniuOptions.SecretKey);
                PutPolicy putPolicy = new PutPolicy();
                // 上传策略，参见 
                // 如果需要设置为"覆盖"上传(如果云端已有同名文件则覆盖)，请使用 SCOPE = "BUCKET:KEY"
                // putPolicy.Scope = bucket + ":" + saveKey;
                putPolicy.Scope = _qiniuOptions.Bucket;
                // 上传策略有效期(对应于生成的凭证的有效期)          
                putPolicy.SetExpires(60 * 60);
                // 上传到云端多少天后自动删除该文件，如果不设置（即保持默认默认）则不删除
                //putPolicy.DeleteAfterDays = 1;
                string uploadToken = Auth.CreateUploadToken(mac, putPolicy.ToJsonString());
                Config.SetZone(ZoneID.CN_East, true);

                UploadManager target = new UploadManager(uploadFromCDN: true);
                HttpResult result = target.UploadStream(stream, block + fileName, uploadToken);
                var rawUrl = _qiniuOptions.Domain + "/" + block + fileName;
                return rawUrl;
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog("上传图片文件报错：", ex);
                return "";
            }


        }
        #endregion



        #region 文件上传
        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileName"><文件名称/param>
        /// <param name="block">上传目录</param>
        /// <param name="fileExtension">扩展名</param>
        /// <returns></returns>
        public string UploadBigFile(Stream stream, string fileName, string block = null)
        {
            try
            {
                if (string.IsNullOrEmpty(block))
                {
                    block = _qiniuOptions.Catalog;
                }
                block = block + "/";
                //var fileExt = Path.GetExtension("").ToLower();
                if (string.IsNullOrEmpty(fileName))
                {

                }
                Mac mac = new Mac(_qiniuOptions.AccessKey, _qiniuOptions.SecretKey);
                PutPolicy putPolicy = new PutPolicy();
                // 上传策略，参见 
                // 如果需要设置为"覆盖"上传(如果云端已有同名文件则覆盖)，请使用 SCOPE = "BUCKET:KEY"
                // putPolicy.Scope = bucket + ":" + saveKey;
                putPolicy.Scope = _qiniuOptions.Bucket;
                // 上传策略有效期(对应于生成的凭证的有效期)          
                putPolicy.SetExpires(60 * 60);
                // 上传到云端多少天后自动删除该文件，如果不设置（即保持默认默认）则不删除
                //putPolicy.DeleteAfterDays = 1;
                string uploadToken = Auth.CreateUploadToken(mac, putPolicy.ToJsonString());
                Config.SetZone(ZoneID.CN_East, true);
                ResumableUploader resumableUploader = new ResumableUploader(uploadFromCDN: true, chunkUnit: ChunkUnit.U128K);
                StreamProgressHandler streamProgressHandler = new StreamProgressHandler(ResumableUploader.DefaultStreamProgressHandler);
                HttpResult result = resumableUploader.UploadStream(stream, block + fileName, uploadToken, streamProgressHandler);
                var rawUrl = _qiniuOptions.Domain + "/" + block + fileName;
                return rawUrl;
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog("上传图片文件报错：", ex);
                return "";
            }


        }
        #endregion


        #region 网络文件上传
        /// <summary>
        /// 网络文件上传
        /// </summary>
        /// <param name="url">文件地址</param>
        /// <returns></returns>
        public string DownloadFile(string fileUrl)
        {
            string returnstring = "";
            try
            {
                if (fileUrl.StartsWith("https"))
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                var request = HttpWebRequest.Create(fileUrl) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                MemoryStream ms = null;
                using (var stream = response.GetResponseStream())
                {
                    var buffer = new byte[response.ContentLength];
                    int offset = 0, actuallyRead = 0;
                    do
                    {
                        actuallyRead = stream.Read(buffer, offset, buffer.Length - offset);
                        offset += actuallyRead;
                    }
                    while (actuallyRead > 0);
                    ms = new MemoryStream(buffer);
                }
                returnstring = UploadstreamFile(ms, null, null, Path.GetExtension(fileUrl).ToLower());
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog("上传图片文件报错：", ex);
                returnstring = "";
            }
            return returnstring;
        }
        #endregion





        /// <summary>
        /// 上传控制
        /// </summary>
        /// <returns></returns>
        private static UPTS uploadControl()
        {
            // 这个函数只是作为一个演示，实际当中请根据需要来设置
            // 这个演示的实际效果是“走走停停”，也就是停一下又继续，如此重复直至上传结束
            paused = !paused;
            if (paused)
            {
                return UPTS.Suspended;
            }
            else
            {
                return UPTS.Activated;
            }
        }

    }
}
