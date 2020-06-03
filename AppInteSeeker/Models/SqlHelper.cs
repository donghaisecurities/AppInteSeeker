using System.Data;
using System.Data.SqlClient;

namespace AppInteSeeker.Models
{
    /// <summary>
    /// 最底层的操作数据库代码
    /// </summary>
    public class SqlHelper
    {

        //SqlCommand.CommandTimeOut：获取或设置在终止执行命令的尝试并生成错误之前的等待时间。
        //SqlConnection.ConnectionTimeout：获取在尝试建立连接时终止尝试并生成错误之前所等待的时间。


        /// <summary>
        /// 将数据加载到本地，在本地对数据进行操作
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数化查询</param>
        /// <returns>返回从数据库中读取到的DataTable表</returns>
        public static DataTable ExecuteQuery(string sql, string connstr, params SqlParameter[] parameter)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameter);
                cmd.CommandTimeout = 180;
                DataTable tab = new DataTable();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tab.Load(reader);
                    return tab;

                }
            }
        }
        /// <summary>
        /// 用于执行增加和删除语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数化查询</param>
        /// <returns>有多少语句执行成功</returns>
        public static int ExecuteNonQuery(string sql, string connstr, params SqlParameter[] parameter)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameter);
                return cmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 执行语句后，返回第一行第一列的数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameter"></param>
        /// <returns>object类型的值</returns>
        public static object ExecuteScalar(string sql, string connstr, params SqlParameter[] parameter)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameter);
                return cmd.ExecuteScalar();
            }
        }
        /// <summary>
        /// 在数据库中，进行数据库的查询操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(string sql, string connstr, params SqlParameter[] parameter)
        {
            SqlConnection conn = new SqlConnection(connstr);
            using (SqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameter);
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }
    }
}
