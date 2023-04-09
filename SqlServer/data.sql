CREATE DATABASE StudentManagement
GO

USE StudentManagement
GO

CREATE TABLE Users
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  Username NVARCHAR(MAX),
  Password NVARCHAR(MAX),
  DisplayName NVARCHAR(MAX),
  Email NVARCHAR(MAX),
  IdOTP UNIQUEIDENTIFIER,
  Online BIT DEFAULT 0,
  -- 1: online, 0: offline
  IdUserRole UNIQUEIDENTIFIER NULL,
  IdAvatar UNIQUEIDENTIFIER NULL,
)
GO

CREATE TABLE UserRole
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  Role NVARCHAR(MAX),
)
GO

CREATE TABLE UserRole_UserInfo
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdRole UNIQUEIDENTIFIER NULL,
  InfoName NVARCHAR(MAX) NOT NULL,
  Type INT,
  IsEnable BIT,
  -- 1: có thể chỉnh sửa 0: không thể
  IsDeleted BIT
  --1: bị xoá --0: còn
)
GO

CREATE TABLE User_UserRole_UserInfo
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdUser UNIQUEIDENTIFIER NULL,
  IdUserRole_Info UNIQUEIDENTIFIER NULL,
  Content NVARCHAR(MAX),
)
GO

CREATE TABLE UserRole_UserInfoItem --Cho UserInfo dạng combobox
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdUserRole_Info UNIQUEIDENTIFIER NULL,
  Content NVARCHAR(MAX),
)
GO

CREATE TABLE Student
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdTrainingForm UNIQUEIDENTIFIER NULL,
  IdFaculty UNIQUEIDENTIFIER NULL,
  Status INT DEFAULT 1,
  -- 1: còn học, 0: đã tốt nghiệp
  IdUsers UNIQUEIDENTIFIER
)
GO

CREATE TABLE Teacher
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdFaculty UNIQUEIDENTIFIER NULL,
  IdUsers UNIQUEIDENTIFIER
)
GO

CREATE TABLE Admin
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdUsers UNIQUEIDENTIFIER
)
GO

CREATE TABLE Faculty
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  DisplayName NVARCHAR(MAX),
  IsDeleted BIT DEFAULT 0,
  FoundationDay DateTime,
)
GO

CREATE TABLE TrainingForm
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  DisplayName NVARCHAR(MAX),
  IsDeleted BIT DEFAULT 0,
)
GO

CREATE TABLE Faculty_TrainingForm
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdTrainingForm UNIQUEIDENTIFIER NULL,
  IdFaculty UNIQUEIDENTIFIER NULL,
  IsDeleted BIT DEFAULT 0,
)
GO

CREATE TABLE ComponentScore
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdSubjectClass UNIQUEIDENTIFIER NULL,
  DisplayName NVARCHAR(MAX),
  ContributePercent FLOAT,
)
GO

CREATE TABLE DetailScore
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdStudent UNIQUEIDENTIFIER NULL,
  IdComponentScore UNIQUEIDENTIFIER NULL,
  Score FLOAT,
)
GO

CREATE TABLE Semester
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  Batch NVARCHAR(MAX),
  DisplayName NVARCHAR(MAX),
  CourseRegisterStatus INT DEFAULT 0,
)
GO

CREATE TABLE Class
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdTrainingForm UNIQUEIDENTIFIER NULL,
  IdFaculty UNIQUEIDENTIFIER NULL,
  DisplayName NVARCHAR(MAX),
  IdTeacher UNIQUEIDENTIFIER NULL,
  IsDeleted BIT DEFAULT 0,
  IdThumbnail UNIQUEIDENTIFIER NULL,
)
GO

CREATE TABLE Subject
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  Credit INT,
  DisplayName NVARCHAR(MAX),
  Code NVARCHAR(MAX),
  Describe NVARCHAR(MAX),
  IsDeleted BIT DEFAULT 0,
)
GO

CREATE TABLE SubjectClass
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdSubject UNIQUEIDENTIFIER NULL,
  StartDate DateTime NULL,
  EndDate DateTime NULL,
  IdSemester UNIQUEIDENTIFIER NULL,
  Period NVARCHAR(MAX) NULL,
  WeekDay INT NULL,
  IdThumbnail UNIQUEIDENTIFIER NULL,
  IdTrainingForm UNIQUEIDENTIFIER NULL,
  Code NVARCHAR(MAX) NOT NULL,
  NumberOfStudents INT NOT NULL DEFAULT 0,
  MaxNumberOfStudents INT NULL,
  IsDeleted BIT NOT NULL DEFAULT 0,
)
GO

CREATE TABLE Examination
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdSubjectClass UNIQUEIDENTIFIER ,
  ExamName NVARCHAR(MAX),
  WeekDay NVARCHAR(MAX) NOT NULL,
  Period NVARCHAR(MAX) NOT NULL,

)
GO

CREATE TABLE Teacher_SubjectClass
(
  IdSubjectClass UNIQUEIDENTIFIER ,
  IdTeacher UNIQUEIDENTIFIER ,

  PRIMARY KEY 
    (IdSubjectClass,IdTeacher),
)
GO

CREATE TABLE CourseRegister
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  Status INT DEFAULT 0,
  -- 0:Chưa đăng ký || 1:Đã đăng ký || 2:Đang chờ duyệt
  IdStudent UNIQUEIDENTIFIER NULL,
  IdSubjectClass UNIQUEIDENTIFIER NULL,
)
GO

CREATE TABLE TrainingScore
(
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  Score FLOAT DEFAULT 0,
  IdSemester UNIQUEIDENTIFIER NULL,
  IdStudent UNIQUEIDENTIFIER NULL,
)
GO

CREATE TABLE Document
(
  Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
  DisplayName NVARCHAR(MAX),
  Content NVARCHAR(MAX),
  CreatedAt DateTime,
  IdPoster UNIQUEIDENTIFIER NULL,
  IdFolder UNIQUEIDENTIFIER,
  IdSubjectClass UNIQUEIDENTIFIER NULL,
  Size BIGINT
)
GO

CREATE TABLE Folder
(
  Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
  DisplayName NVARCHAR(MAX),
  CreatedAt DateTime,
  IdSubjectClass UNIQUEIDENTIFIER NULL,
  IdPoster UNIQUEIDENTIFIER DEFAULT NEWID()
)
GO

CREATE TABLE AbsentCalendar
(
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  IdSubjectClass UNIQUEIDENTIFIER NULL,
  Period NVARCHAR(MAX) NULL,
  Date DateTime,
  Type INT,
)
GO

CREATE TABLE DatabaseImageTable
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  Image NVARCHAR(MAX),
)
GO

CREATE TABLE Notification
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  Topic NVARCHAR(MAX),
  Content NVARCHAR(MAX),
  Time DateTime,
  IdNotificationType UNIQUEIDENTIFIER,
  IdPoster UNIQUEIDENTIFIER NULL,
  IdSubjectClass UNIQUEIDENTIFIER,
)
CREATE TABLE NotificationType
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  Content NVARCHAR(MAX),
)
CREATE TABLE NotificationInfo
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdNotification UNIQUEIDENTIFIER NULL,
  IdUserReceiver UNIQUEIDENTIFIER NULL,
  IsRead BIT DEFAULT 0,
)
CREATE TABLE NotificationComment
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdUserComment UNIQUEIDENTIFIER NULL,
  IdNotification UNIQUEIDENTIFIER NULL,
  Content NVARCHAR(MAX),
  Time DateTime,
)
CREATE TABLE NotificationImages
(
  Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  IdNotification UNIQUEIDENTIFIER NULL,
  IdDatabaseImageTable UNIQUEIDENTIFIER NULL,
)

CREATE TABLE OTP
(
	Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	Code NVARCHAR(MAX),
	Time DATETIME DEFAULT GETDATE(),
)


-- foreign key
ALTER TABLE  Users
ADD FOREIGN KEY (IdAvatar) REFERENCES DatabaseImageTable(Id),
FOREIGN KEY (IdUserRole) REFERENCES UserRole(Id),
FOREIGN KEY (IdOTP) REFERENCES OTP(Id)
GO

ALTER TABLE UserRole_UserInfo
ADD FOREIGN KEY (IdRole) REFERENCES UserRole(Id)
GO

ALTER TABLE User_UserRole_UserInfo
ADD FOREIGN KEY (IdUser) REFERENCES Users(Id),
FOREIGN KEY (IdUserRole_Info) REFERENCES UserRole_UserInfo(Id)
GO

ALTER TABLE UserRole_UserInfoItem
ADD FOREIGN KEY (IdUserRole_Info) REFERENCES UserRole_UserInfo(Id)
GO

ALTER TABLE  Student
ADD FOREIGN KEY(IdTrainingForm) REFERENCES TrainingForm(Id),
FOREIGN KEY(IdUsers) REFERENCES Users(Id),
FOREIGN KEY (IdFaculty) REFERENCES Faculty(Id)
GO



ALTER TABLE  Teacher
ADD FOREIGN KEY(IdUsers) REFERENCES Users(Id),
FOREIGN KEY (IdFaculty) REFERENCES Faculty(Id)
GO



ALTER TABLE  Admin
ADD FOREIGN KEY(IdUsers) REFERENCES Users(Id)
GO



ALTER TABLE  Faculty_TrainingForm
ADD FOREIGN KEY(IdTrainingForm) REFERENCES TrainingForm(Id),
 FOREIGN KEY(IdFaculty) REFERENCES Faculty(Id)
 GO


ALTER TABLE  Class
ADD FOREIGN KEY(IdTrainingForm) REFERENCES TrainingForm(Id),
 FOREIGN KEY(IdFaculty) REFERENCES Faculty(Id),
 FOREIGN KEY(IdTeacher) REFERENCES Teacher(Id),
 FOREIGN KEY(IdThumbnail) REFERENCES DatabaseImageTable(Id)
 GO



ALTER TABLE SubjectClass ADD
FOREIGN KEY (IdSubject) REFERENCES Subject(Id),
FOREIGN KEY (IdSemester) REFERENCES Semester(Id),
FOREIGN KEY(IdThumbnail) REFERENCES DatabaseImageTable(Id),
FOREIGN KEY (IdTrainingForm) REFERENCES TrainingForm(Id)
GO



ALTER TABLE Examination ADD
FOREIGN KEY (IdSubjectClass) REFERENCES SubjectClass(Id)
GO



ALTER TABLE Teacher_SubjectClass ADD
FOREIGN KEY (IdSubjectClass) REFERENCES SubjectClass(Id),
FOREIGN KEY (IdTeacher) REFERENCES Teacher(Id)
GO



ALTER TABLE CourseRegister ADD
FOREIGN KEY (IdSubjectClass) REFERENCES SubjectClass(Id),
FOREIGN KEY (IdStudent) REFERENCES Student(Id)
GO



ALTER TABLE TrainingScore ADD
FOREIGN KEY (IdStudent) REFERENCES Student(Id),
FOREIGN KEY (IdSemester) REFERENCES Semester(Id)
GO


ALTER TABLE ComponentScore ADD
FOREIGN KEY (IdSubjectClass) REFERENCES SubjectClass(Id)
GO


ALTER TABLE DetailScore ADD
FOREIGN KEY (IdStudent) REFERENCES Student(Id),
FOREIGN KEY (IdComponentScore) REFERENCES ComponentScore(Id)
GO

ALTER TABLE Document ADD
FOREIGN KEY (IdPoster) REFERENCES Users(Id),
FOREIGN KEY (IdFolder) REFERENCES Folder(Id),
FOREIGN KEY (IdSubjectClass) REFERENCES SubjectClass(Id)
GO


ALTER TABLE Folder ADD
FOREIGN KEY (IdSubjectClass) REFERENCES SubjectClass(Id),
FOREIGN KEY (IdPoster) REFERENCES Users(Id)
GO

ALTER TABLE AbsentCalendar ADD
FOREIGN KEY (IdSubjectClass) REFERENCES SubjectClass(Id)
GO

ALTER TABLE Notification ADD
FOREIGN KEY (IdPoster) REFERENCES Users(Id),
FOREIGN KEY (IdSubjectClass) REFERENCES SubjectClass(Id)
GO

ALTER TABLE Notification ADD
FOREIGN KEY (IdPoster) REFERENCES Users(Id),
FOREIGN KEY (IdSubjectClass) REFERENCES SubjectClass(Id),
FOREIGN KEY (IdNotificationType) REFERENCES NotificationType(Id)
GO

ALTER TABLE NotificationInfo ADD
FOREIGN KEY (IdNotification) REFERENCES Notification(Id),
FOREIGN KEY (IdUserReceiver) REFERENCES Users(Id)
GO

ALTER TABLE NotificationComment ADD
FOREIGN KEY (IdUserComment) REFERENCES Users(Id),
FOREIGN KEY (IdNotification) REFERENCES Notification(Id)
GO


ALTER TABLE NotificationImages ADD
FOREIGN KEY (IdNotification) REFERENCES Notification(Id),
FOREIGN KEY (IdDatabaseImageTable) REFERENCES DatabaseImageTable(Id)
GO

--INSERT

-- Insert Userrole
INSERT INTO dbo.UserRole
  (Role)
VALUES
  (N'Sinh viên'),
  (N'Giáo viên'),
  (N'Admin')
GO

-- Insert notification type
INSERT INTO dbo.NotificationType
  (Content)
VALUES
  (N'Thông báo chung'),
  (N'Thông báo sinh viên'),
  (N'Thông báo giáo viên'),
  (N'Thông báo Admin'),
  (N'Thông báo nghỉ bù'),
  (N'Thông báo bình luận')
GO

-- Insert Database image
INSERT INTO DatabaseImageTable
  (Image)
values
  ( N'https://tuoitre.uit.edu.vn/wp-content/uploads/2015/07/logo-uit.png' )


-- Insert admin: admin/admin
BEGIN
  DECLARE @IdRole UNIQUEIDENTIFIER
  SET @IdRole = (Select id
  from UserRole
  Where Role = 'Admin')

  DECLARE @IdAvatar UNIQUEIDENTIFIER
  SET @IdAvatar = (SELECT TOP 1
    (Id)
  From DatabaseImageTable)

  INSERT INTO dbo.Users
    (Id, username, DisplayName, Email, Password, IdUserRole, IdAvatar)
  VALUES('29DF1714-C81F-42C2-8C64-6D744D787E0C', 'admin', N'Quản Trị Viên', 'admin@gmail.com', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', @IdRole, @IdAvatar)

  INSERT INTO dbo.Admin
    (IdUsers)
  VALUES
    ('29DF1714-C81F-42C2-8C64-6D744D787E0C')
END
GO

-- Trigger
-- O(1) trigger
CREATE TRIGGER UTG_CountNumberOfStudentsInClass
      ON dbo.CourseRegister
      AFTER INSERT, UPDATE, DELETE
    AS
    BEGIN
    DECLARE @NumberOfStudentsInClass INT
    DECLARE @MaxNumberOfStudentsInClass INT
    SET @NumberOfStudentsInClass = (SELECT NumberOfStudents FROM SubjectClass AS a WHERE Id IN (
      SELECT IdSubjectClass FROM DELETED 
      UNION  
      SELECT IdSubjectClass FROM INSERTED 
    ))

    SET @MaxNumberOfStudentsInClass = (SELECT MaxNumberOfStudents FROM SubjectClass WHERE Id IN (
      SELECT IdSubjectClass FROM DELETED 
      UNION  
      SELECT IdSubjectClass FROM INSERTED 
    ))

    IF (@MaxNumberOfStudentsInClass <= @NumberOfStudentsInClass)
    BEGIN 
      RAISERROR  ('SubjectClass is full', 16, 1)
      ROLLBACK TRANSACTION
    END
    ELSE
    BEGIN
      UPDATE b SET NumberOfStudents = 1 + NumberOfStudents FROM dbo.SubjectClass AS b WHERE Id IN (
        SELECT IdSubjectClass FROM INSERTED 
      )
      UPDATE b SET NumberOfStudents = NumberOfStudents - 1 FROM dbo.SubjectClass AS b WHERE Id IN (
        SELECT IdSubjectClass FROM DELETED 
      )
    END
    END
GO
