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
}