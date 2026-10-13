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
}