using ErrorMessageMapping.Domain;

namespace ErrorMessageMapping.Application;

public interface IErrorMessageMappingRepository
{
    Task<List<ErrorMapping>> GetByMatchTypeAsync(string matchType);
    Task AddAsync(ErrorMapping mapping);
    Task SaveChangesAsync();
}