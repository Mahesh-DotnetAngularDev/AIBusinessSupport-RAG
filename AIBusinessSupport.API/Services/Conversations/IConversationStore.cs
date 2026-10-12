using Microsoft.Extensions.AI;

namespace AIBusinessSupport.Api.Services.Conversations;

public interface IConversationStore
{
    List<ChatMessage> GetMessages(Guid conversationId);

    void AddMessage(
        Guid conversationId,
        ChatMessage message);
}