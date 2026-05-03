namespace ResumeAnalyzer.Application.DTOs;

public class AnalysisResult
{
    public int Score { get; set; }
    public List<string> MissingSkills { get; set; }
    public List<string> Strengths { get; set; }
    public List<string> Suggestions { get; set; }
}