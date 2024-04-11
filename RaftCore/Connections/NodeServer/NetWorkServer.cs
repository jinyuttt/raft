using RaftCore.Connections.NodeServer.HttpServers;
using RaftCore.Connections.NodeServer.TcpServers;
using System.Collections.Generic;

namespace RaftCore.Connections.NodeServer
{
    public class NetWorkServer
    {
        static readonly Dictionary<int, IServer> ValuePairs = new Dictionary<int, IServer>();
        public static IServer CreateHttpServer(RaftNode raftNode, int port)
        {
            HttpProxy httpProxy = new HttpProxy(raftNode, port);
            httpProxy.Start();
            raftNode.Server = httpProxy.Server;
            ValuePairs[port] = raftNode.Server;
            return httpProxy.Server;
        }

        public static IServer CreateTcpServer(RaftNode raftNode, int port)
        {
            TcpProxy httpProxy = new TcpProxy(raftNode, port);
            httpProxy.Start();
             raftNode.Server = httpProxy.Server;
          //  return null;
            return httpProxy.Server;
        }
    }
}
