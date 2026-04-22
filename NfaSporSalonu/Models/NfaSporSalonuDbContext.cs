using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NfaSporSalonu.Models;

public partial class NfaSporSalonuDbContext : DbContext
{
    public NfaSporSalonuDbContext()
    {
    }

    public NfaSporSalonuDbContext(DbContextOptions<NfaSporSalonuDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessCredential> AccessCredentials { get; set; }

    public virtual DbSet<AccessLog> AccessLogs { get; set; }

    public virtual DbSet<MemberMeasurement> MemberMeasurements { get; set; }

    public virtual DbSet<MembershipPlan> MembershipPlans { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TrainerTrainee> TrainerTrainees { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserMembership> UserMemberships { get; set; }

    public virtual DbSet<WorkoutAndDietProgram> WorkoutAndDietPrograms { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=NfaSporSalonuDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessCredential>(entity =>
        {
            entity.HasKey(e => e.CredentialId).HasName("PK__AccessCr__2C58F9CCE609F3FF");

            entity.HasIndex(e => e.CredentialValue, "UQ__AccessCr__3ED7AE5141FF071B").IsUnique();

            entity.Property(e => e.AssignedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CredentialType).HasMaxLength(20);
            entity.Property(e => e.CredentialValue).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithMany(p => p.AccessCredentials)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AccessCre__UserI__60A75C0F");
        });

        modelBuilder.Entity<AccessLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AccessLo__5E548648A9337DDF");

            entity.Property(e => e.AccessTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.AccessType).HasMaxLength(10);
            entity.Property(e => e.DenialReason).HasMaxLength(100);
            entity.Property(e => e.Method).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.AccessLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AccessLog__UserI__656C112C");
        });

        modelBuilder.Entity<MemberMeasurement>(entity =>
        {
            entity.HasKey(e => e.MeasurementId).HasName("PK__MemberMe__85599FB8E86C4A38");

            entity.Property(e => e.Bicep).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Chest).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Height).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MeasurementDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.Waist).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.MemberMeasurements)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__MemberMea__UserI__6E01572D");
        });

        modelBuilder.Entity<MembershipPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Membersh__755C22B7CE885E94");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PlanName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12385833AA");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.NotificationType).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__76969D2E");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A385D166C51");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TransactionId).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Payments__UserId__5AEE82B9");

            entity.HasOne(d => d.UserMembership).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserMembershipId)
                .HasConstraintName("FK__Payments__UserMe__5BE2A6F2");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AF05234AA");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160CD975F2A").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<TrainerTrainee>(entity =>
        {
            entity.HasKey(e => e.RelationId).HasName("PK__TrainerT__E2DA16B57A9D6214");

            entity.Property(e => e.AssignedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Trainee).WithMany(p => p.TrainerTraineeTrainees)
                .HasForeignKey(d => d.TraineeId)
                .HasConstraintName("FK__TrainerTr__Train__6A30C649");

            entity.HasOne(d => d.Trainer).WithMany(p => p.TrainerTraineeTrainers)
                .HasForeignKey(d => d.TrainerId)
                .HasConstraintName("FK__TrainerTr__Train__693CA210");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CC119AC61");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105346104D037").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__RoleId__4E88ABD4");
        });

        modelBuilder.Entity<UserMembership>(entity =>
        {
            entity.HasKey(e => e.UserMembershipId).HasName("PK__UserMemb__5A4E736A1750395C");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.PurchaseDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Plan).WithMany(p => p.UserMemberships)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK__UserMembe__PlanI__571DF1D5");

            entity.HasOne(d => d.User).WithMany(p => p.UserMemberships)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserMembe__UserI__5629CD9C");
        });

        modelBuilder.Entity<WorkoutAndDietProgram>(entity =>
        {
            entity.HasKey(e => e.ProgramId).HasName("PK__WorkoutA__7525605805714695");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.ProgramType).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.Trainee).WithMany(p => p.WorkoutAndDietProgramTrainees)
                .HasForeignKey(d => d.TraineeId)
                .HasConstraintName("FK__WorkoutAn__Train__72C60C4A");

            entity.HasOne(d => d.Trainer).WithMany(p => p.WorkoutAndDietProgramTrainers)
                .HasForeignKey(d => d.TrainerId)
                .HasConstraintName("FK__WorkoutAn__Train__71D1E811");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
