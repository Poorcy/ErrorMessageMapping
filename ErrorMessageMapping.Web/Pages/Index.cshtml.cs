using ErrorMessageMapping.Application;
using ErrorMessageMapping.Domain;
using Microsoft.AspNetCore.Mvc;
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

    [BindProperty]
    public ErrorMapping NewMapping { get; set; } = new();

    public async Task OnGetAsync()
    {
        Mappings = await _repository.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMapping.MatchValue1))
        {
            ModelState.AddModelError("NewMapping.MatchValue1", "條件 1 為必填");
            Mappings = await _repository.GetAllAsync();
            return Page();
        }

        if (NewMapping.MatchType != MappingConstants.MatchTypeExact &&
            NewMapping.MatchType != MappingConstants.MatchTypeFuzzy)
        {
            NewMapping.MatchType = MappingConstants.MatchTypeExact;
        }

        NewMapping.CreatedAt = DateTime.UtcNow;
        NewMapping.CreatedBy = "Web";

        await _repository.AddAsync(NewMapping);
        await _repository.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();
        return RedirectToPage();
    }
}