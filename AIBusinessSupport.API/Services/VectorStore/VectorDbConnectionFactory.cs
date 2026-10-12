
using Npgsql;

namespace AIBusinessSupport.Api.Services.VectorStore;

public sealed class VectorDbConnectionFactory : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public VectorDbConnectionFactory(IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("pgvector")
            ?? throw new InvalidOperationException(
                "The pgvector connection string is not configured.");

        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseVector();

        _dataSource = builder.Build();
    }

    public async Task<NpgsqlConnection> CreateConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }

    public ValueTask DisposeAsync() => _dataSource.DisposeAsync();
}
