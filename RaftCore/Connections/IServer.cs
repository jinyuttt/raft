namespace RaftCore.Connections
{
    public interface IServer
    {
        /// <summary>
        /// 服务IP
        /// </summary>
        public string ServerIP { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int ServerPort { get; set; } 

      
      
        void Start();
    }
}
