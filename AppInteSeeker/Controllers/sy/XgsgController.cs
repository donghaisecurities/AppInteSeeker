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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppInteSeeker.Controllers.sy
{
    [ApiController]
    [Route("api/sy")]
    public class XgsgController : ControllerBase
    {
        /* 系统中总共3个key
       新股详情 XgDetail_688321 
       新股申购专栏总key  ZXXgsg(3个key)
       新股申购专栏科创板总key ZXXgsgKcb(3个key)
       科创板ipo状态更多   ZXXgIPO_more
       科创板首日上市表现更多    ZXXgSsbx_more
       */

        public SiteConfig Config;
        public XgsgController(IOptions<SiteConfig> option)
        {
            Config = option.Value;
        }
        private Dictionary<string, object> dicJAarry;
        private Dictionary<string, object> dicJAarryKcb;
        public RedisHelper redis;
        public string ConnDhzb;
        public string ConnJy;
        public string XgKeyDetail = "XgDetail_";
        [HttpGet("getxgsg")]
        public IEnumerable<StatusRespondBean> GetAll()
        {
            StatusRespondBean srb = new StatusRespondBean() { StatusCode = 1, StatusMessage = "XgsgController开始执行" };

            try
            {
                redis = new RedisHelper(0, Config.RedisConn);//包含DBNub,port
                ConnJy = Config.JySqlConn;
                ConnDhzb = Config.OracleConnDhzb;
                dicJAarry = new Dictionary<string, object>();
                dicJAarryKcb = new Dictionary<string, object>();
                Xgrl();
                Yfxdss();
                Shbx();

                Kcb_Jrxg();
                Kcb_IPO();
                Kcb_Srssbx();



                redis.HashSet("ZXXgsg", dicJAarry);
                redis.HashSet("ZXXgsgKcb", dicJAarryKcb);
                srb.StatusCode = 1;
                srb.StatusMessage = JsonConvert.SerializeObject(dicJAarry);
            }
            catch (Exception ex)
            {
               // Loghelper.Error(this, ex.Message);
                srb.StatusCode = -1;
                srb.StatusMessage = ex.Message;
            }
            yield return srb;
        }

        /// <summary>
        /// 新股日历
        /// </summary>
        private void Xgrl()
        {
            List<XgInfo> List = new List<XgInfo>();
            DateTime dtnow = System.DateTime.Now;

            string sql = string.Format(@"select * from 
                                        (select * from  view_xgzx where fxjc='10' and sgr is not null 
                                        union all select * from view_xgzx where fxjc='20' and sgr between sysdate-1 and sysdate) 
                                        order by sgr ");
            DataTable dt = OracleHelper.ExecuteDataTable(sql, ConnDhzb);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    XgInfo oj = new XgInfo()
                    {
                        dhnm = dr["dhnm"].ToString(),
                        zqdm = dr["zqdm"].ToString(),
                        zqjc = dr["zqjc"].ToString(),
                        sgdm_swfx = dr["sgdm_swfx"].ToString(),
                        ssd = dr["ssd"].ToString().Equals("90") ? "深圳证券交易所" : "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["mgfxj"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["syl"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["fxlsx"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["wsfxjh"], 2) + "万",
                        sgsz = dr["ssd"].ToString().Equals("90") ? "深圳市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万" : "上海市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万",
                        sgsx_swfx = dr["sgsx_swfx"].ToString().Equals("") ? "--" : dr["sgsx_swfx"].ToString(),
                        jys = dr["ssd"].ToString().Equals("90") ? "深" : "沪",
                        bklx = "",
                        zql_wsfx = ConvertHelper.ConvertTo2XS100(dr["zql_wsfx"]) + "%",
                        ssrq = dr["ssrq"].ToString(),
                        myqhl_xg = dr["myqhl_xg"].ToString(),
                        xtcxcs = dr["xtcxcs"].ToString(),
                        gsjj = "",
                        sgr = ConvertHelper.DateToShotStringNull(dr["sgr"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["fxjgggr"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["paydateonline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["sgr"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };

                    XgInfo xg = new XgInfo()
                    {
                        dhnm = dr["dhnm"].ToString(),
                        zqdm = dr["zqdm"].ToString(),
                        zqjc = dr["zqjc"].ToString(),
                        sgdm_swfx = dr["sgdm_swfx"].ToString(),
                        ssd = dr["ssd"].ToString().Equals("90") ? "深圳证券交易所" : "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["mgfxj"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["syl"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["fxlsx"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["wsfxjh"], 2) + "万",
                        sgsz = dr["ssd"].ToString().Equals("90") ? "深圳市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万" : "上海市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万",
                        sgsx_swfx = dr["sgsx_swfx"].ToString().Equals("") ? "--" : dr["sgsx_swfx"].ToString(),
                        jys = dr["ssd"].ToString().Equals("90") ? "深" : "沪",
                        bklx = "",
                        zql_wsfx = ConvertHelper.ConvertTo2XS100(dr["zql_wsfx"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ssrq"]),
                        myqhl_xg = dr["myqhl_xg"].ToString(),
                        xtcxcs = dr["xtcxcs"].ToString(),
                        gsjj = dr["gsjj"].ToString(),
                        sgr = ConvertHelper.DateToShotStringNull(dr["sgr"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["fxjgggr"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["paydateonline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["sgr"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };
                    List.Add(oj);

                    redis.StringSet(XgKeyDetail + xg.zqdm, xg, TimeSpan.FromSeconds(Convert.ToDouble(Config.RedisExpiry)));
                }
            }


            //科创板(10天内),生产环境<=0
            string sql_kcb = string.Format(@"select a.SecuCode,SecurityAbbr,ApplyCodeOnline
,(case when IssuePrice is null then EstiIssuePrice else IssuePrice end) as IssuePrice,PERAfterIssueCutNP,
IssueVol,SharesOnline,ApplyMaxOnline  , a.LotRateOnline ,a.ListedDate,	OnlineStartDate,LotRatePublDate ,PayDateOnline ,
c.BriefIntroText,d.ClosePrice from LC_STIBIPOIssue a
left join SecuMain b on a.SecuCode=b.SecuCode 
left join LC_STIBStockArchives c on b.CompanyCode=c.CompanyCode
left join (select aa.ClosePrice,aa.TradingDay,aa.InnerCode from LC_STIBDailyQuote aa join (
select InnerCode,MAX(TradingDay) TradingDay from  LC_STIBDailyQuote  group by InnerCode) bb 
on aa.InnerCode=bb.InnerCode and aa.TradingDay=bb.TradingDay) d on b.InnerCode=d.InnerCode
where DateDiff(dd,a.OnlineStartDate,GETDATE())>=-10
and DateDiff(dd,a.OnlineStartDate,GETDATE())<=0
order by  a.OnlineStartDate  ");
            DataTable dt_kcb = SqlHelper.ExecuteQuery(sql_kcb, ConnJy);
            if (dt_kcb.Rows.Count > 0)
            {
                foreach (DataRow dr in dt_kcb.Rows)
                {
                    decimal spj = dr["ClosePrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["ClosePrice"]);
                    decimal fxj = dr["IssuePrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["IssuePrice"]);

                    string mqhl = ((spj - fxj) * 500).ToString("G0");
                    XgInfo oj = new XgInfo()
                    {
                        dhnm = "",
                        zqdm = dr["SecuCode"].ToString(),
                        zqjc = dr["SecurityAbbr"].ToString(),
                        sgdm_swfx = dr["ApplyCodeOnline"].ToString(),
                        ssd = "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["IssuePrice"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["PERAfterIssueCutNP"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["IssueVol"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["SharesOnline"], 2) + "万",
                        sgsz = "上海市值" + ConvertHelper.ConvertToDanWei(Convert.ToDouble(dr["ApplyMaxOnline"]) * 10, 2) + "万",
                        sgsx_swfx = dr["ApplyMaxOnline"].ToString(),
                        jys = "沪",
                        bklx = "科",
                        zql_wsfx = ConvertHelper.ConvertTo2XS(dr["LotRateOnline"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ListedDate"]),
                        myqhl_xg = mqhl,
                        xtcxcs = "--",
                        gsjj = "",
                        sgr = ConvertHelper.DateToShotStringNull(dr["OnlineStartDate"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["LotRatePublDate"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["PayDateOnline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["OnlineStartDate"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };

                    XgInfo xg = new XgInfo()
                    {
                        dhnm = "",
                        zqdm = dr["SecuCode"].ToString(),
                        zqjc = dr["SecurityAbbr"].ToString(),
                        sgdm_swfx = dr["ApplyCodeOnline"].ToString(),
                        ssd = "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["IssuePrice"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["PERAfterIssueCutNP"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["IssueVol"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["SharesOnline"], 2) + "万",
                        sgsz = "上海市值" + ConvertHelper.ConvertToDanWei(Convert.ToDouble(dr["ApplyMaxOnline"]) * 10, 2) + "万",
                        sgsx_swfx = dr["ApplyMaxOnline"].ToString(),
                        jys = "沪",
                        bklx = "科",
                        zql_wsfx = ConvertHelper.ConvertTo2XS(dr["LotRateOnline"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ListedDate"]),
                        myqhl_xg = mqhl,
                        xtcxcs = "--",
                        gsjj = dr["BriefIntroText"].ToString(),
                        sgr = ConvertHelper.DateToShotStringNull(dr["OnlineStartDate"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["LotRatePublDate"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["PayDateOnline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["OnlineStartDate"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };
                    List.Add(oj);
                    redis.StringSet(XgKeyDetail + xg.zqdm, xg, TimeSpan.FromSeconds(Convert.ToDouble(Config.RedisExpiry)));
                }
            }
            var _list = List.OrderBy(p => p.orderbydate).ToList();
            dicJAarry.Add("1", _list);
           // redis.StringSet("ZXPendingPurchase2", JsonConvert.SerializeObject(_list));

        }

        /// <summary>
        /// 已发行待上市
        /// </summary>
        private void Yfxdss()
        {
            List<XgInfo> List = new List<XgInfo>();
            DateTime dtnow = System.DateTime.Now;
            string sql = string.Format(@"select * from view_xgzx where fxjc='20' and sgr<sysdate-1 order by sgr desc ");
            DataTable dt = OracleHelper.ExecuteDataTable(sql, ConnDhzb);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    XgInfo oj = new XgInfo()
                    {
                        dhnm = dr["dhnm"].ToString(),
                        zqdm = dr["zqdm"].ToString(),
                        zqjc = dr["zqjc"].ToString(),
                        sgdm_swfx = dr["sgdm_swfx"].ToString(),
                        ssd = dr["ssd"].ToString().Equals("90") ? "深圳证券交易所" : "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["mgfxj"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["syl"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["fxlsx"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["wsfxjh"], 2) + "万",
                        sgsz = dr["ssd"].ToString().Equals("90") ? "深圳市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万" : "上海市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万",
                        sgsx_swfx = dr["sgsx_swfx"].ToString().Equals("") ? "--" : dr["sgsx_swfx"].ToString(),
                        jys = dr["ssd"].ToString().Equals("90") ? "深" : "沪",
                        bklx = "",
                        zql_wsfx = ConvertHelper.ConvertTo2XS100(dr["zql_wsfx"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ssrq"]),
                        myqhl_xg = dr["myqhl_xg"].ToString(),
                        xtcxcs = dr["xtcxcs"].ToString(),
                        gsjj = "",
                        sgr = ConvertHelper.DateToShotStringNull(dr["sgr"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["fxjgggr"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["paydateonline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["sgr"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };

                    XgInfo xg = new XgInfo()
                    {
                        dhnm = dr["dhnm"].ToString(),
                        zqdm = dr["zqdm"].ToString(),
                        zqjc = dr["zqjc"].ToString(),
                        sgdm_swfx = dr["sgdm_swfx"].ToString(),
                        ssd = dr["ssd"].ToString().Equals("90") ? "深圳证券交易所" : "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["mgfxj"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["syl"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["fxlsx"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["wsfxjh"], 2) + "万",
                        sgsz = dr["ssd"].ToString().Equals("90") ? "深圳市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万" : "上海市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万",
                        sgsx_swfx = dr["sgsx_swfx"].ToString().Equals("") ? "--" : dr["sgsx_swfx"].ToString(),
                        jys = dr["ssd"].ToString().Equals("90") ? "深" : "沪",
                        bklx = "",
                        zql_wsfx = ConvertHelper.ConvertTo2XS100(dr["zql_wsfx"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ssrq"]),
                        myqhl_xg = dr["myqhl_xg"].ToString(),
                        xtcxcs = dr["xtcxcs"].ToString(),
                        gsjj = dr["gsjj"].ToString(),
                        sgr = ConvertHelper.DateToShotStringNull(dr["sgr"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["fxjgggr"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["paydateonline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["sgr"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };
                    List.Add(oj);

                    redis.StringSet(XgKeyDetail + xg.zqdm, xg, TimeSpan.FromSeconds(Convert.ToDouble(Config.RedisExpiry)));
                }
            }


            //科创板
            string sql_kcb = string.Format(@"select a.SecuCode,SecurityAbbr,ApplyCodeOnline ,IssuePrice,PERAfterIssueCutNP,
IssueVol,SharesOnline,ApplyMaxOnline  , a.LotRateOnline ,a.ListedDate,	OnlineStartDate,LotRatePublDate ,PayDateOnline ,
c.BriefIntroText,d.ClosePrice from LC_STIBIPOIssue a
left join SecuMain b on a.SecuCode=b.SecuCode 
left join LC_STIBStockArchives c on b.CompanyCode=c.CompanyCode
left join (select aa.ClosePrice,aa.TradingDay,aa.InnerCode from LC_STIBDailyQuote aa join (
select InnerCode,MAX(TradingDay) TradingDay from  LC_STIBDailyQuote  group by InnerCode) bb 
on aa.InnerCode=bb.InnerCode and aa.TradingDay=bb.TradingDay) d on b.InnerCode=d.InnerCode
where DateDiff(day,a.OnlineStartDate,GETDATE())>0 and ( DateDiff(day,a.ListedDate,GETDATE())<0 or a.ListedDate is null)
order by  a.OnlineStartDate  ");
            DataTable dt_kcb = SqlHelper.ExecuteQuery(sql_kcb, ConnJy);
            if (dt_kcb.Rows.Count > 0)
            {
                foreach (DataRow dr in dt_kcb.Rows)
                {
                    decimal spj = dr["ClosePrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["ClosePrice"]);
                    decimal fxj = dr["IssuePrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["IssuePrice"]);

                    string mqhl = ((spj - fxj) * 500).ToString("G0");
                    XgInfo oj = new XgInfo()
                    {
                        dhnm = "",
                        zqdm = dr["SecuCode"].ToString(),
                        zqjc = dr["SecurityAbbr"].ToString(),
                        sgdm_swfx = dr["ApplyCodeOnline"].ToString(),
                        ssd = "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["IssuePrice"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["PERAfterIssueCutNP"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["IssueVol"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["SharesOnline"], 2) + "万",
                        sgsz = "上海市值" + ConvertHelper.ConvertToDanWei(Convert.ToDouble(dr["ApplyMaxOnline"]) * 10, 2) + "万",
                        sgsx_swfx = dr["ApplyMaxOnline"].ToString(),
                        jys = "沪",
                        bklx = "科",
                        zql_wsfx = ConvertHelper.ConvertTo2XS(dr["LotRateOnline"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ListedDate"]),
                        myqhl_xg = mqhl,
                        xtcxcs = "--",
                        gsjj = "",
                        sgr = ConvertHelper.DateToShotStringNull(dr["OnlineStartDate"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["LotRatePublDate"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["PayDateOnline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["OnlineStartDate"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };

                    XgInfo xg = new XgInfo()
                    {
                        dhnm = "",
                        zqdm = dr["SecuCode"].ToString(),
                        zqjc = dr["SecurityAbbr"].ToString(),
                        sgdm_swfx = dr["ApplyCodeOnline"].ToString(),
                        ssd = "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["IssuePrice"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["PERAfterIssueCutNP"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["IssueVol"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["SharesOnline"], 2) + "万",
                        sgsz = "上海市值" + ConvertHelper.ConvertToDanWei(Convert.ToDouble(dr["ApplyMaxOnline"]) * 10, 2) + "万",
                        sgsx_swfx = dr["ApplyMaxOnline"].ToString(),
                        jys = "沪",
                        bklx = "科",
                        zql_wsfx = ConvertHelper.ConvertTo2XS(dr["LotRateOnline"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ListedDate"]),
                        myqhl_xg = mqhl,
                        xtcxcs = "--",
                        gsjj = dr["BriefIntroText"].ToString(),
                        sgr = ConvertHelper.DateToShotStringNull(dr["OnlineStartDate"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["LotRatePublDate"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["PayDateOnline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["OnlineStartDate"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };
                    List.Add(oj);
                    redis.StringSet(XgKeyDetail + xg.zqdm, xg, TimeSpan.FromSeconds(Convert.ToDouble(Config.RedisExpiry)));
                }
            }
            var _list = List.OrderByDescending(p => p.orderbydate).ToList();
            dicJAarry.Add("2", _list);
           // redis.StringSet("ZXTobeListed2", JsonConvert.SerializeObject(_list));
        }
        /// <summary>
        /// 上市表现
        /// </summary>
        private void Shbx()
        {
            List<XgInfo> List = new List<XgInfo>();
            DateTime dtnow = System.DateTime.Now;
            string sql = string.Format(@"select * from  view_xgzx where fxjc='30' and ssrq>(sysdate-60) and XTDM = 'A20310' order by ssrq desc");
            DataTable dt = OracleHelper.ExecuteDataTable(sql, ConnDhzb);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    XgInfo oj = new XgInfo()
                    {
                        dhnm = dr["dhnm"].ToString(),
                        zqdm = dr["zqdm"].ToString(),
                        zqjc = dr["zqjc"].ToString(),
                        sgdm_swfx = dr["sgdm_swfx"].ToString(),
                        ssd = dr["ssd"].ToString().Equals("90") ? "深圳证券交易所" : "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["mgfxj"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["syl"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["fxlsx"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["wsfxjh"], 2) + "万",
                        sgsz = dr["ssd"].ToString().Equals("90") ? "深圳市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万" : "上海市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万",
                        sgsx_swfx = dr["sgsx_swfx"].ToString().Equals("") ? "--" : dr["sgsx_swfx"].ToString(),
                        jys = dr["ssd"].ToString().Equals("90") ? "深" : "沪",
                        bklx = "",
                        zql_wsfx = ConvertHelper.ConvertTo2XS100(dr["zql_wsfx"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ssrq"]),
                        myqhl_xg = dr["myqhl_xg"].ToString(),
                        xtcxcs = dr["xtcxcs"].ToString(),
                        gsjj = "",
                        sgr = ConvertHelper.DateToShotStringNull(dr["sgr"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["fxjgggr"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["paydateonline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["sgr"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };
                    XgInfo xg = new XgInfo()
                    {
                        dhnm = dr["dhnm"].ToString(),
                        zqdm = dr["zqdm"].ToString(),
                        zqjc = dr["zqjc"].ToString(),
                        sgdm_swfx = dr["sgdm_swfx"].ToString(),
                        ssd = dr["ssd"].ToString().Equals("90") ? "深圳证券交易所" : "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["mgfxj"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["syl"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["fxlsx"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["wsfxjh"], 2) + "万",
                        sgsz = dr["ssd"].ToString().Equals("90") ? "深圳市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万" : "上海市值" + ConvertHelper.ConvertToDanWei(dr["sgsx_swfx"], 2) + "万",
                        sgsx_swfx = dr["sgsx_swfx"].ToString().Equals("") ? "--" : dr["sgsx_swfx"].ToString(),
                        jys = dr["ssd"].ToString().Equals("90") ? "深" : "沪",
                        bklx = "",
                        zql_wsfx = ConvertHelper.ConvertTo2XS100(dr["zql_wsfx"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ssrq"]),
                        myqhl_xg = dr["myqhl_xg"].ToString(),
                        xtcxcs = dr["xtcxcs"].ToString(),
                        gsjj = dr["gsjj"].ToString(),
                        sgr = ConvertHelper.DateToShotStringNull(dr["sgr"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["fxjgggr"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["paydateonline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["sgr"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };

                    List.Add(oj);
                    redis.StringSet(XgKeyDetail + xg.zqdm, xg, TimeSpan.FromSeconds(Convert.ToDouble(Config.RedisExpiry)));
                }
            }

            //and a.ListedDate <= dateadd(DAY, 0, GETDATE())
            //科创板
            string sql_kcb = string.Format(@"select a.SecuCode,SecurityAbbr,ApplyCodeOnline ,IssuePrice,PERAfterIssueCutNP,
IssueVol,SharesOnline,ApplyMaxOnline,a.LotRateOnline,a.ListedDate,OnlineStartDate,LotRatePublDate ,PayDateOnline ,
c.BriefIntroText,d.ClosePrice from LC_STIBIPOIssue a
left join SecuMain b on a.SecuCode=b.SecuCode 
left join LC_STIBStockArchives c on b.CompanyCode=c.CompanyCode
left join (select aa.ClosePrice,aa.TradingDay,aa.InnerCode from LC_STIBDailyQuote aa join (
select InnerCode,MAX(TradingDay) TradingDay from  LC_STIBDailyQuote  group by InnerCode) bb 
on aa.InnerCode=bb.InnerCode and aa.TradingDay=bb.TradingDay) d on b.InnerCode=d.InnerCode
where a.ListedDate> dateadd(DAY,-60,GETDATE()) and a.ListedDate <= dateadd(DAY, 0, GETDATE())
order by  a.ListedDate ");
            DataTable dt_kcb = SqlHelper.ExecuteQuery(sql_kcb, ConnJy);
            if (dt_kcb.Rows.Count > 0)
            {
                foreach (DataRow dr in dt_kcb.Rows)
                {
                    decimal spj = dr["ClosePrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["ClosePrice"]);
                    decimal fxj = dr["IssuePrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["IssuePrice"]);

                    string mqhl = ((spj - fxj) * 500).ToString("G0");
                   // Loghelper.Error(this, dr["SecuCode"].ToString() + ":" + mqhl);
                    XgInfo oj = new XgInfo()
                    {
                        dhnm = "",
                        zqdm = dr["SecuCode"].ToString(),
                        zqjc = dr["SecurityAbbr"].ToString(),
                        sgdm_swfx = dr["ApplyCodeOnline"].ToString(),
                        ssd = "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["IssuePrice"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["PERAfterIssueCutNP"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["IssueVol"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["SharesOnline"], 2) + "万",
                        sgsz = "上海市值" + ConvertHelper.ConvertToDanWei(Convert.ToDouble(dr["ApplyMaxOnline"]) * 10, 2) + "万",
                        sgsx_swfx = dr["ApplyMaxOnline"].ToString(),
                        jys = "沪",
                        bklx = "科",
                        zql_wsfx = ConvertHelper.ConvertTo2XS(dr["LotRateOnline"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ListedDate"]),
                        myqhl_xg = mqhl,
                        xtcxcs = "--",
                        gsjj = "",
                        sgr = ConvertHelper.DateToShotStringNull(dr["OnlineStartDate"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["LotRatePublDate"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["PayDateOnline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["OnlineStartDate"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };

                    XgInfo xg = new XgInfo()
                    {
                        dhnm = "",
                        zqdm = dr["SecuCode"].ToString(),
                        zqjc = dr["SecurityAbbr"].ToString(),
                        sgdm_swfx = dr["ApplyCodeOnline"].ToString(),
                        ssd = "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["IssuePrice"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["PERAfterIssueCutNP"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["IssueVol"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["SharesOnline"], 2) + "万",
                        sgsz = "上海市值" + ConvertHelper.ConvertToDanWei(Convert.ToDouble(dr["ApplyMaxOnline"]) * 10, 2) + "万",
                        sgsx_swfx = dr["ApplyMaxOnline"].ToString(),
                        jys = "沪",
                        bklx = "科",
                        zql_wsfx = ConvertHelper.ConvertTo2XS(dr["LotRateOnline"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ListedDate"]),
                        myqhl_xg = mqhl,
                        xtcxcs = "--",
                        gsjj = dr["BriefIntroText"].ToString(),
                        sgr = ConvertHelper.DateToShotStringNull(dr["OnlineStartDate"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["LotRatePublDate"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["PayDateOnline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["OnlineStartDate"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };
                    List.Add(oj);

                    redis.StringSet(XgKeyDetail + xg.zqdm, xg, TimeSpan.FromSeconds(Convert.ToDouble(Config.RedisExpiry)));
                }
            }
            var _list = List.OrderByDescending(p => p.orderbydate).ToList();
            dicJAarry.Add("3", _list);
           // redis.StringSet("ZXListed2", JsonConvert.SerializeObject(_list));
        }

        /// <summary>
        /// 今日新股
        /// </summary>
        private void Kcb_Jrxg()
        {
            JArray jsonArray = new JArray();
            DateTime dtnow = System.DateTime.Now;
            //生产上=0,where DateDiff(dd,a.OnlineStartDate,GETDATE())<=0 and DateDiff(dd,a.OnlineStartDate,GETDATE())>-10
            string sql_kcb = string.Format(@"select a.SecuCode,SecurityAbbr,ApplyCodeOnline ,IssuePrice,PERAfterIssueCutNP,
IssueVol,SharesOnline,ApplyMaxOnline  , a.LotRateOnline ,a.ListedDate,	OnlineStartDate,LotRatePublDate ,PayDateOnline ,
c.BriefIntroText,d.ClosePrice from LC_STIBIPOIssue a
left join SecuMain b on a.SecuCode=b.SecuCode 
left join LC_STIBStockArchives c on b.CompanyCode=c.CompanyCode
left join (select aa.ClosePrice,aa.TradingDay,aa.InnerCode from LC_STIBDailyQuote aa join (
select InnerCode,MAX(TradingDay) TradingDay from  LC_STIBDailyQuote  group by InnerCode) bb 
on aa.InnerCode=bb.InnerCode and aa.TradingDay=bb.TradingDay) d on b.InnerCode=d.InnerCode
where DateDiff(dd,a.OnlineStartDate,GETDATE())=0
order by  a.OnlineStartDate  ");
            DataTable dt_kcb = SqlHelper.ExecuteQuery(sql_kcb, ConnJy);
            if (dt_kcb.Rows.Count > 0)
            {
                foreach (DataRow dr in dt_kcb.Rows)
                {
                    decimal spj = dr["ClosePrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["ClosePrice"]);
                    decimal fxj = dr["IssuePrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["IssuePrice"]);

                    string mqhl = ((spj - fxj) * 500).ToString("G0");

                    XgInfo oj = new XgInfo()
                    {
                        dhnm = "",
                        zqdm = dr["SecuCode"].ToString(),
                        zqjc = dr["SecurityAbbr"].ToString(),
                        sgdm_swfx = dr["ApplyCodeOnline"].ToString(),
                        ssd = "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["IssuePrice"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["PERAfterIssueCutNP"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["IssueVol"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["SharesOnline"], 2) + "万",
                        sgsz = "上海市值" + ConvertHelper.ConvertToDanWei(Convert.ToDouble(dr["ApplyMaxOnline"]) * 10, 2) + "万",
                        sgsx_swfx = dr["ApplyMaxOnline"].ToString(),
                        jys = "沪",
                        bklx = "科",
                        zql_wsfx = ConvertHelper.ConvertTo2XS(dr["LotRateOnline"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ListedDate"]),
                        myqhl_xg = mqhl,
                        xtcxcs = "--",
                        gsjj = "",
                        sgr = ConvertHelper.DateToShotStringNull(dr["OnlineStartDate"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["LotRatePublDate"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["PayDateOnline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["OnlineStartDate"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };

                    XgInfo xg = new XgInfo()
                    {
                        dhnm = "",
                        zqdm = dr["SecuCode"].ToString(),
                        zqjc = dr["SecurityAbbr"].ToString(),
                        sgdm_swfx = dr["ApplyCodeOnline"].ToString(),
                        ssd = "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["IssuePrice"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["PERAfterIssueCutNP"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["IssueVol"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["SharesOnline"], 2) + "万",
                        sgsz = "上海市值" + ConvertHelper.ConvertToDanWei(Convert.ToDouble(dr["ApplyMaxOnline"]) * 10, 2) + "万",
                        sgsx_swfx = dr["ApplyMaxOnline"].ToString(),
                        jys = "沪",
                        bklx = "科",
                        zql_wsfx = ConvertHelper.ConvertTo2XS(dr["LotRateOnline"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ListedDate"]),
                        myqhl_xg = mqhl,
                        xtcxcs = "--",
                        gsjj = dr["BriefIntroText"].ToString(),
                        sgr = ConvertHelper.DateToShotStringNull(dr["OnlineStartDate"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["LotRatePublDate"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["PayDateOnline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["OnlineStartDate"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow)
                    };
                    jsonArray.Add(JToken.FromObject(oj));
                    redis.StringSet(XgKeyDetail + xg.zqdm, xg, TimeSpan.FromSeconds(Convert.ToDouble(Config.RedisExpiry)));
                }

            }

            dicJAarryKcb.Add("1", jsonArray);
        }

        /// <summary>
        /// Ipo状态
        /// </summary>
        private void Kcb_IPO()
        {
            List<JToken> listxw = new List<JToken>();
            JArray jsonArray = new JArray();
            //DateTime dtnow = System.DateTime.Now;

            string sql_kcb = string.Format(@"
select a.InsertTime,a.CSRCIndustryName,a.Sponsor,a.DeclareStateDesc,b.SecuAbbr,b.SecuCode from (
select * from (
SELECT  ROW_NUMBER()   
        over   
        (PARTITION By CompanyCode order by InsertTime desc) as rowId,LC_STIBIPODeclare.*
FROM LC_STIBIPODeclare 
) t
where rowid =1) a left join SecuMain b on a.CompanyCode=b.CompanyCode  where b.ListedSector=7 order by a.InsertTime desc");
            DataTable dt_kcb = SqlHelper.ExecuteQuery(sql_kcb, ConnJy);
            if (dt_kcb.Rows.Count > 0)
            {
                int i = 0;
                foreach (DataRow dr in dt_kcb.Rows)
                {
                    var oj = new
                    {
                        qymc = dr["SecuAbbr"].ToString(),
                        zxzt = dr["DeclareStateDesc"].ToString(),
                        gxsj = ConvertHelper.DateToShotStringNull(dr["InsertTime"]),
                        bjjg = dr["Sponsor"].ToString().Replace("股份有限公司", "").Replace("有限责任公司", "").Replace("有限公司", ""),
                        shhy = dr["CSRCIndustryName"].ToString(),
                        //updateTime = ConvertHelper.DateToShotString(dtnow)
                    };
                    if (i < 5) { jsonArray.Add(JToken.FromObject(oj)); }
                    i++;

                    listxw.Add(JToken.FromObject(oj));
                }

            }

            dicJAarryKcb.Add("2", jsonArray);

            SetRedis(listxw, "ZXXgIPO_more");
        }
        /// <summary>
        /// 首日上市表现
        /// </summary>
        private void Kcb_Srssbx()
        {
            //a.ListedDate> dateadd(DAY,-60,GETDATE()) and
            List<JToken> listxw = new List<JToken>();
            JArray jsonArray = new JArray();
            DateTime dtnow = System.DateTime.Now;

            string sql_kcb = string.Format(@"select a.SecuCode,SecurityAbbr,ApplyCodeOnline ,IssuePrice,PERAfterIssueCutNP,
IssueVol,SharesOnline,ApplyMaxOnline,a.LotRateOnline,a.ListedDate,OnlineStartDate,LotRatePublDate ,PayDateOnline ,
c.BriefIntroText,d.ClosePrice,d.OpenPrice,d.HighPrice,d.TradingDay from LC_STIBIPOIssue a
left join SecuMain b on a.SecuCode=b.SecuCode 
left join LC_STIBStockArchives c on b.CompanyCode=c.CompanyCode
left join LC_STIBDailyQuote d on b.InnerCode=d.InnerCode 
where a.ListedDate <= dateadd(DAY, 0, GETDATE()) and a.ListedDate=d.TradingDay
order by  a.ListedDate desc");
            DataTable dt_kcb = SqlHelper.ExecuteQuery(sql_kcb, ConnJy);
            if (dt_kcb.Rows.Count > 0)
            {
                int i = 0;
                foreach (DataRow dr in dt_kcb.Rows)
                {
                    decimal spj = dr["ClosePrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["ClosePrice"]);
                    decimal fxj = dr["IssuePrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["IssuePrice"]);
                    decimal zgj = dr["HighPrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["HighPrice"]);
                    decimal kpj = dr["OpenPrice"].Equals(DBNull.Value) ? 0 : Convert.ToDecimal(dr["OpenPrice"]);


                    string mqhl_kp = ((kpj - fxj) * 500).ToString("G0");
                    string mqhl_sp = ((spj - fxj) * 500).ToString("G0");
                    string mqhl_zg = ((zgj - fxj) * 500).ToString("G0");
                    var oj = new
                    {
                        dhnm = "",
                        zqdm = dr["SecuCode"].ToString(),
                        zqjc = dr["SecurityAbbr"].ToString(),
                        sgdm_swfx = dr["ApplyCodeOnline"].ToString(),
                        ssd = "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["IssuePrice"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["PERAfterIssueCutNP"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["IssueVol"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["SharesOnline"], 2) + "万",
                        sgsz = "上海市值" + ConvertHelper.ConvertToDanWei(Convert.ToDouble(dr["ApplyMaxOnline"]) * 10, 2) + "万",
                        sgsx_swfx = dr["ApplyMaxOnline"].ToString(),
                        jys = "沪",
                        bklx = "科",
                        zql_wsfx = ConvertHelper.ConvertTo2XS(dr["LotRateOnline"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ListedDate"]),
                        myqhl_xg = "",
                        xtcxcs = "--",
                        gsjj = "",
                        sgr = ConvertHelper.DateToShotStringNull(dr["OnlineStartDate"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["LotRatePublDate"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["PayDateOnline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["OnlineStartDate"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow),
                        mqhl_kpj = mqhl_kp,
                        mqhl_spj = mqhl_sp,
                        mqhl_zgj = mqhl_zg,
                    };
                    if (i < 5) { jsonArray.Add(JToken.FromObject(oj)); }
                    i++;
                    listxw.Add(JToken.FromObject(oj));

                    var xg = new
                    {
                        dhnm = "",
                        zqdm = dr["SecuCode"].ToString(),
                        zqjc = dr["SecurityAbbr"].ToString(),
                        sgdm_swfx = dr["ApplyCodeOnline"].ToString(),
                        ssd = "上海证券交易所",
                        mgfxj = ConvertHelper.ConvertTo2XS_(dr["IssuePrice"]),
                        syl = ConvertHelper.ConvertTo2XS_(dr["PERAfterIssueCutNP"]),
                        fxlsx = ConvertHelper.ConvertToDanWei(dr["IssueVol"], 2) + "万",
                        wsfxjh = ConvertHelper.ConvertToDanWei(dr["SharesOnline"], 2) + "万",
                        sgsz = "上海市值" + ConvertHelper.ConvertToDanWei(Convert.ToDouble(dr["ApplyMaxOnline"]) * 10, 2) + "万",
                        sgsx_swfx = dr["ApplyMaxOnline"].ToString(),
                        jys = "沪",
                        bklx = "科",
                        zql_wsfx = ConvertHelper.ConvertTo2XS(dr["LotRateOnline"]) + "%",
                        ssrq = ConvertHelper.DateToShotStringNull(dr["ListedDate"]),
                        myqhl_xg = "",
                        xtcxcs = "--",
                        gsjj = dr["BriefIntroText"].ToString(),
                        sgr = ConvertHelper.DateToShotStringNull(dr["OnlineStartDate"]),
                        fxjgggr = ConvertHelper.DateToShotStringNull(dr["LotRatePublDate"]),
                        paydateonline = ConvertHelper.DateToShotStringNull(dr["PayDateOnline"]),
                        orderbydate = Convert.ToDateTime(ConvertHelper.DateToShotString(dr["OnlineStartDate"])),
                        updateTime = ConvertHelper.DateToShotString(dtnow),
                        mqhl_kpj = mqhl_kp,
                        mqhl_spj = mqhl_sp,
                        mqhl_zgj = mqhl_zg,
                    };
                    redis.StringSet(XgKeyDetail + xg.zqdm, xg, TimeSpan.FromSeconds(Convert.ToDouble(Config.RedisExpiry)));
                }

            }

            dicJAarryKcb.Add("3", jsonArray);

            SetRedis(listxw, "ZXXgSsbx_more");
        }

        public class XgInfo
        {
            /// <summary>
            /// 东海内码
            /// </summary>
            public string dhnm;
            /// <summary>
            /// 证券代码
            /// </summary>
            public string zqdm;
            /// <summary>
            /// 证券简称
            /// </summary>
            public string zqjc;
            /// <summary>
            /// 申购代码
            /// </summary>
            public string sgdm_swfx;
            /// <summary>
            /// 上市地
            /// </summary>
            public string ssd;
            /// <summary>
            /// 每股发行价
            /// </summary>
            public string mgfxj;
            /// <summary>
            /// 市盈率
            /// </summary>
            public string syl;
            /// <summary>
            /// 总发行量
            /// </summary>
            public string fxlsx;
            /// <summary>
            /// 网上发行量
            /// </summary>
            public string wsfxjh;
            /// <summary>
            /// 申购市值（顶格申购需配市值）
            /// </summary>
            public string sgsz;
            /// <summary>
            /// 上网发行申购上限（网上发行量）
            /// </summary>
            public string sgsx_swfx;
            /// <summary>
            /// 中签率
            /// </summary>
            public string zql_wsfx;
            /// <summary>
            /// 上市日期
            /// </summary>
            public string ssrq;
            /// <summary>
            /// 每一签获利
            /// </summary>
            public string myqhl_xg;
            /// <summary>
            /// 连续涨停天数
            /// </summary>
            public string xtcxcs;
            /// <summary>
            /// 公司简介
            /// </summary>
            public string gsjj;
            /// <summary>
            /// 申购日
            /// </summary>
            public string sgr;
            /// <summary>
            /// 配号时间
            /// </summary>
            public string fxjgggr;
            /// <summary>
            /// 中签时间
            /// </summary>
            public string paydateonline;
            /// <summary>
            /// 排序用
            /// </summary>
            public DateTime orderbydate;
            /// <summary>
            /// 交易所（沪，深）
            /// </summary>
            public string jys;
            /// <summary>
            /// 板块类型（科创板）
            /// </summary>
            public string bklx;
            /// <summary>
            /// redis 更新时间
            /// </summary>
            public string updateTime;

        }


        public void SetRedis<T>(List<T> list, string key)
        {
            bool flag = false;
            int rCount = list.Count;
            if (rCount <= 30)
            {
                string dataKey = "1";
                flag = redis.HashSet(key, dataKey, list);//set集合 追加 dataKey
                if (flag)
                {
                   // Loghelper.Info(this, key + dataKey + "_缓存成功");
                }
                else
                {
                    //Loghelper.Error(this, key + dataKey + "_缓存失败");
                }
            }
            else
            {
                //需要分多少页n+1页
                int page = rCount / 30 + 1;
                //最后一页多少个数据
                int yushu = rCount % 30;
                for (int i = 1; i <= page; i++)
                {
                    //当前页起点index
                    int index = (i - 1) * 30;

                    string dataKey = i.ToString();
                    if (i == page)//最后一页
                    {
                        flag = redis.HashSet(key, dataKey, list.GetRange(index, yushu));
                    }
                    else
                    {
                        flag = redis.HashSet(key, dataKey, list.GetRange(index, 30));//set集合 追加 dataKey
                    }

                    if (flag)
                    {
                        //Loghelper.Info(this, key + dataKey + "_缓存成功");
                    }
                    else
                    {
                       // Loghelper.Error(this, key + dataKey + "_缓存失败");
                    }
                }
            }
        }
    }
}
