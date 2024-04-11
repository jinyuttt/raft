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

        public string ServerIP { get; set; }

        public int Port { get; set; }

        public bool IsConnected { get; set; }
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        ConcurrentDictionary<long, byte[]> dic
            = new ConcurrentDictionary<long, byte[]>();

        public async Task Connect()
        {

            await clientSocket.ConnectAsync(ServerIP, Port);
            await Task.Run(ReceiveMessage);
        }
        private async void ReceiveMessage()
        {

            byte[] recvLen = new byte[4];
            int bytes;
            while (true)
            {
                bytes =await clientSocket.ReceiveAsync(recvLen);
                int len = BitConverter.ToInt32(recvLen);
                byte[] recvBytes = new byte[len + 8];
                bytes =await clientSocket.ReceiveAsync(recvBytes);
                long id = -1;
                var msg = TcpDamil.GetMessage(recvBytes, ref id);
                dic[id] = msg;
            }

        }

        public async Task<byte[]> SendGetReply(byte[] data, TimeSpan timeout)
        {
            byte[] buf = new byte[0];
            if (clientSocket.Connected)
            {
                long id = -1;
                int num = 1;
                var bytes = TcpDamil.BuildMessage(data, ref id);
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

        public async Task<string> SendGetReply(string data, TimeSpan timeout)
        {
            var tmp = Encoding.UTF8.GetBytes(data);
            var r = await this.SendGetReply(tmp, timeout);
            return Encoding.UTF8.GetString(r);
        }

        public void Send(byte[] data)
        {

            if (clientSocket.Connected)
            {
                long id = -1;
                var bytes = TcpDamil.BuildMessage(data, ref id);
                clientSocket.Send(bytes);

            }

        }
    }
}
