using ErrorMessageMapping.Application;
using ErrorMessageMapping.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErrorMessageMapping.Infrastructure;

public class ErrorMappingRepository : IErrorMessageMappingRepository
{
    private readonly AppDbContext _context;

    public ErrorMappingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ErrorMapping>> GetByMatchTypeAsync(string matchType)
    {
        return await _context.ErrorMappings
            .Where(x => x.MatchType == matchType)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(ErrorMapping mapping)
    {
        await _context.ErrorMappings.AddAsync(mapping);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    //public async Task<List<ErrorMapping>> GetAllAsync()
    //{
    //    return await _context.ErrorMappings
    //        .OrderBy(x => x.CreatedAt)
    //        .ToListAsync();
    //}

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.ErrorMappings.FindAsync(id);
        if (entity != null)
        {
            _context.ErrorMappings.Remove(entity);
        }
    }

    public async Task<(List<ErrorMapping> Items, int TotalCount)> GetPagedAsync(
    string? keyword, string? matchType, string? sort, int page, int pageSize)
    {
        var query = _context.ErrorMappings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(matchType))
            query = query.Where(x => x.MatchType == matchType);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();
            query = query.Where(x =>
                x.MatchValue1.Contains(keyword) ||
                (x.MatchValue2 != null && x.MatchValue2.Contains(keyword)) ||
                (x.MatchValue3 != null && x.MatchValue3.Contains(keyword)) ||
                (x.ReplaceMessage != null && x.ReplaceMessage.Contains(keyword)));
        }

        // 排序
        query = sort switch
        {
            "oldest" => query.OrderBy(x => x.CreatedAt),
            "type" => query.OrderBy(x => x.MatchType).ThenByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)   // 預設：最新優先
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<ErrorMapping?> GetByIdAsync(int id)
    {
        return await _context.ErrorMappings.FindAsync(id);
    }

    public async Task UpdateAsync(ErrorMapping mapping)
    {
        _context.ErrorMappings.Update(mapping);
    }
}