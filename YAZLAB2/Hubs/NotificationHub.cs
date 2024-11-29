using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace YAZLAB2.Hubs  // YourNamespace kısmını projenizin ad alanına göre değiştirin.
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
