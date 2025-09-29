using System;
using System.Collections.Generic;
using System.Linq;
using AttendanceSystemProject.Models;

namespace AttendanceSystemProject.Services
{
    public class DatabaseTestService
    {
        // ✅ Test kết nối database
        public static bool TestDatabaseConnection()
        {
            try
            {
                using (var context = new AttendanceSystemContext())
                {
                    // Gửi query đơn giản SELECT 1
                    var connectionTest = context.Database.SqlQuery<int>("SELECT 1").SingleOrDefault();
                    return connectionTest == 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Database connection error: " + ex.Message);
                return false;
            }
        }

        // ✅ Seed dữ liệu mẫu
        public static void SeedSampleData()
        {
            try
            {
                using (var context = new AttendanceSystemContext())
                {
                    // 1️⃣ Seed Departments
                    if (!context.Departments.Any())
                    {
                        var departments = new List<Department>
                        {
                            new Department { Name = "Khoa Công nghệ Thông tin", Code = "CNTT", Description = "Khoa đào tạo về công nghệ thông tin", IsActive = true },
                            new Department { Name = "Khoa Kinh tế", Code = "KT", Description = "Khoa đào tạo về kinh tế và quản lý", IsActive = true },
                            new Department { Name = "Khoa Ngoại ngữ", Code = "NN", Description = "Khoa đào tạo về ngoại ngữ", IsActive = true }
                        };

                        context.Departments.AddRange(departments);
                        context.SaveChanges();
                    }

                    // 2️⃣ Seed Users
                    if (!context.Users.Any())
                    {
                        var firstDepartmentId = context.Departments.First().DepartmentId;

                        var users = new List<User>
                        {
                            new User
                            {
                                Username = "admin",
                                FullName = "Nguyễn Văn Admin",
                                Email = "admin@school.edu.vn",
                                PhoneNumber = "0123456789",
                                Role = 0,
                                IsActive = true,
                                DepartmentId = firstDepartmentId,
                                CreatedDate = DateTime.Now,
                                EmailConfirmed = true
                            },
                            new User
                            {
                                Username = "teacher",
                                FullName = "Trần Thị Giảng Viên",
                                Email = "teacher@school.edu.vn",
                                PhoneNumber = "0987654321",
                                Role = 1,
                                IsActive = true,
                                DepartmentId = firstDepartmentId,
                                CreatedDate = DateTime.Now,
                                EmailConfirmed = true
                            }
                        };

                        context.Users.AddRange(users);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Error seeding data: " + ex.Message);
                throw;
            }
        }
    }
}
