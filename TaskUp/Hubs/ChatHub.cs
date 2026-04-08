using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TaskUp.Services.Chat;
using TaskUp.Models;

namespace TaskUp.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public async Task JoinBoard(int boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"board-{boardId}");
        _logger.LogInformation($"User {Context.UserIdentifier} joined board {boardId}");
    }

    public async Task LeaveBoard(int boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board-{boardId}");
    }

    public async Task SendMessage(int boardId, string content)
    {
        var userId = Context.UserIdentifier;
        var userName = Context.User?.Identity?.Name ?? "Unknown";
        
        var message = await _chatService.SendMessageAsync(boardId, userId, userName, content, null);
        
        await Clients.Group($"board-{boardId}").SendAsync("ReceiveMessage", new
        {
            message.Id,
            message.UserId,
            message.UserName,
            message.Content,
            message.CreatedAt
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"User {Context.UserIdentifier} disconnected");
        await base.OnDisconnectedAsync(exception);
    }
}