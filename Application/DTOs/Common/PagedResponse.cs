namespace NiquiBackend.Application.DTOs.Common;

public class PagedResponse<T>
{
  public List<T> Items { get; set; } = new();
  public int TotalCount { get; set; }
  public int TotalPages { get; set; }
  public int CurrentPage { get; set; }

  public PagedResponse(List<T> items, int count, int pageNumber, int pageSize)
  {
    TotalCount = count;
    CurrentPage = pageNumber;
    TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    Items = items;
  }
}