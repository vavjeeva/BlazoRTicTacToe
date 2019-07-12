using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazoRTicTacToeBot
{
    public class GameEngine
    {
        public string _botPlayer { get; set; }
        public string _humanPlayer { get; set; }

        public GameEngine(string[] currentBoard, string botPlayer, string humanPlayer)
        {           
            _botPlayer = botPlayer;
            _humanPlayer = humanPlayer;
        }


        public MoveResultVO GetBestSpot(string[] board, string player)
        {
            MoveResultVO bestMove = null;
            var availableSpots = GetAvailableSpots(board);
            string[] newboard;
            if (IsWinning(board, _botPlayer))
                return new MoveResultVO() { score = 1 };
            else if (IsWinning(board, _humanPlayer))
                return new MoveResultVO() { score = -1 };
            else if (availableSpots.Length == 0) //Tie
                return new MoveResultVO() { score = 0 };

            var autoMoves = new List<MoveResultVO>();

            foreach (var spot in availableSpots)
            {
                var moveResultVO = new MoveResultVO();
                moveResultVO.index = spot;
                board[int.Parse(spot)] = player;

                if (player == _botPlayer)
                {
                    var result = GetBestSpot(board, _humanPlayer);
                    moveResultVO.score = result.score;
                }
                else
                {
                    var result = GetBestSpot(board, _botPlayer);
                    moveResultVO.score = result.score;
                }

                //reset the spot
                board[int.Parse(spot)] = spot;

                autoMoves.Add(moveResultVO);
            }

            // For the bot turn loop thru the moves and select the move with the top score
            MoveResultVO bestMove = null;
            if (player == _botPlayer)
            {
                var bestScore = -100;
                foreach (var move in autoMoves)
                {
                    if (move.score > bestScore)
                    {
                        bestScore = move.score;
                        bestMove = move;
                    }
                }
            }
            else
            {

                // if its human turn, loop thru the moves and choose the move with the bottom score
                var bestScore = 100;
                foreach (var move in autoMoves)
                {
                    if (move.score < bestScore)
                    {
                        bestScore = move.score;
                        bestMove = move;
                    }
                }
            }
            return bestMove;
        }

        private string[] GetAvailableSpots(string[] board)
        {
            return board.Where(i => !IsPlayed(i)).ToArray();
        }

        private bool IsWinning(string[] board, string player)
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

        private bool IsPlayed(string input)
        {
            return input == "X" || input == "O";
        }
    }

    public class MoveResultVO
    {
        public int score;
        public string index;
    }
}
