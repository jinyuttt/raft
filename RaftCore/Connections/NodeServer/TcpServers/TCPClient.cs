using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaftCore.Connections.NodeServer.TcpServers
{
    public class TCPClient
    {

        /// <summary>
        /// 服务端IP
        /// </summary>
        public string ServerIP { get; set; }

        /// <summary>
        /// 服务端端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 缓存区大小，默认5M
        /// </summary>
        public int BufferSize { get; set; } = 1024 * 1024 * 5;
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnected { get { return clientSocket.Connected; } }

        private readonly Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// 回复的数据
        /// </summary>
        readonly ConcurrentDictionary<long, byte[]> dic = new ConcurrentDictionary<long, byte[]>();

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {

            await clientSocket.ConnectAsync(ServerIP, Port);
            await Task.Run(ReceiveMessage);
        }

        /// <summary>
        /// 接收数据返回
        /// </summary>
        private async void ReceiveMessage()
        {

            byte[] recvLen = new byte[4];//接收数据长度
            int bytesLen=-1;
            Memory<byte> memory = new Memory<byte>(new byte[BufferSize]); 
            long id = -1;
            while (true)
            {   
              
                bytesLen =await clientSocket.ReceiveAsync(recvLen);
                int len = BitConverter.ToInt32(recvLen);
                if (len < BufferSize)
                {
                    bytesLen = await clientSocket.ReceiveAsync(memory);
                    var msg = TcpDelimiter.GetMessage(memory, bytesLen, ref id);
                    dic[id] = msg;
                }
                else
                {
                    byte[] recvBytes = new byte[len + 8];
                   await clientSocket.ReceiveAsync(recvBytes);
                    var msg = TcpDelimiter.GetMessage(recvBytes, ref id);
                    dic[id] = msg;
                }
              
                
                if(!clientSocket.Connected)
                {
                    break;
                }
            }

        }

        /// <summary>
        /// 发送并且接收数据
        /// </summary>
        /// <param name="data">发送的数据</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<byte[]> SendGetReply(byte[] data, TimeSpan timeout)
        {
            byte[] buf = new byte[0];
            if (clientSocket.Connected)
            {
                long id = -1;
                int num = 1;
                var bytes = TcpDelimiter.BuildMessage(data, ref id);
                await clientSocket.SendAsync(bytes);
                while (true)
                {
                    if (dic.TryGetValue(id, out buf))
                    {
                        break;
                    }
                    num++;
                    Thread.Sleep(100);
                    if (num * 100> timeout.TotalMilliseconds)
                    {
                        break;
                    }
                }
            }
            return buf;
        }

        /// <summary>
        /// 发送接收字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<string> SendGetReply(string data, TimeSpan timeout)
        {
            var tmp = Encoding.UTF8.GetBytes(data);
            var r = await this.SendGetReply(tmp, timeout);
            return Encoding.UTF8.GetString(r);
        }

      /// <summary>
      /// 只发送
      /// </summary>
      /// <param name="data"></param>
      /// <returns>-1没有连接</returns>
        public int Send(byte[] data)
        {

            if (clientSocket.Connected)
            {
                long id = -1;
                var bytes = TcpDelimiter.BuildMessage(data, ref id);
                return clientSocket.Send(bytes);

            }
            return -1;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
           clientSocket.Close();
        }
    }
}
