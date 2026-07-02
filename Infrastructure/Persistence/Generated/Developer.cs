using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NiquiBackend.Infrastructure.Persistence.Generated;

[Table("Developer")]
[Index("Email", Name = "UQ__Develope__A9D10534EB4C8B9B", IsUnique = true)]
public partial class Developer
{
    [Key]
    [Column("DeveloperID")]
    public Guid DeveloperId { get; set; }

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

    [InverseProperty("CreatedByDeveloper")]
    public virtual ICollection<Admin> Admins { get; set; } = new List<Admin>();

    [InverseProperty("CreatedByDeveloper")]
    public virtual ICollection<SuperAdmin> SuperAdmins { get; set; } = new List<SuperAdmin>();
}
