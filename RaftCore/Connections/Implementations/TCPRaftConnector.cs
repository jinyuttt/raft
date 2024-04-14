using RaftCore.Components;
using RaftCore.Connections.NodeServer.TcpServers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaftCore.Connections.Implementations
{
    public class TCPRaftConnector : IRaftConnector
    {

        readonly TCPClient client = new TCPClient();

        public uint NodeId { get; private set; }
        private string baseURL { get; set; }
        public TCPRaftConnector(uint nodeId, string baseURL)
        {
            NodeId = nodeId;
            this.baseURL = baseURL;
            string[] addr = baseURL.Split(":");
            client.ServerIP = addr[0];
            client.Port= int.Parse(addr[1]);
           
        }

        public Result<bool> AppendEntries(int term, uint leaderId, int prevLogIndex, int prevLogTerm, List<LogEntry> entries, int leaderCommit)
        {

            // parse response into a Result object
            var res = SendAppendEntries(term, leaderId, prevLogIndex, prevLogTerm, entries, leaderCommit).Result;
            return res;
           // return ParseResultFromJSON(res);

        }

        public void MakeRequest(string command)
        {
            SendMakeRequest(command);
        }

        public Result<bool> RequestVote(int term, uint candidateId, int lastLogIndex, int lastLogTerm)
        {

            var res = SendRequestVote(term, candidateId, lastLogIndex, lastLogTerm).Result;
            return res;
           // return ParseResultFromJSON(res);



        }

        public void TestConnection()
        {
            SendTestConnection();
        }


        private async void SendMakeRequest(string command)
        {

            var req = new Dictionary<string, string> { { "request", command } };
            var reqj = Util.SerializeJson(req);
            var reqmsg = Util.Serialize(new TcpMessage() { Cmd = "makerequest", Content = reqj });
          var rsp=  await client.SendGetReply(reqmsg,TimeSpan.FromSeconds(10));



        }

        private async Task<Result<bool>>SendRequestVote(int term, uint candidateId, int lastLogIndex, int lastLogTerm)
        {
            try
            {
                var req = new Dictionary<string, string>
            {
               { "term", term.ToString() },
               { "candidateId", candidateId.ToString() },
               { "lastLogIndex", lastLogIndex.ToString() },
               { "lastLogTerm", lastLogTerm.ToString() }
            };
                Result<bool>  r = null;
              
                    var command = Util.SerializeJson(req);
                    var tmp = Util.Serialize(new TcpMessage() { Cmd = "requestvote", Content = command });
                   var   ret = await client.SendGetReply(tmp, TimeSpan.FromSeconds(20));
                //

                try
                {
                    r = Util.Deserialize<Result<bool>>(ret);

                    return r;
                }
                catch (Exception ex)
                {
                    return null;
                }
               
            }
            catch (Exception ex)
            {
                return null;
            }
          //  return Util.SerializeJson(r);
        }   

        private async Task<Result<bool>> SendAppendEntries(int term, uint leaderId, int prevLogIndex, int prevLogTerm, List<LogEntry> entries, int leaderCommit)
        {
        //    object[] entriesDict = null;
        //    if (entries != null)
        //    {
        //        entriesDict = new object[entries.Count];
        //        for (int i = 0; i < entries.Count; i++)
        //        {
        //            entriesDict[i] = new { term = entries[i].TermNumber, index = entries[i].Index, command = entries[i].Command };
        //        }
        //    }
            var req = new Dictionary<string, string>
            {
               { "term", term.ToString() },
               { "leaderId", leaderId.ToString() },
               { "prevLogIndex", prevLogIndex.ToString() },
               { "prevLogTerm", prevLogTerm.ToString() },
               { "entries",entries==null?null:Util.SerializeJson(entries) },
               { "leaderCommit", leaderCommit.ToString() }
            };
                var command = Util.SerializeJson(req);
                var tmp = Util.Serialize(new TcpMessage() { Cmd = "appendentries", Content = command });
                var ret= await client.SendGetReply(tmp, TimeSpan.FromSeconds(10));

            return Util.Deserialize<Result<bool>>(ret);

        }

        private async void SendTestConnection()
        {

            // string[] address = baseURL.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            // await tcpClient.Connect(address[0], int.Parse(address[1]));
            //  var replyMsg = await tcpClient.WriteLineAndGetReply("test", TimeSpan.FromSeconds(10));
            await client.Connect();
            await client.SendGetReply("test", TimeSpan.FromSeconds(10));
        }


        private Result<bool> ParseResultFromJSON(string res)
        {
            res = res.Trim();
            if(string.IsNullOrEmpty(res))
            {
                return null;
            }
           return Util.DeserializeJson<Result<bool>>(res);
            // Assumes valid input. One of:
            // {"term":22,"value":false}
            // {"value":true,"term":1}
            //if (res == null) return null;
            //var sep = res.Split(",");
            //string strTerm;
            //string strValue;

            //if (sep[0][sep[0].Length - 1] == 'e')
            //{
            //    strValue = sep[0];
            //    strTerm = sep[1];
            //}
            //else
            //{
            //    strTerm = sep[0];
            //    strValue = sep[1];
            //}
            //strTerm.Trim('{');
            //strTerm.Trim('}');
            //strValue.Trim('{');
            //strValue.Trim('}');

            //int resultTerm;
            //bool resultValue;

            //if (strValue[8] == 'f')
            //{
            //    resultValue = false;
            //}
            //else
            //{
            //    resultValue = true;
            //}

            //resultTerm = int.Parse(strTerm.Substring(7, 1));

            //return new Result<bool>(resultValue, resultTerm);
        }
    }
}
