using ErrorMessageMapping.Application;
using ErrorMessageMapping.Domain;

namespace ErrorMessageMapping.Application.Tests;

public class InMemoryErrorMappingRepository : IErrorMessageMappingRepository
{
    private readonly List<ErrorMapping> _data = new();

    public void Seed(params ErrorMapping[] mappings)
    {
        _data.AddRange(mappings);
    }

    public Task<List<ErrorMapping>> GetByMatchTypeAsync(string matchType)
    {
        var result = _data
            .Where(x => x.MatchType == matchType)
            .OrderBy(x => x.CreatedAt)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<(List<ErrorMapping> Items, int TotalCount)> GetPagedAsync(string? keyword, string? matchType, string? sort, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<ErrorMapping?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(ErrorMapping mapping)
    {
        _data.Add(mapping);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ErrorMapping mapping)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
}