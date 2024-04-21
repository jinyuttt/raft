namespace RaftCore.Connections.NodeServer.TcpServers
{
    internal class TcpMessage
    {
        public string Cmd { get; set; }

        public byte[] Content { get; set; }
    }
}
