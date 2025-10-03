using System;
using System.Web.Http;

namespace AttendanceSystemProject.Controllers
{
    [RoutePrefix("api/ping")]
    public class PingController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(new { message = "pong", serverTimeUtc = DateTime.UtcNow });
        }
    }
}
