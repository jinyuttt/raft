using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace RaftCore.Connections.NodeServer.TcpServers
{
    public class TCPServer:IServer
    {
        private readonly Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private byte[] _result = new byte[4];

        private readonly int port;

        public string ServerIP { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ServerPort { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event Action<TcpResponse> OnReceiveMessage;
        public TCPServer(int port)
        {

            this.port = port;
         
        }

        public void Start()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
            _socket.Bind(endPoint);
        
            _socket.Listen(100); // 设定最多10个排队连接请求
            Task.Run(ListenClientConnect);

        
        }
        private void ListenClientConnect()
        {
            while (true)
            {
                var clientSocket = _socket.Accept();
                //  clientSocket.Send(Encoding.UTF8.GetBytes("我是服务器"));
                //  Console.WriteLine("我是服务器");
              //  Console.WriteLine($"我是{port}");
                var receiveThread = new Thread(ReceiveMessage);
                receiveThread.IsBackground = true;
                receiveThread.Name=port.ToString();
                receiveThread.Start(clientSocket);
            }
        }
        private void ReceiveMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = myClientSocket.Receive(_result);
                    if (receiveNumber == 0)
                        return;
                    int len = BitConverter.ToInt32(_result);
                    byte[] buf = new byte[len];
                    receiveNumber = myClientSocket.Receive(buf);
                    if (receiveNumber == 0) return;
                    long id = -1;
                    var bytes = TcpDamil.GetMessage(buf, ref id);
                    if (bytes != null)
                    {
                       TcpResponse tcpResponse = new TcpResponse(bytes, myClientSocket);
                                    tcpResponse.DataId = id;
                        if (OnReceiveMessage != null)
                        {

                            Task.Run(() => OnReceiveMessage(tcpResponse));


                        }
                       
                    }
                   
                }
                catch (Exception ex)
                {
                    myClientSocket.Close();//关闭Socket并释放资源
                    //myClientSocket.Shutdown(SocketShutdown.Both);//禁止发送和上传
                    break;
                }
            }
        }
    }

    public static class TcpClientEx
    {
        public static bool IsOnline(this TcpClient client)
        {
            return !((client.Client.Poll(15000, SelectMode.SelectRead) && (client.Client.Available == 0)) || !client.Client.Connected);
        }
    }
}
