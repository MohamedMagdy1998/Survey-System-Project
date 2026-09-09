using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Contracts;

public record RequestFilters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    public int PageNumber { get; init; } = 1;
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
    public string? SearchValue { get; init; }

    public string? SortColumn { get; init; }

    public string? SortDirection { get; init; } = "Asc";
}
