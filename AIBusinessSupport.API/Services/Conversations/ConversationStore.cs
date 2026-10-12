using Microsoft.Extensions.AI;

namespace AIBusinessSupport.Api.Services.Conversations;

public class ConversationStore : IConversationStore
{
    private readonly Dictionary<Guid, List<ChatMessage>> _conversations = new();

    public List<ChatMessage> GetMessages(Guid conversationId)
    {
        if (!_conversations.TryGetValue(
                conversationId,
                out var messages))
        {
            messages = new List<ChatMessage>();

            _conversations[conversationId] = messages;
        }

        return messages;
    }

    public void AddMessage(
        Guid conversationId,
        ChatMessage message)
    {
        var messages = GetMessages(conversationId);

        messages.Add(message);
    }
}