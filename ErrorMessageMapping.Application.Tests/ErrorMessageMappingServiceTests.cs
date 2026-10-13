using ErrorMessageMapping.Application;
using ErrorMessageMapping.Domain;

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

        var service = new ErrorMessageMappingService(repo);

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

        var service = new ErrorMessageMappingService(repo);

        var result = await service.ConvertAsync("Your limit price is far from reference price");

        Assert.True(result.IsHit);
        Assert.Equal(MappingConstants.MatchTypeFuzzy, result.MatchType);
        Assert.Equal("價格偏離過大", result.TargetMessage);
    }

    [Fact]
    public async Task Convert_NoMatch_ShouldReturnOriginal()
    {
        var repo = new InMemoryErrorMappingRepository();
        var service = new ErrorMessageMappingService(repo);

        var result = await service.ConvertAsync("hello world");

        Assert.False(result.IsHit);
        Assert.Equal("None", result.MatchType);
        Assert.Equal("hello world", result.TargetMessage);
    }

    [Fact]
    public async Task Convert_ExactShouldWinOverFuzzy()
    {
        var repo = new InMemoryErrorMappingRepository();
        repo.Seed(new ErrorMapping
        {
            MatchType = MappingConstants.MatchTypeExact,
            MatchValue1 = "Work Complete",
            ReplaceMessage = "精確結果"
        });
        repo.Seed(new ErrorMapping
        {
            MatchType = MappingConstants.MatchTypeFuzzy,
            MatchValue1 = "Work",
            MatchValue2 = "Complete",
            ReplaceMessage = "不該用這句"
        });

        var service = new ErrorMessageMappingService(repo);
        var result = await service.ConvertAsync("Work Complete");

        Assert.Equal(MappingConstants.MatchTypeExact, result.MatchType);
        Assert.Equal("精確結果", result.TargetMessage);
    }
}