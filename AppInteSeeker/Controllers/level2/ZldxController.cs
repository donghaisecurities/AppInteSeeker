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
    /// 主力资金动向
    /// </summary>
    [ApiController]
    [Route("api/level2")]
    public class ZldxController : ControllerBase
    {
        public SiteConfig Config;
        public ZldxController(IOptions<SiteConfig> option)
        {
            Config = option.Value;
        }

        public RedisHelper redis;
        public string __zqdm = "";
        public string ConnJy;

        [HttpGet("zldx")]
        public IEnumerable<StatusRespondBean> GetAll()
        {
            StatusRespondBean srb = new StatusRespondBean() { StatusCode = 1, StatusMessage = "ZldxController开始执行" };

            try
            {
                redis = new RedisHelper(0, Config.RedisConn);//包含DBNub,port
                ConnJy = Config.JySqlConn;


                DateTime timeNow = System.DateTime.Now;
                string redisTime = timeNow.ToString("yyyy-MM-dd HH:mm");
                string TradeDayNum = timeNow.ToString("yyyyMMdd");
                string lastTradeDayNum = TradeDayNum;
                string TradeDayStr = timeNow.ToString("yyyy-MM-dd");//当前日期
                string TradeDayStrNow = TradeDayStr;//最近一个交易日
                string sql_day = string.Format(@"
select top 2 * from QT_TradingDayNew  where SecuMarket=83  
and TradingDate <= '{0}' and IfTradingDay=1 order by TradingDate desc", TradeDayStr);

                DataTable dt_day = SqlHelper.ExecuteQuery(sql_day, ConnJy);
                if (dt_day.Rows.Count > 0)
                {
                    DateTime dtime = Convert.ToDateTime(dt_day.Rows[0]["TradingDate"]);
                    TradeDayStrNow = dtime.ToString("yyyy-MM-dd");
                    TradeDayNum = dtime.ToString("yyyyMMdd");
                    lastTradeDayNum = Convert.ToDateTime(dt_day.Rows[1]["TradingDate"]).ToString("yyyyMMdd");//上一个交易日
                }
                //查找所有主力委托
                List<Ggxq> List_zl = new List<Ggxq>();
                Dictionary<RedisValue, RedisValue> dic_list = redis.HashKey("zlwt_" + TradeDayNum).ToDictionary(o => o.Name, p => p.Value);
                //查找最新交易日主力成交量
                Dictionary<RedisValue, RedisValue> zlcjl_dicAll = redis.HashKey("zlcj_" + TradeDayNum).ToDictionary(o => o.Name, p => p.Value);
                //查找最新交易日主力净流入
                Dictionary<RedisValue, RedisValue> zljmr_dicAll = redis.HashKey("zljmr_" + TradeDayNum).ToDictionary(o => o.Name, p => p.Value);
                //查找上一个交易日主力成交量
                Dictionary<RedisValue, RedisValue> zlcjl_dicAll_last = redis.HashKey("zlcj_" + lastTradeDayNum).ToDictionary(o => o.Name, p => p.Value);
                //查找上一个交易日主力净流入
                Dictionary<RedisValue, RedisValue> zljmr_dicAll_last = redis.HashKey("zljmr_" + lastTradeDayNum).ToDictionary(o => o.Name, p => p.Value);



                if (dic_list.Count > 0)
                {
                    foreach (var item in dic_list)
                    {
                        __zqdm = item.Key;
                       // string zqdm = "002604";
                        string zqdm = item.Key;
                        //查找个股行情取总成交量
                        var gghq = redis.StringGet("gghq_" + zqdm);
                        JObject oo = JObject.Parse(gghq);
                        double cjl = Convert.ToDouble(oo["totalValueTrade"].ToString().Trim());
                        //查找主力成交量
                        double zlcjl = 0.0;
                        Dictionary<RedisValue, RedisValue> zlcjl_dic = zlcjl_dicAll.Where(p => p.Key == zqdm).ToDictionary(o => o.Key, p => p.Value);
                        if (zlcjl_dic.Count > 0)
                        {
                            foreach (var itm in zlcjl_dic)
                            {
                                zlcjl = Convert.ToDouble(itm.Value) / 100.0;
                            }

                        }
                        double zlcjl_last = 0.0;
                        Dictionary<RedisValue, RedisValue> zlcjl_dic_last = zlcjl_dicAll_last.Where(p => p.Key == zqdm).ToDictionary(o => o.Key, p => p.Value);
                        if (zlcjl_dic_last.Count > 0)
                        {
                            foreach (var itm in zlcjl_dic_last)
                            {
                                zlcjl_last = Convert.ToDouble(itm.Value) / 100.0;
                            }

                        }
                        //查找主力净流入
                        double zljmr = 0.0;
                        Dictionary<RedisValue, RedisValue> zljmr_dic = zljmr_dicAll.Where(p => p.Key == zqdm).ToDictionary(o => o.Key, p => p.Value);
                        if (zljmr_dic.Count > 0)
                        {
                            foreach (var itm in zljmr_dic)
                            {
                                zljmr = Convert.ToDouble(itm.Value);
                            }

                        }

                        double zljmr_last = 0.0;
                        Dictionary<RedisValue, RedisValue> zljmr_dic_last = zljmr_dicAll_last.Where(p => p.Key == zqdm).ToDictionary(o => o.Key, p => p.Value);
                        if (zljmr_dic_last.Count > 0)
                        {
                            foreach (var itm in zljmr_dic_last)
                            {
                                zljmr_last = Convert.ToDouble(itm.Value);
                            }

                        }
                        ///证券简称
                        string _zqjc = string.Empty;
                        string sql = string.Format(@"select SecuAbbr  from SecuMain a 
                    where a.SecuCode='{0}'  and a.SecuMarket=90 and a.SecuCategory=1 ", zqdm);
                        DataTable dt_name = SqlHelper.ExecuteQuery(sql, ConnJy);
                        if (dt_name.Rows.Count > 0)
                        {
                            _zqjc = dt_name.Rows[0]["SecuAbbr"].ToString();
                        }

                        //string __jmrbd = zljmr_last == 0.0 ? "--" : ConvertHelper.ConvertTo2XS100(zljmr / zljmr_last - 1) + "%";
                        //string __zcjbd = zlcjl_last == 0.0 ? "--" : ConvertHelper.ConvertTo2XS100(zlcjl / zlcjl_last - 1) + "%";
                        //double zcjzb = cjl == 0.0 ? cjl : ConvertHelper.ConvertTo4XDouble(zlcjl / cjl);
                        var zl = new Ggxq()
                        {
                            zqdm = zqdm,
                            zqjc = _zqjc,
                            tradeday = TradeDayNum,
                            jmr = zljmr,
                            jmrbd = zljmr_last == 0.0 ? "--" : ConvertHelper.ConvertTo2XS100(zljmr / zljmr_last - 1) + "%",
                            zcjbd = zlcjl_last == 0.0 ? "--" : ConvertHelper.ConvertTo2XS100(zlcjl / zlcjl_last - 1) + "%",
                            zcjzb = cjl == 0.0 ? cjl : ConvertHelper.ConvertTo4XDouble(zlcjl / cjl),
                        };

                        List_zl.Add(zl);
                    }
                }
                var list_jmr = List_zl.OrderByDescending(p => p.jmr).Take(50);
                var ls_jmr = (from g in list_jmr.ToList()
                              select new
                              {
                                  g.zqdm,
                                  g.zqjc,
                                  jmr = ConvertHelper.ConvertToString(g.jmr / 1000000.0),
                                  g.jmrbd,
                              }).ToList();

                var list_cj = List_zl.OrderByDescending(p => p.zcjzb).Take(50);
                var ls_cj = (from g in list_cj.ToList()
                             select new
                             {
                                 g.zqdm,
                                 g.zqjc,
                                 zcjzb = ConvertHelper.ConvertTo2XS100(g.zcjzb) + "%",
                                 g.zcjbd,
                             }).ToList();


                redis.HashSet("zlzjdx", "jmr", ls_jmr);
                redis.HashSet("zlzjdx", "cjzb", ls_cj);
                redis.HashSet("zlzjdx", "redistime", redisTime);
                srb.StatusCode = 1;
                srb.StatusMessage = "ZldxController执行完成";
            }
            catch (Exception ex)
            {
                srb.StatusCode = -1;
                srb.StatusMessage = ex.Message + ":" + __zqdm;
            }
            yield return srb;
        }



        public class Ggxq
        {
            public string tradeday;
            public string zqjc;
            public string zqdm;
            public double jmr;
            public string jmrbd;
            public double zcjzb;
            public string zcjbd;

        }
    }
}
