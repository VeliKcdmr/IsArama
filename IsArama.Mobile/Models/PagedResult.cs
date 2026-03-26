namespace IsArama.Mobile.Models;

public class PagedResult
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<JobListItem> Jobs { get; set; } = [];
}
