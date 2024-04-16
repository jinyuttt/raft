using System;

namespace RaftCore.Connections.NodeServer.TcpServers
{
    /// <summary>
    /// 数据解析
    /// </summary>
    internal class TcpDelimiter
    {
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
            byte[] result = new byte[message.Length + 4 + 8];
            byte[] len = BitConverter.GetBytes(message.Length+8);
            id = snowflake.NextId();
            byte[] ids = BitConverter.GetBytes(id);
            Array.Copy(len, result, 4);
            Array.Copy(ids, 0, result, 4, 8);
            Array.Copy(message, 0, result, 8 + 4, message.Length);
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
            byte[] result = new byte[message.Length + 4 + 8];
            byte[] len = BitConverter.GetBytes(message.Length);
            byte[] ids = BitConverter.GetBytes(id);
            Array.Copy(len, result, 4);
            Array.Copy(ids, 0, result, 4, 8);
            Array.Copy(message, 0, result, 8 + 4, message.Length);
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
            var bytes = memory.Slice(0, 8).ToArray();
            id = BitConverter.ToInt64(bytes);
            return memory.Slice(8).ToArray();
        }


      

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="memory">数据</param>
        /// <param name="len">数据长度</param>
        /// <param name="id">解析ID</param>
        /// <returns></returns>
        public static byte[] GetMessage(Memory<byte> memory,int len, ref long id)
        { 
            id = BitConverter.ToInt64(memory.Slice(0,8).Span);
            return memory.Slice(8, len-8).ToArray();
        }
    }
}
