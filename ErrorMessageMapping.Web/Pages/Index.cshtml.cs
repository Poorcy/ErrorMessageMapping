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
    public string? Sort { get; set; }

    [BindProperty]
    public ErrorMapping NewMapping { get; set; } = new();

    [BindProperty]
    public ErrorMapping EditMapping { get; set; } = new();

    public async Task OnGetAsync(
    [FromQuery] string? keyword,
    [FromQuery] string? matchType,
    [FromQuery] string? sort,
    [FromQuery] int page = 1)
    {
        Keyword = keyword;
        MatchType = matchType;
        Sort = string.IsNullOrWhiteSpace(sort) ? "newest" : sort;
        CurrentPage = page < 1 ? 1 : page;

        var (items, total) = await _repository.GetPagedAsync(keyword, matchType, Sort, CurrentPage, PageSize);
        Mappings = items;
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
    }

    public async Task<IActionResult> OnPostAsync([FromQuery] string? keyword, [FromQuery] string? matchType, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(NewMapping.MatchValue1))
        {
            await OnGetAsync(keyword, matchType, Sort, page);
            return Page();
        }

        NewMapping.CreatedAt = DateTime.UtcNow;
        NewMapping.CreatedBy = "Web";
        if (NewMapping.MatchType != "Exact" && NewMapping.MatchType != "Fuzzy")
            NewMapping.MatchType = "Exact";

        if (NewMapping.MatchType == "Exact")
        {
            NewMapping.MatchValue2 = null;
            NewMapping.MatchValue3 = null;
        }

        await _repository.AddAsync(NewMapping);
        await _repository.SaveChangesAsync();
        return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", page });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, string? keyword, string? matchType, string? sort, int page = 1)
    {
        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();
        return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", sort = sort ?? "", page = page });
    }

    public async Task<IActionResult> OnPostEditAsync(string? keyword, string? matchType, string? sort, int page = 1)
    {
        var existing = await _repository.GetByIdAsync(EditMapping.Id);
        if (existing == null)
            return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", sort = sort ?? "", page = page });

        existing.MatchType = EditMapping.MatchType;
        existing.MatchValue1 = EditMapping.MatchValue1;
        existing.MatchValue2 = EditMapping.MatchValue2;
        existing.MatchValue3 = EditMapping.MatchValue3;
        existing.ReplaceMessage = EditMapping.ReplaceMessage;

        if (EditMapping.MatchType == "Exact")
        {
            EditMapping.MatchValue2 = null;
            EditMapping.MatchValue3 = null;
        }

        await _repository.UpdateAsync(existing);
        await _repository.SaveChangesAsync();
        return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", page });
    }

    public async Task<IActionResult> OnPostImportAsync(IFormFile csvFile, string? keyword, string? matchType, string? sort, int page = 1)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            TempData["Message"] = "請選擇 CSV 檔案";
            return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", sort = sort ?? "newest", page });
        }

        int successCount = 0;
        var skippedDetails = new List<string>();

        using var reader = new StreamReader(csvFile.OpenReadStream());
        var lineNumber = 0;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                skippedDetails.Add($"第 {lineNumber} 行：空行");
                continue;
            }

            // 標題列
            if (lineNumber == 1 && line.Contains("MatchType", StringComparison.OrdinalIgnoreCase))
                continue;

            var parts = line.Split(',');

            if (parts.Length < 2)
            {
                skippedDetails.Add($"第 {lineNumber} 行：欄位不足");
                continue;
            }

            var rawType = parts[0].Trim();
            var rawValue1 = parts.Length > 1 ? parts[1].Trim() : "";

            // MatchType 或 MatchValue1 空白就跳過
            if (string.IsNullOrWhiteSpace(rawType) || string.IsNullOrWhiteSpace(rawValue1))
            {
                skippedDetails.Add($"第 {lineNumber} 行：類型或條件1為空白 → {line}");
                continue;
            }

            var mapping = new ErrorMapping
            {
                MatchType = rawType,
                MatchValue1 = rawValue1,
                MatchValue2 = parts.Length > 2 ? parts[2].Trim() : null,
                MatchValue3 = parts.Length > 3 ? parts[3].Trim() : null,
                ReplaceMessage = parts.Length > 4 ? parts[4].Trim() : null,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "CSV"
            };

            if (mapping.MatchType != "Exact" && mapping.MatchType != "Fuzzy")
            {
                skippedDetails.Add($"第 {lineNumber} 行：類型不正確（{mapping.MatchType}）");
                continue;
            }

            if (mapping.MatchType == "Exact")
            {
                mapping.MatchValue2 = null;
                mapping.MatchValue3 = null;
            }

            await _repository.AddAsync(mapping);
            successCount++;
        }

        if (successCount > 0)
            await _repository.SaveChangesAsync();

        // 組合訊息
        var message = $"CSV 匯入完成：成功 {successCount} 筆";
        if (skippedDetails.Count > 0)
        {
            message += $"，跳過 {skippedDetails.Count} 筆";
            // 只顯示前 5 筆跳過明細，避免訊息過長
            var preview = string.Join("；", skippedDetails.Take(5));
            if (skippedDetails.Count > 5)
                preview += "……";
            message += $"（{preview}）";
        }

        TempData["Message"] = message;
        return RedirectToPage("/Index", new { keyword = keyword ?? "", matchType = matchType ?? "", sort = sort ?? "newest", page });
    }
}