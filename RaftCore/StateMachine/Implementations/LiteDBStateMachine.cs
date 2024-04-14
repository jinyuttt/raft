using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RaftCore.StateMachine.Implementations
{

    /// <summary>
    /// 测试LiteDB实现，具体需要根据业务开发
    /// </summary>
    public class LiteDBStateMachine : IRaftStateMachine
    {

        readonly ConcurrentDictionary<string, string> dic = new ConcurrentDictionary<string, string>();

        private void Add(string key, string value)
        {
            if (dic.TryGetValue(key, out var dicValue))
            {
                using (var db = new LiteDatabase(dicValue))
                {
                    var lst = db.GetCollection<string>(key);
                    lst.Insert(value);
                }
            }
        }

        private void Delete(string key, string value)
        {
            if (dic.TryGetValue(key, out var dicValue))
            {
                using (var db = new LiteDatabase(dicValue))
                {
                    var lst = db.GetCollection<string>();
                    lst.Delete(value);
                    
                }
            }
        }

        private void Clear(string key, string value)
        {
            if (dic.TryGetValue(key, out var dicValue))
            {
                using (var db = new LiteDatabase(dicValue))
                {
                    var lst = db.GetCollection<string>();
                    lst.DeleteAll();

                }
            }
        }

        private void DropDB(string key)
        {
            if (dic.TryGetValue(key, out var dicValue))
            {
                using (var db = new LiteDatabase(dicValue))
                {
                  db.DropCollection(key);

                }
            }
        }

        private void Create(string key)
        {
           int num=int.Parse(key.Trim());
            for (int i = 0; i < num; i++)
            {
                dic[i.ToString()] = i.ToString() + ".db";
            }
        }
        public void Apply(string command)
        {
            var commands = command.Split(" ",StringSplitOptions.RemoveEmptyEntries);
            switch(commands[0].ToUpper().Trim())
            {
                case "CREATE":
                    dic[commands[1]] = commands[2];
                    break;
                case "SET":
                    Add(commands[1], commands[2]);
                    break;
                case "DELETE":
                    Delete(commands[1], commands[2]);
                    break;
                case "CLEAR":
                    Clear(commands[1], commands[2]);
                    break;
                case "DROP":
                    DropDB(commands[1]);
                    break;
                case "TEST":
                    Create(commands[1]);
                    break;
                default:
                    break;


            }

        }

        public string RequestStatus(string param)
        {
            if(dic.ContainsKey(param)) return dic[param]; return null;
        }

        public void TestConnection()
        {
            var testState = new Dictionary<string, int>();
            testState["X"] = int.Parse("0");
            testState.Clear();
        }
    }
}
