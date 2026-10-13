namespace ErrorMessageMapping.Application;

public class MappingConvertResult
{
    public string MatchType { get; set; } = "None";
    public string SourceMessage { get; set; } = string.Empty;
    public string TargetMessage { get; set; } = string.Empty;
    public bool IsHit { get; set; }
}