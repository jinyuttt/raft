using System;

namespace RaftCore.Connections.NodeServer.TcpServers
{
    /// <summary>
    /// 数据解析
    /// </summary>
    internal class TcpDelimiter
    {
        static readonly byte[] tail = new byte[] {0xAA,0xFF,0xBB,0xCF};

        /// <summary>
        /// 雪花算法
        /// </summary>
        static readonly SnowflakeIdGenerator snowflake = new SnowflakeIdGenerator();

        /// <summary>
        /// 组装数据
        /// </summary>
        /// <param name="message">数据</param>
        /// <param name="id">返回Id</param>
        /// <returns></returns>
        public static byte[] BuildMessage(byte[] message,ref long id)
        {
          
            byte[] result = new byte[message.Length + 4 + 8+4];
            byte[] len = BitConverter.GetBytes(message.Length+12);//消息长度算了校验字节
            id = snowflake.NextId();
            byte[] ids = BitConverter.GetBytes(id);
            Array.Copy(len, result, 4);
            Array.Copy(ids, 0, result, 4, 8);
            Array.Copy(message, 0, result, 8 + 4, message.Length);
            Array.Copy(tail, 0, result, 8 + 4+message.Length,4);
            return result;
        }

        /// <summary>
        /// 组装数据
        /// </summary>
        /// <param name="message">数据</param>
        /// <param name="id">插入ID</param>
        /// <returns></returns>
        public static byte[] BuildMessage(byte[] message,  long id)
        {
            byte[] result = new byte[message.Length + 4 + 8+4];
            byte[] len = BitConverter.GetBytes(message.Length+12);//消息长度算了校验字节
            byte[] ids = BitConverter.GetBytes(id);
            Array.Copy(len, result, 4);
            Array.Copy(ids, 0, result, 4, 8);
            Array.Copy(message, 0, result, 8 + 4, message.Length);
            Array.Copy(tail, 0, result, 8 + 4 + message.Length, 4);
            return result;
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="message">数据</param>
        /// <param name="id">解析ID</param>
        /// <returns></returns>
        public static byte[] GetMessage(byte[] message, ref long id)
        {

            Memory<byte> memory = new Memory<byte>(message);
            var bytes = memory.Slice(4, 8).ToArray();//去头 长度
            id = BitConverter.ToInt64(bytes);
            return memory.Slice(12,message.Length-16).ToArray();//去尾 校验字节
        }




        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="memory">数据</param>
        /// <param name="len">数据长度</param>
        /// <param name="id">解析ID</param>
        /// <returns></returns>
        public static byte[] GetMessage(Memory<byte> memory, int len,  ref long id,int start = 0)
        { 
            id = BitConverter.ToInt64(memory.Slice(start,8).Span);
            return memory.Slice(start+8, len-8).ToArray();
        }
    }
}
