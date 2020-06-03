using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppInteSeeker.Models
{
    public class ConvertHelper
    {
        /// <summary>
        /// 长日期格式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string DateToString(object obj)
        {
            if (!obj.Equals(DBNull.Value))
            {
                if (IsDate(obj.ToString()))
                {
                    return Convert.ToDateTime(obj).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    return obj.ToString();
                }
            }
            else
            {
                return System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

        }
        /// <summary>
        /// 短日期格式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string DateToShotString(object obj)
        {
            if (!obj.Equals(DBNull.Value))
            {
                if (IsDate(obj.ToString()))
                {
                    return Convert.ToDateTime(obj).ToString("yyyy-MM-dd");
                }
                else
                {
                    return obj.ToString();
                }
            }
            else
            {
                return System.DateTime.Now.ToString("yyyy-MM-dd");
            }

        }
        /// <summary>
        /// 短日期格式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>--</returns>
        public static string DateToShotStringNull(object obj)
        {
            if (!obj.Equals(DBNull.Value))
            {
                if (IsDate(obj.ToString()))
                {
                    return Convert.ToDateTime(obj).ToString("yyyy-MM-dd");
                }
                else
                {
                    return obj.ToString();
                }
            }
            else
            {
                return "--";
            }

        }
        public static bool IsDate(string strDate)
        {
            try
            {
                DateTime.Parse(strDate);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 转decimal
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static decimal ConvertToDecimal(object c)
        {
            return c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
        }
        /// <summary>
        /// 求单位
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int GetDanWei(object c, out string dw)
        {
            dw = string.Empty;
            int mark = 1;
            const decimal yi = 100000000;
            const decimal wan = 10000;
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);

            if (num >= yi)//大于等于1亿
            {
                mark = 3;
                dw = "亿";
            }
            else if (num >= wan)//大于等于1万
            {
                mark = 2;
                dw = "万";
            }
            return mark;
        }

        /// <summary>
        /// 按指定单位换算
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ConvertToDanWei(object c, int mark)
        {
            //decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            bool isf = false;
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            string ct = num.ToString();
            const decimal yi = 100000000;
            const decimal wan = 10000;
            if (num < 0)
            {
                isf = true;
                num = Math.Abs(num);
            }
            switch (mark)
            {
                case 1: if (num > 0) { ct = decimal.Round(num, 2).ToString(); } else { num.ToString("G2"); }; break;
                case 2: if ((num / wan) > 0) { ct = Math.Round(num / wan, 2, MidpointRounding.AwayFromZero).ToString(); } else { (num / wan).ToString("G2"); }; break;
                case 3: if ((num / yi) > 0) { ct = Math.Round(num / yi, 2, MidpointRounding.AwayFromZero).ToString(); } else { (num / yi).ToString("G2"); }; break;
                default: break;
            }

            if (isf)
            {
                ct = "-" + ct;
            }

            return ct;
        }


        /// <summary>
        /// 四舍五入,亿,万换算
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string ConvertToString(object c)
        {
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            string ct = num.ToString();
            bool isf = false;
            if (num < 0)
            {
                isf = true;
                num = Math.Abs(num);
            }

            if (num != 0 && num > 0.005m)
            {
                const decimal yi = 100000000;
                const decimal wan = 10000;


                if (num >= yi)//大于等于1亿
                {
                    ct = Math.Round(num / yi, 2, MidpointRounding.AwayFromZero).ToString() + "亿";
                }
                else if (num >= wan)//大于等于1万
                {
                    ct = Math.Round(num / wan, 2, MidpointRounding.AwayFromZero).ToString() + "万";
                }
                else
                {
                    //保留2位小数
                    ct = decimal.Round(num, 2).ToString();
                }

                if (isf)
                {
                    ct = "-" + ct;
                }

                return ct;
            }
            else
            {
                return "0";
            }


        }

        /// <summary>
        /// 百分比四舍五入
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ConvertToBFB(object c)
        {
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            //Convert.ToDouble(dValue).ToString("P")
            if (num != 0 && (num > 0.005m || num < -0.01m))
            {
                decimal d = decimal.Round(decimal.Parse(num.ToString()), 2, MidpointRounding.AwayFromZero);
                return d.ToString();
            }
            else
            {
                return "0";
            }

        }
        /// <summary>
        /// *100 后四舍五入保留2位小数
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ConvertTo2XS100(object c)
        {
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            num = num * 100;
            if (num != 0 && (num > 0.005m || num < -0.01m))
            {
                //Convert.ToDouble(dValue).ToString("P")
                decimal d = decimal.Round(decimal.Parse(num.ToString()), 2, MidpointRounding.AwayFromZero);
                return d.ToString();
            }
            else
            {
                return "0";
            }
        }
        /// <summary>
        /// 四舍五入保留2位小数
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ConvertTo2XS(object c)
        {
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            if (num != 0 && (num > 0.005m || num < -0.01m))
            {
                //Convert.ToDouble(dValue).ToString("P")
                decimal d = decimal.Round(decimal.Parse(num.ToString()), 2, MidpointRounding.AwayFromZero);
                return d.ToString();
            }
            else
            {
                return "0";
            }
        }

        /// <summary>
        /// 四舍五入保留2位小数,返回--
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ConvertTo2XS_(object c)
        {
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            if (num != 0 && (num > 0.005m || num < -0.01m))
            {
                //Convert.ToDouble(dValue).ToString("P")
                decimal d = decimal.Round(decimal.Parse(num.ToString()), 2, MidpointRounding.AwayFromZero);
                return d.ToString();
            }
            else
            {
                return "--";
            }
        }

        /// <summary>
        /// 四舍五入保留2位小数，返回double
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double ConvertTo2XDouble(object c)
        {
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            if (num != 0 && (num > 0.005m || num < -0.01m))
            {
                //Convert.ToDouble(dValue).ToString("P")
               // Loghelper.Error(c, num.ToString());
                decimal d = decimal.Round(decimal.Parse(num.ToString()), 2, MidpointRounding.AwayFromZero);
                return Convert.ToDouble(d.ToString());
            }
            else
            {
                return 0.0;
            }
        }
        /// <summary>
        /// 四舍五入保留4位小数,返回double
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double ConvertTo4XDouble(object c)
        {
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            if (num != 0 && (num > 0.00005m || num < -0.0001m))
            {
                //Convert.ToDouble(dValue).ToString("P")
                decimal d = decimal.Round(decimal.Parse(num.ToString()), 4, MidpointRounding.AwayFromZero);
                return Convert.ToDouble(d.ToString());
            }
            else
            {
                return 0.0;
            }
        }
        /// <summary>
        /// 四舍五入保留4位小数
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ConvertTo4XS(object c)
        {
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            if (num != 0 && (num > 0.00005m || num < -0.0001m))
            {
                //Convert.ToDouble(dValue).ToString("P")
                decimal d = decimal.Round(decimal.Parse(num.ToString()), 4, MidpointRounding.AwayFromZero);
                return d.ToString();
            }
            else
            {
                return "0";
            }
        }
        /// <summary>
        /// 四舍五入保留4位小数
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static decimal ConvertTo4XS_decimal(object c)
        {
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            if (num != 0 && (num > 0.00005m || num < -0.0001m))
            {
                //Convert.ToDouble(dValue).ToString("P")
                decimal d = decimal.Round(decimal.Parse(num.ToString()), 4, MidpointRounding.AwayFromZero);
                return d;
            }
            else
            {
                return 0;
            }
        }


        /// <summary>
        /// 四舍五入保留4位小数,返回--
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ConvertTo4XS_(object c)
        {
            decimal num = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            if (num != 0 && (num > 0.00005m || num < -0.0001m))
            {
                //Convert.ToDouble(dValue).ToString("P")
                decimal d = decimal.Round(decimal.Parse(num.ToString()), 4, MidpointRounding.AwayFromZero);
                return d.ToString();
            }
            else
            {
                return "--";
            }
        }


        /// <summary>
        /// 四舍五入,亿,万换算,返回--
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string ConvertToString_(object c)
        {
            string ct = c.Equals(DBNull.Value) ? "--" : string.IsNullOrEmpty(c.ToString()) ? "--" : c.ToString();
            // string ct = numcc.ToString();

            bool isf = false;
            if (!ct.Equals("--"))
            {
                decimal num = Convert.ToDecimal(ct);
                if (num < 0)
                {
                    isf = true;
                    num = Math.Abs(num);
                }

                if (num != 0 && num > 0.005m)
                {
                    const decimal yi = 100000000;
                    const decimal wan = 10000;


                    if (num >= yi)//大于等于1亿
                    {
                        ct = Math.Round(num / yi, 2, MidpointRounding.AwayFromZero).ToString() + "亿";
                    }
                    else if (num >= wan)//大于等于1万
                    {
                        ct = Math.Round(num / wan, 2, MidpointRounding.AwayFromZero).ToString() + "万";
                    }
                    else
                    {
                        //保留2位小数
                        ct = decimal.Round(num, 2).ToString();
                    }

                    if (isf)
                    {
                        ct = "-" + ct;
                    }

                    return ct;
                }
                else
                {
                    return "0";
                }
            }
            else
            {
                return ct;
            }


        }

        /// <summary>
        /// 增、平、减
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string isUp(object c)
        {
            decimal value = c.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(c.ToString()) ? 0 : Convert.ToDecimal(c);
            string bak = "平";
            // double value = Convert.ToDouble(c);
            if (value > 0)
            {
                bak = "增";
            }
            else if (value < 0)
            {
                bak = "减";
            }
            return bak;
        }
        /// <summary>
        /// 除法
        /// </summary>
        /// <param name="beichushu">被除数</param>
        /// <param name="chushu">除数</param>
        /// <returns></returns>
        public static decimal ChuFa(object beichushu, object chushu)
        {
            decimal bcs = beichushu.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(beichushu.ToString()) ? 0 : Convert.ToDecimal(beichushu);
            decimal cs = chushu.Equals(DBNull.Value) ? 1 : string.IsNullOrEmpty(chushu.ToString()) ? 1 : Convert.ToDecimal(chushu);

            return bcs / cs;
        }

        /// <summary>
        /// 除法,返回--
        /// </summary>
        /// <param name="beichushu">被除数</param>
        /// <param name="chushu">除数</param>
        /// <returns></returns>
        public static string ChuFa_(object beichushu, object chushu)
        {
            string rt = "--";

            decimal bcs = beichushu.Equals(DBNull.Value) ? 0 : string.IsNullOrEmpty(beichushu.ToString()) ? 0 : Convert.ToDecimal(beichushu);
            decimal cs = chushu.Equals(DBNull.Value) ? 1 : string.IsNullOrEmpty(chushu.ToString()) ? 1 : Convert.ToDecimal(chushu);
            if (!chushu.Equals(DBNull.Value))
            {
                if (bcs != 0m && cs != 0m)
                {
                    rt = ConvertTo4XS_(bcs * 100 / cs - 100);
                }
            }
            if (!rt.Equals("--"))
            {
                rt = rt + "%";
            }
            return rt;
        }


        /// <summary>
        /// 将时间戳转换为日期类型，并格式化
        /// </summary>
        /// <param name="longDateTime"></param>
        /// <returns></returns>
        public static string LongDateTimeToDateTimeString(string longDateTime)
        {
            Int64 begtime = Convert.ToInt64(longDateTime) * 10000000;
            DateTime dt_1970 = new DateTime(1970, 1, 1, 8, 0, 0);
            long tricks_1970 = dt_1970.Ticks;//1970年1月1日刻度
            long time_tricks = tricks_1970 + begtime;//日志日期刻度
            DateTime dt = new DateTime(time_tricks);//转化为DateTime
            return dt.ToString("yyyy-MM-dd HH:mm:ss");




            //DateTime startTime = TimeZoneInfo.ConvertTime(new System.DateTime(1970, 1, 1), TimeZoneInfo.Local);
            //DateTime TranslateDate = startTime.AddSeconds(double.Parse(longDateTime));
            //return TranslateDate.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 科学记数法转数字
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public static Decimal ChangeDataToD(string strData)
        {
            Decimal dData = 0.0M;
            if (strData.Contains("E"))
            {
                dData = Convert.ToDecimal(Decimal.Parse(strData.ToString(), System.Globalization.NumberStyles.Float));
            }
            return dData;
        }
    }
}
