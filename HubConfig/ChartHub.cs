using Microsoft.AspNetCore.SignalR;
using BacktesterAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BacktesterAPI.HubConfig
{
    public class ChartHub : Hub
    {
        public async Task BroadcastChartData(List<ChartModel> data) => await Clients.All.SendAsync("broadcastchartdata", data);
    }
}
