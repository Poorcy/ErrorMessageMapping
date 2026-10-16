using ErrorMessageMapping.Domain;

namespace ErrorMessageMapping.Application;

public interface IErrorMessageMappingRepository
{
    Task<List<ErrorMapping>> GetByMatchTypeAsync(string matchType);
    Task<(List<ErrorMapping> Items, int TotalCount)> GetPagedAsync(
    string? keyword, 
    string? matchType, 
    string? sort, 
    int page, 
    int pageSize);
    Task<ErrorMapping?> GetByIdAsync(int id);
    Task AddAsync(ErrorMapping mapping);
    Task UpdateAsync(ErrorMapping mapping);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}