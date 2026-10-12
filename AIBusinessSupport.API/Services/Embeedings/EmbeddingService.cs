
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AIBusinessSupport.Api.Services.Embeddings;

public sealed class EmbeddingService(HttpClient httpClient)
{
    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Text cannot be empty.", nameof(text));
        }

        var response = await httpClient.PostAsJsonAsync(
            "/embed",
            new EmbeddingRequest(text),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<EmbeddingResponse>(
                cancellationToken: cancellationToken);

        if (result?.Embedding is not { Length: 384 } embedding)
        {
            throw new InvalidOperationException(
                "The embedding service did not return 384 dimensions.");
        }

        return embedding;
    }

    private sealed record EmbeddingRequest(
        [property: JsonPropertyName("text")] string Text);

    private sealed record EmbeddingResponse(
        [property: JsonPropertyName("embedding")] float[] Embedding);
}
