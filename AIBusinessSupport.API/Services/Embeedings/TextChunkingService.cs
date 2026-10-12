
namespace AIBusinessSupport.Api.Services.Embeddings;

public sealed class TextChunkingService
{
    public List<string> SplitIntoChunks(
        string text,
        int chunkSize = 500,
        int overlap = 100)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        if (chunkSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSize));
        }

        if (overlap < 0 || overlap >= chunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(overlap));
        }

        var chunks = new List<string>();
        var start = 0;

        while (start < text.Length)
        {
            var length = Math.Min(chunkSize, text.Length - start);
            var chunk = text.Substring(start, length).Trim();

            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }

            if (start + length >= text.Length)
            {
                break;
            }

            start += chunkSize - overlap;
        }

        return chunks;
    }
}
