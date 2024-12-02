using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HLCommon
{
    public class DateFormatHelper
    {
        /// <summary>
        /// 将Unix时间戳转换为DateTime类型时间
        /// </summary>
        /// <param name="d">double 型数字</param>
        /// <returns>DateTime</returns>
        public static System.DateTime ConvertIntDateTime(double d)
        {
            System.DateTime time = System.DateTime.MinValue;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            time = startTime.AddMilliseconds(d);
            return time;
        }

        /// <summary>
        /// 将c# DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>long</returns>
        public static long ConvertDateTimeInt(System.DateTime time)             //double intResult = 0;
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            //intResult = (time- startTime).TotalMilliseconds;
            long t = (time.Ticks - startTime.Ticks) / 10000;            //除10000调整为13位
            return t;
        }


        public static double DateDiff(DateTime DateTime1, DateTime DateTime2)
        {
//            string dateDiff = null;

//            TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
//            TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
//            TimeSpan ts = ts1.Subtract(ts2).Duration();
//            dateDiff = ts.Days.ToString() + "天"
//+ ts.Hours.ToString() + "小时"
//+ ts.Minutes.ToString() + "分钟"
//+ ts.Seconds.ToString() + "秒";
//            return dateDiff;
          return   (DateTime1 - DateTime2).TotalMinutes;
        }  


        /// <summary>
        /// 将c# DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>long</returns>
        public static string GetFormateDate(System.DateTime? datetime,string format="")             //double intResult = 0;
        {
            if (format != "")
            {
                return datetime.HasValue ? datetime.Value.ToString(format) : string.Empty;
            }
            return datetime.HasValue ? datetime.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
        }

        public static string ConvertToDouble(string item){
          return  ( !string.IsNullOrEmpty(item) && item!="?")? string.Format("{0:F2}", Convert.ToDouble(item)):string.Empty;
        }


        public static double ConvertToDouble2(string item)
        {
            return (!string.IsNullOrEmpty(item) && item != "?") ? Math.Round(Convert.ToDouble(item), 2) : 0;
        }
    }
}
