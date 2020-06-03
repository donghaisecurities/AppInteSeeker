using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace AppInteSeeker.Models
{
    public class OracleHelper
    {
        //private static string connStr = "User Id=dhzxdc;Password=tclnftx;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.0.1)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=test)))";
        //private static string connStr = "User Id=dhzxdc;Password=tclnftx;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=128.6.5.19)(PORT=1521)))(CONNECT_DATA =(SERVER = DEDICATED)(SID = database19)))";



        /// <summary>
        /// 执行语句返回的是单行单列的结果 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="connStr"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql, string connStr, params OracleParameter[] parameters)
        {
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }

        }


        #region 执行SQL语句,返回受影响行数
        public static int ExecuteNonQuery(string sql, string connStr, params OracleParameter[] parameters)
        {
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion
        #region 执行SQL语句,返回DataTable;只用来执行查询结果比较少的情况
        public static DataTable ExecuteDataTable(string sql, string connStr, params OracleParameter[] parameters)
        {
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddRange(parameters);
                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable datatable = new DataTable();
                    adapter.Fill(datatable);
                    return datatable;
                }
            }
        }
        #endregion



        public static byte[] ExecuteDataReader(string sql, string connStr, params OracleParameter[] parameters)
        {
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddRange(parameters);
                    OracleDataReader reader = cmd.ExecuteReader();

                    MemoryStream ms = new MemoryStream();

                    if (reader.Read())
                    {
                        OracleBlob blob = (OracleBlob)reader.GetOracleBlob(0);
                        Byte[] buffer = new Byte[blob.Length];
                        blob.Read(buffer, 0, Convert.ToInt32(blob.Length));
                        ms.Write(buffer, 0, Convert.ToInt32(blob.Length));
                        blob.Close();
                    }
                    reader.Close();
                    ms.Position = 0;
                    byte[] result = new byte[ms.Length];
                    ms.Read(result, 0, result.Length);
                    return result;
                }
            }
        }
    }
}
