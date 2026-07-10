using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NiquiBackend.Infrastructure.Persistence.Generated;

[Table("Customer")]
public partial class Customer
{
    [Key]
    [Column("CustomerID")]
    public Guid CustomerId { get; set; }

    [StringLength(101)]
    [Unicode(false)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string Convenio { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string PhoneNumber { get; set; } = null!;

    public bool IsApproved { get; set; }

    public bool IsCalled { get; set; }

    [Column("CreatedBySuperAdminID")]
    public Guid? CreatedBySuperAdminId { get; set; }

    [Column("CreatedByAdminID")]
    public Guid? CreatedByAdminId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("CreatedByAdminId")]
    [InverseProperty("Customers")]
    public virtual Admin? CreatedByAdmin { get; set; }

    [ForeignKey("CreatedBySuperAdminId")]
    [InverseProperty("Customers")]
    public virtual SuperAdmin? CreatedBySuperAdmin { get; set; }
}
