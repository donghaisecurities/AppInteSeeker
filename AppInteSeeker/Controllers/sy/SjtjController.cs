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

namespace AppInteSeeker.Controllers.sy
{
    /// <summary>
    /// 数据淘金
    /// </summary>
    [ApiController]
    [Route("api/sy")]
    public class SjtjController : ControllerBase
    {
        public SiteConfig Config;
        public SjtjController(IOptions<SiteConfig> option)
        {
            Config = option.Value;
        }

        public RedisHelper redis;
        public string oracleConnDhzb;

        [HttpGet("sjtj")]
        public IEnumerable<StatusRespondBean> Get()
        {
            StatusRespondBean srb = new StatusRespondBean() { StatusCode = 0, StatusMessage = "sjtj开始执行" };
            try
            {
                redis = new RedisHelper(0, Config.RedisConn);//包含DBNub,port           

                oracleConnDhzb = Config.OracleConnDhzb;
                Sjtj();
                srb.StatusCode = 1;
                srb.StatusMessage = "sjtj完成";
            }
            catch (Exception ex)
            {
                srb.StatusCode = -1;
                srb.StatusMessage = ex.Message;
            }

            yield return srb;
        }

        /// <summary>
        /// 数据淘金
        /// </summary>
        private void Sjtj()
        {
            JArray jsonArray = new JArray();
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string sql = string.Format(@"select x.*,y.tjly,y.syzt,y.sjly from (select distinct(zxbm),bkmc 
                                            from  tcfghz  where mkbm=3 )x 
                                            inner join tggclcs y on x.zxbm=y.xtdm");
            DataTable dt = OracleHelper.ExecuteDataTable(sql, oracleConnDhzb);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    var obj = new
                    {
                        zxbm = dr["zxbm"].ToString(),
                        bkmc = dr["bkmc"].ToString(),
                        tjly = dr["tjly"].ToString(),
                        sjly= dr["sjly"].ToString(),
                        redistime = date
                    };
                    jsonArray.Add(JToken.FromObject(obj));

                    string sqlDetail = string.Format(@"select * from tcfghz where mkbm=3 and zxbm='{0}'", dr["zxbm"].ToString());
                    DataTable dtDetail = OracleHelper.ExecuteDataTable(sqlDetail, oracleConnDhzb);
                    int rowCt = dtDetail.Rows.Count;
                    string cjsj = Convert.ToDateTime(dtDetail.Rows[0]["cjsj"]).ToString("MM-dd HH:ss");


                    JArray jsonArrayList = new JArray();
                    if (dtDetail.Rows.Count > 0)
                    {
                        foreach (DataRow drr in dtDetail.Rows)
                        {
                            var objDe = new
                            {
                                zqdm = drr["cfgdm"].ToString(),
                                zqmc = drr["cfgjc"].ToString(),
                            };
                            jsonArrayList.Add(JToken.FromObject(objDe));
                        }

                    }

                    var objdetail = new
                    {
                        zxbm = dr["zxbm"].ToString(),
                        bkmc = dr["bkmc"].ToString(),
                        sjly = dr["sjly"].ToString(),
                        updatetime = cjsj,
                        tjly = dr["tjly"].ToString(),
                        xgggs = rowCt,
                        xggg = jsonArrayList,
                        redistime = date
                    };
                    redis.StringSet("SYSjtj_" + dr["zxbm"].ToString(), JsonConvert.SerializeObject(objdetail));
                }
            }

            redis.StringSet("SYSjtj", JsonConvert.SerializeObject(jsonArray));
        }
    }
}
