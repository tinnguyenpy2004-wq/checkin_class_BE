/*
    Attendance System - Code First Initial Schema
    Generated from Entity Framework model on 2025-09-26
    Execute on SQL Server 2016+
*/

IF DB_ID(N'AttendanceSystemDB') IS NULL
    CREATE DATABASE [AttendanceSystemDB];
GO

USE [AttendanceSystemDB];
GO

/* Drop existing objects in dependency order */
IF OBJECT_ID(N'dbo.SystemSettings', 'U') IS NOT NULL DROP TABLE dbo.SystemSettings;
IF OBJECT_ID(N'dbo.EventFeedbacks', 'U') IS NOT NULL DROP TABLE dbo.EventFeedbacks;
IF OBJECT_ID(N'dbo.Certificates', 'U') IS NOT NULL DROP TABLE dbo.Certificates;
IF OBJECT_ID(N'dbo.Attendances', 'U') IS NOT NULL DROP TABLE dbo.Attendances;
IF OBJECT_ID(N'dbo.EventParticipants', 'U') IS NOT NULL DROP TABLE dbo.EventParticipants;
IF OBJECT_ID(N'dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID(N'dbo.ClassSessions', 'U') IS NOT NULL DROP TABLE dbo.ClassSessions;
IF OBJECT_ID(N'dbo.ClassStudents', 'U') IS NOT NULL DROP TABLE dbo.ClassStudents;
IF OBJECT_ID(N'dbo.Classes', 'U') IS NOT NULL DROP TABLE dbo.Classes;
IF OBJECT_ID(N'dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Departments', 'U') IS NOT NULL DROP TABLE dbo.Departments;
GO

/* Departments */
CREATE TABLE dbo.Departments
(
    DepartmentId     INT            IDENTITY(1,1) PRIMARY KEY,
    Name             NVARCHAR(100)  NOT NULL,
    Code             NVARCHAR(20)   NULL,
    Description      NVARCHAR(500)  NULL,
    IsActive         BIT            NOT NULL CONSTRAINT DF_Departments_IsActive DEFAULT (1),
    CreatedDate      DATETIME       NOT NULL CONSTRAINT DF_Departments_CreatedDate DEFAULT (GETDATE())
);
GO
CREATE UNIQUE INDEX IX_Department_Code ON dbo.Departments(Code) WHERE Code IS NOT NULL;
GO

/* Users */
CREATE TABLE dbo.Users
(
    UserId          INT           IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50)  NOT NULL,
    FullName        NVARCHAR(100) NOT NULL,
    Email           NVARCHAR(100) NOT NULL,
    PhoneNumber     NVARCHAR(20)  NULL,
    StudentId       NVARCHAR(20)  NULL,
    Role            INT           NOT NULL,
    DepartmentId    INT           NULL,
    FaceImagePath   NVARCHAR(500) NULL,
    IsActive        BIT           NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedDate     DATETIME      NOT NULL CONSTRAINT DF_Users_CreatedDate DEFAULT (GETDATE()),
    LastLoginDate   DATETIME      NULL
);
GO
ALTER TABLE dbo.Users WITH CHECK
ADD CONSTRAINT FK_Users_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId);
GO
CREATE UNIQUE INDEX IX_User_Username ON dbo.Users(Username);
CREATE UNIQUE INDEX IX_User_Email ON dbo.Users(Email);
CREATE UNIQUE INDEX IX_User_StudentId ON dbo.Users(StudentId) WHERE StudentId IS NOT NULL;
CREATE INDEX IX_User_DepartmentId ON dbo.Users(DepartmentId);
GO

/* Classes */
CREATE TABLE dbo.Classes
(
    ClassId      INT           IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(100) NOT NULL,
    Code         NVARCHAR(20)  NOT NULL,
    Description  NVARCHAR(500) NULL,
    TeacherId    INT           NOT NULL,
    DepartmentId INT           NULL,
    StartDate    DATETIME      NOT NULL,
    EndDate      DATETIME      NOT NULL,
    IsActive     BIT           NOT NULL CONSTRAINT DF_Classes_IsActive DEFAULT (1),
    CreatedDate  DATETIME      NOT NULL CONSTRAINT DF_Classes_CreatedDate DEFAULT (GETDATE())
);
GO
ALTER TABLE dbo.Classes WITH CHECK
ADD CONSTRAINT FK_Classes_Teacher FOREIGN KEY (TeacherId) REFERENCES dbo.Users(UserId);
ALTER TABLE dbo.Classes WITH CHECK
ADD CONSTRAINT FK_Classes_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId);
GO
CREATE UNIQUE INDEX IX_Class_Code ON dbo.Classes(Code);
CREATE INDEX IX_Class_TeacherId ON dbo.Classes(TeacherId);
CREATE INDEX IX_Class_DepartmentId ON dbo.Classes(DepartmentId);
GO

/* ClassStudents */
CREATE TABLE dbo.ClassStudents
(
    ClassStudentId INT      IDENTITY(1,1) PRIMARY KEY,
    ClassId        INT      NOT NULL,
    StudentId      INT      NOT NULL,
    JoinDate       DATETIME NOT NULL CONSTRAINT DF_ClassStudents_JoinDate DEFAULT (GETDATE()),
    IsActive       BIT      NOT NULL CONSTRAINT DF_ClassStudents_IsActive DEFAULT (1)
);
GO
ALTER TABLE dbo.ClassStudents WITH CHECK
ADD CONSTRAINT FK_ClassStudents_Class FOREIGN KEY (ClassId) REFERENCES dbo.Classes(ClassId);
ALTER TABLE dbo.ClassStudents WITH CHECK
ADD CONSTRAINT FK_ClassStudents_Student FOREIGN KEY (StudentId) REFERENCES dbo.Users(UserId);
GO
CREATE UNIQUE INDEX IX_ClassStudent_Unique ON dbo.ClassStudents(ClassId, StudentId);
CREATE INDEX IX_ClassStudent_StudentId ON dbo.ClassStudents(StudentId);
GO

/* ClassSessions */
CREATE TABLE dbo.ClassSessions
(
    SessionId    INT           IDENTITY(1,1) PRIMARY KEY,
    ClassId      INT           NOT NULL,
    Title        NVARCHAR(100) NOT NULL,
    Description  NVARCHAR(500) NULL,
    StartTime    DATETIME      NOT NULL,
    EndTime      DATETIME      NOT NULL,
    Location     NVARCHAR(200) NULL,
    IsActive     BIT           NOT NULL CONSTRAINT DF_ClassSessions_IsActive DEFAULT (1),
    CreatedDate  DATETIME      NOT NULL CONSTRAINT DF_ClassSessions_CreatedDate DEFAULT (GETDATE()),
    QRCodeData   NVARCHAR(500) NULL,
    QRCodeExpiry DATETIME      NULL
);
GO
ALTER TABLE dbo.ClassSessions WITH CHECK
ADD CONSTRAINT FK_ClassSessions_Class FOREIGN KEY (ClassId) REFERENCES dbo.Classes(ClassId);
GO
CREATE INDEX IX_ClassSessions_ClassId ON dbo.ClassSessions(ClassId);
GO

/* Events */
CREATE TABLE dbo.Events
(
    EventId            INT           IDENTITY(1,1) PRIMARY KEY,
    Name               NVARCHAR(100) NOT NULL,
    Code               NVARCHAR(20)  NOT NULL,
    Description        NVARCHAR(1000) NULL,
    OrganizerId        INT           NOT NULL,
    DepartmentId       INT           NULL,
    StartDate          DATETIME      NOT NULL,
    EndDate            DATETIME      NOT NULL,
    Location           NVARCHAR(200) NULL,
    MaxParticipants    INT           NOT NULL DEFAULT (0),
    Status             INT           NOT NULL,
    RequiresCertificate BIT          NOT NULL CONSTRAINT DF_Events_RequiresCertificate DEFAULT (0),
    CreatedDate        DATETIME      NOT NULL CONSTRAINT DF_Events_CreatedDate DEFAULT (GETDATE()),
    QRCodeData         NVARCHAR(500) NULL,
    QRCodeExpiry       DATETIME      NULL
);
GO
ALTER TABLE dbo.Events WITH CHECK
ADD CONSTRAINT FK_Events_Organizer FOREIGN KEY (OrganizerId) REFERENCES dbo.Users(UserId);
ALTER TABLE dbo.Events WITH CHECK
ADD CONSTRAINT FK_Events_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId);
GO
CREATE UNIQUE INDEX IX_Event_Code ON dbo.Events(Code);
CREATE INDEX IX_Event_OrganizerId ON dbo.Events(OrganizerId);
CREATE INDEX IX_Event_DepartmentId ON dbo.Events(DepartmentId);
GO

/* EventParticipants */
CREATE TABLE dbo.EventParticipants
(
    ParticipantId   INT      IDENTITY(1,1) PRIMARY KEY,
    EventId         INT      NOT NULL,
    UserId          INT      NOT NULL,
    RegistrationDate DATETIME NOT NULL CONSTRAINT DF_EventParticipants_RegistrationDate DEFAULT (GETDATE()),
    IsApproved      BIT      NOT NULL CONSTRAINT DF_EventParticipants_IsApproved DEFAULT (0)
);
GO
ALTER TABLE dbo.EventParticipants WITH CHECK
ADD CONSTRAINT FK_EventParticipants_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId);
ALTER TABLE dbo.EventParticipants WITH CHECK
ADD CONSTRAINT FK_EventParticipants_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);
GO
CREATE UNIQUE INDEX IX_EventParticipant_Unique ON dbo.EventParticipants(EventId, UserId);
CREATE INDEX IX_EventParticipant_UserId ON dbo.EventParticipants(UserId);
GO

/* Attendances */
CREATE TABLE dbo.Attendances
(
    AttendanceId     INT           IDENTITY(1,1) PRIMARY KEY,
    UserId           INT           NOT NULL,
    ClassSessionId   INT           NULL,
    EventId          INT           NULL,
    Status           INT           NOT NULL,
    CheckInTime      DATETIME      NOT NULL,
    CheckOutTime     DATETIME      NULL,
    AttendanceMethod NVARCHAR(200) NULL,
    Notes            NVARCHAR(500) NULL,
    RecordedDate     DATETIME      NOT NULL CONSTRAINT DF_Attendances_RecordedDate DEFAULT (GETDATE())
);
GO
ALTER TABLE dbo.Attendances WITH CHECK
ADD CONSTRAINT FK_Attendances_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);
ALTER TABLE dbo.Attendances WITH CHECK
ADD CONSTRAINT FK_Attendances_ClassSession FOREIGN KEY (ClassSessionId) REFERENCES dbo.ClassSessions(SessionId);
ALTER TABLE dbo.Attendances WITH CHECK
ADD CONSTRAINT FK_Attendances_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId);
GO
CREATE UNIQUE INDEX IX_Attendance_ClassSession_User ON dbo.Attendances(ClassSessionId, UserId) WHERE ClassSessionId IS NOT NULL;
CREATE UNIQUE INDEX IX_Attendance_Event_User ON dbo.Attendances(EventId, UserId) WHERE EventId IS NOT NULL;
CREATE INDEX IX_Attendance_UserId ON dbo.Attendances(UserId);
GO

/* Certificates */
CREATE TABLE dbo.Certificates
(
    CertificateId     INT           IDENTITY(1,1) PRIMARY KEY,
    UserId            INT           NOT NULL,
    EventId           INT           NOT NULL,
    CertificateNumber NVARCHAR(100) NOT NULL,
    IssueDate         DATETIME      NOT NULL CONSTRAINT DF_Certificates_IssueDate DEFAULT (GETDATE()),
    FilePath          NVARCHAR(500) NULL,
    IsSent            BIT           NOT NULL CONSTRAINT DF_Certificates_IsSent DEFAULT (0),
    SentDate          DATETIME      NULL
);
GO
ALTER TABLE dbo.Certificates WITH CHECK
ADD CONSTRAINT FK_Certificates_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);
ALTER TABLE dbo.Certificates WITH CHECK
ADD CONSTRAINT FK_Certificates_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId);
GO
CREATE UNIQUE INDEX IX_Certificate_Number ON dbo.Certificates(CertificateNumber);
CREATE INDEX IX_Certificate_UserId ON dbo.Certificates(UserId);
CREATE INDEX IX_Certificate_EventId ON dbo.Certificates(EventId);
GO

/* EventFeedbacks */
CREATE TABLE dbo.EventFeedbacks
(
    FeedbackId   INT           IDENTITY(1,1) PRIMARY KEY,
    UserId       INT           NOT NULL,
    EventId      INT           NOT NULL,
    Rating       INT           NOT NULL,
    Comments     NVARCHAR(1000) NULL,
    SubmittedDate DATETIME     NOT NULL CONSTRAINT DF_EventFeedbacks_SubmittedDate DEFAULT (GETDATE())
);
GO
ALTER TABLE dbo.EventFeedbacks WITH CHECK
ADD CONSTRAINT FK_EventFeedbacks_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);
ALTER TABLE dbo.EventFeedbacks WITH CHECK
ADD CONSTRAINT FK_EventFeedbacks_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId);
GO
CREATE UNIQUE INDEX IX_EventFeedback_Unique ON dbo.EventFeedbacks(EventId, UserId);
CREATE INDEX IX_EventFeedback_UserId ON dbo.EventFeedbacks(UserId);
GO

/* SystemSettings */
CREATE TABLE dbo.SystemSettings
(
    SettingId    INT           IDENTITY(1,1) PRIMARY KEY,
    SettingKey   NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(500) NULL,
    Description  NVARCHAR(200) NULL,
    LastUpdated  DATETIME      NOT NULL CONSTRAINT DF_SystemSettings_LastUpdated DEFAULT (GETDATE())
);
GO
CREATE UNIQUE INDEX IX_SystemSetting_Key ON dbo.SystemSettings(SettingKey);
GO

PRINT 'AttendanceSystem schema created successfully.';
