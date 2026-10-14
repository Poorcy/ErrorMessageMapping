using ErrorMessageMapping.Application;
using ErrorMessageMapping.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ErrorMessageMapping.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IErrorMessageMappingRepository _repository;

    public IndexModel(IErrorMessageMappingRepository repository)
    {
        _repository = repository;
    }

    public List<ErrorMapping> Mappings { get; set; } = new();

    public async Task OnGetAsync()
    {
        Mappings = await _repository.GetAllAsync();
    }
}