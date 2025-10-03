using System.Linq;
using System.Web.Http;
using AttendanceSystemProject.Models;

namespace AttendanceSystemProject.Controllers
{
    [RoutePrefix("api/departments")]
    public class DepartmentsApiController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetDepartments()
        {
            using (var db = new AttendanceSystemContext())
            {
                var data = db.Departments
                    .OrderBy(d => d.DepartmentId)
                    .Select(d => new { d.DepartmentId, d.Name, d.Code, d.IsActive })
                    .ToList();
                return Ok(data);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetDepartment(int id)
        {
            using (var db = new AttendanceSystemContext())
            {
                var d = db.Departments
                    .Where(x => x.DepartmentId == id)
                    .Select(x => new { x.DepartmentId, x.Name, x.Code, x.IsActive })
                    .FirstOrDefault();
                if (d == null) return NotFound();
                return Ok(d);
            }
        }
    }
}
