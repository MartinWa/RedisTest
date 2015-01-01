using System;
using System.Threading;
using System.Web.Http;
using StackExchange.Redis;

namespace RedisTest.Controllers
{
    public class DataController : ApiController
    {
        static readonly ConnectionMultiplexer Redis = ConnectionMultiplexer.Connect("");

        public IHttpActionResult Get()
        {
            var redisdb = Redis.GetDatabase();
            for (var i = 0; i < 10; i++)
            {
                redisdb.KeyDelete(string.Format("data with key {0}", i));

            }
            return Ok();
        }


        public IHttpActionResult Get(int id)
        {
            if (id == 0)
            {
                return Get();
            }
            string result;
            var redisdb = Redis.GetDatabase();
            var data = redisdb.StringGet(string.Format("data with key {0}", id));
            if (data.IsNullOrEmpty)
            {
                result = GetValueFromDataSource();
                redisdb.StringSet(string.Format("data with key {0}", id), result, TimeSpan.FromSeconds(20));
            }
            else
            {
                result = data;
            }
            return Ok(result);
        }

        private static string GetValueFromDataSource()
        {
            Thread.Sleep(1 * 1000);
            return "Everything is Awesome!";
        }
    }
}
