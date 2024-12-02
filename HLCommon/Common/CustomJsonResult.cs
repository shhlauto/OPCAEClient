using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace HLCommon
{

    /// <summary>
    /// 自定义Json视图
    /// </summary>
    public class CustomJsonResult : JsonResult
    {
        /// <summary>
        /// 格式化字符串
        /// </summary>
        public string FormateStr
        {
            get;
            set;
        }

        /// <summary>
        /// 重写执行视图
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var response = context.HttpContext.Response;

            //response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            response.ContentType = "text/plain";

            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }

         //   if (Data != null)
         //   {
                var jss = new JavaScriptSerializer();
                string jsonString = jss.Serialize(Data);
                const string p = @"\\/Date\((\d+)\)\\/";
                var matchEvaluator = new MatchEvaluator(ConvertJsonDateToDateString);
                var reg = new Regex(p);
                jsonString = reg.Replace(jsonString, matchEvaluator);

                response.Write(jsonString.Replace("nodechecked", "checked"));
          //  }
        }

        /// <summary>  
        /// 将Json序列化的时间由/Date(1294499956278)转为字符串 .
        /// </summary>  
        /// <param name="m">正则匹配</param>
        /// <returns>格式化后的字符串</returns>
        private string ConvertJsonDateToDateString(Match m)
        {
            var dt = new DateTime(1970, 1, 1);
            dt = dt.AddMilliseconds(long.Parse(m.Groups[1].Value));
            dt = dt.ToLocalTime();
            return dt.ToString(FormateStr);
        }
    }

}
