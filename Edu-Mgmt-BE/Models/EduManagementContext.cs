using System;
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

        public virtual DbSet<Class> Class { get; set; }
        public virtual DbSet<ClassDetail> ClassDetail { get; set; }
        public virtual DbSet<SchoolSubject> SchoolSubject { get; set; }
        public virtual DbSet<Student> Student { get; set; }
        public virtual DbSet<SystemRole> SystemRole { get; set; }
        public virtual DbSet<SystemUser> SystemUser { get; set; }
        public virtual DbSet<Teacher> Teacher { get; set; }
        public virtual DbSet<UserDetail> UserDetail { get; set; }

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
            modelBuilder.Entity<Class>(entity =>
            {
                entity.Property(e => e.ClassId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.ClassName).HasMaxLength(255);

                entity.Property(e => e.ClassYear).HasMaxLength(15);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedUser)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyUser)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.Class)
                    .HasForeignKey(d => d.TeacherId)
                    .HasConstraintName("FK__Class__TeacherId__36B12243");
            });

            modelBuilder.Entity<ClassDetail>(entity =>
            {
                entity.Property(e => e.ClassDetailId).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.ClassDetail)
                    .HasForeignKey(d => d.ClassId)
                    .HasConstraintName("FK__ClassDeta__Class__3A81B327");
            });

            modelBuilder.Entity<SchoolSubject>(entity =>
            {
                entity.HasKey(e => e.SubjectId)
                    .HasName("PK__SchoolSu__AC1BA3A82259C4CC");

                entity.Property(e => e.SubjectId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedUser)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyUser)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SubjectName).HasMaxLength(255);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.Property(e => e.StudentId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedUser)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyUser)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.StudentDob)
                    .HasColumnName("StudentDOB")
                    .HasMaxLength(15);

                entity.Property(e => e.StudentGender).HasMaxLength(6);

                entity.Property(e => e.StudentImage).HasMaxLength(255);

                entity.Property(e => e.StudentName).HasMaxLength(255);

                entity.Property(e => e.StudentPhone).HasMaxLength(20);
            });

            modelBuilder.Entity<SystemRole>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("PK__SystemRo__8AFACE1A2B40E09F");

                entity.Property(e => e.RoleName).HasMaxLength(255);
            });

            modelBuilder.Entity<SystemUser>(entity =>
            {
                entity.HasIndex(e => e.UserUsername)
                    .HasName("UQ__SystemUs__04C7FD874DD42B62")
                    .IsUnique();

                entity.Property(e => e.SystemUserId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedUser)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyUser)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

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

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedUser)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyUser)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TeacherDob)
                    .HasColumnName("TeacherDOB")
                    .HasMaxLength(15);

                entity.Property(e => e.TeacherEmail).HasMaxLength(255);

                entity.Property(e => e.TeacherGender).HasMaxLength(6);

                entity.Property(e => e.TeacherImage).HasMaxLength(255);

                entity.Property(e => e.TeacherName).HasMaxLength(255);

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
