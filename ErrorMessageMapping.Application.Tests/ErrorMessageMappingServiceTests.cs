using ErrorMessageMapping.Application;
using ErrorMessageMapping.Domain;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErrorMessageMapping.Application.Tests;

public class ErrorMessageMappingServiceTests
{
    [Fact]
    public async Task Convert_ExactMatch_ShouldReturnExact()
    {
        // Arrange
        var repo = new InMemoryErrorMappingRepository();
        repo.Seed(new ErrorMapping
        {
            MatchType = MappingConstants.MatchTypeExact,
            MatchValue1 = "Work Complete",
            ReplaceMessage = "此筆已處理完成"
        });

        var service = new ErrorMessageMappingService(repo, NullLogger<ErrorMessageMappingService>.Instance);

        // Act
        var result = await service.ConvertAsync("  work complete  ");

        // Assert
        Assert.True(result.IsHit);
        Assert.Equal(MappingConstants.MatchTypeExact, result.MatchType);
        Assert.Equal("此筆已處理完成", result.TargetMessage);
    }

    [Fact]
    public async Task Convert_FuzzyMatch_ShouldReturnFuzzy()
    {
        var repo = new InMemoryErrorMappingRepository();
        repo.Seed(new ErrorMapping
        {
            MatchType = MappingConstants.MatchTypeFuzzy,
            MatchValue1 = "limit price",
            MatchValue2 = "reference price",
            ReplaceMessage = "價格偏離過大"
        });

        var service = new ErrorMessageMappingService(repo, NullLogger<ErrorMessageMappingService>.Instance);

        var result = await service.ConvertAsync("Your limit price is far from reference price");

        Assert.True(result.IsHit);
        Assert.Equal(MappingConstants.MatchTypeFuzzy, result.MatchType);
        Assert.Equal("價格偏離過大", result.TargetMessage);
    }

    [Fact]
    public async Task Convert_NoMatch_ShouldReturnOriginal()
    {
        var repo = new InMemoryErrorMappingRepository();
        var service = new ErrorMessageMappingService(repo, NullLogger<ErrorMessageMappingService>.Instance);

        var result = await service.ConvertAsync("hello world");

        Assert.False(result.IsHit);
        Assert.Equal("None", result.MatchType);
        Assert.Equal("hello world", result.TargetMessage);
    }

    //[Fact]
    //public async Task Convert_ExactShouldWinOverFuzzy()
    //{
    //    var repo = new InMemoryErrorMappingRepository();
    //    repo.Seed(new ErrorMapping
    //    {
    //        MatchType = MappingConstants.MatchTypeExact,
    //        MatchValue1 = "Work Complete",
    //        ReplaceMessage = "精確結果"
    //    });
    //    repo.Seed(new ErrorMapping
    //    {
    //        MatchType = MappingConstants.MatchTypeFuzzy,
    //        MatchValue1 = "Work",
    //        MatchValue2 = "Complete",
    //        ReplaceMessage = "不該用這句"
    //    });

    //    var service = new ErrorMessageMappingService(repo, NullLogger<ErrorMessageMappingService>.Instance);
    //    var result = await service.ConvertAsync("Work Complete");

    //    Assert.Equal(MappingConstants.MatchTypeExact, result.MatchType);
    //    Assert.Equal("精確結果", result.TargetMessage);
    //}
    [Fact]
    public async Task Convert_EmptyOrNull_ShouldReturnEmpty()
    {
        var repo = new InMemoryErrorMappingRepository();
        var service = new ErrorMessageMappingService(repo, NullLogger<ErrorMessageMappingService>.Instance);

        var result1 = await service.ConvertAsync("");
        var result2 = await service.ConvertAsync(null!);
        var result3 = await service.ConvertAsync("   ");

        Assert.False(result1.IsHit);
        Assert.False(result2.IsHit);
        Assert.False(result3.IsHit);
    }

    [Fact]
    public async Task Convert_Exact_ShouldIgnoreCaseAndTrim()
    {
        var repo = new InMemoryErrorMappingRepository();
        repo.Seed(new ErrorMapping
        {
            MatchType = MappingConstants.MatchTypeExact,
            MatchValue1 = "Work Complete",
            ReplaceMessage = "已完成"
        });

        var service = new ErrorMessageMappingService(repo, NullLogger<ErrorMessageMappingService>.Instance);
        var result = await service.ConvertAsync("  work complete  ");

        Assert.True(result.IsHit);
        Assert.Equal("Exact", result.MatchType);
        Assert.Equal("已完成", result.TargetMessage);
    }

    [Fact]
    public async Task Convert_ExactShouldWinOverFuzzy()
    {
        var repo = new InMemoryErrorMappingRepository();
        repo.Seed(new ErrorMapping
        {
            MatchType = MappingConstants.MatchTypeExact,
            MatchValue1 = "Work Complete",
            ReplaceMessage = "精準結果"
        });
        repo.Seed(new ErrorMapping
        {
            MatchType = MappingConstants.MatchTypeFuzzy,
            MatchValue1 = "Work",
            MatchValue2 = "Complete",
            ReplaceMessage = "不該命中"
        });

        var service = new ErrorMessageMappingService(repo, NullLogger<ErrorMessageMappingService>.Instance);
        var result = await service.ConvertAsync("Work Complete");

        Assert.Equal("Exact", result.MatchType);
        Assert.Equal("精準結果", result.TargetMessage);
    }

    [Fact]
    public async Task Convert_Fuzzy_MoreConditionsShouldWin()
    {
        var repo = new InMemoryErrorMappingRepository();
        repo.Seed(new ErrorMapping
        {
            MatchType = MappingConstants.MatchTypeFuzzy,
            MatchValue1 = "limit",
            ReplaceMessage = "條件少的"
        });
        repo.Seed(new ErrorMapping
        {
            MatchType = MappingConstants.MatchTypeFuzzy,
            MatchValue1 = "limit",
            MatchValue2 = "price",
            ReplaceMessage = "條件多的"
        });

        var service = new ErrorMessageMappingService(repo, NullLogger<ErrorMessageMappingService>.Instance);
        var result = await service.ConvertAsync("your limit price is too high");

        Assert.Equal("Fuzzy", result.MatchType);
        Assert.Equal("條件多的", result.TargetMessage);
    }

    [Fact]
    public async Task Convert_Fuzzy_MissingCondition_ShouldNotMatch()
    {
        var repo = new InMemoryErrorMappingRepository();
        repo.Seed(new ErrorMapping
        {
            MatchType = MappingConstants.MatchTypeFuzzy,
            MatchValue1 = "limit",
            MatchValue2 = "reference",
            ReplaceMessage = "不該命中"
        });

        var service = new ErrorMessageMappingService(repo, NullLogger<ErrorMessageMappingService>.Instance);
        var result = await service.ConvertAsync("only limit appears here");

        Assert.False(result.IsHit);
        Assert.Equal("None", result.MatchType);
    }

    [Fact]
    public async Task Convert_ReplaceMessageNull_ShouldReturnEmptyString()
    {
        var repo = new InMemoryErrorMappingRepository();
        repo.Seed(new ErrorMapping
        {
            MatchType = MappingConstants.MatchTypeExact,
            MatchValue1 = "Test",
            ReplaceMessage = null
        });

        var service = new ErrorMessageMappingService(repo, NullLogger<ErrorMessageMappingService>.Instance);
        var result = await service.ConvertAsync("Test");

        Assert.True(result.IsHit);
        Assert.Equal(string.Empty, result.TargetMessage);
    }
}