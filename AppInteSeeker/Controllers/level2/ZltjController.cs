using AppInteSeeker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedisHelp;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AppInteSeeker.Controllers.level2
{
    /// <summary>
    /// job 跑第二，依托ggxq 的次数
    /// </summary>
    [ApiController]
    [Route("api/level2")]
    public class ZltjController : ControllerBase
    {
        public SiteConfig Config;
        public ZltjController(IOptions<SiteConfig> option)
        {
            Config = option.Value;
        }

        public RedisHelper redis;

        public string ConnJy;

        [HttpGet("zltj")]
        public IEnumerable<StatusRespondBean> GetAll()
        {
            StatusRespondBean srb = new StatusRespondBean() { StatusCode = 1, StatusMessage = "zltjController开始执行" };

            try
            {
                redis = new RedisHelper(0, Config.RedisConn);//包含DBNub,port
                ConnJy = Config.JySqlConn;


                string[] Day = new string[] { "周日", "周一", "周二", "周三", "周四", "周五", "周六" };



                DateTime timeNow = System.DateTime.Now;
                string redisTime = timeNow.ToString("yyyy-MM-dd HH:mm");
                string TradeDayNum = timeNow.ToString("yyyyMMdd");
                string TradeDayStr = timeNow.ToString("yyyy-MM-dd");//当前日期
                string TradeDayStrNow = TradeDayStr;//最近一个交易日
                string sql_day = string.Format(@"
select top 5 * from QT_TradingDayNew  where SecuMarket=83  
and TradingDate <= '{0}' and IfTradingDay=1 order by TradingDate desc", TradeDayStr);

                DataTable dt_day = SqlHelper.ExecuteQuery(sql_day, ConnJy);
                if (dt_day.Rows.Count > 0)
                {
                    JArray jsonArray = new JArray();
                    foreach (DataRow dr in dt_day.Rows)
                    {
                        DateTime dttime = Convert.ToDateTime(dr["TradingDate"]);
                        string _week = Day[Convert.ToInt32(dttime.DayOfWeek.ToString("d"))].ToString();
                        var time = new
                        {
                            //tradedaystr = dttime.ToString("yyyy年MM月dd日"),
                            tradedaynum = dttime.ToString("yyyyMMdd"),
                            day = dttime.ToString("MM-dd"),
                            week = timeNow.Day == dttime.Day ? "今天" : _week,
                            redistime = redisTime
                        };
                        jsonArray.Add(JToken.FromObject(time));
                    }
                    //设置最近5个交易日日历
                    redis.StringSet("level2_tradeday", JsonConvert.SerializeObject(jsonArray));

                    DateTime dtime = Convert.ToDateTime(dt_day.Rows[0]["TradingDate"]);
                    TradeDayStrNow = dtime.ToString("yyyy-MM-dd");
                    TradeDayNum = dtime.ToString("yyyyMMdd");
                }

                //设置redis更新时间
                redis.HashSet("level2_buy", "redistime", redisTime);
                redis.HashSet("level2_buy", "tradeday", TradeDayNum);

                Zxglb(TradeDayNum, "cjd", "buy", redisTime, timeNow);
                Zxglb(TradeDayNum, "sdd", "buy", redisTime, timeNow);
                Zxglb(TradeDayNum, "tljd", "buy", redisTime, timeNow);

                srb.StatusCode = 1;
                srb.StatusMessage = "ZltjController执行完成";
            }
            catch (Exception ex)
            {
                srb.StatusCode = -1;
                srb.StatusMessage = ex.Message;
            }
            yield return srb;
        }

        /// <summary>
        /// level列表页面
        /// </summary>
        /// <param name="tradeDay">交易日时间</param>
        /// <param name="level2Type">策略类型</param>
        /// <param name="side">买卖方向</param>
        private void Zxglb(string tradeDay, string level2Type, string side, string uptime, DateTime dtnow)
        {
            string sideNum = "2";
            if (side == "buy")
            {
                sideNum = "1";
            }

            // JArray jsonArray = new JArray();
            List<Zqxx> list = new List<Zqxx>();
            //Dictionary<RedisValue, RedisValue> dic_buy = redis.HashKey(level2Type + "_" + tradeDay + "_buy").ToDictionary(o => o.Name, p => p.Value);
            //Dictionary<RedisValue, RedisValue> dic_buy_pri = redis.HashKey(level2Type + "_" + tradeDay + "_buy_pri").ToDictionary(o => o.Name, p => p.Value);

            //从个股详情统计有交易的次数
            Dictionary<RedisValue, RedisValue> dic_buy = redis.HashKey(level2Type + "_" + tradeDay + "_buys").ToDictionary(o => o.Name, p => p.Value);

            Dictionary<RedisValue, RedisValue> dic_buy_pri = redis.HashKey(level2Type + "_" + tradeDay + "_buy_pri").ToDictionary(o => o.Name, p => p.Value);
            Dictionary<RedisValue, RedisValue> dic_sell_pri = redis.HashKey(level2Type + "_" + tradeDay + "_sell_pri").ToDictionary(o => o.Name, p => p.Value);

            //求并集
            //  var list_hb = dic_buy.Keys.Union(dic_sell.Keys).ToList();
            var list_hb = dic_buy.Keys.ToList();

            if (list_hb.Count > 0)
            {
                foreach (var item in list_hb)
                {
                    // string secid = item.Key;
                    // int buy_ct = Convert.ToInt32(item.Value);

                    string secid = item;
                    int buy_ct = 0;
                    int sell_ct = 0;
                    double buy_pri = 0.0;
                    double sell_pri = 0.0;

                    string _zqjc = "";


                    buy_ct = dic_buy.ContainsKey(secid) == false ? 0 : Convert.ToInt32(dic_buy[secid]);
                    // sell_ct = dic_sell.ContainsKey(secid) == false ? 0 : Convert.ToInt32(dic_sell[secid]);
                    buy_pri = dic_buy_pri.ContainsKey(secid) == false ? 0 : Convert.ToDouble(dic_buy_pri[secid]);
                    sell_pri = dic_sell_pri.ContainsKey(secid) == false ? 0 : Convert.ToDouble(dic_sell_pri[secid]);


                    ///取委托记录wt_20200511_cjd_002456
                   // Dictionary<RedisValue, RedisValue> dic_wt = redis.HashKey("wt_" + tradeDay + "_" + level2Type + "_" + secid).ToDictionary(o => o.Name, p => p.Value);
                    ///取成交记录cj_20200511_cjd_002456
                    Dictionary<RedisValue, RedisValue> dic_cj = redis.HashKey("cj_" + tradeDay + "_" + level2Type + "_" + secid).ToDictionary(o => o.Name, p => p.Value);
                    string cjPri = "0";
                    foreach (var itm in dic_cj)
                    {
                        string md = itm.Key;
                        string _val = itm.Value;
                        JObject oo = JObject.Parse(_val);
                        cjPri = (Convert.ToDouble(oo["CjPx"].ToString().Trim()) / 10000.0).ToString("0.00");//第一笔成交价
                        break;

                        //JObject oo = JObject.Parse(_val);
                        //string Side = oo["WtSide"].ToString().Trim();

                        //if (Side == sideNum)
                        //{
                        //    cjPri = (Convert.ToDouble(oo["CjPx"].ToString().Trim()) / 10000.0).ToString("0.00");//第一笔成交价
                        //    break;
                        //}

                    }


                    string sql = string.Format(@"select SecuAbbr  from SecuMain a 
                    where a.SecuCode='{0}'  and a.SecuMarket=90 and a.SecuCategory=1 ", secid);
                    DataTable dt_name = SqlHelper.ExecuteQuery(sql, ConnJy);
                    if (dt_name.Rows.Count > 0)
                    {
                        _zqjc = dt_name.Rows[0]["SecuAbbr"].ToString();
                    }


                    Zqxx zqxx = new Zqxx
                    {
                        zqdm = secid,
                        zqjc = _zqjc,
                        cs = buy_ct + sell_ct,
                        je = buy_pri - sell_pri,
                        cjj = cjPri,
                        cjje = buy_pri + sell_pri,
                    };
                    list.Add(zqxx);
                }
                var list_cs_des = list.Where(o => o.cjje != 0.0).OrderByDescending(a => a.cs);
                var cs_des = from g in list_cs_des.ToList()
                             select new
                             {
                                 g.zqdm,
                                 g.zqjc,
                                 jrcs = g.cs.ToString(),
                                 cjje = ConvertHelper.ConvertToString((Convert.ToDouble(g.je) / 1000000.0)),
                                 g.cjj
                             };



                var list_pri_des = list.Where(o => o.cjje != 0.0).OrderByDescending(a => a.je);
                var pri_des = from g in list_pri_des.ToList()
                              select new
                              {
                                  g.zqdm,
                                  g.zqjc,
                                  jrcs = g.cs.ToString(),
                                  cjje = ConvertHelper.ConvertToString((Convert.ToDouble(g.je) / 1000000.0)),
                                  g.cjj
                              };

                var list_sy = cs_des.ToList();


                redis.HashSet("level2_" + side, level2Type, list_sy);
                //if (list_sy.Count > 5)//取前5
                //{
                //    redis.HashSet("level2_" + side, level2Type, JsonConvert.SerializeObject(list_sy.GetRange(0, 5)));
                //    //redis.StringSet("level2_" + level2Type + "_" + side, JsonConvert.SerializeObject(list_sy.GetRange(0, 5)));
                //}
                //else
                //{
                //    redis.HashSet("level2_" + side, level2Type, JsonConvert.SerializeObject(list_sy.GetRange(0, list_sy.Count)));
                //    //redis.StringSet("level2_" + level2Type + "_" + side, JsonConvert.SerializeObject(list_sy.GetRange(0, list_sy.Count)));
                //}
                //用于个股详情近5日动态查询
                redis.HashSet("level2_" + tradeDay + "_" + side, level2Type, list_sy);

                SetRedis(cs_des.ToList(), "level2_" + tradeDay + "_" + level2Type + "_cs_desc_" + side, 10);
                SetRedis(cs_des.Reverse().ToList(), "level2_" + tradeDay + "_" + level2Type + "_cs_asc_" + side, 10);
                SetRedis(pri_des.ToList(), "level2_" + tradeDay + "_" + level2Type + "_pri_desc_" + side, 10);
                SetRedis(pri_des.Reverse().ToList(), "level2_" + tradeDay + "_" + level2Type + "_pri_asc_" + side, 10);

                //个股详情页 的今日次数与金额
                foreach (var item in cs_des.ToList())
                {
                    redis.HashSet("ggxq_" + tradeDay + "_" + item.zqdm, level2Type + "_ct", JsonConvert.SerializeObject(item));
                }
            }
            else
            {
                if (dtnow.ToString("yyyyMMdd").Equals(tradeDay) && dtnow.Hour >= 9)//如果当天是交易日且当前时间大于等于9点，则覆盖昨天的数据
                {
                    redis.HashSet("level2_" + side, level2Type, "[]");
                }
            }

        }



        public class Zqxx
        {
            /// <summary>
            /// 证券代码
            /// </summary>
            public string zqdm;
            /// <summary>
            /// 证券简称
            /// </summary>
            public string zqjc;
            /// <summary>
            /// 次数
            /// </summary>
            public int cs;

            /// <summary>
            /// 净买入
            /// </summary>
            public double je;

            /// <summary>
            /// 成交价
            /// </summary>
            public string cjj;
            /// <summary>
            /// 成交金额
            /// </summary>
            public double cjje;

        }



        public void SetRedis<T>(List<T> list, string key, int pagesize)
        {
            int rCount = list.Count;
            if (rCount <= pagesize)
            {
                string dataKey = "1";
                redis.HashSet(key, dataKey, list);//set集合 追加 dataKey

            }
            else
            {
                //需要分多少页n+1页
                int page = rCount / pagesize + 1;
                //最后一页多少个数据
                int yushu = rCount % pagesize;
                for (int i = 1; i <= page; i++)
                {
                    //当前页起点index
                    int index = (i - 1) * pagesize;

                    string dataKey = i.ToString();
                    if (i == page)//最后一页
                    {
                        redis.HashSet(key, dataKey, list.GetRange(index, yushu));
                    }
                    else
                    {
                        redis.HashSet(key, dataKey, list.GetRange(index, pagesize));//set集合 追加 dataKey
                    }

                }
            }
        }
    }
}
