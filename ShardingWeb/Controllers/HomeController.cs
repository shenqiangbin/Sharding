using ShardingWeb.DB;
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
                    return Content("初始化成功");
                else
                    return Content("初始化失败");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public ActionResult InitData()
        {
            try
            {
                string format = @"INSERT INTO `log` (`date`, `thread`, `level`, `logger`, `message`, `userid`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');";

                int num = 21; //记录条数
                StringBuilder buidler = new StringBuilder();
                int count = 1;

                for (int i = 0; i < num; i++)
                {
                    if (i != 0 && i % 10 == 0) // 每多少条提交一次
                    {
                        MySqlHelper.ExeTransaction(buidler.ToString());
                        buidler.Clear();
                    }
                    buidler.Append(string.Format(format, DateTime.Now, count, count, "logger", "message" + count, "user"));
                    count++;
                }

                if (buidler.ToString() != "")
                    MySqlHelper.ExeTransaction(buidler.ToString());

                return Content("初始化成功");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }


    }
}