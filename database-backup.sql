﻿--USE master
--ALTER DATABASE EduManagement set single_user WITH ROLLBACK IMMEDIATE
--DROP DATABASE EduManagement

CREATE DATABASE EduManagement
 GO
USE EduManagement
 GO

CREATE TABLE SchoolYear
(
	SchoolYearId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	SchoolYearName NVARCHAR(20),
	SchoolYearDate NVARCHAR(9),
	ActiveYear NVARCHAR(4),

	CreatedDate DATETIME DEFAULT GETDATE(),
	ModifyDate DATETIME DEFAULT GETDATE(),
	CreatedUser DATETIME DEFAULT GETDATE(),
	ModifyUser DATETIME DEFAULT GETDATE(),
)
GO

CREATE TABLE Class
(
	ClassId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	ShowClassId NVARCHAR(255) UNIQUE,
	ClassName NVARCHAR(255),

	CreatedDate DATETIME DEFAULT GETDATE(),
	ModifyDate DATETIME DEFAULT GETDATE(),
	CreatedUser DATETIME DEFAULT GETDATE(),
	ModifyUser DATETIME DEFAULT GETDATE(),
)


CREATE TABLE Student
(
	StudentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	ShowStudentId NVARCHAR(255) UNIQUE,
	StudentName NVARCHAR(255),
	StudentGender NVARCHAR(6),
	StudentDOB NVARCHAR(15),
	StudentImage NVARCHAR(255),
	StudentDescription NVARCHAR(MAX),
	StudentPhone NVARCHAR(20),
	StudentAddress NVARCHAR(255),

	CreatedDate DATETIME DEFAULT GETDATE(),
	ModifyDate DATETIME DEFAULT GETDATE(),
	CreatedUser DATETIME DEFAULT GETDATE(),
	ModifyUser DATETIME DEFAULT GETDATE(),
)


CREATE TABLE Teacher
(
	TeacherId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	ShowTeacherId NVARCHAR(255) UNIQUE,
	TeacherName NVARCHAR(255),
	TeacherGender NVARCHAR(6),
	TeacherDOB NVARCHAR(15),
	TeacherImage NVARCHAR(255),
	TeacherDescription NVARCHAR(MAX),
	TeacherEmail NVARCHAR(255),
	TeacherPhone NVARCHAR(20),
	TeacherAddress NVARCHAR(255),

	CreatedDate DATETIME DEFAULT GETDATE(),
	ModifyDate DATETIME DEFAULT GETDATE(),
	CreatedUser DATETIME DEFAULT GETDATE(),
	ModifyUser DATETIME DEFAULT GETDATE(),
)


CREATE TABLE ClassDetail
(
	ClassDetailId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

	SchoolYearId UNIQUEIDENTIFIER REFERENCES SchoolYear(SchoolYearId) NOT NULL,
	TeacherId UNIQUEIDENTIFIER REFERENCES Teacher(TeacherId),
	StudentId UNIQUEIDENTIFIER REFERENCES Student(StudentId),
	ClassId UNIQUEIDENTIFIER REFERENCES Class(ClassId) NOT NULL,
)

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

	SystemUserId UNIQUEIDENTIFIER REFERENCES SystemUser(SystemUserId) NOT NULL,
	SystemRoleId INT REFERENCES SystemRole(RoleId) NOT NULL,
)
GO

INSERT INTO SystemRole
VALUES
	(N'admin'),
	(N'teacher'),
	(N'student')

GO
INSERT INTO SystemUser
VALUES
	(N'DD114F18-0493-4E35-9BBC-663B8C5E6C61', N'Teacher Test', N'TCR0001', N'e10adc3949ba59abbe56e057f20f883e', DEFAULT, DEFAULT, DEFAULT, DEFAULT), 
	(N'8A1B746B-C6F9-4627-B803-271ADB35C289', N'Admin', N'hoag-admin', N'21232f297a57a5a743894a0e4a801fc3', DEFAULT, DEFAULT, DEFAULT, DEFAULT)
 GO

INSERT INTO Teacher
VALUES
	(N'88CA6417-6B71-48B2-9FD9-47C57F384947', 
	N'TCR0001', 
	N'Hoang',
	N'Nam',
	N'23/07/1999',
	NULL, 
	NULL, 
	NULL, 
	NULL, 
	NULL, 
	DEFAULT, 
	DEFAULT, 
	DEFAULT, 
	DEFAULT)
 GO

 INSERT INTO UserDetail
 VALUES
 	(
 		DEFAULT, NULL, N'8A1B746B-C6F9-4627-B803-271ADB35C289', 1
 )
 GO
 INSERT INTO UserDetail
 VALUES
 	(
 		DEFAULT, N'88CA6417-6B71-48B2-9FD9-47C57F384947', N'DD114F18-0493-4E35-9BBC-663B8C5E6C61', 2
 )
 GO
 
INSERT INTO SchoolYear
VALUES
	(N'0FF0AB90-85DE-417C-8C62-6ADF29919874', N'Năm học 2019', N'2019-2020', N'2019' , DEFAULT, DEFAULT, DEFAULT, DEFAULT),
	(N'8EB9DFD4-E492-4276-8060-1B23D45C04E9', N'Năm học 2020', N'2020-2021', N'2020', DEFAULT, DEFAULT, DEFAULT, DEFAULT),
	(N'2AFCB4E5-CB05-4072-9510-99FD4224BEF0', N'Năm học 2021', N'2021-2022', N'2021', DEFAULT, DEFAULT, DEFAULT, DEFAULT)

INSERT INTO Class
VALUES
	(N'72E24AAA-F3EF-49B5-8C2E-2C869B688C42', N'2A1', N'2A1', DEFAULT, DEFAULT, DEFAULT, DEFAULT),
	(N'03F47CCC-24B4-484F-B08F-370B3DCBE2BA', N'1A3', N'1A3', DEFAULT, DEFAULT, DEFAULT, DEFAULT),
	(N'F3BDF8C4-46E8-4A30-9BC6-4C2A563ABA16', N'1A2', N'1A2', DEFAULT, DEFAULT, DEFAULT, DEFAULT),
	(N'19F46F02-A2FC-469A-8E2C-57D15B3D7356', N'1A1', N'1A1', DEFAULT, DEFAULT, DEFAULT, DEFAULT),
	(N'1DA1CCC5-4ECD-4180-9755-ABA6AFC6E097', N'2A2', N'2A2', DEFAULT, DEFAULT, DEFAULT, DEFAULT)
GO

INSERT INTO ClassDetail
VALUES
	(
		N'59E146A8-9D3D-4B52-BEA2-7B3BFA914A26', 
		N'2AFCB4E5-CB05-4072-9510-99FD4224BEF0',
		N'88CA6417-6B71-48B2-9FD9-47C57F384947',
		null,
		N'03F47CCC-24B4-484F-B08F-370B3DCBE2BA'
 )
 GO

 select * from UserDetail 
 join SystemUser
 on UserDetail.SystemUserId = SystemUser.SystemUserId
 join SystemRole
 on UserDetail.SystemRoleId = SystemRole.RoleId

 select * from SystemRole where RoleId in 
 (select distinct SystemRoleId from UserDetail where SystemUserId = N'A5044EF6-062D-42CD-978A-AC4D3E49D206')

 SELECT * FROM SchoolYear WHERE CHARINDEX(N'21', SchoolYearName) > 0 OR CHARINDEX(N'21', SchoolYearDate) > 0

 SELECT DISTINCT Class.* FROM Class, ClassDetail, Teacher
 WHERE Class.ClassId = ClassDetail.ClassId
 AND ClassDetail.TeacherId = N'88CA6417-6B71-48B2-9FD9-47C57F384947' 

 SELECT * FROM Class
 where
 CHARINDEX(N'2A', ClassName) > 0 OR CHARINDEX(N'2A', ShowClassId) > 0

 select * from UserDetail 
 join SystemUser
 on UserDetail.SystemUserId = SystemUser.SystemUserId
 join Teacher
 on UserDetail.UserId = Teacher.TeacherId

 select * from UserDetail

 select DISTINCT Teacher.* from Teacher
 join UserDetail on UserDetail.UserId = Teacher.TeacherId
join SystemUser on UserDetail.SystemUserId = N'DD114F18-0493-4E35-9BBC-663B8C5E6C61'