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

    public async Task<(List<ErrorMapping> Items, int TotalCount)> GetPagedAsync(string? keyword, int page, int pageSize)
    {
        var query = _context.ErrorMappings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();
            query = query.Where(x =>
                x.MatchValue1.Contains(keyword) ||
                (x.MatchValue2 != null && x.MatchValue2.Contains(keyword)) ||
                (x.MatchValue3 != null && x.MatchValue3.Contains(keyword)) ||
                (x.ReplaceMessage != null && x.ReplaceMessage.Contains(keyword)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
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