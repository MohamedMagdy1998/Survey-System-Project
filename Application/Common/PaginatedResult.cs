using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common;

public class PaginatedResult<T>
{
    public ICollection<T> Items { get; init; } = [];
    public int TotalPages { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedResult(ICollection<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}