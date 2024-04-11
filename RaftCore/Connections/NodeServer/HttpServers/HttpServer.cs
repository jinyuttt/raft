using System.Net.Sockets;
using System.Net;
using RaftCore.Connections;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;


public class HttpServer : IServer
    {
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string ServerIP { get;  set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int ServerPort { get;  set; }

        /// <summary>
        /// 是否运行
        /// </summary>
        public bool IsRunning { get; private set; }
  

    /// <summary>
    /// 服务端Socket
    /// </summary>
    private TcpListener serverListener;

        /// <summary>
        /// 日志单例对象
        /// </summary>
       // public static ILogger _logger = Log.Logger;

        public event Action<HttpRequest, HttpResponse> PostOps;//post事件
        public event Action<HttpRequest, HttpResponse> GetOps;//get事件

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口</param>
        public HttpServer(int port)
        {
            this.ServerPort = port;
        }
        public HttpServer(string port)
        {
            this.ServerPort = Convert.ToInt32(port);
        }
        //开启服务器
        public void Start()
        { if (IsRunning) return;
        Task.Factory.StartNew(() =>
        {
  //创建服务端Socket
            this.serverListener = new TcpListener(IPAddress.Any, ServerPort);
            this.IsRunning = true;
            this.serverListener.Start();
          //  _logger.Information(string.Format("Http服务器正在监听{0}", serverListener.LocalEndpoint.ToString()));
            try
            {
                while (IsRunning)
                {
                    TcpClient client = serverListener.AcceptTcpClient();
                    Thread requestThread = new Thread(() => { ProcessRequest(client); });
                    requestThread.IsBackground = true;
                    requestThread.Start();
                }
            }
            catch
            {

            }
        });
        }
        //关闭服务器
        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            serverListener.Stop();
        }

        #region 内部方法
        /// <summary>
        /// 处理客户端请求
        /// </summary>
        /// <param name="tcpClient">客户端Socket</param>
        private void ProcessRequest(TcpClient tcpClient)
        {
            //处理请求
            Stream clientStream = tcpClient.GetStream();
            Thread.Sleep(390);//不知道为什么不Sleep一下接收不到body，可以试试去掉
            if (clientStream != null)
            {
                //构造HTTP请求
                HttpRequest request = new HttpRequest(clientStream);

                //构造HTTP响应
                HttpResponse response = new HttpResponse(clientStream);

                //处理请求类型
                switch (request.Method)//根据请求方法触发不同事件
                {
                    case "GET":
                        OnGet(request, response);
                        break;
                    case "POST":
                        OnPost(request, response);
                        break;
                    default:
                        OnDefault(request, response);
                        break;
                }
            }

        }
        #endregion

        #region 方法
        /// <summary>
        /// 响应Get请求
        /// </summary>
        /// <param name="request">请求报文</param>
        public void OnGet(HttpRequest request, HttpResponse response)
        {
            GetOps(request, response);
        }

        /// <summary>
        /// 响应Post请求
        /// </summary>
        /// <param name="request"></param>
        public void OnPost(HttpRequest request, HttpResponse response)
        {
            PostOps(request, response);
        }

        /// <summary>
        /// 响应默认请求
        /// </summary>
        public void OnDefault(HttpRequest request, HttpResponse response)
        {

        }
        #endregion
    }

