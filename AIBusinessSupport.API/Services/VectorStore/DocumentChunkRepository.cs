
using Npgsql;
using System.Numerics;
using PgVectorValue = Pgvector.Vector;

namespace AIBusinessSupport.Api.Services.VectorStore;

public sealed class DocumentChunkRepository(
    VectorDbConnectionFactory connectionFactory)
{
    public async Task SaveChunkAsync(
        string documentName,
        string chunkText,
        float[] embedding,
        CancellationToken cancellationToken = default)
    {
        if (embedding.Length != 384)
        {
            throw new ArgumentException(
                "Embedding must contain exactly 384 dimensions.",
                nameof(embedding));
        }

        await using var connection =
            await connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            INSERT INTO document_chunks
                (document_name, chunk_text, embedding)
            VALUES
                (@documentName, @chunkText, @embedding);
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("documentName", documentName);
        command.Parameters.AddWithValue("chunkText", chunkText);
        command.Parameters.AddWithValue(
            "embedding",
            new PgVectorValue(embedding));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
    public async Task DeleteChunksAsync(
    string documentName,
    CancellationToken cancellationToken = default)
    {
        await using var connection =
            await connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
        DELETE FROM document_chunks
        WHERE document_name = @documentName;
        """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("documentName", documentName);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveChunkAsync(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    string documentName,
    string chunkText,
    float[] embedding,
    CancellationToken cancellationToken = default)
    {
        if (embedding.Length != 384)
        {
            throw new ArgumentException(
                "Embedding must contain exactly 384 dimensions.",
                nameof(embedding));
        }

        const string sql = """
        INSERT INTO document_chunks
            (document_name, chunk_text, embedding)
        VALUES
            (@documentName, @chunkText, @embedding);
        """;

        await using var command = new NpgsqlCommand(
            sql, connection, transaction);

        command.Parameters.AddWithValue("documentName", documentName);
        command.Parameters.AddWithValue("chunkText", chunkText);
        command.Parameters.AddWithValue(
            "embedding",
            new PgVectorValue(embedding));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
    public async Task DeleteChunksAsync(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    string documentName,
    CancellationToken cancellationToken = default)
    {
        const string sql = """
        DELETE FROM document_chunks
        WHERE document_name = @documentName;
        """;

        await using var command = new NpgsqlCommand(
            sql, connection, transaction);

        command.Parameters.AddWithValue("documentName", documentName);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
    public async Task<List<object>> GetDocumentsAsync(
    CancellationToken cancellationToken = default)
    {
        await using var connection =
            await connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
        SELECT
            document_name,
            COUNT(*) AS chunk_count,
            MAX(created_at) AS uploaded_at
        FROM document_chunks
        GROUP BY document_name
        ORDER BY MAX(created_at) DESC;
        """;

        await using var command = new NpgsqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var documents = new List<object>();

        while (await reader.ReadAsync(cancellationToken))
        {
            documents.Add(new
            {
                documentName = reader.GetString(0),
                chunkCount = reader.GetInt64(1),
                uploadedAt = reader.GetDateTime(2)
            });
        }

        return documents;
    }
    public async Task<int> DeleteDocumentAsync(
    string documentName,
    CancellationToken cancellationToken = default)
    {
        await using var connection =
            await connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
        DELETE FROM document_chunks
        WHERE document_name = @documentName;
        """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("documentName", documentName);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
