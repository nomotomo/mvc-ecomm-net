using Catalog.Core.Entities;

namespace Catalog.Core.Specs;

public class Pagination<T> where T: class 
{
    public Pagination()
    {
        
    }
    
    public Pagination(int pageNumber, int pageSize, int pageIndex, int totalItems, IReadOnlyList<T> data)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        PageIndex = pageIndex;
        TotalItems = totalItems;
        Data = data;
    }

    public Pagination(int pageIndex, int pageSize, long totalCount, IReadOnlyList<T> totalItems)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalItems = (int)totalCount;
        Data = totalItems;
    }

    public IReadOnlyList<T> Data { get; set; }

    public int TotalItems { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public int PageNumber { get; set; }
}