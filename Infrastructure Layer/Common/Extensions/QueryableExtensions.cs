using Application.Common;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure_Layer.Common.Extensions;

public static class QueryableExtensions
{
    public static async Task<PaginatedResult<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items, count, pageNumber, pageSize);
    }
}