using System;
using Edu_Mgmt_BE.Models.CustomModel.Class;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class EduManagementContext : DbContext
    {
        public EduManagementContext()
        {
        }

        public EduManagementContext(DbContextOptions<EduManagementContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Answer> Answer { get; set; }
        public virtual DbSet<AnswerFileDetail> AnswerFileDetail { get; set; }
        public virtual DbSet<Class> Class { get; set; }
        public virtual DbSet<ClassDetail> ClassDetail { get; set; }
        public virtual DbSet<FileUpload> FileUpload { get; set; }
        public virtual DbSet<HomeWork> HomeWork { get; set; }
        public virtual DbSet<HomeWorkClassDetail> HomeWorkClassDetail { get; set; }
        public virtual DbSet<HomeWorkFileDetail> HomeWorkFileDetail { get; set; }
        public virtual DbSet<HomeWorkResultDetail> HomeWorkResultDetail { get; set; }
        public virtual DbSet<Result> Result { get; set; }
        public virtual DbSet<Student> Student { get; set; }
        public virtual DbSet<SystemRole> SystemRole { get; set; }
        public virtual DbSet<SystemUser> SystemUser { get; set; }
        public virtual DbSet<Teacher> Teacher { get; set; }
        public virtual DbSet<UserDetail> UserDetail { get; set; }
        public DbSet<ClassQuery> ClassQuery { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=OPEN-AI;Database=EduManagement;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClassQuery>().HasNoKey();

            modelBuilder.Entity<Answer>(entity =>
            {
                entity.Property(e => e.AnswerId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SubmitTime).HasColumnType("datetime");

                entity.HasOne(d => d.HomeWork)
                    .WithMany(p => p.Answer)
                    .HasForeignKey(d => d.HomeWorkId)
                    .HasConstraintName("FK__Answer__HomeWork__4F7CD00D");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Answer)
                    .HasForeignKey(d => d.StudentId)
                    .HasConstraintName("FK__Answer__StudentI__5070F446");
            });

            modelBuilder.Entity<AnswerFileDetail>(entity =>
            {
                entity.HasKey(e => e.FileUploadDetailId)
                    .HasName("PK__AnswerFi__F1AA951B7970ECCB");

                entity.Property(e => e.FileUploadDetailId).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.Answer)
                    .WithMany(p => p.AnswerFileDetail)
                    .HasForeignKey(d => d.AnswerId)
                    .HasConstraintName("FK__AnswerFil__Answe__5535A963");

                entity.HasOne(d => d.FileUpload)
                    .WithMany(p => p.AnswerFileDetail)
                    .HasForeignKey(d => d.FileUploadId)
                    .HasConstraintName("FK__AnswerFil__FileU__5441852A");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.Property(e => e.ClassId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.ClassName).HasMaxLength(255);

                entity.Property(e => e.ClassYear).HasMaxLength(15);

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.Class)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("FK__Class__TeacherId__2B3F6F97");
            });

            modelBuilder.Entity<ClassDetail>(entity =>
            {
                entity.Property(e => e.ClassDetailId).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.ClassDetail)
                    .HasForeignKey(d => d.ClassId)
                    .HasConstraintName("FK__ClassDeta__Class__2F10007B");
            });

            modelBuilder.Entity<FileUpload>(entity =>
            {
                entity.Property(e => e.FileUploadId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.FileUploadName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.FileUploadUrl)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<HomeWork>(entity =>
            {
                entity.Property(e => e.HomeWorkId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.HomeWorkName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.HomeWorkType)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.OnlyAssignStudent).HasDefaultValueSql("((0))");

                entity.Property(e => e.RequiredLogin).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<HomeWorkClassDetail>(entity =>
            {
                entity.HasKey(e => e.HomeWorkClassDetail1)
                    .HasName("PK__HomeWork__8AF9F51D1ABFCA15");

                entity.Property(e => e.HomeWorkClassDetail1)
                    .HasColumnName("HomeWorkClassDetail")
                    .HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.HomeWorkClassDetail)
                    .HasForeignKey(d => d.ClassId)
                    .HasConstraintName("FK__HomeWorkC__Class__4222D4EF");

                entity.HasOne(d => d.HomeWork)
                    .WithMany(p => p.HomeWorkClassDetail)
                    .HasForeignKey(d => d.HomeWorkId)
                    .HasConstraintName("FK__HomeWorkC__HomeW__412EB0B6");
            });

            modelBuilder.Entity<HomeWorkFileDetail>(entity =>
            {
                entity.HasKey(e => e.FileUploadDetailId)
                    .HasName("PK__HomeWork__F1AA951B5C10F45C");

                entity.Property(e => e.FileUploadDetailId).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.FileUpload)
                    .WithMany(p => p.HomeWorkFileDetail)
                    .HasForeignKey(d => d.FileUploadId)
                    .HasConstraintName("FK__HomeWorkF__FileU__48CFD27E");

                entity.HasOne(d => d.HomeWork)
                    .WithMany(p => p.HomeWorkFileDetail)
                    .HasForeignKey(d => d.HomeWorkId)
                    .HasConstraintName("FK__HomeWorkF__HomeW__49C3F6B7");
            });

            modelBuilder.Entity<HomeWorkResultDetail>(entity =>
            {
                entity.Property(e => e.HomeWorkResultDetailId).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.Answer)
                    .WithMany(p => p.HomeWorkResultDetail)
                    .HasForeignKey(d => d.AnswerId)
                    .HasConstraintName("FK__HomeWorkR__Answe__5FB337D6");

                entity.HasOne(d => d.HomeWork)
                    .WithMany(p => p.HomeWorkResultDetail)
                    .HasForeignKey(d => d.HomeWorkId)
                    .HasConstraintName("FK__HomeWorkR__HomeW__60A75C0F");

                entity.HasOne(d => d.Result)
                    .WithMany(p => p.HomeWorkResultDetail)
                    .HasForeignKey(d => d.ResultId)
                    .HasConstraintName("FK__HomeWorkR__Resul__5EBF139D");
            });

            modelBuilder.Entity<Result>(entity =>
            {
                entity.Property(e => e.ResultId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FinalScore)
                    .HasMaxLength(4)
                    .HasDefaultValueSql("(N'0.0')");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.Property(e => e.StudentId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.StudentDob)
                    .HasColumnName("StudentDOB")
                    .HasMaxLength(15);

                entity.Property(e => e.StudentGender).HasMaxLength(6);

                entity.Property(e => e.StudentName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.StudentPhone)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<SystemRole>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("PK__SystemRo__8AFACE1AC3165AAF");

                entity.Property(e => e.RoleName).HasMaxLength(255);
            });

            modelBuilder.Entity<SystemUser>(entity =>
            {
                entity.HasIndex(e => e.UserUsername)
                    .HasName("UQ__SystemUs__04C7FD874CA3ACB5")
                    .IsUnique();

                entity.Property(e => e.SystemUserId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.UserPassword)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.UserUsername)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.Property(e => e.TeacherId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.TeacherDob)
                    .HasColumnName("TeacherDOB")
                    .HasMaxLength(15);

                entity.Property(e => e.TeacherEmail).HasMaxLength(255);

                entity.Property(e => e.TeacherGender).HasMaxLength(6);

                entity.Property(e => e.TeacherName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.TeacherPhone)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<UserDetail>(entity =>
            {
                entity.Property(e => e.UserDetailId).HasDefaultValueSql("(newid())");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
