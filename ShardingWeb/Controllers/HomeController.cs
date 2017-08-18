using Dapper;
using PagedList;
using ShardingWeb.DB;
using ShardingWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

                int num = 5 * 100 * 10000; //记录条数
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

        public JsonResult AsyncInitData()
        {
            try
            {
                Session["abc"] = "abc";
                TaskStatus.TaskList.Add(Session.SessionID, null);

                Thread thread = new Thread(new ParameterizedThreadStart(InitDataMethod));
                thread.IsBackground = true;
                thread.Start(Session.SessionID);
                return Json(new { status = "ok", msg = "开始执行" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = "fail", msg = ex.Message }, JsonRequestBehavior.AllowGet);
                throw;
            }

        }

        public JsonResult GetTaskStatus()
        {
            try
            {
                var status = TaskStatus.TaskList[Session.SessionID];
                if (status.StatusDesc == "执行结束")
                    TaskStatus.TaskList.Remove(Session.SessionID);

                return Json(new { status = "ok", taskStatus = status, taskCount = TaskStatus.TaskList.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = "fail", msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public void InitDataMethod(object sessionIdObj)
        {
            try
            {
                string sessionId = sessionIdObj.ToString();
                DateTime startTime = DateTime.Now;

                string format = @"INSERT INTO `log` (`date`, `thread`, `level`, `logger`, `message`, `userid`, `enable`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');";

                int num = 1 * 100 * 10000; //记录条数

                TaskStatus taskStatus = new TaskStatus() { HandledNum = 0, StatusDesc = "执行中...", TotalNum = num };
                TaskStatus.TaskList[sessionId] = taskStatus;

                StringBuilder buidler = new StringBuilder();
                int count = 1;
                var time = DateTime.Now.AddHours(1);
                bool flag = true;

                for (int i = 0; i < num; i++)
                {
                    if (i != 0 && i % 1000 == 0) // 每多少条提交一次
                    {
                        MySqlHelper.ExeTransaction(buidler.ToString());
                        taskStatus.HandledNum = count;
                        TaskStatus.TaskList[sessionId] = taskStatus;
                        buidler.Clear();
                    }
                    buidler.Append(string.Format(format, time = time.AddSeconds(1), count, count, "logger", "message" + count, "user", flag ? 1 : 0));
                    count++;
                    flag = !flag;
                }

                if (buidler.ToString() != "")
                {
                    MySqlHelper.ExeTransaction(buidler.ToString());
                    taskStatus.HandledNum = count;
                    TaskStatus.TaskList[sessionId] = taskStatus;
                }

                taskStatus.StatusDesc = "执行结束";
                taskStatus.Msg = $"数据录入完成。条数 {num} 用时： {(DateTime.Now - startTime).TotalSeconds} 秒";

            }
            catch (Exception ex)
            {
                //ViewBag.Msg = ex.Message;
            }
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

            string order = "date desc";

            int currentPage = page;
            int itemsPerPage = 10;

            string limitStr = $" limit {(currentPage - 1) * itemsPerPage},{itemsPerPage}";

            string selectSql = string.Format(sql, string.Join(" and ", whereList.ToArray()), order, limitStr);
            string countSql = string.Format(countSqlFormat, string.Join(" and ", whereList.ToArray()));

            DynamicParameters para = new DynamicParameters();
            //para.Add($"@userName", "%" + query.UserName + "%");

            var list = MySqlHelper.Query<Log>(selectSql, para);
            var totalCount = MySqlHelper.ExecuteScalar(countSql, para);

            var pageList = new StaticPagedList<Log>(list, page, itemsPerPage, totalCount);
            ViewBag.UserResult = pageList;
            ViewBag.TotalCount = totalCount;

            ViewBag.Msg = $"用时： {(DateTime.Now - startTime).TotalSeconds} 秒";

            return View();
        }
    }
}