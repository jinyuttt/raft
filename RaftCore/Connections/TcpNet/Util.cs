using System.Text.Json;

namespace RaftCore.Connections.TcpNet
{
    internal class Util
    {
        public static byte[] Serialize<T>(T obj)
        {
          return  JsonSerializer.SerializeToUtf8Bytes(obj);
        }

        public static T Deserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }

        public static string SerializeJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static T DeserializeJson<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }
    }
}
