using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Http;
using Newtonsoft.Json;
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
            ContentDto result;
            var redisdb = Redis.GetDatabase();
            var resultString = redisdb.StringGet(string.Format("data with key {0}", id));
            if (resultString.IsNullOrEmpty)
            {
                result = GetValueFromDataSource(id);
                resultString = JsonConvert.SerializeObject(result);
                redisdb.StringSet(string.Format("data with key {0}", id), resultString);
            }
            else
            {
                result = JsonConvert.DeserializeObject<ContentDto>(resultString);
            }
            return Ok(result);
        }

        private static ContentDto GetValueFromDataSource(int id)
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
