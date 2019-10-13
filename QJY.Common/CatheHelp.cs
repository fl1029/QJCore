using System;
using System.Collections;
using Microsoft.Extensions.Caching.Memory;


namespace QJY.Common
{
    /// <summary>
    /// HttpRuntime Cache读取设置缓存信息封装
    /// 使用描述：给缓存赋值使用HttpRuntimeCache.Set(key,value....)
    /// 读取缓存中的值使用JObject jObject=HttpRuntimeCache.Get(key) as JObject，读取到值之后就可以进行一系列判断
    /// </summary>
    public class CacheHelp
    {
       
        static MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        /// 缓存指定对象，设置缓存
        /// </summary>
        public static bool Set(string key, object value)
        {
            if (key != null)
            {
                cache.Set(key, value);
            }
            return true;
        }
        /// <summary>
        /// 创建缓存项过期
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="obj">object对象</param>
        /// <param name="expires">过期时间(秒)</param>
        public static void Set(string key, object value, int expires)
        {
            if (key != null)
            {
                cache.Set(key, value, new MemoryCacheEntryOptions()
                    //设置缓存时间，如果被访问重置缓存时间。设置相对过期时间x秒
                    .SetSlidingExpiration(TimeSpan.FromSeconds(expires)));
            }
        }

        public static object Get(string key)
        {
            object val = null;
            if (key != null && cache.TryGetValue(key, out val))
            {

                return val;
            }
            else
            {
                return default(object);
            }
        }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <typeparam name="T">T对象</typeparam>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            object obj = Get(key);
            return obj == null ? default(T) : (T)obj;
        }


        /// <summary>
        /// 移除缓存项的文件
        /// </summary>
        /// <param name="key">缓存Key</param>
        public static void Remove(string key)
        {
            cache.Remove(key);
        }

    }





}
