using RaftCore;
using RaftCore.Connections;
using RaftCore.Connections.Implementations;
using RaftCore.StateMachine.Implementations;
using Serilog.Formatting.Json;
using Serilog;
using System.Text.Json;
using RaftCore.Connections.NodeServer;
using RaftCore.Connections.NodeServer.TcpServers;
using System.Text;
using System.Collections.Concurrent;

namespace ConsoleApp1
{
    internal class Program
    {
        static ConcurrentDictionary<string, RaftNode> dic= new ConcurrentDictionary<string, RaftNode>();
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
          //  testtcp();
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
            //  .Enrich.WithEnvironmentUserName()
             // .Enrich.WithInfo()
              
              //使用自定义的Enricher
              .WriteTo.Console(new JsonFormatter())
              .WriteTo.File("SerilFileLog.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}]{Message:lj}{NewLine}{Exception}")
              .CreateLogger();
          //  logBuilder.AddSerilog();
            for (int i = 1; i < 4; i++)
            {
                 testTcp((uint)i, 8080+i);
               //test((uint)i, 8080 + i);
            }
            while(true)
            {
                Console.ReadLine();
                var node = dic["8081"];
                Console.WriteLine(node.NodeState);
                node.MakeRequest("create test test.db");
                node.MakeRequest("Set test 8081");
                node.MakeRequest("delete test 8081");
                node.MakeRequest("clear test 8081");
                //  node.MakeRequest("Set X 8081");
                //node = dic["8082"];
                //Console.WriteLine(node.NodeState);
                ////node.MakeRequest("8082");
                //node = dic["8083"];
                //Console.WriteLine(node.NodeState);
                //node.MakeRequest("8083");
            }
            Console.ReadKey();
        }

        static void test(uint num,int port)
        {
            var cluster = new RaftCluster();
            cluster.AddNode(new APIRaftConnector(1, "http://localhost:8081"));
            cluster.AddNode(new APIRaftConnector(2, "http://localhost:8082"));
            cluster.AddNode(new APIRaftConnector(3, "http://localhost:8083"));

            //SimpleWebServer server = new SimpleWebServer();
            //server.start("lcoalhost", port);
           
            var node = new RaftNode(num, new NumeralStateMachine());
            node.Configure(cluster);
            NetWorkServer.CreateHttpServer(node, port);
            node.Run();
            dic[port.ToString()] = node;

        }
        static void testTcp(uint num, int port)
        {
            Task.Run(() =>
            {
                var cluster = new RaftCluster();
                cluster.AddNode(new TCPRaftConnector(1, "127.0.0.1:8081"));
                cluster.AddNode(new TCPRaftConnector(2, "127.0.0.1:8082"));
                cluster.AddNode(new TCPRaftConnector(3, "127.0.0.1:8083"));



                var node = new RaftNode(num, new NumeralStateMachine());
                NetWorkServer.CreateTcpServer(node, port);
                node.Configure(cluster);
                node.Run();
                dic[port.ToString()] = node;
            });



        }

        static async void testtcp()
        {
            TCPServer server = new TCPServer(8081);
            server.OnReceiveMessage += (s) => {
               var m= Encoding.UTF8.GetString(s.GetMsg());
                Console.WriteLine(m);
                s.Data = Encoding.UTF8.GetBytes("hello");
                s.Send();
            };
            server.Start();

            TCPClient tCPClient = new TCPClient();
            tCPClient.Port = 8081;
            tCPClient.ServerIP = "127.0.0.1";
            tCPClient.Connect();
           var r= tCPClient.SendGetReply("hi!",TimeSpan.FromSeconds(10));
          
            Console.WriteLine(r);
        }

       
      
    }
}
