using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NiquiBackend.Infrastructure.Persistence.Generated;

public partial class NiquiDbContext : DbContext
{
    public NiquiDbContext()
    {
    }

    public NiquiDbContext(DbContextOptions<NiquiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Developer> Developers { get; set; }

    public virtual DbSet<SuperAdmin> SuperAdmins { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=JUAN_CAMILO;Database=Niqui;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admin__719FE4E849EDA77D");

            entity.Property(e => e.AdminId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByDeveloper).WithMany(p => p.Admins).HasConstraintName("FK__Admin__CreatedBy__5EBF139D");

            entity.HasOne(d => d.CreatedBySuperAdmin).WithMany(p => p.Admins).HasConstraintName("FK__Admin__CreatedBy__5FB337D6");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B89D64FB1A");

            entity.Property(e => e.CustomerId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CreatedByAdmin).WithMany(p => p.Customers).HasConstraintName("FK_Customer_Admin");

            entity.HasOne(d => d.CreatedBySuperAdmin).WithMany(p => p.Customers).HasConstraintName("FK_Customer_SuperAdmin");
        });

        modelBuilder.Entity<Developer>(entity =>
        {
            entity.HasKey(e => e.DeveloperId).HasName("PK__Develope__DE084CD141C1F2B8");

            entity.Property(e => e.DeveloperId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.HasKey(e => e.SuperAdminId).HasName("PK__SuperAdm__103FDA68E44C86AE");

            entity.Property(e => e.SuperAdminId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByDeveloper).WithMany(p => p.SuperAdmins)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SuperAdmi__Creat__59063A47");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
