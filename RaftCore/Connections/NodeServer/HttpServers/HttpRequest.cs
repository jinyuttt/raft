using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


    public class HttpRequest : BaseHeader
    {
        #region 字段属性
        /// <summary>
        /// URL参数
        /// </summary>
        public Dictionary<string, string> Params { get; private set; }
        /// <summary>
        /// http请求方法
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// http请求的URL地址
        /// </summary>
        public string URL { get; set; }
        /// <summary>
        /// http协议版本
        /// </summary>
        public string HTTP_Version { get; set; }
        #endregion

        /// <summary>
        /// 定义缓冲区
        /// </summary>
        private const int MAX_SIZE = 1024 * 1024 * 2;
        private byte[] bytes = new byte[MAX_SIZE];
        private Stream DataStream;

        public HttpRequest(Stream stream)
        {
            this.DataStream = stream;
            string dataString = GetData(DataStream);

            var dataArray = Regex.Split(dataString, Environment.NewLine);
            var requestLine = Regex.Split(dataArray[0], @"(\s+)")
                .Where(e => e.Trim() != string.Empty)
                .ToArray();//分割出请求行的信息
            if (requestLine.Length > 0) this.Method = requestLine[0];
            if (requestLine.Length > 1) this.URL = Uri.UnescapeDataString(requestLine[1]);
            if (requestLine.Length > 2) this.HTTP_Version = requestLine[2];

            this.Headers = GetHeader(dataArray, out int index);

            if (this.Method == "POST")
            {
                this.Content_Length = Convert.ToInt32(Headers["Content-Length"]);
                if (Content_Length != 0)//数据长度不等于0
                {
                    this.Content_Type = Headers["Content-Type"];
                if(Content_Type.Contains("charset"))
                    this.Encoding = Content_Type.Split(':')[1] == "utf-8" ? Encoding.UTF8 : Encoding.Default;
                    this.Content = GetBody(dataArray, index);//真正的数据
                    //_logger.Debug("收到消息:\r\n" + Content + "\r\n");
                    //Task.Run(() =>
                    //{
                    //if (socket != null) 
                    //{
                    //byte[] buffer = this.Encoding.GetBytes(this.Content);
                    //socket.Send(buffer);
                    //}
                    //else
                    //{
                    //   _logger.Debug("上位机未连接！");
                    //}
                    //});
                }
                else
                {
                    //_logger.Information("报文为空！");
                }
            }
        }

        /// <summary>
        /// 获取数据流中的数据data
        /// </summary>
        /// <param name="DataStream"></param>
        /// <returns></returns>
        public string GetData(Stream DataStream)
        {
            try
            {
                var length = 0;
                var data = string.Empty;
                do
                {
                    length = DataStream.Read(bytes, 0, MAX_SIZE - 1);
                    data += Encoding.UTF8.GetString(bytes, 0, length);
                }
                while (length > 0 && !data.Contains("\r\n\r\n"));
                return data;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 获取报头header
        /// </summary>
        /// <param name="dataArray"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetHeader(string[] dataArray, out int index)
        {
            var header = new Dictionary<string, string>();
            index = 0;
            foreach (var item in dataArray)
            {
                index++;
                if (item == "")//读取到空行表示header已经读取完成
                {
                    return header;
                }
                if (item.Contains(':'))
                {
                    var dataTemp = item.Split(':').ToList();//把报头数据分割以键值对方式存入字典
                    if (dataTemp.Count > 2)//特殊情况Host有两个“:”
                    {
                        for (int i = 2; i < dataTemp.Count;)
                        {
                            dataTemp[1] += ":" + dataTemp[i];
                            dataTemp.Remove(dataTemp[i]);
                        }
                    }
                    header.Add(dataTemp[0].Trim(), dataTemp[1].Trim());
                }
            }
            return header;
        }

        /// <summary>
        /// 获取报文body
        /// </summary>
        /// <param name="dataArray"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetBody(string[] dataArray, int index)
        {
            string Body = string.Empty;
            for (int i = index; i < dataArray.Length; i++)
            {
                Body += dataArray[i];
            }
            return Body;
        }


        #region 反射
        //通过反射查找请求对应的方法（未完成）
        public void reflex()
        {
            Type t1 = this.GetType();
            //根据字符标题，取得当前函数调用
            MethodInfo method = t1.GetMethod("miaox");
            //获取需要传入的参数

            ParameterInfo[] parms = method.GetParameters();
            int haha = 1111;
            //调用
            method.Invoke(this, new object[] { haha });
        }

        public void miao(int x)
        {

        }
        #endregion
    }

