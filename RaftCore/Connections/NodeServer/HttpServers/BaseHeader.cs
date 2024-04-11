using System.Collections.Generic;
using System.Text;

public class BaseHeader
{
    /// <summary>
    /// http编码格式
    /// </summary>
    public Encoding Encoding { get; set; }=Encoding.UTF8;
    /// <summary>
    /// 报文类型
    /// </summary>
    public string Content_Type { get; set; } = "application/json;charset:utf-8;";
    /// <summary>
    /// 报文长度
    /// </summary>
    public int Content_Length { get; set; }
    /// <summary>
    /// 报头的编码格式字段
    /// </summary>
    public string Content_Encoding { get; set; }
    /// <summary>
    /// 报文Body
    /// </summary>
    public string Content { get; set; } = "OK";
    /// <summary>
    /// 用于存储报头的字典
    /// </summary>
    public Dictionary<string, string> Headers { get; set; }
    /// <summary>
    /// 日志单例对象  
    /// </summary>
   // public static ILogger _logger = Log.Logger;
}