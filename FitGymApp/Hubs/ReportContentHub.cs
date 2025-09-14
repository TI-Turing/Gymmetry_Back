using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FitGymApp.Hubs
{
    public class ReportContentHub : Hub
    {
        // Groups idea: moderators join "moderators" group from client
        public async Task JoinModerators()
        {
            if (Context.User != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "moderators");
            }
        }
    }
}
