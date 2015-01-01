using System.Web.Http;

namespace RedisTest.Controllers
{
    public class DataController : ApiController
    {
        public IHttpActionResult Get(int id)
        {
            return Ok(id);
        }
    }
}
