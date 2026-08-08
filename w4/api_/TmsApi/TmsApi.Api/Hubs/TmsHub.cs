using Microsoft.AspNetCore.SignalR;
using TmsApi.Application.Hubs;

namespace TmsApi.Api.Hubs;

public class TmsHub : Hub<ITmsHubClient>
{
    public override async Task OnConnectedAsync()
    {
        // Grab studentId from the URL (e.g., /hubs/tms?studentId=1)
        var studentId = Context.GetHttpContext()?.Request.Query["studentId"].ToString();
        
        if (!string.IsNullOrWhiteSpace(studentId))
        {
            // Add this specific user to their own private group
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.Student(studentId));
        }
        await base.OnConnectedAsync();
    }
}

public static class GroupNames
{
    public static string Student(string studentId) => $"student-{studentId}";
}