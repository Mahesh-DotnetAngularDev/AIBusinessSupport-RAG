using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIBusinessSupport.API.AI
{
    public interface IAIService
    {
        Task<string> ChatAsync(Guid conversationId, string message);
        IAsyncEnumerable<string> ChatStreamAsync(
    Guid conversationId,
    string message,
    CancellationToken cancellationToken = default);
    }
}