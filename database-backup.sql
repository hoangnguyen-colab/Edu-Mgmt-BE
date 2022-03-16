﻿--USE master
--ALTER DATABASE EduManagement set single_user WITH ROLLBACK IMMEDIATE
--DROP DATABASE EduManagement

CREATE DATABASE EduManagement
 GO
USE EduManagement
 GO

CREATE TABLE Student
(
	StudentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	StudentPhone NVARCHAR(20),
	StudentName NVARCHAR(255),
	StudentGender NVARCHAR(6),
	StudentDOB NVARCHAR(15),
	StudentImage NVARCHAR(255),

	CreatedDate DATETIME DEFAULT GETDATE(),
	ModifyDate DATETIME DEFAULT GETDATE(),
	CreatedUser DATETIME DEFAULT GETDATE(),
	ModifyUser DATETIME DEFAULT GETDATE(),
)


CREATE TABLE Teacher
(
	TeacherId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	TeacherName NVARCHAR(255),
	TeacherPhone NVARCHAR(20) NOT NULL,
	TeacherGender NVARCHAR(6),
	TeacherDOB NVARCHAR(15),
	TeacherImage NVARCHAR(255),
	TeacherEmail NVARCHAR(255),

	CreatedDate DATETIME DEFAULT GETDATE(),
	ModifyDate DATETIME DEFAULT GETDATE(),
	CreatedUser DATETIME DEFAULT GETDATE(),
	ModifyUser DATETIME DEFAULT GETDATE(),
)

CREATE TABLE Class
(
	ClassId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	ClassName NVARCHAR(255),
	ClassYear NVARCHAR(15),

	CreatedDate DATETIME DEFAULT GETDATE(),
	ModifyDate DATETIME DEFAULT GETDATE(),
	CreatedUser DATETIME DEFAULT GETDATE(),
	ModifyUser DATETIME DEFAULT GETDATE(),

	TeacherId UNIQUEIDENTIFIER REFERENCES Teacher(TeacherId) ON DELETE CASCADE NOT NULL,
)

CREATE TABLE ClassDetail
(
	ClassDetailId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	StudentId UNIQUEIDENTIFIER NOT NULL,
	
	ClassId UNIQUEIDENTIFIER REFERENCES Class(ClassId) ON DELETE CASCADE NOT NULL ,
)


CREATE TABLE SystemUser
(
	SystemUserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	Username NVARCHAR(255) NOT NULL,
	UserUsername NVARCHAR(255) UNIQUE NOT NULL,
	UserPassword NVARCHAR(255) NOT NULL,

	CreatedDate DATETIME DEFAULT GETDATE(),
	ModifyDate DATETIME DEFAULT GETDATE(),
	CreatedUser DATETIME DEFAULT GETDATE(),
	ModifyUser DATETIME DEFAULT GETDATE(),
)

CREATE TABLE SystemRole
(
	RoleId INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	RoleName NVARCHAR(255),
)


CREATE TABLE UserDetail
(
	UserDetailId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	UserId UNIQUEIDENTIFIER DEFAULT NULL,
	-- can be teacherid, studentid or null

	SystemUserId UNIQUEIDENTIFIER NOT NULL,
	SystemRoleId INT NOT NULL,
)
GO

CREATE TABLE SchoolSubject
(
	SubjectId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	SubjectName NVARCHAR(255),
	SubjectDescription NVARCHAR(MAX),

	CreatedDate DATETIME DEFAULT GETDATE(),
	ModifyDate DATETIME DEFAULT GETDATE(),
	CreatedUser DATETIME DEFAULT GETDATE(),
	ModifyUser DATETIME DEFAULT GETDATE(),
)

INSERT INTO SystemRole
VALUES
	(N'admin'),
	(N'teacher'),
	(N'student')

GO

INSERT INTO SystemUser
VALUES
	(N'DD114F18-0493-4E35-9BBC-663B8C5E6C61', N'Teacher Test', N'0979045160', N'e10adc3949ba59abbe56e057f20f883e', DEFAULT, DEFAULT, DEFAULT, DEFAULT), 
	(N'8A1B746B-C6F9-4627-B803-271ADB35C289', N'Admin', N'hoag-admin', N'21232f297a57a5a743894a0e4a801fc3', DEFAULT, DEFAULT, DEFAULT, DEFAULT),
	(N'A2E48929-2688-4F4F-8EE2-291D67568888', N'Teacher Hoag', N'09123123123', N'e10adc3949ba59abbe56e057f20f883e', DEFAULT, DEFAULT, DEFAULT, DEFAULT)
 GO

 INSERT INTO SystemUser
VALUES
 GO

INSERT INTO Teacher
VALUES
	(N'88CA6417-6B71-48B2-9FD9-47C57F384947', 
	N'Hoang',
	N'0979045160',
	N'Nam',
	N'23/07/1999',
	NULL, 
	NULL, 
	DEFAULT, 
	DEFAULT, 
	DEFAULT, 
	DEFAULT),
	(N'D60C8577-585E-44CC-8AD0-70B052875FA8', 
	N'Teacher Hoag',
	N'09123123123',
	N'Nam',
	N'23/07/1999',
	NULL, 
	NULL, 
	DEFAULT, 
	DEFAULT, 
	DEFAULT, 
	DEFAULT)
 GO
 
 select * from Teacher
 select * from UserDetail
 select * from SystemUser

 INSERT INTO UserDetail
 VALUES
 	(DEFAULT, NULL, N'8A1B746B-C6F9-4627-B803-271ADB35C289', 1),
 	(DEFAULT, N'88CA6417-6B71-48B2-9FD9-47C57F384947', N'DD114F18-0493-4E35-9BBC-663B8C5E6C61', 2),
 	(DEFAULT, N'D60C8577-585E-44CC-8AD0-70B052875FA8', N'A2E48929-2688-4F4F-8EE2-291D67568888', 2)
 GO

INSERT INTO Class
VALUES
	(N'72E24AAA-F3EF-49B5-8C2E-2C869B688C42', N'1A1', N'2022-2023', DEFAULT, DEFAULT, DEFAULT, DEFAULT, N'88CA6417-6B71-48B2-9FD9-47C57F384947'),
	(N'03F47CCC-24B4-484F-B08F-370B3DCBE2BA', N'1A2', N'2022-2023', DEFAULT, DEFAULT, DEFAULT, DEFAULT, N'88CA6417-6B71-48B2-9FD9-47C57F384947')
GO

 select * from UserDetail 
 join SystemUser
 on UserDetail.SystemUserId = SystemUser.SystemUserId
 join SystemRole
 on UserDetail.SystemRoleId = SystemRole.RoleId
 where SystemRole.RoleId = 1

 select * from SystemRole where RoleId in 
 (select distinct SystemRoleId from UserDetail where SystemUserId = N'A5044EF6-062D-42CD-978A-AC4D3E49D206')

SELECT DISTINCT Class.* 
FROM Class 
WHERE Class.TeacherId = N'88CA6417-6B71-48B2-9FD9-47C57F384947'
AND CHARINDEX(N'2', ClassName) > 0

select * from Teacher
 
 select Student.* from ClassDetail, Student, Class
 Where ClassDetail.StudentId = Student.StudentId
 AND ClassDetail.ClassId = Class.ClassId
  AND Class.ClassId = N'72E24AAA-F3EF-49B5-8C2E-2C869B688C42'

 SELECT * FROM Class
 where
 CHARINDEX(N'A', ClassName) > 0 

 select * from UserDetail 
 join SystemUser
 on UserDetail.SystemUserId = SystemUser.SystemUserId
 join Teacher
 on UserDetail.UserId = Teacher.TeacherId

 select * from UserDetail

 select DISTINCT Teacher.* from Teacher
 join UserDetail on UserDetail.UserId = Teacher.TeacherId
join SystemUser on UserDetail.SystemUserId = N'DD114F18-0493-4E35-9BBC-663B8C5E6C61'