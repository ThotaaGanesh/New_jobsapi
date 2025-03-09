using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace JobsApi.Models;

public partial class JobsApiContext : DbContext
{
    public JobsApiContext()
    {
    }

    public JobsApiContext(DbContextOptions<JobsApiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SendNotification> SendNotifications { get; set; }

    public virtual DbSet<Subscribe> Subscribes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=jobpoc.database.windows.net;Database=JobPOC;User Id=jobpoc;Password=Passw0rd;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("Job");

            entity.Property(e => e.CompanyName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.JobName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastUpdatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Location)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Qualification)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Salary)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SendNotification>(entity =>
        {
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Job).WithMany(p => p.SendNotifications)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SendNotifications_Job");

            entity.HasOne(d => d.User).WithMany(p => p.SendNotifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SendNotifications_User");
        });

        modelBuilder.Entity<Subscribe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Subscription");

            entity.ToTable("Subscribe");

            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Qualification)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SendTo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SubscriptionType)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.EmailId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastUpdatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(1000);
            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.OrganisationName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Qualification)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SubscriptionType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_User_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
