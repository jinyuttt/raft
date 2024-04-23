using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;


namespace RaftCore.Connections.NodeServer.TcpServers
{
    public class TCPServer:IServer
    {
        private readonly Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
       

        private readonly int port;

        public string ServerIP { get ; set ; }
        public int ServerPort { get; set; }

        /// <summary>
        /// 缓存区大小，默认1M
        /// </summary>
        public int BufferSize { get; set; } = 1024*1024;

        /// <summary>
        /// 接收数据
        /// </summary>
        public event Action<TcpResponse> OnReceiveMessage;
        public TCPServer(int port)
        {

            this.port = port;
         
        }

       /// <summary>
       /// 启动
       /// </summary>
        public void Start()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
            _socket.Bind(endPoint);
            _socket.Listen(100); // 设定最多100个排队连接请求
            Task.Run(ListenClientConnect);

        
        }

        /// <summary>
        /// 接收连接
        /// </summary>
        private void ListenClientConnect()
        {
            while (true)
            {
                var clientSocket = _socket.Accept();
                //var receiveThread = new Thread(ReceiveMessage);
                //receiveThread.IsBackground = true;
                //receiveThread.Name=port.ToString();
                //receiveThread.Start(clientSocket);
              
                Task.Run(() => { ReceiveBufferMessage(clientSocket); });
            }
        }

        private async void ReceiveBufferMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            TcpPacketHandler tcpPacket = new TcpPacketHandler();
            tcpPacket.PacketReceived += async (p) => { 
                long id = -1;
                byte[] receiveBytes = null;
                receiveBytes = TcpDelimiter.GetMessage(p,  ref id);
                if (receiveBytes != null)
                {
                    TcpResponse tcpResponse = new TcpResponse(receiveBytes, myClientSocket);
                    tcpResponse.DataId = id;
                    if (OnReceiveMessage != null)
                    {
                        await Task.Run(() => OnReceiveMessage(tcpResponse));
                    }
                }

            };
            byte[] buffer = new byte[BufferSize];
            while (myClientSocket.Connected)
            {
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = await myClientSocket.ReceiveAsync(buffer);
                    tcpPacket.ReceiveData(buffer, receiveNumber);
                   

                }
                catch (Exception ex)
                {
                    myClientSocket.Close();//关闭Socket并释放资源

                    break;
                }
            }
            tcpPacket = null;
        }


        ///// <summary>
        ///// 接收数据
        ///// </summary>
        ///// <param name="clientSocket"></param>
        //private async void ReceiveMessage(object clientSocket)
        //{
        //    Socket myClientSocket = (Socket)clientSocket;
        //    byte[] _result = new byte[4];
        //    Memory<byte> buffer = new Memory<byte>(new byte[BufferSize]);//10M
           
        //    while (myClientSocket.Connected)
        //    {
        //        try
        //        {
        //            //通过clientSocket接收数据  
        //            byte[] receiveBytes = null;
        //            int receiveNumber = await myClientSocket.ReceiveAsync(_result);
        //            if (receiveNumber == 0)
        //                return;
        //            int len = BitConverter.ToInt32(_result);
        //            long id = -1;
        //            if (len < buffer.Length)
        //            {
        //                //缓存接收
        //                int currrn = 0;
        //                while (currrn < len)
        //                {
        //                    receiveNumber = await myClientSocket.ReceiveAsync(buffer);
        //                    if (receiveNumber == 0) return;

        //                    while (receiveNumber < len)
        //                    {
        //                        receiveNumber = await myClientSocket.ReceiveAsync(buffer);
        //                    }
        //                }
        //            }

        //            // receiveBytes = TcpDelimiter.GetMessage(buffer, receiveNumber, ref id);

        //            else
        //            {
        //                //超过缓存，数据接收
        //                byte[] buf = new byte[len];
        //                receiveNumber = await myClientSocket.ReceiveAsync(buf);
        //                if (receiveNumber == 0) return;
        //                receiveBytes = TcpDelimiter.GetMessage(buf, ref id);
        //            }

        //            // var bytes = TcpDamil.GetMessage(buf, ref id);
        //            if (receiveBytes != null)
        //            {
        //                TcpResponse tcpResponse = new TcpResponse(receiveBytes, myClientSocket);
        //                tcpResponse.DataId = id;
        //                if (OnReceiveMessage != null)
        //                {
        //                    await Task.Run(() => OnReceiveMessage(tcpResponse));
        //                }

        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            myClientSocket.Close();//关闭Socket并释放资源

        //            break;
        //        }
        //    }
        //}
  
    
    }

  
}
