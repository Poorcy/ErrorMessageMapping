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

    public async Task<IActionResult> OnPostImportAsync(IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            TempData["Message"] = "請選擇 CSV 檔案";
            return RedirectToPage();
        }

        using var reader = new StreamReader(csvFile.OpenReadStream());
        var lineNumber = 0;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            lineNumber++;

            // 跳過標題列
            if (lineNumber == 1 && line != null && line.Contains("MatchType"))
                continue;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',');

            if (parts.Length < 2)
                continue;

            var mapping = new ErrorMapping
            {
                MatchType = parts[0].Trim(),
                MatchValue1 = parts.Length > 1 ? parts[1].Trim() : "",
                MatchValue2 = parts.Length > 2 ? parts[2].Trim() : null,
                MatchValue3 = parts.Length > 3 ? parts[3].Trim() : null,
                ReplaceMessage = parts.Length > 4 ? parts[4].Trim() : null,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "CSV"
            };

            if (string.IsNullOrWhiteSpace(mapping.MatchValue1))
                continue;

            if (mapping.MatchType != MappingConstants.MatchTypeExact &&
                mapping.MatchType != MappingConstants.MatchTypeFuzzy)
                mapping.MatchType = MappingConstants.MatchTypeExact;

            await _repository.AddAsync(mapping);
        }

        await _repository.SaveChangesAsync();
        TempData["Message"] = "CSV 匯入完成";
        return RedirectToPage();
    }
}