using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppInteSeeker.Models
{

    public class SiteConfig
    {
        /// <summary>
        /// 域名
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// redis key 失效时间
        /// </summary>
        public string RedisExpiry { get; set; }
        /// <summary>
        /// 中台数据库链接
        /// </summary>
        public string OracleConnZT { get; set; }

        /// <summary>
        /// redis链接
        /// </summary>
        public string RedisConn { get; set; }

        /// <summary>
        /// 聚源-新股相关
        /// </summary>
        public string JySqlConn { get; set; }


        /// <summary>
        /// 财联社
        /// </summary>
        public string MysqlConnCLS { get; set; }

        /// <summary>
        /// 华尔街
        /// </summary>
        public string MysqlConnHej { get; set; }


        /// <summary>
        /// 东海资讯-新股相关
        /// </summary>
        public string OracleConnDhzb { get; set; }

        ///<summary>
        /// 拖拉机 黑名单
        /// </summary>
        public string TljdList { get; set; }

        /// <summary>
        /// 超级单 黑名单
        /// </summary>
        public string CjdList { get; set; }


        /// <summary>
        /// 闪电单 黑名单
        /// </summary>
        public string SddList { get; set; }
    }
}
