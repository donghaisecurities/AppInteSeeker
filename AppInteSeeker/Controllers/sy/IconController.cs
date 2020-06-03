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
    public class IconController : ControllerBase
    {
        public SiteConfig Config;
        public IconController(IOptions<SiteConfig> option)
        {
            Config = option.Value;
        }

        public RedisHelper redis;
        public string ConnZT;
        public string mysqlConnCls;
        public string mysqlConnHej;
        public string ConnJy;

        [HttpGet("gnan")]
        public IEnumerable<StatusRespondBean> Get()
        {
            StatusRespondBean srb = new StatusRespondBean() { StatusCode = 0, StatusMessage = "gnan开始执行" };
            try
            {
                redis = new RedisHelper(0, Config.RedisConn);//包含DBNub,port           
                ConnZT = Config.OracleConnZT;
                mysqlConnCls = Config.MysqlConnCLS;
                mysqlConnHej = Config.MysqlConnHej;
                ConnJy = Config.JySqlConn;

                Csgg();

                Gnan();
                SYlbt();
                ClsTt();

                Hej724();
                ClsYw();

                Jxcp();

                srb.StatusCode = 1;
                srb.StatusMessage = "gnan完成";
            }
            catch (Exception ex)
            {
                srb.StatusCode = -1;
                srb.StatusMessage = ex.Message;
            }

            yield return srb;
        }

        /// <summary>
        /// 测试公告
        /// </summary>
        private void Csgg()
        {
            JArray jsonArray = new JArray();
            DateTime dtnow = System.DateTime.Now;
            string date = dtnow.ToString("yyyy-MM-dd HH:mm:ss");

            string sql = string.Format(@"select * from  intems.PM_Test_Notice_NEW where id=1");
            DataTable dt = OracleHelper.ExecuteDataTable(sql, ConnZT);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    int _sftz = 0;
                    //DateTime dtime = Convert.ToDateTime("20190326 12:23:23");
                    string _ksrq = dr["CSKSRQ"].ToString();
                    string _jsrq = dr["CSJSRQ"].ToString();


                    DateTime kssj = Convert.ToDateTime(_ksrq.Substring(0, 4) + "-" + _ksrq.Substring(4, 2) + "-" + _ksrq.Substring(6, 2) + " " + dr["CSKSSJ"].ToString());
                    DateTime jssj = Convert.ToDateTime(_jsrq.Substring(0, 4) + "-" + _jsrq.Substring(4, 2) + "-" + _jsrq.Substring(6, 2) + " " + dr["CSJSSJ"].ToString());

                    if (kssj <= dtnow && dtnow <= jssj)
                    {
                        _sftz = 1;
                    }

                    var gg = new
                    {
                        sftz = _sftz,
                        ggnr = dr["TZNR"].ToString(),
                        redistime = date
                    };
                    jsonArray.Add(JToken.FromObject(gg));
                }
            }

            redis.StringSet("SYCsgg", JsonConvert.SerializeObject(jsonArray));
        }


        /// <summary>
        /// 功能按钮
        /// </summary>
        private void Gnan()
        {
            JArray jsonArray = new JArray();
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string sql = string.Format(@"select * from PM_Func_Icon_NEW where syzt=1 order by px asc ");
            DataTable dt = OracleHelper.ExecuteDataTable(sql, ConnZT);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string sql_blob = "select tb from  PM_Func_Icon_NEW where bh=" + dr["bh"].ToString();

                    Byte[] blob = OracleHelper.ExecuteDataReader(sql_blob, ConnZT);

                    var gnan = new
                    {
                        bh = dr["px"].ToString(),
                        bq = dr["bq"].ToString(),
                        tbmc = dr["tbmc"].ToString(),
                        tb = Convert.ToBase64String(blob),
                        tbcode = dr["tbcode"].ToString(),
                        url = dr["dz"].ToString(),
                        sfqx = dr["sfqx"].ToString(),
                        mdmc = dr["mdmc"].ToString(),
                        redistime = date
                    };
                    jsonArray.Add(JToken.FromObject(gnan));
                }
            }

            redis.StringSet("SYGnan", JsonConvert.SerializeObject(jsonArray));
        }
        /// <summary>
        /// 轮播图
        /// </summary>
        private void SYlbt()
        {
            JArray jsonArray = new JArray();
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string sql = string.Format(@"select * from  intems.PM_Picture_Carousel x where x.lbsx>0  and yymk=2 order by x.lbsx");
            DataTable dt = OracleHelper.ExecuteDataTable(sql, ConnZT);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string _tpfj = dr["tpfj"] == null ? "" : dr["tpfj"].ToString();
                    if (!string.IsNullOrEmpty(_tpfj))
                    {
                        _tpfj = Config.Path + "/PMPictureCarousel/" + _tpfj;
                    }

                    var lbt = new
                    {
                        id = dr["id"].ToString(),
                        tpfj = _tpfj,
                        tplj = dr["tplj"].ToString(),
                        tpmc = dr["tpmc"].ToString(),
                        lbsx = dr["lbsx"].ToString(),
                        redistime = date
                    };
                    jsonArray.Add(JToken.FromObject(lbt));
                }
            }

            redis.StringSet("SYlbt", JsonConvert.SerializeObject(jsonArray));
        }



        /// <summary>
        /// 财联社-头条
        /// </summary>
        private void ClsTt()
        {
            List<string> article_id = new List<string>();
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            JArray jsonArray = new JArray();

            string sql = string.Format(@"
SELECT b.id,b.title,substr(c.name,1,2) as mark,from_unixtime(a.sort_score) as ctime 
FROM lian_depth_column_recommend a 
join lian_v1_article b on a.article_id=b.id
join lian_subject_category c on a.column_id=c.columnId
where a.pid=0 order by a.sort_score desc limit 15; ");
            DataTable dt = MySQLHelper.ExecuteDataTable(mysqlConnCls, sql, null);


            //只取前5个不重复数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                if (article_id.Count >= 5)
                {
                    break;
                }
                if (article_id.Contains(dt.Rows[i]["id"].ToString()))
                {
                    continue;
                }
                else
                {
                    article_id.Add(dt.Rows[i]["id"].ToString());
                }
                var oj = new
                {
                    id = dt.Rows[i]["id"].ToString(),
                    mark = dt.Rows[i]["mark"].ToString(),
                    title = dt.Rows[i]["title"].ToString(),
                    gxsj = ConvertHelper.DateToString(dt.Rows[i]["ctime"].ToString()),
                    redistime = date
                };
                jsonArray.Add(JToken.FromObject(oj));
            }

            redis.StringSet("SYTt", JsonConvert.SerializeObject(jsonArray));
        }


        /// <summary>
        /// 财联社-要闻
        /// </summary>
        private void ClsYw()
        {
            //SELECT id,title,from_unixtime(a.ctime) as ctime  FROM clszx.lian_v1_article where type=0 order by ctime desc;

            long zxId = 1;
            string redisKey = "SYYw";
            string sql = string.Empty;
            DateTime dtTime = System.DateTime.Now;
            DateTime dayNow = Convert.ToDateTime(dtTime.ToString("yyyy-MM-dd"));
            string date = dtTime.ToString("yyyy-MM-dd HH:mm:ss");
            //redis找list中最新的那条记录
            List<Zx> zxList = new List<Zx>();
            zxList = redis.LRange<Zx>(redisKey, 0, 0);
            if (zxList.Count > 0)
            {
                zxId = Convert.ToInt64(zxList[0].id);
                sql = string.Format(@"
select * from (
SELECT id,title,author,from_unixtime(ctime) as ctime  FROM lian_v1_article where id>{0} and  type=0  order by ctime desc) a
order by a.ctime asc ", zxId);

            }
            else
            {
                sql = string.Format(@"
select * from (
SELECT id,title,author,from_unixtime(ctime) as ctime  FROM lian_v1_article where type=0 order by ctime desc limit 30) a
order by a.ctime asc ");
            }


            DataTable dt = MySQLHelper.ExecuteDataTable(mysqlConnCls, sql, null);



            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string _gxsj = ConvertHelper.DateToString(dr["ctime"]);


                    //DateTime yyTime = Convert.ToDateTime(dr["ctime"]);
                    //if (yyTime > dayNow)
                    //{
                    //    //今天的消息用时间
                    //    _gxsj = yyTime.ToString("HH:mm");
                    //}
                    //else
                    //{
                    //    //昨天的消息用日期
                    //    _gxsj = yyTime.ToString("MM-dd");
                    //}

                    var oj = new
                    {
                        id = dr["id"].ToString(),
                        mark = "",
                        title = dr["title"].ToString(),
                        author = string.IsNullOrEmpty(dr["author"].ToString()) ? "财联社" : dr["author"].ToString(),
                        gxsj = _gxsj,
                        redistime = date
                    };
                    redis.ListLeftPush(redisKey, JToken.FromObject(oj));
                }

            }


        }
        /// <summary>
        /// 华尔街7*24
        /// </summary>
        private void Hej724()
        {
            long zxId = 1;
            string redisKey = "SY724";
            string sql = string.Empty;
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //redis找list中最新的那条记录
            List<Zx> zxList = new List<Zx>();
            zxList = redis.LRange<Zx>(redisKey, 0, 0);
            if (zxList.Count > 0)
            {
                zxId = Convert.ToInt64(zxList[0].id);
                sql = string.Format(@"
select * from (
SELECT msg_id,title,summary,msg_created_at,stocks,case FIND_IN_SET('469', subjects) when 1 then '利好' else '' end as mark 
FROM xgb_live_msgs where msg_id>{0} order by msg_created_at desc ) a
order by a.msg_created_at asc", zxId);

            }
            else
            {
                sql = string.Format(@"
select * from (
SELECT msg_id,title,summary,msg_created_at,stocks,case FIND_IN_SET('469', subjects) when 1 then '利好' else '' end as mark 
FROM xgb_live_msgs order by msg_created_at desc limit 30) a
order by a.msg_created_at asc ");
            }

            DataTable dt = MySQLHelper.ExecuteDataTable(mysqlConnHej, sql, null);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {

                    JArray jsonArrayList = new JArray();

                    if (!string.IsNullOrEmpty(dr["stocks"].ToString().Trim()))
                    {
                        string[] stocks = dr["stocks"].ToString().Split(";");
                        if (stocks.Length > 0)
                        {
                            foreach (var item in stocks)
                            {
                                string[] zq = item.Split(",");
                                var objDe = new
                                {
                                    zqdm = zq[0].Substring(0, 6),
                                    zqmc = zq[1],
                                };
                                jsonArrayList.Add(JToken.FromObject(objDe));
                            }
                        }
                    }
                    var oj = new
                    {
                        id = dr["msg_id"].ToString(),
                        mark = string.IsNullOrEmpty(dr["mark"].ToString()) ? "" : "利好",
                        title = dr["title"].ToString(),
                        content = string.IsNullOrEmpty(dr["summary"].ToString().Trim()) ? "" : dr["summary"].ToString(),
                        gxsj = Convert.ToDateTime(dr["msg_created_at"]).ToString("HH:mm"),
                        xggg = jsonArrayList,
                        redistime = date
                    };
                    redis.ListLeftPush(redisKey, JToken.FromObject(oj));
                }

            }

        }


        /// <summary>
        /// 精选产品
        /// </summary>
        private void Jxcp()
        {
            JArray jsonArray = new JArray();
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string sql = string.Format(@"select * from PM_TJCP");
            DataTable dt = OracleHelper.ExecuteDataTable(sql, ConnZT);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {

                    string sql_jy = "select * from SecuMain where SecuCategory=8 and SecuCode='" + dr["cpdm"].ToString() + "'";
                    DataTable dt_jy = SqlHelper.ExecuteQuery(sql_jy, ConnJy);

                    string rq = "--";
                    string jz = "--";
                    string _syqj = "--";
                    string _syl = "--";

                    if (dt_jy.Rows.Count > 0)
                    {

                        string sql_jj = string.Format(@"select UnitNV,TradingDay,tt.* FROM MF_NetValuePerformance aa
join  (
SELECT TOP 1 * FROM (
SELECT InnerCode,RRInThreeYear AS MaxNum,'近三年收益率' AS hbl FROM MF_NetValuePerformance  where InnerCode= {0}  
UNION 
SELECT InnerCode,RRSinceStart AS MaxNum,'成立以来收益率' AS hbl FROM MF_NetValuePerformance  where InnerCode= {0}  
UNION 
SELECT InnerCode,RRInSingleMonth AS MaxNum,'近一月收益率' AS hbl FROM MF_NetValuePerformance  where InnerCode= {0} 
UNION 
SELECT InnerCode,RRInThreeMonth AS MaxNum,'近三月收益率' AS hbl FROM MF_NetValuePerformance  where InnerCode= {0} 
UNION 
SELECT InnerCode,RRInSixMonth AS MaxNum,'近六月收益率' AS hbl FROM MF_NetValuePerformance  where InnerCode= {0} 
UNION 
SELECT InnerCode,RRInSingleYear AS MaxNum,'近一年收益率' AS hbl FROM MF_NetValuePerformance  where InnerCode= {0} ) AS T
ORDER BY T.MaxNum DESC ) as tt on aa.InnerCode=tt.InnerCode where aa.InnerCode={0}  ", dt_jy.Rows[0]["InnerCode"].ToString());
                        DataTable dt_jj = SqlHelper.ExecuteQuery(sql_jj, ConnJy);


                        if (dt_jj.Rows.Count > 0)
                        {
                            rq = Convert.ToDateTime(dt_jj.Rows[0]["TradingDay"]).ToString("MM-dd");
                            jz = ConvertHelper.ConvertTo4XS(dt_jj.Rows[0]["UnitNV"]);
                            _syqj = dt_jj.Rows[0]["hbl"].ToString();
                            _syl = ConvertHelper.ConvertTo2XS_(dt_jj.Rows[0]["MaxNum"]) + "%";
                        }
                    }
                    var obj = new
                    {
                        cpdm = string.IsNullOrEmpty(dr["cpdm"].ToString()) ? "" : dr["cpdm"].ToString(),
                        cpmc = string.IsNullOrEmpty(dr["cpmc"].ToString()) ? "" : dr["cpmc"].ToString(),
                        jjlx = string.IsNullOrEmpty(dr["jjlx"].ToString()) ? "" : dr["jjlx"].ToString(),
                        syqj = _syqj,
                        yzzq = string.IsNullOrEmpty(dr["yzzq"].ToString()) ? "" : dr["yzzq"].ToString(),
                        syl = _syl,
                        jzlx = string.IsNullOrEmpty(dr["jzlx"].ToString()) ? "" : dr["jzlx"].ToString(),
                        jzsz = jz,
                        qgje = string.IsNullOrEmpty(dr["qgje"].ToString()) ? "" : dr["qgje"].ToString(),
                        fxdj = string.IsNullOrEmpty(dr["fxdj"].ToString()) ? "" : dr["fxdj"].ToString(),
                        flgg = string.IsNullOrEmpty(dr["flgg"].ToString()) ? "" : dr["flgg"].ToString(),
                        cpggy = string.IsNullOrEmpty(dr["cpggy"].ToString()) ? "" : dr["cpggy"].ToString(),
                        zxjzrq = rq,
                        redistime = date
                    };
                    jsonArray.Add(JToken.FromObject(obj));
                }
            }

            redis.StringSet("SYJx", JsonConvert.SerializeObject(jsonArray));
        }

        public class Zx
        {
            public string id;
            public string mark;
            public string title;
            public string gxsj;
            public string redistime;
        }
    }
}
