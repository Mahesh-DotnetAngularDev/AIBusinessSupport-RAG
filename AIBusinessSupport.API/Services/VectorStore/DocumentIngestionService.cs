
using AIBusinessSupport.Api.Services.Embeddings;
using Npgsql;

namespace AIBusinessSupport.Api.Services.VectorStore;

public sealed class DocumentIngestionService(
    TextChunkingService chunkingService,
    EmbeddingService embeddingService,
    DocumentChunkRepository repository,
    VectorDbConnectionFactory connectionFactory)
{
    public async Task<int> IngestTextAsync(
        string documentName,
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentName))
            throw new ArgumentException(
                "Document name cannot be empty.", nameof(documentName));

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException(
                "Document text cannot be empty.", nameof(text));

        var chunks = chunkingService.SplitIntoChunks(text);

        if (chunks.Count == 0)
            throw new InvalidOperationException(
                "No valid text chunks were generated.");

        // Generate all embeddings before touching existing data.
        var embeddedChunks = new List<(string Text, float[] Embedding)>();

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var embedding = await embeddingService.GenerateEmbeddingAsync(
                chunk, cancellationToken);

            embeddedChunks.Add((chunk, embedding));
        }

        // Replace existing chunks atomically.
        await using var connection =
            await connectionFactory.CreateConnectionAsync(cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await repository.DeleteChunksAsync(
                connection, transaction, documentName, cancellationToken);

            foreach (var item in embeddedChunks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await repository.SaveChunkAsync(
                    connection,
                    transaction,
                    documentName,
                    item.Text,
                    item.Embedding,
                    cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return chunks.Count;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
