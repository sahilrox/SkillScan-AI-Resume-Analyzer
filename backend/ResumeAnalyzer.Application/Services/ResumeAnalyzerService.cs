using ResumeAnalyzer.Application.Interfaces;
using ResumeAnalyzer.Application.DTOs;
using ResumeAnalyzer.Application.Helpers;

namespace ResumeAnalyzer.Application.Services;

public class ResumeAnalyzerService
{
    private readonly ILLMService _llmService;
    private readonly IEmbeddingService _embeddingService;

    public ResumeAnalyzerService(
    ILLMService llmService,
    IEmbeddingService embeddingService)
    {
        _llmService = llmService;
        _embeddingService = embeddingService;
    }

    public async Task<AnalysisResult> AnalyzeAsync(string resume, string jd)
    {
        return await _llmService.AnalyzeResumeAsync(resume, jd);
    }
}