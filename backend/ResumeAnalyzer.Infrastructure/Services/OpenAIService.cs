using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ResumeAnalyzer.Application.Interfaces;
using ResumeAnalyzer.Application.DTOs;

namespace ResumeAnalyzer.Infrastructure.Services;

public class OpenAIService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAIService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    public async Task<AnalysisResult> AnalyzeResumeAsync(string resume, string jd)
    {
        var prompt = $@"
You are an expert technical recruiter.

Analyze the resume against the job description.

Return ONLY JSON in this format:
{{
  ""score"": number,
  ""missingSkills"": [string],
  ""strengths"": [string],
  ""suggestions"": [string]
}}

Resume:
{resume}

Job Description:
{jd}
Give a score between 0 and 100 based on relevance.
Do not give extremely low scores unless the resume is completely unrelated.
Always return at least 2–3 missing skills if any mismatch exists.
";

        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/chat/completions"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        // 🔥 Handle HTTP errors
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"OpenAI API error: {json}");
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // 🔥 Safe parsing (no crash)
        if (!root.TryGetProperty("choices", out var choices))
        {
            throw new Exception($"Invalid OpenAI response: {json}");
        }

        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (!string.IsNullOrWhiteSpace(content))
        {
            int start = content.IndexOf('{');
            int end = content.LastIndexOf('}');

            if (start >= 0 && end > start)
            {
                content = content.Substring(start, end - start + 1);
            }
            else
            {
                throw new Exception($"Invalid JSON format from LLM: {content}");
            }
        }

        // ✅ ADD FIX HERE 👇

        if (content.StartsWith("```"))
        {
            var firstBrace = content.IndexOf("{");
            var lastBrace = content.LastIndexOf("}");

            if (firstBrace != -1 && lastBrace != -1)
            {
                content = content.Substring(firstBrace, lastBrace - firstBrace + 1);
            }
        }

        // Optional safety
        content = content.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new Exception("Empty response from OpenAI");
        }

        // 🔥 Deserialize to strongly typed model
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var result = JsonSerializer.Deserialize<AnalysisResult>(content, options);

            if (result == null)
                throw new Exception("Deserialization returned null");

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse AI response: {content}", ex);
        }
    }
}