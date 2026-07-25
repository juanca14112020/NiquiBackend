using Microsoft.EntityFrameworkCore;

namespace NiquiBackend.Infrastructure.Persistence.Generated;

public partial class NiquiDbContext
{
    public DbSet<MassCallExecution> MassCallExecutions => Set<MassCallExecution>();

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MassCallExecution>().ToTable("MassCallExecution");
    }
}