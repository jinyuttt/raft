using RaftCore.Components;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace RaftCore.Connections.NodeServer.HttpServers
{
    internal class HttpProxy
    {
        private int port;
        RaftNode node = null;
        public HttpServer Server { get; private set; }
        public HttpProxy(RaftNode node,int port) {
            this.port = port;
            this.node = node;
        }
        public void Start()
        {
            Server = new HttpServer(port);
            Server.GetOps += Server_GetOps;
            Server.PostOps += Server_PostOps;
            Server.Start();
        }

        private void Server_PostOps(HttpRequest arg1, HttpResponse arg2)
        {
            // 解析查询字符串参数
            Dictionary<String, String> query_pairs = new Dictionary<String, String>();
            String query = arg1.Content; 
            String[] pairs = query.Split("&");
            foreach (String pair in pairs)
            {
                int idx = pair.IndexOf("=");
                query_pairs.Add(pair.Substring(0, idx), pair.Substring(idx + 1));
            }
           
           

            if (arg1.URL == "/requestvote")
            {
                var r = node.RequestVote(int.Parse(query_pairs["term"]), uint.Parse(query_pairs["candidateId"]), int.Parse(query_pairs["lastLogIndex"]), int.Parse(query_pairs["lastLogTerm"]));
                string ret = JsonSerializer.Serialize(r);
                arg2.Content = ret;
                arg2.Send();
            }
            else if (arg1.URL == "/appendentries")
            {
                  List<LogEntry> logEntries = null;
                string tmp;
                if (!string.IsNullOrEmpty(query_pairs["entries"]))
                {
                    tmp= WebUtility.UrlDecode(query_pairs["entries"]);
                    logEntries = JsonSerializer.Deserialize<List<LogEntry>>(tmp);
                }
                  var r = node.AppendEntries(int.Parse(query_pairs["term"]), uint.Parse(query_pairs["leaderId"]), int.Parse(query_pairs["prevLogIndex"]), int.Parse(query_pairs["prevLogTerm"]), logEntries, int.Parse(query_pairs["leaderCommit"]));
                string ret = JsonSerializer.Serialize(r);
                arg2.Content = ret;
                arg2.Send();
            }
            else if (arg1.URL == "/makerequest")
            {
                node.MakeRequest(query_pairs["request"]);
                arg2.Send();
            }
        }

        private void Server_GetOps(HttpRequest arg1, HttpResponse arg2)
        {
            if (arg1.URL == "/test")
            {
                node.StateMachine.TestConnection();
                arg2.Send();
            }
        }
    }
}
