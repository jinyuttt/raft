namespace RaftCore.Connections
{
    public interface IServer
    {
        public string ServerIP { get; set; }

        public int ServerPort { get; set; } 
      
        void Start();
    }
}
