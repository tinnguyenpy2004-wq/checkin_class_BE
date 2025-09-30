using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace AttendanceSystemProject.Models
{
    public class AttendanceSystemContext : DbContext
    {
        public AttendanceSystemContext() : base(GetConnectionString())
        {
            // Performance: disable proxies/lazy loading by default
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.AutoDetectChangesEnabled = true; // keep true for safety; toggle off in bulk ops
        }

        private static string GetConnectionString()
        {
            // Support either full connection string via env var or fallback to named connection
            var envConnectionString = Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING");
            return string.IsNullOrWhiteSpace(envConnectionString) ? "DefaultConnection" : envConnectionString;
        }

        // Các bảng
        public DbSet<LoginOtp> LoginOtps { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassStudent> ClassStudents { get; set; }
        public DbSet<ClassSession> ClassSessions { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<EventFeedback> EventFeedbacks { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Bỏ s pluralizing convention
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Bảng LoginOtp
            modelBuilder.Entity<LoginOtp>().ToTable("LoginOtps");

            // ⚙ Cấu hình bảng User
            modelBuilder.Entity<User>()
                .ToTable("Users");

            // ⚙ Cấu hình bảng Department (khớp với script DB dùng tên "Departments")
            modelBuilder.Entity<Department>()
                .ToTable("Departments");

            // Quan hệ User - Department (User thuộc 1 Department)
            modelBuilder.Entity<User>()
                .HasOptional(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .WillCascadeOnDelete(false);

            // ⚙ Cấu hình bảng Class
            modelBuilder.Entity<Class>()
                .HasOptional(c => c.Department)
                .WithMany(d => d.Classes)
                .HasForeignKey(c => c.DepartmentId)
                .WillCascadeOnDelete(false);

            // ⚙ Cấu hình ClassStudent
            modelBuilder.Entity<ClassStudent>()
                .HasRequired(cs => cs.Class)
                .WithMany(c => c.ClassStudents)
                .HasForeignKey(cs => cs.ClassId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClassStudent>()
                .HasRequired(cs => cs.Student)
                .WithMany() // ❌ bỏ WithMany(u => u.ClassStudents) vì User không còn property này
                .HasForeignKey(cs => cs.StudentId)
                .WillCascadeOnDelete(false);

            // ⚙ Cấu hình ClassSession
            modelBuilder.Entity<ClassSession>()
                .HasRequired(cs => cs.Class)
                .WithMany(c => c.ClassSessions)
                .HasForeignKey(cs => cs.ClassId)
                .WillCascadeOnDelete(false);

            // ⚙ Cấu hình EventParticipant
            modelBuilder.Entity<EventParticipant>()
                .HasRequired(ep => ep.Event)
                .WithMany(e => e.EventParticipants)
                .HasForeignKey(ep => ep.EventId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EventParticipant>()
                .HasRequired(ep => ep.User)
                .WithMany() // ❌ bỏ WithMany(u => u.EventParticipants)
                .HasForeignKey(ep => ep.UserId)
                .WillCascadeOnDelete(false);

            // ⚙ Cấu hình Attendance
            modelBuilder.Entity<Attendance>()
                .HasRequired(a => a.User)
                .WithMany() // ❌ bỏ WithMany(u => u.Attendances)
                .HasForeignKey(a => a.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Attendance>()
                .HasOptional(a => a.ClassSession)
                .WithMany(cs => cs.Attendances)
                .HasForeignKey(a => a.ClassSessionId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Attendance>()
                .HasOptional(a => a.Event)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EventId)
                .WillCascadeOnDelete(false);

            // ⚙ Cấu hình Certificate
            modelBuilder.Entity<Certificate>()
                .HasRequired(c => c.User)
                .WithMany() // ❌ bỏ WithMany(u => u.Certificates)
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Certificate>()
                .HasRequired(c => c.Event)
                .WithMany(e => e.Certificates)
                .HasForeignKey(c => c.EventId)
                .WillCascadeOnDelete(false);

            // ⚙ Cấu hình EventFeedback
            modelBuilder.Entity<EventFeedback>()
                .HasRequired(ef => ef.User)
                .WithMany() // ❌ bỏ WithMany(u => u.EventFeedbacks)
                .HasForeignKey(ef => ef.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EventFeedback>()
                .HasRequired(ef => ef.Event)
                .WithMany(e => e.EventFeedbacks)
                .HasForeignKey(ef => ef.EventId)
                .WillCascadeOnDelete(false);

            // Note: Unique indices should be created via EF6 Migrations or [Index] attributes.
            // Removed unsupported HasIndex() calls for EF6 compatibility.
            modelBuilder.Entity<AuditLog>()
                .ToTable("AuditLogs");
        }
    }
}
