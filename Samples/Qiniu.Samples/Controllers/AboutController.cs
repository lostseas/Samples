using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Qiniu.Samples.Qiniu;

namespace Qiniu.Samples.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutController : ControllerBase
    {
        private readonly IFileUploadHelper _fileUploadHelper;

        public AboutController(IFileUploadHelper fileUploadHelper)
        {
            _fileUploadHelper = fileUploadHelper;
        }
        [HttpPost("UpdloadImages")]
        public string UpdloadImages()
        {
            if (!Request.ContentType.Contains("form-data"))
            {
                //LogHelper.WriteLog("UploadImagesNotToken_NULL");
                return "请求格式错误";
                //throw new apiException();
            }
            var files = Request.Form.Files;
            if (files.Count == 0)
                throw new Exception("请选择上传文件");

            //if (files.Count > 1) throw new Exception("请选择单个文件");
            var ulrs = new List<string>();
            foreach (var item in files)
            {
                //if (item.Length > 5 * 1024 * 1024 * 8)//超过5m
                //{
                //    throw new Exception("超出上传大小限制");
                //}
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        item.CopyTo(stream);
                        stream.Position = 0;
                        var url = _fileUploadHelper.UploadstreamFile(stream);
                        ulrs.Add(url);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return ulrs.FirstOrDefault();
        }

        [HttpPost("UpdloadFile")]
        public string UpdloadFile()
        {
            if (!Request.ContentType.Contains("form-data"))
            {
                //LogHelper.WriteLog("UploadImagesNotToken_NULL");
                return "请求格式错误";
                //throw new apiException();
            }
            var files = Request.Form.Files;
            if (files.Count == 0)
                throw new Exception("请选择上传文件");

            //if (files.Count > 1) throw new Exception("请选择单个文件");
            var ulrs = new List<string>();
            var file = files[0];

            try
            {
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    stream.Position = 0;
                    var url = _fileUploadHelper.UploadBigFile(stream, file.FileName);
                    return url;
                }
            }
            catch (Exception ex)
            {
            }

            return ulrs.FirstOrDefault();
        }



        #region  private 
        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="flag">压缩质量（数字越小压缩率越高）1-100</param>
        /// <returns></returns>
        public Stream CompressImage(MemoryStream image, int flag = 50)
        {
            try
            {
                System.Drawing.Image iSource = System.Drawing.Image.FromStream(image);
                if (iSource.Width < 720)
                {
                    return image;
                }
                int dHeight = iSource.Height;
                int dWidth = iSource.Width;
                int sW = 0, sH = 0;
                //按比例缩放
                Size tem_size = new Size(iSource.Width, iSource.Height);
                if (tem_size.Width > dHeight || tem_size.Width > dWidth)
                {
                    if ((tem_size.Width * dHeight) > (tem_size.Width * dWidth))
                    {
                        sW = dWidth;
                        sH = (dWidth * tem_size.Height) / tem_size.Width;
                    }
                    else
                    {
                        sH = dHeight;
                        sW = (tem_size.Width * dHeight) / tem_size.Height;
                    }
                }
                else
                {
                    sW = tem_size.Width;
                    sH = tem_size.Height;
                }

                using (var ob = new Bitmap(dWidth, dHeight))
                {
                    Graphics g = Graphics.FromImage(ob);
                    g.Clear(Color.WhiteSmoke);
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);
                    g.Dispose();
                    //以下代码为保存图片时，设置压缩质量
                    EncoderParameters ep = new EncoderParameters();
                    long[] qy = new long[1];
                    qy[0] = flag;//设置压缩的比例1-100
                    EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
                    ep.Param[0] = eParam;
                    var imageStream = new MemoryStream();
                    ob.Save(imageStream, ImageFormat.Jpeg);
                    return imageStream;
                }
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog(ex.Message, ex);
                return image;
            }
        }

        /// <summary>
        /// 请求处理，返回二维码图片
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string PostMoths(string url, string param)
        {
            string strURL = url;
            System.Net.HttpWebRequest request;
            request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            string paraUrlCoded = param;
            byte[] payload;
            payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
            request.ContentLength = payload.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.Stream s;
            s = response.GetResponseStream();//返回图片数据流
            byte[] tt = StreamToBytes(s);//将数据流转为byte[]

            ////在文件名前面加上时间，以防重名
            //string imgName = DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg";
            ////文件存储相对于当前应用目录的虚拟目录
            //string path = "/image/";
            ////获取相对于应用的基目录,创建目录
            //string imgPath = System.AppDomain.CurrentDomain.BaseDirectory + path;     //通过此对象获取文件名

            MemoryStream ms = null;
            ms = new MemoryStream(tt);
            var returnstring = _fileUploadHelper.UploadstreamFile(ms);
            return returnstring;
        }
        ///将数据流转为byte[]
        private static byte[] StreamToBytes(Stream stream)
        {
            List<byte> bytes = new List<byte>();
            int temp = stream.ReadByte();
            while (temp != -1)
            {
                bytes.Add((byte)temp);
                temp = stream.ReadByte();
            }
            return bytes.ToArray();
        }
        #endregion
    }
}
