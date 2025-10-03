-- Tạo cơ sở dữ liệu
IF DB_ID('AttendanceSystemDB') IS NULL
BEGIN
    CREATE DATABASE AttendanceSystemDB;
END
GO
USE AttendanceSystemDB;
GO

-- Xóa các bảng nếu tồn tại để tạo lại từ đầu
IF OBJECT_ID('dbo.SystemSettings', 'U') IS NOT NULL DROP TABLE [dbo].[SystemSettings];
IF OBJECT_ID('dbo.EventParticipants', 'U') IS NOT NULL DROP TABLE [dbo].[EventParticipant];
IF OBJECT_ID('dbo.EventFeedbacks', 'U') IS NOT NULL DROP TABLE [dbo].[EventFeedbacks];
IF OBJECT_ID('dbo.Certificates', 'U') IS NOT NULL DROP TABLE [dbo].[Certificates];
IF OBJECT_ID('dbo.Attendances', 'U') IS NOT NULL DROP TABLE [dbo].[Attendances];
IF OBJECT_ID('dbo.ClassStudent', 'U') IS NOT NULL DROP TABLE [dbo].[ClassStudent];
IF OBJECT_ID('dbo.ClassSessions', 'U') IS NOT NULL DROP TABLE [dbo].[ClassSessions];
IF OBJECT_ID('dbo.LoginOtps', 'U') IS NOT NULL DROP TABLE [dbo].[LoginOtps];
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE [dbo].[AuditLogs];
IF OBJECT_ID('dbo.Event', 'U') IS NOT NULL DROP TABLE [dbo].[Event];
IF OBJECT_ID('dbo.Class', 'U') IS NOT NULL DROP TABLE [dbo].[Class];
IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL DROP TABLE [dbo].[Departments];
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE [dbo].[Users];
GO

-- Tạo bảng Users
CREATE TABLE [dbo].[Users](
    [UserId] [int] IDENTITY(1,1) NOT NULL,
    [Username] [nvarchar](50) NOT NULL,
    [FullName] [nvarchar](100) NOT NULL,
    [Email] [nvarchar](100) NOT NULL,
    [PhoneNumber] [nvarchar](20) NULL,
    [StudentId] [nvarchar](20) NULL,
    [Role] [int] NOT NULL,
    [DepartmentId] [int] NULL,
    [FaceImagePath] [nvarchar](500) NULL,
    [IsActive] [bit] NOT NULL,
    [CreatedDate] [datetime] NOT NULL,
    [LastLoginDate] [datetime] NULL,
    [EmailConfirmed] [bit] NOT NULL,
    [PasswordHash] [nvarchar](255) NULL,
    [AccessFailedCount] [int] NOT NULL DEFAULT(0),
    [LockoutEnd] [datetime] NULL,
    [LastPasswordChangeAt] [datetime] NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng Departments
CREATE TABLE [dbo].[Departments](
    [DepartmentId] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [Code] [nvarchar](20) NULL,
    [Description] [nvarchar](500) NULL,
    [IsActive] [bit] NOT NULL,
    [CreatedDate] [datetime] NOT NULL,
    PRIMARY KEY CLUSTERED ([DepartmentId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng Class
CREATE TABLE [dbo].[Class](
    [ClassId] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [Code] [nvarchar](20) NOT NULL,
    [Description] [nvarchar](500) NULL,
    [TeacherId] [int] NOT NULL,
    [DepartmentId] [int] NULL,
    [StartDate] [datetime] NOT NULL,
    [EndDate] [datetime] NOT NULL,
    [IsActive] [bit] NOT NULL,
    [CreatedDate] [datetime] NOT NULL,
    PRIMARY KEY CLUSTERED ([ClassId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng Event
CREATE TABLE [dbo].[Event](
    [EventId] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [Code] [nvarchar](50) NOT NULL,
    [Description] [nvarchar](1000) NULL,
    [OrganizerId] [int] NOT NULL,
    [DepartmentId] [int] NULL,
    [StartDate] [datetime] NOT NULL,
    [EndDate] [datetime] NOT NULL,
    [Location] [nvarchar](200) NULL,
    [MaxParticipants] [int] NOT NULL,
    [Status] [nvarchar](50) NOT NULL,
    [RequiresCertificate] [bit] NOT NULL,
    [CreatedDate] [datetime] NOT NULL,
    [QRCodeData] [nvarchar](500) NULL,
    [QRCodeExpiry] [datetime] NULL,
    PRIMARY KEY CLUSTERED ([EventId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng AuditLogs
CREATE TABLE [dbo].[AuditLogs](
    [AuditLogId] [int] IDENTITY(1,1) NOT NULL,
    [ActorUserId] [int] NULL,
    [TargetUserId] [int] NULL,
    [Action] [nvarchar](50) NOT NULL,
    [IpAddress] [nvarchar](64) NULL,
    [CreatedAt] [datetime] NOT NULL DEFAULT (GETDATE()),
    PRIMARY KEY CLUSTERED ([AuditLogId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng LoginOtps
CREATE TABLE [dbo].[LoginOtps](
    [OtpId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [Purpose] [nvarchar](50) NOT NULL,
    [Code] [nvarchar](128) NOT NULL,
    [ExpiresAt] [datetime] NOT NULL,
    [ConsumedAt] [datetime] NULL,
    [CreatedAt] [datetime] NOT NULL,
    PRIMARY KEY CLUSTERED ([OtpId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng ClassSessions
CREATE TABLE [dbo].[ClassSessions](
    [SessionId] [int] IDENTITY(1,1) NOT NULL,
    [ClassId] [int] NOT NULL,
    [Title] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](500) NULL,
    [StartTime] [datetime] NOT NULL,
    [EndTime] [datetime] NOT NULL,
    [Location] [nvarchar](200) NULL,
    [IsActive] [bit] NOT NULL,
    [CreatedDate] [datetime] NOT NULL,
    [QRCodeData] [nvarchar](500) NULL,
    [QRCodeExpiry] [datetime] NULL,
    PRIMARY KEY CLUSTERED ([SessionId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng ClassStudents
CREATE TABLE [dbo].[ClassStudent](
    [ClassStudentId] [int] IDENTITY(1,1) NOT NULL,
    [ClassId] [int] NOT NULL,
    [StudentId] [int] NOT NULL,
    [JoinDate] [datetime] NOT NULL,
    [IsActive] [bit] NOT NULL,
    PRIMARY KEY CLUSTERED ([ClassStudentId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng Attendances
CREATE TABLE [dbo].[Attendances](
    [AttendanceId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [ClassSessionId] [int] NULL,
    [EventId] [int] NULL,
    [Status] [int] NOT NULL,
    [CheckInTime] [datetime] NOT NULL,
    [CheckOutTime] [datetime] NULL,
    [AttendanceMethod] [nvarchar](200) NULL,
    [Notes] [nvarchar](500) NULL,
    [RecordedDate] [datetime] NOT NULL,
    PRIMARY KEY CLUSTERED ([AttendanceId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng Certificates
CREATE TABLE [dbo].[Certificates](
    [CertificateId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [EventId] [int] NOT NULL,
    [CertificateNumber] [nvarchar](100) NOT NULL,
    [IssueDate] [datetime] NOT NULL,
    [FilePath] [nvarchar](500) NULL,
    [IsSent] [bit] NOT NULL,
    [SentDate] [datetime] NULL,
    PRIMARY KEY CLUSTERED ([CertificateId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng EventFeedbacks
CREATE TABLE [dbo].[EventFeedbacks](
    [FeedbackId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [EventId] [int] NOT NULL,
    [Rating] [int] NOT NULL,
    [Comments] [nvarchar](1000) NULL,
    [SubmittedDate] [datetime] NOT NULL,
    PRIMARY KEY CLUSTERED ([FeedbackId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng EventParticipant
CREATE TABLE [dbo].[EventParticipant](
    [ParticipantId] [int] IDENTITY(1,1) NOT NULL,
    [EventId] [int] NOT NULL,
    [UserId] [int] NOT NULL,
    [RegistrationDate] [datetime] NOT NULL,
    [IsApproved] [bit] NOT NULL,
    PRIMARY KEY CLUSTERED ([ParticipantId] ASC)
) ON [PRIMARY]
GO

-- Tạo bảng SystemSettings
CREATE TABLE [dbo].[SystemSettings](
    [SettingId] [int] IDENTITY(1,1) NOT NULL,
    [SettingKey] [nvarchar](100) NOT NULL,
    [SettingValue] [nvarchar](500) NULL,
    [Description] [nvarchar](200) NULL,
    [LastUpdated] [datetime] NOT NULL,
    PRIMARY KEY CLUSTERED ([SettingId] ASC)
) ON [PRIMARY]
GO

-- Thêm các cột bổ sung cho Users
IF COL_LENGTH('dbo.Users', 'AccessFailedCount') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD AccessFailedCount INT NOT NULL DEFAULT(0);
END
IF COL_LENGTH('dbo.Users', 'LockoutEnd') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD LockoutEnd DATETIME NULL;
END
IF COL_LENGTH('dbo.Users', 'LastPasswordChangeAt') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD LastPasswordChangeAt DATETIME NULL;
END
GO

-- Thay đổi độ dài cột Code trong LoginOtps
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LoginOtps' AND COLUMN_NAME = 'Code' AND CHARACTER_MAXIMUM_LENGTH < 128)
BEGIN
    ALTER TABLE LoginOtps ALTER COLUMN Code NVARCHAR(128) NOT NULL;
END
GO

-- Tạo các chỉ mục
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]([Email] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Username] ON [dbo].[Users]([Username] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_StudentId] ON [dbo].[Users]([StudentId] ASC) WHERE ([StudentId] IS NOT NULL);
CREATE NONCLUSTERED INDEX [IX_Users_DepartmentId] ON [dbo].[Users]([DepartmentId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Departments_Code] ON [dbo].[Departments]([Code] ASC) WHERE ([Code] IS NOT NULL);
CREATE NONCLUSTERED INDEX [IX_Departments_IsActive] ON [dbo].[Departments]([IsActive] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Class_Code] ON [dbo].[Class]([Code] ASC);
CREATE NONCLUSTERED INDEX [IX_Class_DepartmentId] ON [dbo].[Class]([DepartmentId] ASC);
CREATE NONCLUSTERED INDEX [IX_Class_TeacherId] ON [dbo].[Class]([TeacherId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Event_Code] ON [dbo].[Event]([Code] ASC);
CREATE NONCLUSTERED INDEX [IX_Event_DepartmentId] ON [dbo].[Event]([DepartmentId] ASC);
CREATE NONCLUSTERED INDEX [IX_Event_OrganizerId] ON [dbo].[Event]([OrganizerId] ASC);
CREATE NONCLUSTERED INDEX [IX_LoginOtps_User_Purpose_Expires] ON [dbo].[LoginOtps]([UserId] ASC, [Purpose] ASC, [ExpiresAt] ASC);
CREATE NONCLUSTERED INDEX [IX_ClassSessions_ClassId] ON [dbo].[ClassSessions]([ClassId] ASC);
CREATE NONCLUSTERED INDEX [IX_ClassStudents_StudentId] ON [dbo].[ClassStudent]([StudentId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_ClassStudent_Unique] ON [dbo].[ClassStudent]([ClassId] ASC, [StudentId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Attendances_ClassSession_User] ON [dbo].[Attendances]([ClassSessionId] ASC, [UserId] ASC) WHERE ([ClassSessionId] IS NOT NULL);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Attendances_Event_User] ON [dbo].[Attendances]([EventId] ASC, [UserId] ASC) WHERE ([EventId] IS NOT NULL);
CREATE NONCLUSTERED INDEX [IX_Attendances_UserId] ON [dbo].[Attendances]([UserId] ASC);
CREATE NONCLUSTERED INDEX [IX_Certificates_EventId] ON [dbo].[Certificates]([EventId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Certificates_Number] ON [dbo].[Certificates]([CertificateNumber] ASC);
CREATE NONCLUSTERED INDEX [IX_Certificates_UserId] ON [dbo].[Certificates]([UserId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_EventFeedbacks_Unique] ON [dbo].[EventFeedbacks]([EventId] ASC, [UserId] ASC);
CREATE NONCLUSTERED INDEX [IX_EventFeedbacks_UserId] ON [dbo].[EventFeedbacks]([UserId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_EventParticipant_Unique] ON [dbo].[EventParticipant]([EventId] ASC, [UserId] ASC);
CREATE NONCLUSTERED INDEX [IX_EventParticipant_UserId] ON [dbo].[EventParticipant]([UserId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_SystemSettings_Key] ON [dbo].[SystemSettings]([SettingKey] ASC);
GO

-- Thêm ràng buộc mặc định
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [DF_Users_IsActive] DEFAULT ((1)) FOR [IsActive];
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [DF_Users_CreatedDate] DEFAULT (GETDATE()) FOR [CreatedDate];
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [DF_Users_EmailConfirmed] DEFAULT ((0)) FOR [EmailConfirmed];
ALTER TABLE [dbo].[Departments] ADD CONSTRAINT [DF_Departments_IsActive] DEFAULT ((1)) FOR [IsActive];
ALTER TABLE [dbo].[Departments] ADD CONSTRAINT [DF_Departments_CreatedDate] DEFAULT (GETDATE()) FOR [CreatedDate];
ALTER TABLE [dbo].[Class] ADD CONSTRAINT [DF_Class_IsActive] DEFAULT ((1)) FOR [IsActive];
ALTER TABLE [dbo].[Class] ADD CONSTRAINT [DF_Class_CreatedDate] DEFAULT (GETDATE()) FOR [CreatedDate];
ALTER TABLE [dbo].[Event] ADD CONSTRAINT [DF_Event_RequiresCertificate] DEFAULT ((0)) FOR [RequiresCertificate];
ALTER TABLE [dbo].[Event] ADD CONSTRAINT [DF_Event_CreatedDate] DEFAULT (GETDATE()) FOR [CreatedDate];
ALTER TABLE [dbo].[LoginOtps] ADD CONSTRAINT [DF_LoginOtps_CreatedAt] DEFAULT (GETDATE()) FOR [CreatedAt];
ALTER TABLE [dbo].[ClassSessions] ADD CONSTRAINT [DF_ClassSessions_IsActive] DEFAULT ((1)) FOR [IsActive];
ALTER TABLE [dbo].[ClassSessions] ADD CONSTRAINT [DF_ClassSessions_CreatedDate] DEFAULT (GETDATE()) FOR [CreatedDate];
ALTER TABLE [dbo].[ClassStudent] ADD CONSTRAINT [DF_ClassStudent_JoinDate] DEFAULT (GETDATE()) FOR [JoinDate];
ALTER TABLE [dbo].[ClassStudent] ADD CONSTRAINT [DF_ClassStudent_IsActive] DEFAULT ((1)) FOR [IsActive];
ALTER TABLE [dbo].[Attendances] ADD CONSTRAINT [DF_Attendances_RecordedDate] DEFAULT (GETDATE()) FOR [RecordedDate];
ALTER TABLE [dbo].[Certificates] ADD CONSTRAINT [DF_Certificates_IssueDate] DEFAULT (GETDATE()) FOR [IssueDate];
ALTER TABLE [dbo].[Certificates] ADD CONSTRAINT [DF_Certificates_IsSent] DEFAULT ((0)) FOR [IsSent];
ALTER TABLE [dbo].[EventFeedbacks] ADD CONSTRAINT [DF_EventFeedbacks_SubmittedDate] DEFAULT (GETDATE()) FOR [SubmittedDate];
ALTER TABLE [dbo].[EventParticipant] ADD CONSTRAINT [DF_EventParticipant_RegistrationDate] DEFAULT (GETDATE()) FOR [RegistrationDate];
ALTER TABLE [dbo].[EventParticipant] ADD CONSTRAINT [DF_EventParticipant_IsApproved] DEFAULT ((0)) FOR [IsApproved];
ALTER TABLE [dbo].[SystemSettings] ADD CONSTRAINT [DF_SystemSettings_LastUpdated] DEFAULT (GETDATE()) FOR [LastUpdated];
GO

-- Thêm ràng buộc khóa ngoại
ALTER TABLE [dbo].[Users] WITH CHECK ADD CONSTRAINT [FK_Users_Departments] FOREIGN KEY([DepartmentId]) REFERENCES [dbo].[Departments] ([DepartmentId]);
ALTER TABLE [dbo].[Class] WITH CHECK ADD CONSTRAINT [FK_Class_Teacher] FOREIGN KEY([TeacherId]) REFERENCES [dbo].[Users] ([UserId]);
ALTER TABLE [dbo].[Class] WITH CHECK ADD CONSTRAINT [FK_Class_Department] FOREIGN KEY([DepartmentId]) REFERENCES [dbo].[Departments] ([DepartmentId]);
ALTER TABLE [dbo].[Event] WITH CHECK ADD CONSTRAINT [FK_Event_Organizer] FOREIGN KEY([OrganizerId]) REFERENCES [dbo].[Users] ([UserId]);
ALTER TABLE [dbo].[Event] WITH CHECK ADD CONSTRAINT [FK_Event_Department] FOREIGN KEY([DepartmentId]) REFERENCES [dbo].[Departments] ([DepartmentId]);
ALTER TABLE [dbo].[LoginOtps] WITH CHECK ADD CONSTRAINT [FK_LoginOtps_User] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([UserId]);
ALTER TABLE [dbo].[ClassSessions] WITH CHECK ADD CONSTRAINT [FK_ClassSessions_Class] FOREIGN KEY([ClassId]) REFERENCES [dbo].[Class] ([ClassId]);
ALTER TABLE [dbo].[ClassStudent] WITH CHECK ADD CONSTRAINT [FK_ClassStudent_Class] FOREIGN KEY([ClassId]) REFERENCES [dbo].[Class] ([ClassId]);
ALTER TABLE [dbo].[ClassStudent] WITH CHECK ADD CONSTRAINT [FK_ClassStudent_Student] FOREIGN KEY([StudentId]) REFERENCES [dbo].[Users] ([UserId]);
ALTER TABLE [dbo].[Attendances] WITH CHECK ADD CONSTRAINT [FK_Attendances_User] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([UserId]);
ALTER TABLE [dbo].[Attendances] WITH CHECK ADD CONSTRAINT [FK_Attendances_ClassSession] FOREIGN KEY([ClassSessionId]) REFERENCES [dbo].[ClassSessions] ([SessionId]);
ALTER TABLE [dbo].[Attendances] WITH CHECK ADD CONSTRAINT [FK_Attendances_Event] FOREIGN KEY([EventId]) REFERENCES [dbo].[Event] ([EventId]);
ALTER TABLE [dbo].[Certificates] WITH CHECK ADD CONSTRAINT [FK_Certificates_User] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([UserId]);
ALTER TABLE [dbo].[Certificates] WITH CHECK ADD CONSTRAINT [FK_Certificates_Event] FOREIGN KEY([EventId]) REFERENCES [dbo].[Event] ([EventId]);
ALTER TABLE [dbo].[EventFeedbacks] WITH CHECK ADD CONSTRAINT [FK_EventFeedbacks_User] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([UserId]);
ALTER TABLE [dbo].[EventFeedbacks] WITH CHECK ADD CONSTRAINT [FK_EventFeedbacks_Event] FOREIGN KEY([EventId]) REFERENCES [dbo].[Event] ([EventId]);
ALTER TABLE [dbo].[EventParticipant] WITH CHECK ADD CONSTRAINT [FK_EventParticipants_User] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([UserId]);
ALTER TABLE [dbo].[EventParticipant] WITH CHECK ADD CONSTRAINT [FK_EventParticipants_Event] FOREIGN KEY([EventId]) REFERENCES [dbo].[Event] ([EventId]);
GO

-- Dữ liệu mẫu cho bảng Departments
INSERT INTO [dbo].[Departments] (
    [Name], 
    [Code], 
    [Description], 
    [IsActive], 
    [CreatedDate]
)
VALUES
    ('Công nghệ Thông tin', 'CNTT', 'Khoa Công nghệ Thông tin', 1, GETDATE()),
    ('Khoa học Máy tính', 'KHMT', 'Khoa Khoa học Máy tính', 1, GETDATE()),
    ('Quản trị Kinh doanh', 'QTKD', 'Khoa Quản trị Kinh doanh', 1, GETDATE()),
    ('Kỹ thuật Điện', 'KTĐ', 'Khoa Kỹ thuật Điện', 0, GETDATE());
GO

-- Dữ liệu mẫu cho bảng Users (tài khoản đầu tiên là admin)
INSERT INTO [dbo].[Users] (
    [Username], 
    [FullName], 
    [Email], 
    [PhoneNumber], 
    [StudentId], 
    [Role], 
    [DepartmentId], 
    [FaceImagePath], 
    [IsActive], 
    [CreatedDate], 
    [LastLoginDate], 
    [EmailConfirmed], 
    [PasswordHash], 
    [AccessFailedCount], 
    [LockoutEnd], 
    [LastPasswordChangeAt]
)
VALUES
  ('admin', 'Quản trị viên', 'admin@gmail.com', '0123456789', NULL, 1, 1, NULL, 1, GETDATE(), NULL, 1,
     'AQAAAAIAAYagAAAAEKvAnZ4G9ClIglgQfZRcYxzFYlhtQhHbHW4tbFXXeBi5gkOqD+rOykN86nqU39yCmg==', 
     0, NULL, GETDATE()),
    ('teacher1', 'Nguyễn Văn A', 'teacher1@school.edu', '0987654321', NULL, 2, 1, NULL, 1, GETDATE(), NULL, 1,
     'AQAAAAIAAYagAAAAEKvAnZ4G9ClIglgQfZRcYxzFYlhtQhHbHW4tbFXXeBi5gkOqD+rOykN86nqU39yCmg==', 
     0, NULL, GETDATE()),
    ('teacher2', 'Trần Thị B', 'teacher2@school.edu', '0912345678', NULL, 2, 2, NULL, 1, GETDATE(), NULL, 1,
     'AQAAAAIAAYagAAAAEKvAnZ4G9ClIglgQfZRcYxzFYlhtQhHbHW4tbFXXeBi5gkOqD+rOykN86nqU39yCmg==', 
     0, NULL, GETDATE()),
    ('student1', 'Lê Văn C', 'student1@school.edu', '0932145678', 'ST001', 3, 1, NULL, 1, GETDATE(), NULL, 1,
     'AQAAAAIAAYagAAAAEKvAnZ4G9ClIglgQfZRcYxzFYlhtQhHbHW4tbFXXeBi5gkOqD+rOykN86nqU39yCmg==', 
     0, NULL, GETDATE()),
    ('student2', 'Phạm Thị D', 'student2@school.edu', '0941234567', 'ST002', 3, 2, NULL, 1, GETDATE(), NULL, 1,
     'AQAAAAIAAYagAAAAEKvAnZ4G9ClIglgQfZRcYxzFYlhtQhHbHW4tbFXXeBi5gkOqD+rOykN86nqU39yCmg==', 
     0, NULL, GETDATE());
GO

-- Dữ liệu mẫu cho bảng Class
INSERT INTO [dbo].[Class] (
    [Name], 
    [Code], 
    [Description], 
    [TeacherId], 
    [DepartmentId], 
    [StartDate], 
    [EndDate], 
    [IsActive], 
    [CreatedDate]
)
VALUES
    ('Lập trình C# Cơ bản', 'CS101', 'Khóa học giới thiệu về lập trình C#', 2, 1, '2025-10-01 08:00:00', '2025-12-15 17:00:00', 1, GETDATE()),
    ('Cơ sở dữ liệu', 'DB201', 'Khóa học về quản trị cơ sở dữ liệu SQL', 2, 1, '2025-09-15 09:00:00', '2025-11-30 17:00:00', 1, GETDATE()),
    ('Toán rời rạc', 'MATH301', 'Khóa học về toán học rời rạc cho CNTT', 3, 2, '2025-10-10 07:30:00', '2026-01-20 17:00:00', 1, GETDATE()),
    ('Lập trình Web', 'WEB401', 'Khóa học phát triển ứng dụng web', 2, 1, '2025-11-01 10:00:00', '2026-02-15 17:00:00', 1, GETDATE()),
    ('Trí tuệ nhân tạo', 'AI501', 'Khóa học về các khái niệm cơ bản của AI', 3, 1, '2025-09-01 08:30:00', '2025-12-01 17:00:00', 0, GETDATE());
GO

-- Dữ liệu mẫu cho bảng Event
INSERT INTO [dbo].[Event] (
    [Name], 
    [Code], 
    [Description], 
    [OrganizerId], 
    [DepartmentId], 
    [StartDate], 
    [EndDate], 
    [Location], 
    [MaxParticipants], 
    [Status], 
    [RequiresCertificate], 
    [CreatedDate], 
    [QRCodeData], 
    [QRCodeExpiry]
)
VALUES
    ('Hội thảo Công nghệ 2025', 'TECH2025', 'Hội thảo về xu hướng công nghệ mới', 2, 1, '2025-10-15 09:00:00', '2025-10-15 17:00:00', 'Hội trường A', 100, 'Open', 1, GETDATE(), 'qrcode_tech2025', '2025-10-16 00:00:00'),
    ('Workshop AI', 'AIWS2025', 'Workshop thực hành về trí tuệ nhân tạo', 3, 1, '2025-11-01 08:00:00', '2025-11-01 12:00:00', 'Phòng Lab 1', 50, 'Open', 1, GETDATE(), 'qrcode_aiws2025', '2025-11-02 00:00:00'),
    ('Ngày hội Sinh viên', 'SV2025', 'Sự kiện giao lưu sinh viên', 2, 2, '2025-12-01 07:00:00', '2025-12-01 18:00:00', 'Sân trường', 200, 'Open', 0, GETDATE(), NULL, NULL),
    ('Hội thảo Quản trị', 'QTKD2025', 'Hội thảo về quản trị kinh doanh hiện đại', 3, 3, '2025-10-20 09:00:00', '2025-10-20 16:00:00', 'Hội trường B', 80, 'Open', 1, GETDATE(), 'qrcode_qtkd2025', '2025-10-21 00:00:00'),
    ('Hackathon 2025', 'HACK2025', 'Cuộc thi lập trình 24h', 2, 1, '2025-11-15 08:00:00', '2025-11-16 08:00:00', 'Phòng Lab 2', 60, 'Open', 1, GETDATE(), 'qrcode_hack2025', '2025-11-17 00:00:00');
GO