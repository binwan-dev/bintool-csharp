namespace BinTool.Dapper;

public class PagedData<T>
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
    
    public int TotalPage { get; set; }
    
    public int TotalRow { get; set; }

    public List<T>? Data { get; set; }
}