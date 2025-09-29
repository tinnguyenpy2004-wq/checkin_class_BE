using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using AttendanceSystemProject.Models;

namespace AttendanceSystemProject.Services
{
    public class DatabaseService
    {
        private string connectionString;

        public DatabaseService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<Department> GetAllDepartments()
        {
            List<Department> departments = new List<Department>();
            
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT DepartmentId, Name, Code, Description, IsActive, CreatedDate FROM Departments WHERE IsActive = 1";
                    
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                departments.Add(new Department
                                {
                                    DepartmentId = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Code = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    IsActive = reader.GetBoolean(4),
                                    CreatedDate = reader.GetDateTime(5)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting departments: " + ex.Message);
            }
            
            return departments;
        }

        public int AddDepartment(Department department)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"INSERT INTO Departments (Name, Code, Description, IsActive, CreatedDate) 
                                 VALUES (@Name, @Code, @Description, @IsActive, @CreatedDate);
                                 SELECT SCOPE_IDENTITY();";
                    
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", department.Name);
                        command.Parameters.AddWithValue("@Code", (object)department.Code ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Description", (object)department.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IsActive", department.IsActive);
                        command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding department: " + ex.Message);
            }
        }

        public int GetDepartmentCount()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT COUNT(*) FROM Departments WHERE IsActive = 1";
                    
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        return (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int GetUserCount()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT COUNT(*) FROM Users WHERE IsActive = 1";
                    
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        return (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public void SeedSampleData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if departments exist
                    string checkDeptSql = "SELECT COUNT(*) FROM Departments";
                    using (SqlCommand command = new SqlCommand(checkDeptSql, connection))
                    {
                        int count = (int)command.ExecuteScalar();
                        if (count == 0)
                        {
                            // Insert sample departments
                            string insertDeptSql = @"
                                INSERT INTO Departments (Name, Code, Description, IsActive, CreatedDate) VALUES
                                (N'Khoa Công nghệ Thông tin', 'CNTT', N'Khoa đào tạo về công nghệ thông tin', 1, GETDATE()),
                                (N'Khoa Kinh tế', 'KT', N'Khoa đào tạo về kinh tế và quản lý', 1, GETDATE()),
                                (N'Khoa Ngoại ngữ', 'NN', N'Khoa đào tạo về ngoại ngữ', 1, GETDATE())";
                            
                            using (SqlCommand insertCommand = new SqlCommand(insertDeptSql, connection))
                            {
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }

                    // Check if users exist
                    string checkUserSql = "SELECT COUNT(*) FROM Users";
                    using (SqlCommand command = new SqlCommand(checkUserSql, connection))
                    {
                        int count = (int)command.ExecuteScalar();
                        if (count == 0)
                        {
                            // Get first department ID
                            string getDeptIdSql = "SELECT TOP 1 DepartmentId FROM Departments";
                            int deptId;
                            using (SqlCommand getDeptCommand = new SqlCommand(getDeptIdSql, connection))
                            {
                                deptId = (int)getDeptCommand.ExecuteScalar();
                            }

                            // Insert sample users
                            string insertUserSql = @"
                                INSERT INTO Users (FirstName, LastName, Email, Phone, Role, IsActive, CreatedDate, DepartmentId) VALUES
                                (N'Nguyễn', N'Văn Admin', 'admin@school.edu.vn', '0123456789', 'Admin', 1, GETDATE(), @DeptId),
                                (N'Trần', N'Thị Giảng viên', 'teacher@school.edu.vn', '0987654321', 'Teacher', 1, GETDATE(), @DeptId)";
                            
                            using (SqlCommand insertCommand = new SqlCommand(insertUserSql, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@DeptId", deptId);
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error seeding data: " + ex.Message);
            }
        }
    }
}