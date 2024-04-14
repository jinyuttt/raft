using System;

namespace RaftCore.Connections.NodeServer.TcpServers
{
    internal class TcpDamil
    {
      static readonly SnowflakeIdGenerator snowflake = new SnowflakeIdGenerator();
        public static byte[] BuildMessage(byte[] message,ref long id)
        {
            byte[] result = new byte[message.Length + 4 + 8];
            Memory<byte> memory = new Memory<byte>(result);
            byte[] len = BitConverter.GetBytes(message.Length+8);
            id = snowflake.nextId();
            byte[] ids = BitConverter.GetBytes(id);
            Array.Copy(len, result, 4);
            Array.Copy(ids, 0, result, 4, 8);
            Array.Copy(message, 0, result, 8 + 4, message.Length);
            return result;
        }

        public static byte[] BuildMessage(byte[] message,  long id)
        {
            byte[] result = new byte[message.Length + 4 + 8];
            Memory<byte> memory = new Memory<byte>(result);
            byte[] len = BitConverter.GetBytes(message.Length);
            byte[] ids = BitConverter.GetBytes(id);
            Array.Copy(len, result, 4);
            Array.Copy(ids, 0, result, 4, 8);
            Array.Copy(message, 0, result, 8 + 4, message.Length);
            return result;
        }
        public static byte[] GetMessage(byte[] message, ref long id)
        {

            Memory<byte> memory = new Memory<byte>(message);
            var bytes = memory.Slice(0, 8).ToArray();
            id = BitConverter.ToInt64(bytes);
            return memory.Slice(8).ToArray();
        }


        //

      

      
        public static byte[] GetMessage(Memory<byte> memory,int len, ref long id)
        {

            // Memory<byte> memory = new Memory<byte>(message);
            // var bytes = memory.Slice(0, 8).ToArray();
            //  id = BitConverter.ToInt64(bytes);
            id = BitConverter.ToInt64(memory.Slice(0,8).Span);
            return memory.Slice(8, len-8).ToArray();
        }
    }
}
