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
using System.Text;

namespace AppInteSeeker.Controllers.level2
{
    [ApiController]
    [Route("api/level2")]
    public class Ggxq5dayController : ControllerBase
    {
        public SiteConfig Config;
        public Ggxq5dayController(IOptions<SiteConfig> option)
        {
            Config = option.Value;
        }

        public RedisHelper redis;

        public string ConnJy;

        [HttpGet("ggxq5day")]
        public IEnumerable<StatusRespondBean> GetAll()
        {
            StatusRespondBean srb = new StatusRespondBean() { StatusCode = 1, StatusMessage = "Ggxq5dayController开始执行" };

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
select top 5 * from QT_TradingDayNew  where SecuMarket=83  
and TradingDate <= '{0}' and IfTradingDay=1 order by TradingDate desc", TradeDayStr);

                DataTable dt_day = SqlHelper.ExecuteQuery(sql_day, ConnJy);

                //深圳市场股票池
                string sql_gpc = string.Format(@" select SecuCode from SecuMain where SecuMarket=90 and SecuCategory=1  and ListedState=1");

                DataTable dt_gpc = SqlHelper.ExecuteQuery(sql_gpc, ConnJy);


                foreach (DataRow drgp in dt_gpc.Rows)
                {
                    string secucode = drgp["SecuCode"].ToString();
                    //string secucode = "300148";

                    //StringBuilder sb = new StringBuilder();
                    List<day5> list = new List<day5>();

                    foreach (DataRow dr in dt_day.Rows)
                    {
                        DateTime dtime = Convert.ToDateTime(dr["TradingDate"]);
                        TradeDayStrNow = dtime.ToString("yyyy-MM-dd");
                        TradeDayNum = dtime.ToString("yyyyMMdd");

                        //取指定tradeDay所有level2股票
                        Dictionary<RedisValue, RedisValue> dic_buy = redis.HashKey("level2_" + TradeDayNum + "_buy").ToDictionary(o => o.Name, p => p.Value);

                        foreach (var item in dic_buy)
                        {
                            string type = item.Key;
                            //string _val = item.Value;
                            List<Bean> beanList = JsonConvert.DeserializeObject<List<Bean>>(item.Value);
                            var secBean = beanList.Where(o => o.zqdm == secucode).ToList().FirstOrDefault();
                            if (secBean != null)
                            {
                                string side = "净流入";
                                if (secBean.cjje.StartsWith("-"))
                                {
                                    side = "净流出";
                                }
                                string typeStr = "";

                                switch (type)
                                {
                                    case "tljd":
                                        typeStr = "拖拉机单"; break;
                                    case "sdd":
                                        typeStr = "闪电单"; break;
                                    case "cjd":
                                        typeStr = "超级单"; break;
                                    default: break;
                                }
                                day5 obj = new day5
                                {
                                    tradeday = TradeDayStrNow,
                                    zqdm = secucode,
                                    type = typeStr,
                                    side= side,
                                    jrcs = secBean.jrcs,
                                    cjje = secBean.cjje
                                };
                                list.Add(obj);
                            }

                        }

                    }
                    if (list.Count > 0)
                    {
                        redis.StringSet("ggxq_5day_" + secucode, list, TimeSpan.FromSeconds(Convert.ToDouble(Config.RedisExpiry)));
                    }
                }

                srb.StatusCode = 1;
                srb.StatusMessage = "Ggxq5dayController执行完成";
            }
            catch (Exception ex)
            {
                srb.StatusCode = -1;
                srb.StatusMessage = ex.Message;
            }
            yield return srb;
        }

        public class Bean
        {
            public string zqdm;
            public string zqjc;
            public string jrcs;
            public string cjje;
            public string cjj;

        }
        public class day5
        {
            public string tradeday;
            public string zqdm;
            public string type;
            public string side;
            public string jrcs;
            public string cjje;

        }
    }
}
