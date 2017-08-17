using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShardingWeb.DB
{
    public class MySqlHelper
    {
        private static MySqlConnection _conn;

        protected static MySqlConnection GetConn()
        {
            //不要每次都是新建一个，这样连接过多就会崩溃
            if (_conn == null)
            {
                string connStr = "";
                connStr = "server=127.0.0.1;database=shardingdb;Uid=root;Pwd=123456;Allow User Variables=True";
                //connStr = ConfigurationManager.ConnectionStrings["connStr"].ToString();
                MySqlConnection conn = new MySqlConnection(connStr);
                _conn = conn;
            }

            if (_conn.State == System.Data.ConnectionState.Closed)
                _conn.Open();

            return _conn;
        }

        public static bool ExeTransaction(string sql)
        {
            var conn = GetConn();

            var tran = conn.BeginTransaction();
            try
            {
                conn.Execute(sql);
                tran.Commit();
                return true;
            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw ex;
            }
        }

        public static IEnumerable<T> Query<T>(string sql, object para)
        {
            return GetConn().Query<T>(sql, para);
        }

        public static int ExecuteScalar(string sql,object para)
        {
            return GetConn().ExecuteScalar<int>(sql, para);
        }

    }
}