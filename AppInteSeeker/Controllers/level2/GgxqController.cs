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
    /// 该 job 跑第一，为了取到 成交的次数
    /// </summary>
    [ApiController]
    [Route("api/level2")]
    public class GgxqController : ControllerBase
    {
        public SiteConfig Config;
        public GgxqController(IOptions<SiteConfig> option)
        {
            Config = option.Value;
        }

        public RedisHelper redis;

        public string ConnJy;

        [HttpGet("ggxq")]
        public IEnumerable<StatusRespondBean> GetAll()
        {
            StatusRespondBean srb = new StatusRespondBean() { StatusCode = 1, StatusMessage = "GgxqController开始执行" };

            try
            {
                redis = new RedisHelper(0, Config.RedisConn);//包含DBNub,port
                ConnJy = Config.JySqlConn;


                DateTime timeNow = System.DateTime.Now;
                string redisTime = timeNow.ToString("yyyy-MM-dd HH:mm");
                string TradeDayNum = timeNow.ToString("yyyyMMdd");
                string TradeDayStr = timeNow.ToString("yyyy-MM-dd");//当前日期
                string TradeDayStrNow = TradeDayStr;//最近一个交易日
                string sql_day = string.Format(@"
select top 1 * from QT_TradingDayNew  where SecuMarket=83  
and TradingDate <= '{0}' and IfTradingDay=1 order by TradingDate desc", TradeDayStr);

                DataTable dt_day = SqlHelper.ExecuteQuery(sql_day, ConnJy);
                if (dt_day.Rows.Count > 0)
                {
                    DateTime dtime = Convert.ToDateTime(dt_day.Rows[0]["TradingDate"]);
                    TradeDayStrNow = dtime.ToString("yyyy-MM-dd");
                    TradeDayNum = dtime.ToString("yyyyMMdd");
                }
                Ggxq_sdd(TradeDayNum, "sdd", "buy");
                Ggxq_cjd(TradeDayNum, "cjd", "buy");
                Ggxq_tljd(TradeDayNum, "tljd", "buy");


                srb.StatusCode = 1;
                srb.StatusMessage = "GgxqController执行完成";
            }
            catch (Exception ex)
            {
                srb.StatusCode = -1;
                srb.StatusMessage = ex.Message;
            }
            yield return srb;
        }
        /// <summary>
        /// 超级单
        /// </summary>
        /// <param name="tradeDay"></param>
        /// <param name="level2Type"></param>
        /// <param name="side"></param>
        private void Ggxq_cjd(string tradeDay, string level2Type, string side)
        {
            //取今日所有level2股票
            Dictionary<RedisValue, RedisValue> dic_buy = redis.HashKey(level2Type + "_" + tradeDay + "_buy").ToDictionary(o => o.Name, p => p.Value);
            Dictionary<RedisValue, RedisValue> dic_sell = redis.HashKey(level2Type + "_" + tradeDay + "_sell").ToDictionary(o => o.Name, p => p.Value);
            //求并集
            var list_hb = dic_buy.Keys.Union(dic_sell.Keys).ToList();
            //黑名单
            List<string> hmd = Config.CjdList.Split(",").ToList();

            foreach (var item in list_hb)
            {
                string secid = item;
                if (!hmd.Contains(secid))
                {

                    JArray jsonArray = new JArray();
                    JArray jsonArrayList = new JArray();

                    //string secid = "002975";
                    List<WtBean> listwt = new List<WtBean>();
                    List<CjBean> listcj = new List<CjBean>();
                    //取委托记录
                    Dictionary<RedisValue, RedisValue> dic_wt = redis.HashKey("wt_" + tradeDay + "_" + level2Type + "_" + secid).ToDictionary(o => o.Name, p => p.Value);
                    //取成交记录
                    Dictionary<RedisValue, RedisValue> dic_cj = redis.HashKey("cj_" + tradeDay + "_" + level2Type + "_" + secid).ToDictionary(o => o.Name, p => p.Value);
                    foreach (var item_wt in dic_wt)
                    {
                        WtBean wt = JsonConvert.DeserializeObject<WtBean>(item_wt.Value);
                        //if (wt.Side.Trim() == "1")
                        listwt.Add(wt);
                    }

                    foreach (var item_cj in dic_cj)
                    {
                        CjBean cj = JsonConvert.DeserializeObject<CjBean>(item_cj.Value);
                        listcj.Add(cj);
                    }

                    var wtlist = listwt.OrderBy(o => o.TransactTime).ToList();

                    foreach (var wt in wtlist)
                    {
                        //找到该委托单的成交单
                        var cjlist = listcj.Where(o => o.WtSeqNum == wt.ApplSeqNum).OrderBy(p => p.CjTime).ToList();

                        double cjs = 0.0;
                        if (cjlist.Count > 0)
                        {
                            foreach (var cj in cjlist)
                            {
                                cjs += cj.CjQty;
                            }
                            string wttime = cjlist[0].WtTime.ToString().Trim().Substring(8, 6);
                            var obj = new
                            {
                                wtsj = wttime.Substring(0, 2) + ":" + wttime.Substring(2, 2) + ":" + wttime.Substring(4, 2),
                                side = cjlist[0].WtSide.Trim() == "1" ? "买" : "卖",
                                wtjg = (Convert.ToDouble(cjlist[0].WtPx) / 10000.0).ToString(),
                                wtss = (Convert.ToDouble(cjlist[0].WtQty) / 10000.0).ToString(),
                                cjss = (Convert.ToDouble(cjs) / 10000.0).ToString(),

                            };
                            jsonArray.Add(JToken.FromObject(obj));

                            //画图用
                            var objlist = new
                            {
                                wtsj = wttime.Substring(0, 2) + ":" + wttime.Substring(2, 2),
                                side = cjlist[0].WtSide.Trim() == "1" ? "买" : "卖",
                                wtjg = (Convert.ToDouble(cjlist[0].WtPx) / 10000.0).ToString(),
                            };
                            jsonArrayList.Add(JToken.FromObject(objlist));
                        }

                    }

                    if (jsonArray.Count > 0)
                    {
                        redis.HashSet("ggxq_" + tradeDay + "_" + secid, level2Type, JsonConvert.SerializeObject(jsonArray));
                        //统计有交易的次数
                        redis.HashSet(level2Type + "_" + tradeDay + "_buys", secid, jsonArray.Count);
                    }
                    if (jsonArrayList.Count > 0)
                    {
                        redis.HashSet("ggxq_" + tradeDay + "_" + secid, level2Type + "_pic", JsonConvert.SerializeObject(jsonArrayList));
                    }
                }
                else
                {
                    if (redis.HashExists(level2Type + "_" + tradeDay + "_buys", secid))
                    {
                        redis.HashDelete(level2Type + "_" + tradeDay + "_buys", secid);
                    }
                }
            }
        }


        /// <summary>
        /// 闪电 单
        /// </summary>
        /// <param name="tradeDay"></param>
        /// <param name="level2Type"></param>
        /// <param name="side"></param>
        private void Ggxq_sdd(string tradeDay, string level2Type, string side)
        {
            //取今日所有level2股票
            Dictionary<RedisValue, RedisValue> dic_buy = redis.HashKey(level2Type + "_" + tradeDay + "_buy").ToDictionary(o => o.Name, p => p.Value);
            Dictionary<RedisValue, RedisValue> dic_sell = redis.HashKey(level2Type + "_" + tradeDay + "_sell").ToDictionary(o => o.Name, p => p.Value);
            //求并集
            var list_hb = dic_buy.Keys.Union(dic_sell.Keys).ToList();
            //黑名单
            List<string> hmd = Config.SddList.Split(",").ToList();
            foreach (var item in list_hb)
            {
                string secid = item;
                if (!hmd.Contains(secid))
                {

                    JArray jsonArray = new JArray();
                    JArray jsonArrayList = new JArray();
                    // string secid = item.Key;

                    List<WtBean> listwt = new List<WtBean>();
                    List<CjBean> listcj = new List<CjBean>();
                    //取委托记录
                    Dictionary<RedisValue, RedisValue> dic_wt = redis.HashKey("wt_" + tradeDay + "_" + level2Type + "_" + secid).ToDictionary(o => o.Name, p => p.Value);
                    //取成交记录
                    Dictionary<RedisValue, RedisValue> dic_cj = redis.HashKey("cj_" + tradeDay + "_" + level2Type + "_" + secid).ToDictionary(o => o.Name, p => p.Value);
                    foreach (var item_wt in dic_wt)
                    {
                        WtBean wt = JsonConvert.DeserializeObject<WtBean>(item_wt.Value);
                        //if (wt.Side.Trim() == "1")
                        listwt.Add(wt);
                    }

                    foreach (var item_cj in dic_cj)
                    {
                        CjBean cj = JsonConvert.DeserializeObject<CjBean>(item_cj.Value);
                        listcj.Add(cj);
                    }

                    var wtlist = listwt.OrderBy(o => o.TransactTime).ToList();

                    foreach (var wt in wtlist)
                    {
                        //找到该委托单的成交单
                        var cjlist = listcj.Where(o => o.WtSeqNum == wt.ApplSeqNum).OrderBy(p => p.CjTime).ToList();
                        double pjj = 0.0;
                        double cjs = 0.0;
                        if (cjlist.Count > 0)
                        {
                            foreach (var cj in cjlist)
                            {
                                pjj += cj.CjPx;
                                cjs += cj.CjQty;
                            }
                            string wttime = cjlist[0].WtTime.ToString().Trim().Substring(8, 6);
                            var obj = new
                            {
                                wtsj = wttime.Substring(0, 2) + ":" + wttime.Substring(2, 2) + ":" + wttime.Substring(4, 2),
                                side = cjlist[0].WtSide.Trim() == "1" ? "买" : "卖",
                                wtss = (Convert.ToDouble(cjlist[0].WtQty) / 10000.0).ToString(),
                                cjss = (Convert.ToDouble(cjs) / 10000.0).ToString(),
                                pjjg = (Convert.ToDouble(pjj / cjlist.Count) / 10000.0).ToString("0.00"),

                            };
                            jsonArray.Add(JToken.FromObject(obj));


                            //画图用
                            var objlist = new
                            {
                                wtsj = wttime.Substring(0, 2) + ":" + wttime.Substring(2, 2),
                                side = cjlist[0].WtSide.Trim() == "1" ? "买" : "卖",
                                wtjg = (Convert.ToDouble(cjlist[0].WtPx) / 10000.0).ToString(),
                            };
                            jsonArrayList.Add(JToken.FromObject(objlist));
                        }

                    }

                    if (jsonArray.Count > 0)
                    {
                        redis.HashSet("ggxq_" + tradeDay + "_" + secid, level2Type, JsonConvert.SerializeObject(jsonArray));
                        //统计有交易的次数
                        redis.HashSet(level2Type + "_" + tradeDay + "_buys", secid, jsonArray.Count);
                    }

                    if (jsonArrayList.Count > 0)
                    {
                        redis.HashSet("ggxq_" + tradeDay + "_" + secid, level2Type + "_pic", JsonConvert.SerializeObject(jsonArrayList));
                    }
                }
                else
                {
                    if (redis.HashExists(level2Type + "_" + tradeDay + "_buys", secid))
                    {
                        redis.HashDelete(level2Type + "_" + tradeDay + "_buys", secid);
                    }
                }
            }

        }


        /// <summary>
        /// 拖拉机单
        /// </summary>
        /// <param name="tradeDay"></param>
        /// <param name="level2Type"></param>
        /// <param name="side"></param>
        private void Ggxq_tljd(string tradeDay, string level2Type, string side)
        {
            //取今日所有level2股票
            Dictionary<RedisValue, RedisValue> dic_buy = redis.HashKey(level2Type + "_" + tradeDay + "_buy").ToDictionary(o => o.Name, p => p.Value);
            Dictionary<RedisValue, RedisValue> dic_sell = redis.HashKey(level2Type + "_" + tradeDay + "_sell").ToDictionary(o => o.Name, p => p.Value);
            //求并集
            var list_hb = dic_buy.Keys.Union(dic_sell.Keys).ToList();
            //黑名单
            List<string> hmd = Config.TljdList.Split(",").ToList();

            foreach (var item in list_hb)
            {
                string secid = item;
                if (!hmd.Contains(secid))
                {

                    JArray jsonArray = new JArray();
                    JArray jsonArrayList = new JArray();
                    //  string secid = item.Key;

                    //string secid = "002975";
                    List<WtBean> listwt = new List<WtBean>();
                    List<CjBean> listcj = new List<CjBean>();
                    //取委托记录
                    Dictionary<RedisValue, RedisValue> dic_wt = redis.HashKey("wt_" + tradeDay + "_" + level2Type + "_" + secid).ToDictionary(o => o.Name, p => p.Value);
                    //取成交记录
                    Dictionary<RedisValue, RedisValue> dic_cj = redis.HashKey("cj_" + tradeDay + "_" + level2Type + "_" + secid).ToDictionary(o => o.Name, p => p.Value);
                    foreach (var item_wt in dic_wt)
                    {
                        WtBean wt = JsonConvert.DeserializeObject<WtBean>(item_wt.Value);
                        // if (wt.Side.Trim() == "1")
                        listwt.Add(wt);
                    }

                    foreach (var item_cj in dic_cj)
                    {
                        CjBean cj = JsonConvert.DeserializeObject<CjBean>(item_cj.Value);
                        listcj.Add(cj);
                    }

                    int wtct = 0;
                    string last_wtsj = "";
                    string last_wtss = "";

                    var wtlist = listwt.OrderBy(o => o.TransactTime).ToList();

                    foreach (var wt in wtlist)
                    {
                        string wttime = wt.TransactTime.ToString().Trim().Substring(8, 6);
                        string wt_sj = wttime.Substring(0, 2) + ":" + wttime.Substring(2, 2) + ":" + wttime.Substring(4, 2);

                        string _wtsj = wttime.Substring(0, 2) + ":" + wttime.Substring(2, 2);

                        string _wtss = (Convert.ToDouble(wt.OrderQty) / 10000.0).ToString();
                        if (_wtsj != last_wtsj || _wtss != last_wtss)//委托时间 或者委托手数都不相同，此笔委托为另外一个拖拉机单
                        {
                            wtct++;
                            last_wtsj = _wtsj;
                            last_wtss = _wtss;
                        }
                        else
                        {
                            //此笔委托与上一笔委托同一拖拉机单
                            wt_sj = "";//时间不显示
                        }


                        //找到该委托单的成交单
                        var cjlist = listcj.Where(o => o.WtSeqNum == wt.ApplSeqNum).OrderBy(p => p.CjTime).ToList();

                        double cjs = 0.0;
                        if (cjlist.Count > 0)
                        {
                            foreach (var cj in cjlist)
                            {
                                cjs += cj.CjQty;
                            }

                            var obj = new
                            {
                                wtsj = wt_sj,
                                side = cjlist[0].WtSide.Trim() == "1" ? "买" : "卖",
                                wtjg = (Convert.ToDouble(cjlist[0].WtPx) / 10000.0).ToString(),
                                wtss = _wtss,
                                cjss = (Convert.ToDouble(cjs) / 10000.0).ToString(),

                            };
                            jsonArray.Add(JToken.FromObject(obj));


                            //画图用
                            var objlist = new
                            {
                                wtsj = wttime.Substring(0, 2) + ":" + wttime.Substring(2, 2),
                                side = cjlist[0].WtSide.Trim() == "1" ? "买" : "卖",
                                wtjg = (Convert.ToDouble(cjlist[0].WtPx) / 10000.0).ToString(),
                            };
                            jsonArrayList.Add(JToken.FromObject(objlist));
                        }
                        else
                        {

                            var obj = new
                            {
                                wtsj = wt_sj,
                                side = wt.Side.Trim() == "1" ? "买" : "卖",
                                wtjg = (Convert.ToDouble(wt.Price) / 10000.0).ToString(),
                                wtss = _wtss,
                                cjss = (Convert.ToDouble(cjs) / 10000.0).ToString(),

                            };
                            jsonArray.Add(JToken.FromObject(obj));


                            //画图用
                            var objlist = new
                            {
                                wtsj = wttime.Substring(0, 2) + ":" + wttime.Substring(2, 2),
                                side = wt.Side.Trim() == "1" ? "买" : "卖",
                                wtjg = (Convert.ToDouble(wt.Price) / 10000.0).ToString(),
                            };
                            jsonArrayList.Add(JToken.FromObject(objlist));

                        }

                    }
                    //统计有交易的次数
                    redis.HashSet(level2Type + "_" + tradeDay + "_buys", secid, wtct);

                    if (jsonArray.Count > 0)
                    {
                        redis.HashSet("ggxq_" + tradeDay + "_" + secid, level2Type, JsonConvert.SerializeObject(jsonArray));
                    }

                    if (jsonArrayList.Count > 0)
                    {
                        redis.HashSet("ggxq_" + tradeDay + "_" + secid, level2Type + "_pic", JsonConvert.SerializeObject(jsonArrayList));
                    }
                }
                else
                {
                    if (redis.HashExists(level2Type + "_" + tradeDay + "_buys", secid))
                    {
                        redis.HashDelete(level2Type + "_" + tradeDay + "_buys", secid);
                    }
                }
            }

        }


        public class WtBean
        {
            public string ChannelNo;
            public string ApplSeqNum;
            public string MdStreamID;
            public string SecurityID;
            public string SecurityIDSource;
            public string Price;
            public string OrderQty;
            public string Side;
            public double TransactTime;
        }
        public class CjBean
        {
            public string SecurityID;
            public double WtTime;
            public double CjTime;
            public string WtSide;
            public string WtSeqNum;
            public string WtPx;
            public string WtQty;
            public double CjPx;
            public double CjQty;
        }
    }
}
