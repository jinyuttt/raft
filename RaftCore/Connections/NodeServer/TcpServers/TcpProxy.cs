using RaftCore.Components;
using RaftCore.Connections.TcpNet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RaftCore.Connections.NodeServer.TcpServers
{
    public class TcpProxy
    {
        private int port;
        RaftNode node = null;

       
        public TCPServer Server { get; private set; }

        public TcpProxy(RaftNode node, int port)
        {
            this.port = port;
            this.node = node;

        }

        public void Start()
        {
            Server = new  TCPServer(port);
            Server.Start();
            Server.OnReceiveMessage += (msg) =>
            {


                var mymsg = Encoding.UTF8.GetString(msg.GetMsg());
                if (mymsg == "test")
                {
                    msg.Data = Encoding.UTF8.GetBytes("hi");
                    msg.Send();
                }
            
                else
                {
                    var reqmsg = Util.Deserialize<TcpMessage>(msg.GetMsg());
                    if (reqmsg != null)
                    {
                        if (reqmsg.Cmd == "requestvote")
                        {
                            var query_pairs = Util.DeserializeJson<Dictionary<string, string>>(reqmsg.Content);
                            var r = node.RequestVote(int.Parse(query_pairs["term"]), uint.Parse(query_pairs["candidateId"]), int.Parse(query_pairs["lastLogIndex"]), int.Parse(query_pairs["lastLogTerm"]));
                            byte[] ret = Util.Serialize(r);
                            msg.Data = ret;
                            msg.Send();//注意方式；

                        }

                        if (reqmsg.Cmd == "appendentries")
                        {
                            List<LogEntry> lst = null;
                            var query_pairs = Util.DeserializeJson<Dictionary<string, string>>(reqmsg.Content);
                            if (!string.IsNullOrEmpty(query_pairs["entries"]))
                            {
                                lst = JsonSerializer.Deserialize<List<LogEntry>>(query_pairs["entries"]);
                            }
                               var r = node.AppendEntries(int.Parse(query_pairs["term"]), uint.Parse(query_pairs["leaderId"]), int.Parse(query_pairs["prevLogIndex"]), int.Parse(query_pairs["prevLogTerm"]), lst, int.Parse(query_pairs["leaderCommit"]));
                                byte[] ret = Util.Serialize(r);
                                msg.Data = ret;
                                msg.Send();//注意方式；
                            
                        }
                        else if (reqmsg.Cmd == "makerequest")
                        {
                            try
                            {
                                var query_pairs = Util.DeserializeJson<Dictionary<string, string>>(reqmsg.Content);
                                node.MakeRequest(query_pairs["request"]);

                                msg.Send("addlog");
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }

             
            };
        }
    }
    
}
