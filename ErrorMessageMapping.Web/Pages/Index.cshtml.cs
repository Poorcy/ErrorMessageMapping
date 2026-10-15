using ErrorMessageMapping.Application;
using ErrorMessageMapping.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ErrorMessageMapping.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IErrorMessageMappingRepository _repository;
    private const int PageSize = 10;

    public IndexModel(IErrorMessageMappingRepository repository)
    {
        _repository = repository;
    }

    public List<ErrorMapping> Mappings { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public string? Keyword { get; set; }
    public string? MatchType { get; set; }

    [BindProperty]
    public ErrorMapping NewMapping { get; set; } = new();

    [BindProperty]
    public ErrorMapping EditMapping { get; set; } = new();

    public async Task OnGetAsync([FromQuery] string? keyword, [FromQuery] string? matchType, [FromQuery] int page = 1)
    {
        Keyword = keyword;
        MatchType = matchType;
        CurrentPage = page < 1 ? 1 : page;

        var (items, total) = await _repository.GetPagedAsync(keyword, matchType, CurrentPage, PageSize);
        Mappings = items;
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
    }

    public async Task<IActionResult> OnPostAsync([FromQuery] string? keyword, [FromQuery] string? matchType, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(NewMapping.MatchValue1))
        {
            await OnGetAsync(keyword, matchType, page);
            return Page();
        }

        NewMapping.CreatedAt = DateTime.UtcNow;
        NewMapping.CreatedBy = "Web";
        if (NewMapping.MatchType != "Exact" && NewMapping.MatchType != "Fuzzy")
            NewMapping.MatchType = "Exact";

        await _repository.AddAsync(NewMapping);
        await _repository.SaveChangesAsync();
        return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", page });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, string? keyword, string? matchType, int page = 1)
    {
        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();
        return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", page });
    }

    public async Task<IActionResult> OnPostEditAsync(string? keyword, string? matchType, int page = 1)
    {
        var existing = await _repository.GetByIdAsync(EditMapping.Id);
        if (existing == null)
            return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", page });

        existing.MatchType = EditMapping.MatchType;
        existing.MatchValue1 = EditMapping.MatchValue1;
        existing.MatchValue2 = EditMapping.MatchValue2;
        existing.MatchValue3 = EditMapping.MatchValue3;
        existing.ReplaceMessage = EditMapping.ReplaceMessage;

        await _repository.UpdateAsync(existing);
        await _repository.SaveChangesAsync();
        return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", page });
    }

    public async Task<IActionResult> OnPostImportAsync(IFormFile csvFile, string? keyword, string? matchType, int page = 1)
    {
        // 保留你原本的 CSV 匯入邏輯，最後改成：
        // return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", page });
        // （請把原本方法裡的 RedirectToPage() 換成帶參數的版本）

        if (csvFile == null || csvFile.Length == 0)
        {
            TempData["Message"] = "請選擇 CSV 檔案";
            return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", page });
        }

        using var reader = new StreamReader(csvFile.OpenReadStream());
        var lineNumber = 0;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            lineNumber++;
            if (lineNumber == 1 && line != null && line.Contains("MatchType")) continue;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 2) continue;

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

            if (string.IsNullOrWhiteSpace(mapping.MatchValue1)) continue;
            if (mapping.MatchType != "Exact" && mapping.MatchType != "Fuzzy")
                mapping.MatchType = "Exact";

            await _repository.AddAsync(mapping);
        }

        await _repository.SaveChangesAsync();
        TempData["Message"] = "CSV 匯入完成";
        return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", page });
    }
}