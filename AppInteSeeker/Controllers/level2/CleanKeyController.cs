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
    public class CleanKeyController : ControllerBase
    {
        public SiteConfig Config;
        public CleanKeyController(IOptions<SiteConfig> option)
        {
            Config = option.Value;
        }

        public RedisHelper redis;
        List<string> list;

        [HttpGet("cleankey/{day}")]
        public IEnumerable<StatusRespondBean> GetAll(string day)
        {
            StatusRespondBean srb = new StatusRespondBean() { StatusCode = 1, StatusMessage = "CleanKeyController开始执行" };

            try
            {
                redis = new RedisHelper(0, Config.RedisConn);//包含DBNub,port
                list = new List<string>();


                DateTime timeNow = System.DateTime.Now.AddDays(-20);//删除20天前的数据
                string TradeDayNum = timeNow.ToString("yyyyMMdd");
                if (!day.Equals("0"))
                {
                    TradeDayNum = day;//不为0 就按前20天数据删除
                }
                ///主力
                Clean_zlcj(TradeDayNum);
                Clean_zlwt(TradeDayNum);
                Clean_zljmr(TradeDayNum);

                //每日个股成交、委托记录
                Clean_cjjl(TradeDayNum);
                Clean_wtjl(TradeDayNum);

                //每日个股详情
                Clean_ggxq(TradeDayNum);

                ///拖拉机单，闪电单，超级单每日统计记录
                Clean_tljd(TradeDayNum);
                Clean_sdd(TradeDayNum);
                Clean_cjd(TradeDayNum);

                //level2
                Clean_level2(TradeDayNum);

                srb.StatusCode = 1;
                srb.StatusMessage = JsonConvert.SerializeObject(list);
            }
            catch (Exception ex)
            {
                srb.StatusCode = -1;
                srb.StatusMessage = ex.Message;
            }
            yield return srb;
        }



        private void Clean_level2(string tradeDay)
        {
            //level2_20200511_cjd_cs_asc_buy
            string keyPattern = "level2_" + tradeDay + "_*";
            var script = " local res = redis.call('keys',@pattern) " + " return res ";
            RedisResult redisResult = redis.ScriptEvaluate(script, keyPattern);
            long ct = 0;
            if (!redisResult.IsNull)
            {
                // List<string> list = (List<string>)redisResult;
                string[] preSult = (string[])redisResult;//将返回的结果集转为数组
                ct = redis.KeyDeleteAll(preSult);
            }

            list.Add(keyPattern + "，共删除" + ct + "个");
        }


        private void Clean_ggxq(string tradeDay)
        {
            //ggxq_20200512_002709
            string keyPattern = "ggxq_" + tradeDay + "_*";
            var script = " local res = redis.call('keys',@pattern) " + " return res ";
            RedisResult redisResult = redis.ScriptEvaluate(script, keyPattern);
            long ct = 0;
            if (!redisResult.IsNull)
            {
                // List<string> list = (List<string>)redisResult;
                string[] preSult = (string[])redisResult;//将返回的结果集转为数组
                ct = redis.KeyDeleteAll(preSult);
            }

            list.Add(keyPattern + "，共删除" + ct + "个");
        }

        private void Clean_tljd(string tradeDay)
        {
            //tljd_20200511_buy,tljd_20200511_buy_pri，tljd_20200515_sell，tljd_20200515_sell_pri
            string keyPattern = "tljd_" + tradeDay + "_*";
            var script = " local res = redis.call('keys',@pattern) " + " return res ";
            RedisResult redisResult = redis.ScriptEvaluate(script, keyPattern);
            long ct = 0;
            if (!redisResult.IsNull)
            {
                // List<string> list = (List<string>)redisResult;
                string[] preSult = (string[])redisResult;//将返回的结果集转为数组
                ct = redis.KeyDeleteAll(preSult);
            }
            list.Add(keyPattern + "，共删除" + ct + "个");
        }
        private void Clean_sdd(string tradeDay)
        {
            //sdd_20200511_buy,sdd_20200511_buy_pri，sdd_20200515_sell，sdd_20200515_sell_pri
            string keyPattern = "sdd_" + tradeDay + "_*";
            var script = " local res = redis.call('keys',@pattern) " + " return res ";
            RedisResult redisResult = redis.ScriptEvaluate(script, keyPattern);
            long ct = 0;
            if (!redisResult.IsNull)
            {
                // List<string> list = (List<string>)redisResult;
                string[] preSult = (string[])redisResult;//将返回的结果集转为数组
                ct = redis.KeyDeleteAll(preSult);
            }
            list.Add(keyPattern + "，共删除" + ct + "个");
        }

        private void Clean_cjd(string tradeDay)
        {
            //cjd_20200511_buy,cjd_20200511_buy_pri，cjd_20200525_sell，cjd_20200525_sell_pri
            string keyPattern = "cjd_" + tradeDay + "_*";
            var script = " local res = redis.call('keys',@pattern) " + " return res ";
            RedisResult redisResult = redis.ScriptEvaluate(script, keyPattern);
            long ct = 0;
            if (!redisResult.IsNull)
            {
                // List<string> list = (List<string>)redisResult;
                string[] preSult = (string[])redisResult;//将返回的结果集转为数组
                ct = redis.KeyDeleteAll(preSult);
            }
            list.Add(keyPattern + "，共删除" + ct + "个");
        }

        private void Clean_cjjl(string tradeDay)
        {
            //cj_20200512_sdd_300009,cj_20200512_cjd_300803
            string keyPattern = "cj_" + tradeDay + "_*";
            var script = " local res = redis.call('keys',@pattern) " + " return res ";
            RedisResult redisResult = redis.ScriptEvaluate(script, keyPattern);
            long ct = 0;
            if (!redisResult.IsNull)
            {
                // List<string> list = (List<string>)redisResult;
                string[] preSult = (string[])redisResult;//将返回的结果集转为数组
                ct = redis.KeyDeleteAll(preSult);
            }
            list.Add(keyPattern + "，共删除" + ct + "个");
        }


        private void Clean_wtjl(string tradeDay)
        {
            //wt_20200511_cjd_000066,wt_20200513_sdd_000661,wt_20200527_tljd_000021
            string keyPattern = "wt_" + tradeDay + "_*";
            var script = " local res = redis.call('keys',@pattern) " + " return res ";
            RedisResult redisResult = redis.ScriptEvaluate(script, keyPattern);
            long ct = 0;
            if (!redisResult.IsNull)
            {
                // List<string> list = (List<string>)redisResult;
                string[] preSult = (string[])redisResult;//将返回的结果集转为数组
                ct = redis.KeyDeleteAll(preSult);
            }
            list.Add(keyPattern + "，共删除" + ct + "个");
        }


        private void Clean_zlcj(string tradeDay)
        {
            string keyPattern = "zlcj_" + tradeDay;
            var script = " local res = redis.call('keys',@pattern) " + " return res ";
            RedisResult redisResult = redis.ScriptEvaluate(script, keyPattern);
            long ct = 0;
            if (!redisResult.IsNull)
            {
                // List<string> list = (List<string>)redisResult;
                string[] preSult = (string[])redisResult;//将返回的结果集转为数组
                ct = redis.KeyDeleteAll(preSult);
            }
            list.Add(keyPattern + "，共删除" + ct + "个");
        }

        private void Clean_zlwt(string tradeDay)
        {
            string keyPattern = "zlwt_" + tradeDay;
            var script = " local res = redis.call('keys',@pattern) " + " return res ";
            RedisResult redisResult = redis.ScriptEvaluate(script, keyPattern);
            long ct = 0;
            if (!redisResult.IsNull)
            {
                // List<string> list = (List<string>)redisResult;
                string[] preSult = (string[])redisResult;//将返回的结果集转为数组
                ct = redis.KeyDeleteAll(preSult);
            }
            list.Add(keyPattern + "，共删除" + ct + "个");
        }
        private void Clean_zljmr(string tradeDay)
        {
            string keyPattern = "zljmr_" + tradeDay;
            var script = " local res = redis.call('keys',@pattern) " + " return res ";
            RedisResult redisResult = redis.ScriptEvaluate(script, keyPattern);
            long ct = 0;
            if (!redisResult.IsNull)
            {
                // List<string> list = (List<string>)redisResult;
                string[] preSult = (string[])redisResult;//将返回的结果集转为数组
                ct = redis.KeyDeleteAll(preSult);
            }
            list.Add(keyPattern + "，共删除" + ct + "个");
        }
    }
}
