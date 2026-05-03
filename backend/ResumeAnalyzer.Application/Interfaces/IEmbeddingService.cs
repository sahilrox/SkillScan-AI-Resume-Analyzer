namespace ResumeAnalyzer.Application.Interfaces;

public interface IEmbeddingService
{
    Task<List<float>> GetEmbeddingAsync(string text);
}