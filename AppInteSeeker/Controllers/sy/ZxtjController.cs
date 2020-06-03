using AppInteSeeker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedisHelp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AppInteSeeker.Controllers.sy
{
    [ApiController]
    [Route("api/sy")]
    public class ZxtjController : ControllerBase
    {
        public SiteConfig Config;
        public ZxtjController(IOptions<SiteConfig> option)
        {
            Config = option.Value;
        }

        public RedisHelper redis;

        public string mysqlConnCls;


        /// <summary>
        /// 首页- 推荐
        /// </summary>
        /// <returns></returns>
        [HttpGet("zxtj")]
        public IEnumerable<StatusRespondBean> Get()
        {
            StatusRespondBean srb = new StatusRespondBean() { StatusCode = 0, StatusMessage = "Zxtj开始执行" };
            try
            {
                redis = new RedisHelper(0, Config.RedisConn);//包含DBNub,port           

                mysqlConnCls = Config.MysqlConnCLS;

                ClsPmzb();
                ClsJrht();
                ClsRmht();     


                srb.StatusCode = 1;
                srb.StatusMessage = "Zxtj已完成";
            }
            catch (Exception ex)
            {
                srb.StatusCode = -1;
                srb.StatusMessage = ex.Message;
            }

            yield return srb;
        }
        /// <summary>
        /// 今日话题
        /// </summary>
        private void ClsJrht()
        {
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            JArray jsonArray = new JArray();

            string sql = string.Format(@"
           select a.id,a.name,a.description,from_unixtime(a.create_time) ctime FROM lian_subject a 
left join lian_subject_article_assoc b on a.id=b.subject_id
left join lian_v1_article c on b.article_id=c.id
where a.is_del=0 and 
a.id in
   (select subject_id from lian_subject_category_assoc 
      where subject_category_id in 
      (select id from lian_subject_category where name in ('宏观','股市','公司','地产','金融','基金','环球','科创版'))
   ) 
group by a.id,a.name 
order by  a.create_time desc limit 3 ");
            DataTable dt = MySQLHelper.ExecuteDataTable(mysqlConnCls, sql, null);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    var oj = new
                    {
                        id = dr["id"].ToString(),
                        title = dr["name"].ToString(),
                        desc = dr["description"].ToString(),
                        ctime = dr["ctime"].ToString(),
                        redistime = date
                    };
                    jsonArray.Add(JToken.FromObject(oj));
                }
            }
            redis.StringSet("SYJrht", JsonConvert.SerializeObject(jsonArray));
        }

        /// <summary>
        /// 热门话题
        /// </summary>
        private void ClsRmht()
        {
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            JArray jsonArray = new JArray();

            string sql = string.Format(@"
            select  a.id,a.name,a.description,count(c.id) ccount ,from_unixtime(a.create_time) ctime FROM lian_subject a 
left join lian_subject_article_assoc b on a.id=b.subject_id
left join lian_v1_article c on b.article_id=c.id
where a.is_del=0 and 
a.id in
   (select subject_id from lian_subject_category_assoc 
      where subject_category_id in 
      (select id from lian_subject_category where name in ('宏观','股市','公司','地产','金融','基金','环球','科创版'))
   ) 
 -- and substr(from_unixtime(c.ctime),1,10)=substr(now(),1,10)
group by a.id,a.name order by ccount desc limit 12 ");
            DataTable dt = MySQLHelper.ExecuteDataTable(mysqlConnCls, sql, null);


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string _mark = "";
                if (i == 0)
                {
                    _mark = "hot";
                }
                var oj = new
                {
                    id = dt.Rows[i]["id"].ToString(),
                    title = dt.Rows[i]["name"].ToString(),
                    mark = _mark,
                    ctime = dt.Rows[i]["ctime"].ToString(),
                    redistime = date
                };
                jsonArray.Add(JToken.FromObject(oj));
            }

            redis.StringSet("SYRmht", JsonConvert.SerializeObject(jsonArray));
        }

        /// <summary>
        /// 财联社-盘面直播
        /// </summary>
        private void ClsPmzb()
        {
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            JArray jsonArray = new JArray();

            string sql = string.Format(@"
            SELECT b.id,b.title,b.brief,b.type ,from_unixtime(b.ctime) ctime FROM lian_subject_article_assoc a
            join lian_v1_article b on a.article_id = b.id
            where a.subject_id = 1103 order by b.ctime desc limit 1  ");
            DataTable dt = MySQLHelper.ExecuteDataTable(mysqlConnCls, sql, null);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string _title = string.Empty;
                    if (dr["type"].ToString().Equals("-1"))//快讯,
                    {
                        _title = dr["brief"].ToString();
                    }
                    else
                    {
                        _title = dr["title"].ToString();

                    }
                    var oj = new
                    {
                        id = "1103",
                        title = _title,
                        gxsj = Convert.ToDateTime(dr["ctime"]).ToString("HH:mm"),
                        redistime = date
                    };
                    jsonArray.Add(JToken.FromObject(oj));
                }
            }
            redis.StringSet("SYPmzb", JsonConvert.SerializeObject(jsonArray));

        }

    }
}
