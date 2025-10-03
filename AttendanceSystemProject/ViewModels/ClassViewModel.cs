using System.Collections.Generic;

namespace AttendanceSystemProject.ViewModels
{
   
        public class ClassViewModel
        {
            public int ClassId { get; set; }
            public string ClassName { get; set; }
            public string Code { get; set; }
            public int? TeacherId { get; set; }
            public string TeacherName { get; set; }
        }
    }
