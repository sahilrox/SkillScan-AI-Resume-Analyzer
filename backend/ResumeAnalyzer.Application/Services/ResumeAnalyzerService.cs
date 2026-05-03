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

    public async Task<object> AnalyzeAsync(string resume, string jd)
    {
        // 🔥 Step 1: Get embeddings
        var resumeEmbedding = await _embeddingService.GetEmbeddingAsync(resume);
        var jdEmbedding = await _embeddingService.GetEmbeddingAsync(jd);

        // 🔥 Step 2: Calculate similarity
        var similarity = SimilarityHelper.CosineSimilarity(resumeEmbedding, jdEmbedding);

        // 🔥 Step 3: Get LLM analysis
        var analysis = await _llmService.AnalyzeResumeAsync(resume, jd);

        // 🔥 Step 4: Return combined response
        return new
        {
            similarityScore = similarity,
            analysis = analysis
        };
    }
}