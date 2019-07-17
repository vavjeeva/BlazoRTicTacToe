## Realtime Blazor Tic-Tac-Toe game - Bot vs Multiplayer using signalR
![](https://jeevasubburaj.com/images/realtime-tic-tac-toe-blazor-game-with-bot-vs-multiplayer-using-signalR/BlazoRTicTacToe_Game.png)

In this article, we will see how to create a bot vs. multiplayer tic-tac-toe game in blazor. Blazor is an open source .NET web front-end framework that allows us to create client-side applications using C# and HTML. This is a simple asp.net core hosted server-side blazor front-end application with Game UI razor component and signalR game hub to connect players with the bot to play the game. The Game Bot is created with .Net Core Background Service with core game engine to identify the best move against a player using minimax recursive algorithm.  The entire source code is uploaded in my [github](https://github.com/vavjeeva/BlazoRTicTacToe) repository.

<!-- more -->

### Architecture

![Architecture](https://jeevasubburaj.com/images/realtime-tic-tac-toe-blazor-game-with-bot-vs-multiplayer-using-signalR/Architecture.png)

- **Tic-Tac-Toe Blazor App** - This is a server-side Blazor App with Tic-Tac-Toe UI razor component. The razor component will have the game board design and its logic.
- **SignalR Game Hub** -  This hub holds the signalR methods to send messages between player and bot.
- **Tic-Tac-Toe Bot Client** - Bot client is based on .Net Core background service and contains the core game engine using the minimax algorithm. Whenever the player sends the move details to signalR hub, it will send it to bot with the current board state and bot will get the next best spot available using core game engine and send it back to hub with the bot move. The hub will send back to the caller, and the player UI will get updated the move details in real time.

### How It Works - Demo

![](https://jeevasubburaj.com/images/realtime-tic-tac-toe-blazor-game-with-bot-vs-multiplayer-using-signalR/How_It_Works.gif)

### Steps

##### TicTacToe Blazor App

As a first step, Launch the Latest Visual Studio 2019 and create a new blazor project by selecting Asp.net Core web application and select the Blazor Server App.

![1563216885439](https://jeevasubburaj.com/images/realtime-tic-tac-toe-blazor-game-with-bot-vs-multiplayer-using-signalR/1563216885439.png)

![1563216913702](https://jeevasubburaj.com/images/realtime-tic-tac-toe-blazor-game-with-bot-vs-multiplayer-using-signalR/1563216913702.png)

I used the Blazor Server Side app for this example, but you can use client-side Blazor as well. Right now, client-side blazor app doesn't have any official blazor signalR client due to the [dependency of web socket support](https://github.com/aspnet/AspNetCore/issues/9825#issuecomment-488028164) in the runtime. However, there are community version of the blazor signalR client is available.

In the solution explorer, add a new Razor component called **TicTacToe.razor** file and put the Tic-Tac-Toe board design and logic in the component. It also initializes the signalR hub client.


``` html
@using Microsoft.AspNetCore.SignalR.Client
@using BlazoRTicTacToeGameEngine

@if (@playerWon != null)
{
    <div class="container h-100" style="width:500px;background:#ff6a00;padding:40px">
        <div class="row h-50 justify-content-center align-items-center">
            <span style="font-size:xx-large">@playerWon Won !</span>
        </div>
    </div>
}
else if (@isDraw)
{
    <div class="container h-100" style="width:600px;background:#ff6a00;padding:40px">
        <div class="row h-50 justify-content-center align-items-center">
            <span style="font-size:xx-large">It's a Draw !</span>
        </div>
    </div>
}
else if (@playerWon == null)
{

    <div class="container-fluid" style="width:500px;">
        <div class="row justify-content-center align-items-center">
            <div class="col-3 col-class text-center" @onclick="@(() => OnSelect(0))">
                <span style="font-size:xx-large">@ShowBoard(0)</span>
            </div>
            <div class="col-3 col-class text-center" @onclick="@(() => OnSelect(1))">
                <span style="font-size:xx-large">@ShowBoard(1)</span>
            </div>
            <div class="col-3 col-class text-center" @onclick="@(() => OnSelect(2))">
                <span style="font-size:xx-large">@ShowBoard(2)</span>
            </div>
        </div>
        <div class="row justify-content-center align-items-center">
            <div class="col-3 col-class text-center" @onclick="@(() => OnSelect(3))">
                <span style="font-size:xx-large">@ShowBoard(3)</span>
            </div>
            <div class="col-3 col-class text-center" @onclick="@(() => OnSelect(4))">
                <span style="font-size:xx-large">@ShowBoard(4)</span>
            </div>
            <div class="col-3 col-class text-center" @onclick="@(() => OnSelect(5))">
                <span style="font-size:xx-large">@ShowBoard(5)</span>
            </div>
        </div>
        <div class="row justify-content-center align-items-center">
            <div class="col-3 col-class text-center" @onclick="@(() => OnSelect(6))">
                <span style="font-size:xx-large">@ShowBoard(6)</span>
            </div>
            <div class="col-3 col-class text-center" @onclick="@(() => OnSelect(7))">
                <span style="font-size:xx-large">@ShowBoard(7)</span>
            </div>
            <div class="col-3 col-class text-center" @onclick="@(() => OnSelect(8))">
                <span style="font-size:xx-large">@ShowBoard(8)</span>
            </div>
        </div>
    </div>
}
<div class="text-center" style="padding:5px;">
    <button class="btn btn-link" style="color:#ff6a00;font-weight:600" @onclick="@(()=>RestartGame())">Restart</button>
</div>
```
In this component, we have three layouts. The main layout will render the tic-tac-toe board. The other two layouts will show the result of the winner or draw panel. The main layout is using the bootstrap container to design the board, and each cell is associated with onclick event method to notify the hub with the selected cell value.

``` csharp
@code {
    private string[] board = new string[9];
    HubConnection connection;
    GameEngine engine = new GameEngine();
    string playerWon = null;
    bool isDraw = false;

    protected async override Task OnInitAsync()
    {
        for (var i = 0; i < 9; i++)
        {
            board[i] = i.ToString();
        }

        //Initialize SignalR
        connection = new HubConnectionBuilder()
        .WithUrl("https://localhost:5001/gamehub")
        .Build();

        connection.On<string[]>("NotifyUser", NotifyUser);
        await connection.StartAsync();
    }

    Task NotifyUser(string[] newboard)
    {
        board = newboard;
        if (engine.IsWon(board, engine.botPlayer))
            playerWon = "Bot";
        else if (engine.GetAvailableSpots(board).Length == 0)
            isDraw = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task OnSelect(int index)
    {
        if (!engine.IsPlayed(board[index]))
        {
            board[index] = engine.humanPlayer;
            if (engine.IsWon(board, engine.humanPlayer))
                playerWon = "Player";
            else if (engine.GetAvailableSpots(board).Length == 0)
                isDraw = true;
            else
                await connection.InvokeAsync("OnUserMoveReceived", board);

            StateHasChanged();
        }
    }

    private string ShowBoard(int index)
    {
        return engine.IsPlayed(board[index]) ? board[index] : string.Empty;
    }

    private void RestartGame()
    {
        playerWon = null;
        isDraw = false;
        for (var i = 0; i < 9; i++)
        {
            board[i] = i.ToString();
        }
        StateHasChanged();
    }

}
```
In OnInitAsync Method, We initialize the board with the default index values. By default, the player will use **X** symbol and the bot will use **O** symbol to play. 

We will also initialize signalR hub in OnInitAsync method. On click of the Cell, OnSelect method gets executed, and board item array will now have the data with the player move and send the entire board array as a parameter to hub method **OnUserMoveReceived**. It also listens to **NotifyUser** Hub method which is invoked by a bot with its move. 

#### Game Hub

``` csharp
public class GameHub : Hub
    {
        ILogger<GameHub> _logger;
        private static readonly string BOT_GROUP = "BOT";

        public GameHub(ILogger<GameHub> logger)
        {
            _logger = logger;           
        }

        public async Task OnBotConnected()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, BOT_GROUP);
            _logger.LogInformation("Bot joined");
        }

        public async Task OnBotDisconnected()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, BOT_GROUP);
            _logger.LogInformation("Bot left");
        }

        public async Task OnBotMoveReceived(string[] board, string connectionID)
        {
            await Clients.Client(connectionID).SendAsync("NotifyUser", board);
        }

        public async Task OnUserMoveReceived(string[] board)
        {
            await Clients.Group(BOT_GROUP).SendAsync("NotifyBot", board, Context.ConnectionId);
        }
    }
```

This hub class will hold the following signalR methods.

- **OnBotConnected** - This method gets executed when the bot connected to signalR hub. It also adds the bot client into BOT group. This group is used to communicate with BOT only to send the message with the latest move from the player.
- **OnBotDisconnected** - This method gets executed when the bot disconnected from signalR hub. It also removes the bot from BOT group.
- **OnBotMoveReceived** - This method is used to notify the player (caller) after bot finish with the move and ready for the player to respond.
- **OnUserMoveReceived** - This method is used to notify the bot after the player finish with the move and ready for a bot to respond.

#### BlazoR TicTacToe Bot

``` csharp
public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private HubConnection connection;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        async Task NotifyBot(string[] board, string connectionID)
        {
            GameEngine engine = new GameEngine();
            _logger.LogInformation($"Move received from {connectionID}");
            Move move = engine.GetBestSpot(board, engine.botPlayer);
            board[int.Parse(move.index)] = engine.botPlayer;
            _logger.LogInformation($"Bot Move with the index of {move.index} send to {connectionID}");
            await connection.InvokeAsync("OnBotMoveReceived", board, connectionID);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/gamehub")
                .Build();
            connection.On<string[], string>("NotifyBot", NotifyBot);
            await connection.StartAsync(); // Start the connection.

            //Add to BOT Group When Bot Connected
            await connection.InvokeAsync("OnBotConnected");
            _logger.LogInformation("Bot connected");

            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            await connection?.InvokeAsync("OnBotDisconnected");
            connection?.DisposeAsync();
            _logger.LogInformation("Bot disconnected");
            await base.StopAsync(cancellationToken);
        }
```



The game bot is developed using .Net Core background service; when it started, it will connect to signalR hub. When it joins, it invokes **OnBotConnected** method to add it into the BOT signalR group. When it receives the message from hub with the board array data, it calculates the next best move by calling **GetBestSpot** method from the game engine and sends it back to the caller with its move.

When the background service is stopped, it disposes the signalR connection and remove it from BOT group.

#### Core Game Engine

``` csharp
public class GameEngine
    {
        public readonly string botPlayer = "O";
        public readonly string humanPlayer = "X";
        public Move GetBestSpot(string[] board, string player)
        {
            Move bestMove = null;
            var availableSpots = GetAvailableSpots(board);
            foreach (var spot in availableSpots)
            {
                string[] newboard = (string[])board.Clone();
                var newMove = new Move();
                newMove.index = spot;
                newboard[int.Parse(spot)] = player;

                if (!IsWon(newboard, player) && GetAvailableSpots(newboard).Length > 0)
                {
                    if (player == botPlayer)
                    {
                        var result = GetBestSpot(newboard, humanPlayer);
                        newMove.index = result.index;
                        newMove.score = result.score;
                    }
                    else
                    {
                        var result = GetBestSpot(newboard, botPlayer);
                        newMove.index = result.index;
                        newMove.score = result.score;
                    }
                }
                else
                {
                    if (IsWon(newboard, botPlayer))
                        newMove.score = 1;
                    else if (IsWon(newboard, humanPlayer))
                        newMove.score = -1;
                    else
                        newMove.score = 0;
                }

                if (bestMove == null ||
                    (player == botPlayer && newMove.score < bestMove.score) ||
                    (player == humanPlayer && newMove.score > bestMove.score))
                {
                    bestMove = newMove;
                }
            }
            return bestMove;
        }

        public string[] GetAvailableSpots(string[] board)
        {
            return board.Where(i => !IsPlayed(i)).ToArray();
        }

        public bool IsWon(string[] board, string player)
        {
            if (
                   (board[0] == player && board[1] == player && board[2] == player) ||
                   (board[3] == player && board[4] == player && board[5] == player) ||
                   (board[6] == player && board[7] == player && board[8] == player) ||
                   (board[0] == player && board[3] == player && board[6] == player) ||
                   (board[1] == player && board[4] == player && board[7] == player) ||
                   (board[2] == player && board[5] == player && board[8] == player) ||
                   (board[0] == player && board[4] == player && board[8] == player) ||
                   (board[2] == player && board[4] == player && board[6] == player)
                   )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsPlayed(string input)
        {
            return input == "X" || input == "O";
        }
    }

    public class Move
    {
        public int score;
        public string index;
    }
```

I used minimax algorithm in the game engine to find the best available spot. Minimax algorithm is a recursive algorithm which will play all possible movement by itself and as opponent until it reaches the terminal state (win or draw) and then decides the best move from those all possible iteration. You can refer this [article](https://www.freecodecamp.org/news/how-to-make-your-tic-tac-toe-game-unbeatable-by-using-the-minimax-algorithm-9d690bad4b37/) to understand more details about minimax algorithm. 

#### Conclusion

Blazor is super useful for .NET developers who are not interested in learning javascript for front-end development. This article shows how easy to develop real time blazor application with signalR. I have used minimax algorithm to identify the best spot available. It will be more interesting to use reinforcement machine learning algorithm for AI to learn  and identify based on rewards instead of recursive minimax algorithm. This will be a good use case to try when ML.NET introduce reinforcement learning library.

The entire source code is uploaded in my [github](https://github.com/vavjeeva/BlazoRTicTacToe) repository. Happy Coding.
