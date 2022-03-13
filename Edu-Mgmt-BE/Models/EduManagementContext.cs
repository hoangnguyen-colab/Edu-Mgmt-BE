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
        public virtual DbSet<SchoolYear> SchoolYear { get; set; }
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
                optionsBuilder.UseSqlServer("workstation id=edu-management-db.mssql.somee.com;packet size=4096;user id=hoangnguyencolab_SQLLogin_1;pwd=kwmozz73is;data source=edu-management-db.mssql.somee.com;persist security info=true;initial catalog=edu-management-db;TrustServerCertificate=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasIndex(e => e.ShowClassId)
                    .HasName("UQ__Class__0AD31F941E29172C")
                    .IsUnique();

                entity.Property(e => e.ClassId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.ClassName).HasMaxLength(255);

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

                entity.Property(e => e.ShowClassId).HasMaxLength(255);
            });

            modelBuilder.Entity<ClassDetail>(entity =>
            {
                entity.Property(e => e.ClassDetailId).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.ClassDetail)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ClassDeta__Class__45F365D3");

                entity.HasOne(d => d.SchoolYear)
                    .WithMany(p => p.ClassDetail)
                    .HasForeignKey(d => d.SchoolYearId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ClassDeta__Schoo__4316F928");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.ClassDetail)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ClassDeta__Stude__44FF419A");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.ClassDetail)
                    .HasForeignKey(d => d.TeacherId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ClassDeta__Teach__440B1D61");
            });

            modelBuilder.Entity<SchoolSubject>(entity =>
            {
                entity.HasKey(e => e.SubjectId)
                    .HasName("PK__SchoolSu__AC1BA3A87F0AE3D2");

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

            modelBuilder.Entity<SchoolYear>(entity =>
            {
                entity.Property(e => e.SchoolYearId).HasDefaultValueSql("(newid())");

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

                entity.Property(e => e.ActiveYear).HasMaxLength(5);

                entity.Property(e => e.SchoolYearDate).HasMaxLength(9);

                entity.Property(e => e.SchoolYearName).HasMaxLength(20);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.ShowStudentId)
                    .HasName("UQ__Student__E1B7D39A38F0F650")
                    .IsUnique();

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

                entity.Property(e => e.ShowStudentId).HasMaxLength(255);

                entity.Property(e => e.StudentAddress).HasMaxLength(255);

                entity.Property(e => e.StudentImage).HasMaxLength(255);

                entity.Property(e => e.StudentName).HasMaxLength(255);

                entity.Property(e => e.StudentPhone).HasMaxLength(20);
            });

            modelBuilder.Entity<SystemRole>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("PK__SystemRo__8AFACE1A71FD289C");

                entity.Property(e => e.RoleName).HasMaxLength(255);
            });

            modelBuilder.Entity<SystemUser>(entity =>
            {
                entity.HasIndex(e => e.UserUsername)
                    .HasName("UQ__SystemUs__04C7FD87DB968C15")
                    .IsUnique();

                entity.HasIndex(e => e.Username)
                    .HasName("UQ__SystemUs__536C85E4818150D7")
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
                entity.HasIndex(e => e.ShowTeacherId)
                    .HasName("UQ__Teacher__AD1641E1D8996341")
                    .IsUnique();

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

                entity.Property(e => e.ShowTeacherId).HasMaxLength(255);

                entity.Property(e => e.TeacherAddress).HasMaxLength(255);

                entity.Property(e => e.TeacherEmail).HasMaxLength(255);

                entity.Property(e => e.TeacherImage).HasMaxLength(255);

                entity.Property(e => e.TeacherName).HasMaxLength(255);

                entity.Property(e => e.TeacherPhone).HasMaxLength(20);
            });

            modelBuilder.Entity<UserDetail>(entity =>
            {
                entity.Property(e => e.UserDetailId).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.SystemRole)
                    .WithMany(p => p.UserDetail)
                    .HasForeignKey(d => d.SystemRoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UserDetai__Syste__5CD6CB2B");

                entity.HasOne(d => d.SystemUser)
                    .WithMany(p => p.UserDetail)
                    .HasForeignKey(d => d.SystemUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UserDetai__Syste__5BE2A6F2");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
