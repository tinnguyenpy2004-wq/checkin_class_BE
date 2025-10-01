using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using AttendanceSystemProject.Models;
using AttendanceSystemProject.Security;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceSystemProject.Services
{
    public class DatabaseService
    {
        private string connectionString;
        private readonly AttendanceSystemContext _db = new AttendanceSystemProject.Models.AttendanceSystemContext();
        private static List<Department> _cachedDepartments;
        private static DateTime _departmentsCacheExpireAt = DateTime.MinValue;

        public DatabaseService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public bool TestConnection()
        {
            try { return _db.Database.Exists(); } catch { return false; }
        }

        public List<Department> GetAllDepartments()
        {
            try
            {
                var now = DateTime.UtcNow;
                if (_cachedDepartments != null && now < _departmentsCacheExpireAt)
                {
                    return _cachedDepartments;
                }

                var data = _db.Departments.AsNoTracking().Where(d => d.IsActive).OrderBy(d => d.Name).ToList();
                _cachedDepartments = data;
                _departmentsCacheExpireAt = now.AddMinutes(5);
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting departments: " + ex.Message);
            }
        }

        public int AddDepartment(Department department)
        {
            try
            {
                department.CreatedDate = DateTime.Now;
                _db.Departments.Add(department);
                _db.SaveChanges();
                // Invalidate cache
                _cachedDepartments = null;
                return department.DepartmentId;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding department: " + ex.Message);
            }
        }

        public int GetDepartmentCount()
        {
            try { return _db.Departments.Count(d => d.IsActive); } catch { return 0; }
        }

        public int GetUserCount()
        {
            try { return _db.Users.Count(u => u.IsActive); } catch { return 0; }
        }
    }
}