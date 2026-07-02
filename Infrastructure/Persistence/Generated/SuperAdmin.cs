using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NiquiBackend.Infrastructure.Persistence.Generated;

[Table("SuperAdmin")]
[Index("Email", Name = "UQ__SuperAdm__A9D105347D569C14", IsUnique = true)]
public partial class SuperAdmin
{
    [Key]
    [Column("SuperAdminID")]
    public Guid SuperAdminId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string FirstName { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string LastName { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string DocumentType { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string DocumentNumber { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string PasswordHash { get; set; } = null!;

    public bool IsActive { get; set; }

    [Column("CreatedByDeveloperID")]
    public Guid CreatedByDeveloperId { get; set; }

    [InverseProperty("CreatedBySuperAdmin")]
    public virtual ICollection<Admin> Admins { get; set; } = new List<Admin>();

    [ForeignKey("CreatedByDeveloperId")]
    [InverseProperty("SuperAdmins")]
    public virtual Developer CreatedByDeveloper { get; set; } = null!;

    [InverseProperty("CreatedBySuperAdmin")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
