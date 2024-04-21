using System;
using System.Text.Json;

namespace RaftCore.Connections.NodeServer.TcpServers
{
    internal class Util
    {

        static Util(){
         
        }
      static  SnowflakeIdGenerator snowflakeIdGenerator = new SnowflakeIdGenerator();
        public static long GetId()
        {
          return  snowflakeIdGenerator.NextId();
        }

        public static byte[] Serialize<T>(T obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }

        public static T Deserialize<T>(byte[] data)
        {
            try
            {
                if (data == null)
                {
                    return default(T);
                }
                return JsonSerializer.Deserialize<T>(data);
            }
            catch(Exception ex)
            {
                return default(T);
            }
        }

        public static string SerializeJson<T>(T obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj);
            }
            catch (Exception e)
            {
                return null;
            }

        }
        
            
        public static T DeserializeJson<T>(string data)
        {
           
            return JsonSerializer.Deserialize<T>(data);
        }
    }
}
