
using Npgsql;
using AIBusinessSupport.Api.Services.Embeddings;

namespace AIBusinessSupport.Api.Services.VectorStore;

public sealed class DocumentSearchRepository(
    VectorDbConnectionFactory connectionFactory,
    EmbeddingService embeddingService)
{
    public async Task<List<string>> SearchAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        var embedding = await embeddingService.GenerateEmbeddingAsync(
            question, cancellationToken);

        await using var connection =
            await connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT chunk_text
            FROM document_chunks
            WHERE embedding IS NOT NULL
            ORDER BY embedding <=> @embedding
            LIMIT 5;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue(
            "embedding",
            new Pgvector.Vector(embedding));

        var results = new List<string>();

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }
}
