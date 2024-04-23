using System.Net.Sockets;
using System.Text;

namespace RaftCore.Connections.NodeServer.TcpServers
{

    /// <summary>
    /// 接收
    /// </summary>
    public class TcpResponse
    {

        /// <summary>
        /// 回传的ID
        /// </summary>
        public long DataId {  get; set; }

        /// <summary>
        /// 回传的数据
        /// </summary>
        public byte[]  Data { get; set; }

        private readonly byte[] bytes;

        private readonly Socket mySocket;
        public TcpResponse(byte[] msg,Socket socket)
        {
            bytes = msg;
            mySocket = socket;
        }


        /// <summary>
        /// 接收的数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetMsg()
        {
            return bytes;
        }

        /// <summary>
        /// 回传数据
        /// </summary>
        public void Send()
        {
           var by = TcpDelimiter.BuildMessage(Data, DataId);
            mySocket.Send(by);
        }

        /// <summary>
        /// 单独发送字符串
        /// </summary>
        /// <param name="msg"></param>
        public void Send(string msg)
        {
           Data=Encoding.UTF8.GetBytes(msg);
           this.Send();
        }

        /// <summary>
        /// 主动关闭
        /// </summary>
        public void Close()
        {
            mySocket.Close();
        }
    }
}
