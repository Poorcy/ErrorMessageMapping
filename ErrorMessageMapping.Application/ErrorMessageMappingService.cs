using ErrorMessageMapping.Domain;

namespace ErrorMessageMapping.Application;

public class ErrorMessageMappingService
{
    private readonly IErrorMessageMappingRepository _repository;

    public ErrorMessageMappingService(IErrorMessageMappingRepository repository)
    {
        _repository = repository;
    }

    public async Task<MappingConvertResult> ConvertAsync(string rawMessage)
    {
        var raw = (rawMessage ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(raw))
        {
            return new MappingConvertResult { SourceMessage = raw };
        }

        // 1. 先做精準比對
        var exactList = await _repository.GetByMatchTypeAsync(MappingConstants.MatchTypeExact);
        foreach (var item in exactList.OrderBy(x => x.CreatedAt))
        {
            if (string.Equals(item.MatchValue1.Trim(), raw, StringComparison.OrdinalIgnoreCase))
            {
                return new MappingConvertResult
                {
                    MatchType = MappingConstants.MatchTypeExact,
                    SourceMessage = raw,
                    TargetMessage = item.ReplaceMessage ?? string.Empty,
                    IsHit = true
                };
            }
        }

        // 2. 再做模糊比對（條件多的優先，相同條件數則早建立的優先）
        var fuzzyList = await _repository.GetByMatchTypeAsync(MappingConstants.MatchTypeFuzzy);
        var orderedFuzzy = fuzzyList
            .Select(x => new
            {
                Mapping = x,
                ConditionCount = CountConditions(x)
            })
            .Where(x => x.ConditionCount > 0)
            .OrderByDescending(x => x.ConditionCount)
            .ThenBy(x => x.Mapping.CreatedAt);

        foreach (var item in orderedFuzzy)
        {
            if (IsFuzzyMatch(raw, item.Mapping))
            {
                return new MappingConvertResult
                {
                    MatchType = MappingConstants.MatchTypeFuzzy,
                    SourceMessage = raw,
                    TargetMessage = item.Mapping.ReplaceMessage ?? string.Empty,
                    IsHit = true
                };
            }
        }

        // 未命中
        return new MappingConvertResult
        {
            MatchType = "None",
            SourceMessage = raw,
            TargetMessage = raw,
            IsHit = false
        };
    }

    private static int CountConditions(ErrorMapping mapping)
    {
        int count = 0;
        if (!string.IsNullOrWhiteSpace(mapping.MatchValue1)) count++;
        if (!string.IsNullOrWhiteSpace(mapping.MatchValue2)) count++;
        if (!string.IsNullOrWhiteSpace(mapping.MatchValue3)) count++;
        return count;
    }

    private static bool IsFuzzyMatch(string raw, ErrorMapping mapping)
    {
        var upper = raw.ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(mapping.MatchValue1) &&
            !upper.Contains(mapping.MatchValue1.Trim().ToUpperInvariant()))
            return false;

        if (!string.IsNullOrWhiteSpace(mapping.MatchValue2) &&
            !upper.Contains(mapping.MatchValue2.Trim().ToUpperInvariant()))
            return false;

        if (!string.IsNullOrWhiteSpace(mapping.MatchValue3) &&
            !upper.Contains(mapping.MatchValue3.Trim().ToUpperInvariant()))
            return false;

        return true;
    }

    public async Task AddMappingAsync(ErrorMapping mapping)
    {
        mapping.CreatedAt = DateTime.UtcNow;
        await _repository.AddAsync(mapping);
        await _repository.SaveChangesAsync();
    }
}