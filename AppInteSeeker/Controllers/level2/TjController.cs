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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppInteSeeker.Controllers.level2
{
    [ApiController]
    [Route("api/level2")]
    public class TjController : Controller
    {
        public SiteConfig Config;
        public TjController(IOptions<SiteConfig> option)
        {
            Config = option.Value;
        }

        public RedisHelper redis;

        public string ConnJy;

        [HttpGet("tj")]
        public IEnumerable<StatusRespondBean> GetAll()
        {
            StatusRespondBean srb = new StatusRespondBean() { StatusCode = 1, StatusMessage = "TjController开始执行" };

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
                if (dt_day.Rows.Count > 0)
                {
                    DateTime dtime = Convert.ToDateTime(dt_day.Rows[0]["TradingDate"]);
                    TradeDayStrNow = dtime.ToString("yyyy-MM-dd");
                    TradeDayNum = dtime.ToString("yyyyMMdd");
                }




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

        private void Zxglb(string tradeDay, string level2Type, string side, string uptime, DateTime dtnow)
        {
            //从个股详情统计有交易的次数
            Dictionary<RedisValue, RedisValue> dic_buy = redis.HashKey(level2Type + "_" + tradeDay + "_buys").ToDictionary(o => o.Name, p => p.Value);
            var list_hb = dic_buy.Keys.ToList();

            if (list_hb.Count > 0)
            {
                foreach (var item in list_hb)
                {
                    string secid = item;
                }

            }

        }
    }
}
