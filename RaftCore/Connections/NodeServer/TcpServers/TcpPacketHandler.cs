using System;
using System.Collections.Generic;
using System.Linq;

namespace RaftCore.Connections.NodeServer.TcpServers
{

    /// <summary>
    /// 处理接收的包
    /// </summary>
    public class TcpPacketHandler
    {
        static readonly byte[] tail = new byte[] { 0xAA, 0xFF, 0xBB, 0xCF };
        private byte[] buffer = new byte[10240];
        private int receivedLength = 0;
        private Queue<byte[]> packetQueue = new Queue<byte[]>();
        private byte[] buftail = new byte[4];

        /// <summary>
        /// 每包最大数据量，默认1G，-1时不检查
        /// </summary>
        public static int MaxSize { get; set; } = 1024 * 1204 * 1024;
        public event Action<byte[]> PacketReceived;
        
        /// <summary>
        /// 转载数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public void ReceiveData(byte[] data, int length)
        {
            // 将接收到的数据添加到缓冲区
            Buffer.BlockCopy(data, 0, buffer, receivedLength, length);
            receivedLength += length;

            while (true)
            {
                // 假设头部为4个字节，包含消息的总长度
                if (receivedLength >= 4)
                {
                    int messageLength = BitConverter.ToInt32(buffer, 0);
                    if (messageLength > buffer.Length)
                    {
                        //缓存收取不下，扩展缓存
                        //
                        if (MaxSize > messageLength||MaxSize<1)
                        {
                            byte[] tmp = new byte[buffer.Length];
                            Buffer.BlockCopy(buffer, 0, tmp, 0, tmp.Length);
                            buffer = new byte[messageLength + 10240];
                            Buffer.BlockCopy(tmp, 0, buffer, 0, tmp.Length);
                        }
                        else
                        {
                            //1.消息大于最大缓存接收，抛弃消息
                            //2.消息接收异常，长度不正确，抛弃
                            try
                            {
                                ProcessError();
                            }
                            catch {
                            }
                            continue;
                        }
                    }
                    if(messageLength<0)
                    {
                        ProcessError();
                        continue;
                    }
                    if (receivedLength >= messageLength + 4)
                    {
                        // 提取一个完整的消息
                        byte[] fullMessage = new byte[messageLength + 4];//消息长度算了校验字节
                        Buffer.BlockCopy(buffer, 0, fullMessage, 0, messageLength + 4);
                        //校验
                        Buffer.BlockCopy(buffer, messageLength, buftail, 0,  4);
                        if (buftail.SequenceEqual(tail))
                        {
                            // 将消息加入队列以供处理
                            packetQueue.Enqueue(fullMessage);
                        }
                        else
                        {
                            //从buffer中找到完整数据
                            ProcessError();
                        }

                        // 移动缓冲区剩余的数据到起始位置
                        Buffer.BlockCopy(buffer, messageLength + 4, buffer, 0, receivedLength - (messageLength + 4));
                        receivedLength -= messageLength + 4;
                    }
                    else
                    {
                        // 数据不足以构成一个完整的消息，退出循环
                        break;
                    }
                }
                else
                {
                    // 数据不足以读取头部信息，退出循环
                    break;
                }
            }
            if (PacketReceived != null)
            {
                // 处理队列中的完整消息
                while (packetQueue.Count > 0)
                {
                    byte[] packet = packetQueue.Dequeue();
                    int msgLength = BitConverter.ToInt32(packet, 0);
                    PacketReceived(packet);
                    // string message = Encoding.UTF8.GetString(packet, 4, msgLength);
                    // 在这里处理消息
                    // Console.WriteLine("Received message: " + message);
                }
            }
        }

        /// <summary>
        /// 处理不正常的数据
        /// </summary>
        private void ProcessError()
        {

            try
            {
                int idx = ByteByte(buffer, tail);
                if (idx >= 0)
                {
                    //移动buffer，去除校验
                    int num = idx + 1;
                    Buffer.BlockCopy(buffer, idx + 4, buffer, 0, receivedLength - idx-4);
                    receivedLength = receivedLength - idx -4;
                }
                else
                {
                    //保留最后3字节
                    Buffer.BlockCopy(buffer, receivedLength - 3, buffer, 0, 3);
                    receivedLength = 3;
                }
            }
            catch (Exception ex)
            {

            }
        }

        // KMP算法：得到next数组
        // 思路
        // 1.next数组，按照定义，0和1位置一定分别为-1和0
        public static int[] GetNext(byte[] bytes)
        {
            int[] next = new int[bytes.Length];
            int j = 0;
            int k = -1;
            next[j] = k;
            while (j < bytes.Length - 1)
            {
                // next[j]是str[j]的最长公共前后缀的长度，也就是说str[0]到str[k]是str[j]的最长公共前后缀，
                if (k == -1 || bytes[j] == bytes[k])// 如果相等，str[k] == str[j]，即找到了str[0]->str[k]和str[j-k]->str[j]，也就是0到k和j-k到j的一个公共前后缀，
                {
                    k++;
                    next[j + 1] = k;// 也就可以得出j+1的最长公共前后缀，因为此时k=j，而0-k是j的一个公共前后缀，因此，j+1的最长公共前后缀也就是k+1
                                    // 所以next[j+1]，也就是j+1的最长公共前后缀的长度为k+1
                    j++;
                }
                else// 如果不相等，那么就需要进一步寻找更短的最长公共前后缀，
                {
                    k = next[k];// 而next[k]则表示str[k]的最长公共前后缀所在的位置，也就是更短的最长公共前后缀
                }
            }
            return next;
        }
        // KMP算法：模式匹配
        public static int ByteByte(byte[] haystack, byte[] needle)
        {
            int len = haystack.Length;
            if (len == 1)
            {
                return 0;
            }
            int[] next = GetNext(needle);
            int index = 0;
            int i = 0;
            while (index < next.Length && i < len)
            {
                if (index == -1 || haystack[i] == needle[index])
                {
                    i++;
                    index++;
                }
                else
                {
                    index = next[index];
                }
            }
            if (i >= len && index < next.Length)
            {
                return -1;
            }
            return i - needle.Length;
        }
    }
}
