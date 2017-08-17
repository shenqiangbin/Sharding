using Dapper;
using PagedList;
using ShardingWeb.DB;
using ShardingWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ShardingWeb.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DB()
        {
            return View();
        }

        public ActionResult InitDB()
        {
            try
            {
                var filePath = Server.MapPath("~/sql/InitDB.sql");
                string sql = System.IO.File.ReadAllText(filePath);
                if (MySqlHelper.ExeTransaction(sql))
                    ViewBag.Msg = "数据库初始化成功";
                else
                    ViewBag.Msg = "数据库初始化失败";
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }

            return View("DB");
        }

        public ActionResult InitData()
        {
            try
            {
                DateTime startTime = DateTime.Now;

                string format = @"INSERT INTO `log` (`date`, `thread`, `level`, `logger`, `message`, `userid`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');";

                int num = 100 * 10000; //记录条数
                StringBuilder buidler = new StringBuilder();
                int count = 1;

                for (int i = 0; i < num; i++)
                {
                    if (i != 0 && i % 10000 == 0) // 每多少条提交一次
                    {
                        MySqlHelper.ExeTransaction(buidler.ToString());
                        buidler.Clear();
                    }
                    buidler.Append(string.Format(format, DateTime.Now, count, count, "logger", "message" + count, "user"));
                    count++;
                }

                if (buidler.ToString() != "")
                    MySqlHelper.ExeTransaction(buidler.ToString());

                ViewBag.Msg = $"数据录入完成。条数 {num} 用时： {(DateTime.Now - startTime).TotalSeconds} 秒";
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }

            return View("DB");
        }

        public ActionResult Log(int page = 1)
        {
            DateTime startTime = DateTime.Now;

            var whereList = new List<string>();
            whereList.Add("1=1");

            string sql = @"
select * from (select *,(@rowNum:=@rowNum+1) as Number from (
SELECT 
	*
 FROM log
where {0} order by {1} {2}
)t,(Select (@rowNum :=0) ) b) tt ";

            string countSqlFormat = @"
SELECT 
	count(id)
 FROM log
where {0} ";

            string order = "id asc";

            int currentPage = page;
            int itemsPerPage = 10;

            string limitStr = $" limit {(currentPage - 1) * itemsPerPage},{itemsPerPage}";

            string selectSql = string.Format(sql, string.Join(" and ", whereList.ToArray()), order, limitStr);
            string countSql = string.Format(countSqlFormat, string.Join(" and ", whereList.ToArray()));

            DynamicParameters para = new DynamicParameters();
            //para.Add($"@userName", "%" + query.UserName + "%");

            var list = MySqlHelper.Query<Log>(selectSql, para);
            var totalCount= MySqlHelper.ExecuteScalar(countSql, para);

            var pageList = new StaticPagedList<Log>(list, page, itemsPerPage, totalCount);
            ViewBag.UserResult = pageList;

            ViewBag.Msg = $"用时： {(DateTime.Now - startTime).TotalSeconds} 秒";

            return View();
        }
    }
}