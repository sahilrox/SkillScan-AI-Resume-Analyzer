using ResumeAnalyzer.Application.DTOs;

namespace ResumeAnalyzer.Application.Interfaces;

public interface ILLMService
{
    Task<AnalysisResult> AnalyzeResumeAsync(string resume, string jd);
}