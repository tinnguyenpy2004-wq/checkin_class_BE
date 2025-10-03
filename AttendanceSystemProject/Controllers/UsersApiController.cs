using System.Linq;
using System.Web.Http;
using AttendanceSystemProject.Models;

namespace AttendanceSystemProject.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersApiController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetUsers()
        {
            using (var db = new AttendanceSystemContext())
            {
                var data = db.Users
                    .OrderBy(u => u.UserId)
                    .Take(50)
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        u.Email,
                        u.FullName,
                        u.Role,
                        u.IsActive,
                        u.DepartmentId
                    })
                    .ToList();
                return Ok(data);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetUser(int id)
        {
            using (var db = new AttendanceSystemContext())
            {
                var u = db.Users
                    .Where(x => x.UserId == id)
                    .Select(x => new
                    {
                        x.UserId,
                        x.Username,
                        x.Email,
                        x.FullName,
                        x.Role,
                        x.IsActive,
                        x.DepartmentId
                    })
                    .FirstOrDefault();
                if (u == null) return NotFound();
                return Ok(u);
            }
        }
    }
}
