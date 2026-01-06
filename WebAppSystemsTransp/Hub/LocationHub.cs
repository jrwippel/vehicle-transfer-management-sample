using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WebAppSystemsTransp.Hubs
{
    public class LocationHub : Hub
    {
        public async Task SendLocation(string vehicleId, double latitude, double longitude)
        {
            await Clients.All.SendAsync("ReceiveLocationUpdate", vehicleId, latitude, longitude);
        }
    }
}
