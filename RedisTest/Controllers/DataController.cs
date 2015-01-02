using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RedisTest.Controllers
{
    public class DataController : ApiController
    {
        static readonly ConnectionMultiplexer Redis = ConnectionMultiplexer.Connect("");
        private const string RedisKeyForNode = "data with key {0}";

        public IHttpActionResult Get()
        {
            var redisdb = Redis.GetDatabase();
            var redisValues = redisdb.StringGet(GetAllKeys());
            var result = redisValues.Select(redisValue => redisValue.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<ContentDto>(redisValue));
            return Ok(result);
        }

        public IHttpActionResult Get(int id)
        {
            ContentDto result;
            var redisdb = Redis.GetDatabase();
            if (id >= 10) // Make it easy to delete so I do not have to impliment delete in frontend
            {
                return Delete();
            }
            var resultString = redisdb.StringGet(string.Format(RedisKeyForNode, id));
            if (resultString.IsNullOrEmpty)
            {
                result = GetValueFromSlowDataSource(id);
                resultString = JsonConvert.SerializeObject(result);
                redisdb.StringSet(string.Format(RedisKeyForNode, id), resultString);
            }
            else
            {
                result = JsonConvert.DeserializeObject<ContentDto>(resultString);
            }
            return Ok(result);
        }

        public IHttpActionResult Delete()
        {
            Redis.GetDatabase().KeyDelete(GetAllKeys());
            return Ok("Deleted");
        }

        private static RedisKey[] GetAllKeys()
        {
            const int length = 10;
            var redisKeys = new RedisKey[length];
            var keys = redisKeys;
            for (var i = 0; i < length; i++)
            {
                keys[i] = (string.Format(RedisKeyForNode, i));
            }
            return keys;
        }

        private static ContentDto GetValueFromSlowDataSource(int id)
        {
            Thread.Sleep(1 * 1000);
            return new ContentDto
            {
                ContentId = id,
                CreatedDate = DateTime.UtcNow,
                OwnerPortalId = 23,
                BasedOnId = 34,
                Type = ContentType.Link,
                Translations = new List<TranslationDto>
                {
                    new TranslationDto
                    {
                        CreatedDate = DateTime.UtcNow,
                        Id = id,
                        Name = "Name",
                        PublishState = PublishState.Cc,
                        UpdatedDate = null
                    }
                }
            };
        }
    }

    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class ContentDto
    {
        [JsonProperty]
        public int ContentId { get; internal set; }
        [JsonProperty]
        public DateTime CreatedDate { get; internal set; }
        [JsonProperty]
        public int OwnerPortalId { get; internal set; }
        [JsonProperty]
        public int BasedOnId { get; internal set; }
        [JsonProperty]
        public ICollection<TranslationDto> Translations { get; internal set; }
        [JsonProperty]
        public ContentType Type { get; internal set; }
    }
    public class TranslationDto
    {
        [JsonProperty]
        public int Id { get; internal set; }
        public string Name { get; set; }
        [JsonProperty]
        public DateTime CreatedDate { get; internal set; }
        public DateTime? UpdatedDate { get; set; }
        public PublishState PublishState { get; set; }
    }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    public enum ContentType
    {
        Folder,
        Guide,
        Link
    }

    public enum PublishState
    {
        Aa,
        Bb,
        Cc
    }
}
