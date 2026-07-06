using Microsoft.AspNetCore.RateLimiting;

namespace NiquiBackend.Application.DTOs.Customer;

public class CustomerBulkImportResultDto
{
    public int TotalRows { get; set; }
    public int Inserted { get; set; }
    public int Rejected { get; set; }
    public List <string> Errors { get; set; } = new(); 
}