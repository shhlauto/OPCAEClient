using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Script.Serialization;

namespace HLCommon
{
    public static class JsonHelper
    {
        static readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        static readonly Type typeOfConvert = typeof(Convert);
        /**/
        /// <summary>  
        /// 返回本对象的Json序列化  
        /// </summary>  
        /// <param name="obj"></param>  
        /// <returns></returns>  
        public static string ToJson(object obj)
        {
            return serializer.Serialize(obj);
        }
        /**/
        /// <summary>  
        /// 返回对象序列化  
        /// </summary>  
        /// <param name="obj">源对象</param>  
        /// <param name="selfName">数据源节点名称</param>  
        /// <param name="others">其它节点对象</param>  
        /// <returns></returns>  
        public static string ToJson(object obj, string selfName, object others)
        {
            serializer.MaxJsonLength = int.MaxValue;
            SortedList<string, object> data = new SortedList<string, object> { { selfName, obj } };
            PropertyInfo[] pis = others.GetType().GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                data.Add(pi.Name, pi.GetValue(others, null));
            }
            return serializer.Serialize(data);
        }


        /**/
        /// <summary>  
        /// 对象序列化  
        /// </summary>  
        /// <param name="obj">源对象</param>  
        /// <param name="selfName">源节点名称</param>  
        /// <returns></returns>  
        public static string ToJson(object obj, string selfName)
        {
            SortedList<string, object> data = new SortedList<string, object> { { selfName, obj } };
            return ToJson(data);
        }

        /**/
        /// <summary>  
        /// 对象序列化  
        /// </summary>  
        /// <param name="obj">源对象</param>  
        /// <param name="recursionDepth">获取或设置用于约束要处理的对象级别的数目的限制,默认值为 100;该属性设置为小于 1 的值时将引发System.ArgumentOutOfRangeException</param>  
        /// <returns></returns>  
        public static string ToJson(object obj, int recursionDepth)
        {
            serializer.RecursionLimit = recursionDepth;
            return serializer.Serialize(obj);
        }

        /// <summary>
        /// 反序列化json字符串为单个对象
        /// </summary>
        /// <typeparam name="T">目标对象类</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns>目标对象实例</returns>
        public static T ToObject<T>(string json) where T : class, new()
        {
            #region

            try
            {
                return serializer.Deserialize<T>(json);
            }
            catch
            {
                Dictionary<string, object> jsonData = serializer.Deserialize<Dictionary<string, object>>(json);

                T target = new T();
                Type type = typeof(T);
                PropertyInfo[] propertyInfos = type.GetProperties();

                foreach (var propertyInfo in propertyInfos)
                {
                    if (jsonData.ContainsKey(propertyInfo.Name))
                    {
                        Type proType = propertyInfo.GetValue(target, null).GetType();
                        MethodInfo methodInfo = typeOfConvert.GetMethod("To" + proType.Name);
                        object param = jsonData[propertyInfo.Name];

                        if (!string.IsNullOrEmpty(param.ToString()))
                        {
                            propertyInfo.SetValue(target, methodInfo.Invoke(null, new[] { param }), null);
                        }
                    }
                }

                return target;
            }

            #endregion
        }

        /// <summary>
        /// 反序列化json字符串为集合
        /// </summary>
        /// <typeparam name="T">目标对象类</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns>目标对象实例</returns>
        public static List<T> ToList<T>(string json) where T : new()
        {
            #region

            try
            {
                return serializer.Deserialize<List<T>>(json);
            }
            catch
            {
                List<Dictionary<string, object>> jsonData = serializer.Deserialize<List<Dictionary<string, object>>>(json);
                List<T> rtnData = new List<T>();

                foreach (Dictionary<string, object> propertyDic in jsonData)
                {
                    T target = new T();
                    Type targetType = typeof(T);
                    PropertyInfo[] propertyInfos = targetType.GetProperties();

                    foreach (var propertyInfo in propertyInfos)
                    {
                        if (propertyDic.ContainsKey(propertyInfo.Name))
                        {
                            Type proType = propertyInfo.GetValue(target, null).GetType();
                            MethodInfo methodInfo = typeOfConvert.GetMethod("To" + proType.Name, new[] { typeof(string) });
                            object param = propertyDic[propertyInfo.Name];

                            if (!string.IsNullOrEmpty(param.ToString()))
                            {
                                propertyInfo.SetValue(target, methodInfo.Invoke(null, new[] { param }), null);
                            }
                        }
                    }

                    rtnData.Add(target);
                }

                return rtnData;
            }

            #endregion
        }

        /// <summary>
        /// 反序列化json字符串为字典
        /// </summary>
        /// <typeparam name="T">目标对象类</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns>目标对象实例</returns>
        public static Dictionary<string, T> ToDictionary<T>(string json) where T : new()
        {
            #region

            try
            {
                return serializer.Deserialize<Dictionary<string, T>>(json);
            }
            catch
            {
                Dictionary<string, object> jsonData = serializer.Deserialize<Dictionary<string, object>>(json);
                Dictionary<string, T> rtnData = new Dictionary<string, T>();

                foreach (KeyValuePair<string, object> jsonPair in jsonData)
                {
                    T target = new T();
                    Type targetType = typeof(T);
                    PropertyInfo[] propertyInfos = targetType.GetProperties();
                    Dictionary<string, object> propertyValues = serializer.Deserialize<Dictionary<string, object>>(jsonPair.Value.ToString());

                    if (propertyValues != null)
                    {
                        foreach (var propertyInfo in propertyInfos)
                        {
                            if (propertyValues.ContainsKey(propertyInfo.Name))
                            {
                                Type proType = propertyInfo.GetValue(target, null).GetType();

                                MethodInfo methodInfo = typeOfConvert.GetMethod("To" + proType.Name, new[] { typeof(string) });
                                object param = propertyValues[propertyInfo.Name];

                                if (!string.IsNullOrEmpty(param.ToString()))
                                {
                                    if (proType == param.GetType())
                                    {
                                        propertyInfo.SetValue(target, param, null);
                                    }
                                    else
                                    {
                                        propertyInfo.SetValue(target, methodInfo.Invoke(null, new[] { param }), null);
                                    }
                                }
                            }
                        }
                    }

                    rtnData.Add(jsonPair.Key, target);
                }

                return rtnData;
            }

            #endregion
        }

        /// <summary>
        /// 反序列化json字符串为嵌套字典
        /// </summary>
        /// <typeparam name="T">目标对象类</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns>目标对象实例</returns>
        public static Dictionary<string, Dictionary<string, T>> ToNestingDictionary<T>(string json) where T : new()
        {
            #region

            try
            {
                return serializer.Deserialize<Dictionary<string, Dictionary<string, T>>>(json);
            }
            catch
            {
                Dictionary<string, object> jsonData = serializer.Deserialize<Dictionary<string, object>>(json);
                Dictionary<string, Dictionary<string, T>> rtnData = new Dictionary<string, Dictionary<string, T>>();

                foreach (KeyValuePair<string, object> jsonPair in jsonData)
                {
                    Dictionary<string, object> tempDictionary = (jsonPair.Value as Dictionary<string, object>) ?? new Dictionary<string, object>();
                    Dictionary<string, T> tempRtnData = new Dictionary<string, T>();

                    foreach (KeyValuePair<string, object> tempPair in tempDictionary)
                    {
                        T target = new T();
                        Type targetType = typeof(T);
                        PropertyInfo[] propertyInfos = targetType.GetProperties();
                        Dictionary<string, object> propertyValues = serializer.Deserialize<Dictionary<string, object>>(tempPair.Value.ToString());

                        if (propertyValues != null)
                        {
                            foreach (var propertyInfo in propertyInfos)
                            {
                                if (propertyValues.ContainsKey(propertyInfo.Name))
                                {
                                    Type proType = propertyInfo.GetValue(target, null).GetType();

                                    MethodInfo methodInfo = typeOfConvert.GetMethod("To" + proType.Name, new[] { typeof(string) });
                                    object param = propertyValues[propertyInfo.Name];

                                    if (!string.IsNullOrEmpty(param.ToString()))
                                    {
                                        if (proType == param.GetType())
                                        {
                                            propertyInfo.SetValue(target, param, null);
                                        }
                                        else
                                        {
                                            propertyInfo.SetValue(target, methodInfo.Invoke(null, new[] { param }), null);
                                        }
                                    }
                                }
                            }
                        }

                        tempRtnData.Add(tempPair.Key, target);
                    }

                    rtnData.Add(jsonPair.Key, tempRtnData);
                }

                return rtnData;
            }

            #endregion
        }
    }
}
