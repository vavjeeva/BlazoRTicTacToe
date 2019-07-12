using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazoRTicTacToe
{
    public class GameHub : Hub
    {
        ILogger<GameHub> _logger;

        public GameHub(ILogger<GameHub> logger)
        {
            _logger = logger;
            _logger.LogInformation("GameHub Created");
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("New Connection from Client");
            await base.OnConnectedAsync();
        }

        public async Task OnMoveReceived(int index, string player)
        {            
            await Clients.Others.SendAsync("OnMoveReceived", index, player);
        }
    }
}
