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

        // ⚠ ĐÃ VÔ HIỆU HÓA: Không còn seed dữ liệu mẫu trong code
        public static void SeedSampleData() { }
    }
}
