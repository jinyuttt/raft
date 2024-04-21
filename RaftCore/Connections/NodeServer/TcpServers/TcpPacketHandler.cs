using System;
using System.Collections.Generic;
using System.Text;

namespace RaftCore.Connections.NodeServer.TcpServers
{
    internal class TcpPacketHandler
    {
        private byte[] buffer = new byte[1024*10];
        private int receivedLength = 0;
        private Queue<byte[]> packetQueue = new Queue<byte[]>();

        public event Action<byte[]> PacketReceived;
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
                    if (receivedLength >= messageLength + 4)
                    {
                        // 提取一个完整的消息
                        byte[] fullMessage = new byte[messageLength + 4];
                        Buffer.BlockCopy(buffer, 0, fullMessage, 0, messageLength + 4);

                        // 将消息加入队列以供处理
                        packetQueue.Enqueue(fullMessage);

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
    }
}
