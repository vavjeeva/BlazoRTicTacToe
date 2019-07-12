using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazoRTicTacToeBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private string[] _currentBoard = new string[9];
        private readonly string _botPlayer = "O";
        private HubConnection connection;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        async Task OnMoveReceived(int index, string humanPlayer)
        {
            _currentBoard[index] = humanPlayer;
            GameEngine engine = new GameEngine(_currentBoard, _botPlayer, humanPlayer);
            MoveResultVO moveResultVO = engine.GetBestSpot(_currentBoard, _botPlayer);
            string methodToInvoke = "OnMoveReceived";
            //if (moveResultVO.score == -1)
            //{
            //    //Human Player Won
            //    methodToInvoke = "OnMatchLoss";
            //}
            //else if (moveResultVO.score == 0)
            //{
            //    methodToInvoke = "OnMatchTie";
            //}

            await connection.InvokeAsync(methodToInvoke, int.Parse(moveResultVO.index), _botPlayer);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            for (var i = 0; i < 9; i++)
            {
                _currentBoard[i] = i.ToString();
            }

            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/gamehub")
                .ConfigureLogging(logging => {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddConsole();
                })
                .Build();
            connection.On<int, string>("OnMoveReceived", OnMoveReceived);            
            await connection.StartAsync(); // Start the connection.

            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
