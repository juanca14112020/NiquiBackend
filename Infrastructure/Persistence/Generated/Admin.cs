using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NiquiBackend.Infrastructure.Persistence.Generated;

[Table("Admin")]
[Index("Email", Name = "UQ__Admin__A9D10534B03CA4D4", IsUnique = true)]
public partial class Admin
{
    [Key]
    [Column("AdminID")]
    public Guid AdminId { get; set; }

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
    public Guid? CreatedByDeveloperId { get; set; }

    [Column("CreatedBySuperAdminID")]
    public Guid? CreatedBySuperAdminId { get; set; }

    [ForeignKey("CreatedByDeveloperId")]
    [InverseProperty("Admins")]
    public virtual Developer? CreatedByDeveloper { get; set; }

    [ForeignKey("CreatedBySuperAdminId")]
    [InverseProperty("Admins")]
    public virtual SuperAdmin? CreatedBySuperAdmin { get; set; }

    [InverseProperty("CreatedByAdmin")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
