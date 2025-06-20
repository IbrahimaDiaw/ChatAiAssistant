namespace ChatAI_Assistant.Shared.DTOs.AI;

public class AIContentAnalysis
{
    public string Sentiment { get; set; } = "neutral";
    public double SentimentScore { get; set; } = 0.0;
    public Dictionary<string, double> EmotionScores { get; set; } = new();
    public string Intent { get; set; } = "general";
    public double IntentConfidence { get; set; } = 0.0;
    public List<string> Topics { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public double Toxicity { get; set; } = 0.0;
    public bool IsAppropriate { get; set; } = true;
    public string Language { get; set; } = "en";
}