using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
                模拟通过一致性哈希算法来均匀的发布数据。也就是Sharding，分片，分区。
                关于概念参照：http://www.zsythink.net/archives/1182
                存储和数据均使用内存来展现，代替了数据库或者服务器或者表    
            */

            //Test();
            TestHashKeyMethod();

            //var data = GetUsers();

            //Console.WriteLine($"数据量：{data.Count}");
            //Console.WriteLine("展示前100条数据：");
            //for (int i = 0; i < 100; i++)
            //{
            //    Console.WriteLine($"userId:{data[i].UserId}   userName:{data[i].UserName} hash(userid): ");
            //}


            Console.ReadKey();
        }


        static IList<User> GetUsers()
        {
            int num = 3; //代表多少万条数据
            List<User> users = new List<User>();
            for (int i = 0; i < 10000 * num; i++)
            {
                users.Add(new User { UserId = i + 1, UserName = "用户" + (i + 1) });
            }
            return users;
        }

        static void Test()
        {
            List<Server> servers = new List<Server>();
            for (int i = 0; i < 1000; i++)
            {
                servers.Add(new Server("192.168.0." + i));
            }

            ConsistentHash<Server> ch = new ConsistentHash<Server>();
            ch.Init(servers);

            int search = 100000;

            DateTime start = DateTime.Now;
            SortedList<int, string> ay1 = new SortedList<int, string>();
            for (int i = 0; i < search; i++)
            {
                string temp = ch.GetNode(i.ToString()).IP;

                ay1[i] = temp;
            }
            TimeSpan ts = DateTime.Now - start;
            Console.WriteLine(search + " each use seconds: " + (double)(ts.TotalSeconds / search));

            //ch.Add(new Server(1000));
            ch.Remove(servers[1]);
            SortedList<int, string> ay2 = new SortedList<int, string>();
            for (int i = 0; i < search; i++)
            {
                string temp = ch.GetNode(i.ToString()).IP;

                ay2[i] = temp;
            }

            int diff = 0;
            for (int i = 0; i < search; i++)
            {
                if (ay1[i] != ay2[i])
                {
                    diff++;
                }
            }

            Console.WriteLine("diff: " + diff);
        }

        static void TestHashKeyMethod()
        {
            // 假设我们有三个组，可以来存放人。也可以理解成，我们要把我们的学生分成3个组，均匀分布。
            List<StoreList> storeLists = new List<StoreList>();
            storeLists.Add(new StoreList(1)); //第一组
            storeLists.Add(new StoreList(2)); //第二组
            storeLists.Add(new StoreList(3)); //第三组

            //假设我们有10万学生
            int num = 10;
            List<User> users = new List<User>();
            for (int i = 0; i < 10000 * num; i++)
            {
                users.Add(new User { UserId = i + 1, UserName = "学生" + (i + 1) });
            }

            //将组按hash计算的形式分布到环上
            ConsistentHash<StoreList> ch = new ConsistentHash<StoreList>();
            ch.Init(storeLists);

            //每个学生找到自己应该放的组
            foreach (var user in users)
            {
                //学生id是唯一的，我们那就将学生id作为key来用
                StoreList storeList = ch.GetNode(user.UserId.ToString());
                storeList.Welcome(user);
                Console.WriteLine($"{user.UserName} 在 {storeList.Id} 组");
            }

            //看下每组人数(这是分布是否均匀的检测指标)
            foreach (var item in storeLists)
            {
                Console.WriteLine($"{item.Id} 组 {item.list.Count} 人");
            }

            //查找学生id是6000的学生
            var startTime = DateTime.Now;
            var userid = 6000;
            var node = ch.GetNode(userid.ToString());
            var student = node.list.FirstOrDefault(m => m.UserId == userid);
            var elpased = (DateTime.Now - startTime).TotalMilliseconds;
            Console.WriteLine("用时：" + elpased + " " + student.UserName);
        }
    }

    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    class Server
    {
        public string IP { get; set; }

        public Server(string ip)
        {
            IP = ip;
        }

        public override int GetHashCode()
        {
            return IP.GetHashCode();
        }
    }


    /// <summary>
    /// 表示一个组，每个组都有一个编号，可以存放用户数据
    /// </summary>
    public class StoreList
    {
        private int id;
        public List<User> list = new List<User>();

        public StoreList(int id)
        {
            this.id = id;
        }

        public int Id { get { return id; } }

        //作为存储功能的组，必须重写GetHashCode，GetHashCode的值将作为唯一性hash的key来使用。
        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }

        //欢迎新同学加入
        public void Welcome(User user)
        {
            list.Add(user);
        }
    }
}
