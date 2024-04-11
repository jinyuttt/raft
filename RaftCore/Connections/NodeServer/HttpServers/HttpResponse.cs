using System;
using System.IO;
using System.Text;


    public class HttpResponse : BaseHeader
    {
    #region 字段属性
    /// <summary>
    /// http协议版本
    /// </summary>
    public string HTTP_Version { get; set; } = "HTTP/1.1";
        /// <summary>
        /// 状态码
        /// </summary>
        public string StatusCode { get; set; } = "200";
        /// <summary>
        /// http数据流
        /// </summary>
        private Stream DataStream;
        #endregion
        public HttpResponse(Stream stream)
        {
            this.DataStream = stream;
        }

        /// <summary>
        /// 构建响应头部
        /// </summary>
        /// <returns></returns>
        protected string BuildHeader()
        {
            StringBuilder builder = new StringBuilder();//StringBuilder(字符串变量)进行运算时，是一直在已有对象操作的，适合大量频繁字符串的拼接或删除
                                                        //string(字符串常量)在进行运算的时候是重新生成了一个新的string对象，不适合大量频繁字符串的拼接或删除

            if (!string.IsNullOrEmpty(StatusCode))
                builder.Append(HTTP_Version + " " + StatusCode + "\r\n");

            if (!string.IsNullOrEmpty(this.Content_Type))
                builder.AppendLine("Content-Type:" + this.Content_Type);
            return builder.ToString();
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public void Send()
        {
            if (!DataStream.CanWrite) return;
            try
            {
                //发送响应头
                var header = BuildHeader();
                byte[] headerBytes = this.Encoding.GetBytes(header);
                DataStream.Write(headerBytes, 0, headerBytes.Length);

                //发送空行
                byte[] lineBytes = this.Encoding.GetBytes(System.Environment.NewLine);
                DataStream.Write(lineBytes, 0, lineBytes.Length);

                //发送内容
                byte[] buffer = this.Encoding.GetBytes(this.Content);
                DataStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
              //  _logger.Debug("服务器响应异常！" + e.Message);
            }
            finally
            {
                DataStream.Close();
            }
        }
    }

