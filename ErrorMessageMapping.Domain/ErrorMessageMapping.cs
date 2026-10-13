namespace ErrorMessageMapping.Domain;

public class ErrorMapping
{
    public int Id { get; set; }
    public string MatchType { get; set; } = "Exact";
    public string MatchValue1 { get; set; } = string.Empty;
    public string? MatchValue2 { get; set; }
    public string? MatchValue3 { get; set; }
    public string? ReplaceMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "System";
}