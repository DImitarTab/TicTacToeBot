using ChessBoxing.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Converters;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace ChessBoxing.Controllers
{
	public class HomeController : Controller
	{


		public const int PLAYER_1 = 1;
		public const int PLAYER_2 = 2;

		public const string PLAYER_1_SYMBOL = "X";
		public const string PLAYER_2_SYMBOL = "O";

		
		GameState stateGame = GameState.Instance;

		//}
		

	//public HomeController(ILogger<HomeController> logger)
	//{
	//	_logger = logger;
	//	stateGame = new GameState();

	//}


	public IActionResult Index()
		{
			

			if (ModelState.IsValid)
			{
				return View(stateGame);
			}

			return View();

		}


		public void SwitchPlayer(GameState newGameState)
		{

			newGameState.CurrentPlayer = newGameState.CurrentPlayer == Player.X ? Player.O : Player.X;

		}

		public int GetCurrentPlayersNumber(GameState newGameState) =>
			(int)newGameState.CurrentPlayer == PLAYER_1
				? PLAYER_1
				: PLAYER_2;

		public Player GetCurrentPlayersSymbol(GameState newGameState) =>

			newGameState.CurrentPlayer == Player.X
				? Player.X
				: Player.O;


		public bool canMakeMove(int r, GameState statee)
		{
			return statee.ResultEnd == ResultState.InProgress && statee.Board.Positions[r] == 0;
		}

		//public bool isGridFull()
		//{
		//	return stateGame.TurnsPassed == 9;
		//}

		//public bool AreSquaresMarked(List<int[]> squares)
		//{
		//	foreach (var r in squares)
		//	{
		//		if (r.All( x => stateGame.Board.Positions[x] == GetCurrentPlayersNumber()))
		//		{
		//			return true;
		//		}
		//	}
		//	return false;
		//}

		public ResultState DidMoveWin(GameState newStateGame)
		{

			var _rowIndexes = new List<int[]>();
			var _colIndexes = new List<int[]>();
			var _diagIndexes = new List<int[]>();

			var mainDiag = new int[3] { 0, 4, 8 };
			var reverseDiag = new int[3] { 2, 4, 6 };

			_diagIndexes.Add(reverseDiag);
			_diagIndexes.Add(mainDiag);

			for (var i = 0; i < 3; i++) 
			{
				var row = new int[3];
				var col = new int[3];
				for (var j = 0; j < 3; j++)
				{

					row[j] = (i * 3) + j;
					col[j] = (j * 3) + i;
				}

				_colIndexes.Add(col);
				_rowIndexes.Add(row);
			}


			var resultState = CheckRowsForWinner(_rowIndexes, newStateGame);
			if (resultState != ResultState.InProgress)
			{
				newStateGame.ResultEnd = resultState;
				newStateGame.GameOver = true;

				return resultState;
			}

			resultState = CheckColsForWinner(_colIndexes, newStateGame);
			if (resultState != ResultState.InProgress)
			{
				newStateGame.ResultEnd = resultState;
				newStateGame.GameOver = true;
				return resultState;
			}

			resultState = CheckDiagsForWinner(_diagIndexes, newStateGame);
			if (resultState != ResultState.InProgress)
			{
				newStateGame.ResultEnd = resultState;
				newStateGame.GameOver = true;

				return resultState;
			}


			if (!newStateGame.Board.AvailablePositions.Any())
			{
				newStateGame.ResultEnd = ResultState.Draw;
				newStateGame.GameOver = true;

				return ResultState.Draw;
			}


			return ResultState.InProgress;




		}
		public ResultState CheckDiagsForWinner(List<int[]> checker, GameState newStateGame)
		{
			foreach (var diag in checker)
			{
				if (diag.All(x => newStateGame.Board.Positions[x] == PLAYER_1))
				{
					return ResultState.Player1Win;
				}

				if (diag.All(x => newStateGame.Board.Positions[x] == PLAYER_2))
				{
					return ResultState.Player2Win;
				}
			}

			return ResultState.InProgress;
		}

		public ResultState CheckColsForWinner(List<int[]> checker, GameState newStateGame)
		{
			foreach (var col in checker)
			{
				if (col.All(x => newStateGame.Board.Positions[x] == PLAYER_1))
				{
					return ResultState.Player1Win;
				}

				if (col.All(x => newStateGame.Board.Positions[x] == PLAYER_2))
				{
					return ResultState.Player2Win;
				}
			}

			return ResultState.InProgress;
		}


		public ResultState CheckRowsForWinner(List<int[]> checker, GameState newStateGame)
		{
			foreach (var row in checker)
			{
				if (row.All(x => newStateGame.Board.Positions[x] == PLAYER_1))
				{
					return ResultState.Player1Win;
				}

				if (row.All(x => newStateGame.Board.Positions[x] == PLAYER_2))
				{
					return ResultState.Player2Win;
				}
			}

			return ResultState.InProgress;
		}
		[HttpPost]
		public ActionResult ResetBoard()
		{
			stateGame.ReleaseInstance();
			return Json(JsonConvert.SerializeObject(stateGame));
		}


		[HttpPost]
		public ActionResult MakeMove(int r)
		{
			
			if (!canMakeMove(r, stateGame))
			{
				var resultIfFalse = new { pesho = stateGame, legalMove = false };
				return Json(JsonConvert.SerializeObject(resultIfFalse));
			}


			ApplyMove(r, stateGame);

			DidMoveWin(stateGame);



			if (DidMoveWin(stateGame) == ResultState.Player1Win)
			{
	
				var resultifEnd = new { legalMove = true, pesho = stateGame};

				return Json(JsonConvert.SerializeObject(resultifEnd));

			}
			if (DidMoveWin(stateGame) == ResultState.Player2Win)
			{
				var resultifNot = new { legalMove = true, pesho = stateGame };

				return Json(JsonConvert.SerializeObject(resultifNot));
			}

			if (DidMoveWin(stateGame) == ResultState.Draw)
			{
				var resultifNot = new { legalMove = true, pesho = stateGame };

				return Json(JsonConvert.SerializeObject(resultifNot));
			}


			var botMove = MakeBotMove(stateGame);

			ApplyMove(botMove, stateGame);

			DidMoveWin(stateGame);

			var resultBot = new { pesho = stateGame, legalMove = true, botMove};
			return Json(JsonConvert.SerializeObject(resultBot));
		}


		public void ApplyMove (int r, GameState newGameState)
		{
			newGameState.Board.Positions[r] = GetCurrentPlayersNumber(newGameState);
			newGameState.Board.AvailablePositions.Remove(r);
			SwitchPlayer(newGameState);
			newGameState.TurnsPassed++;
		}

		//public void gameRestart()
		//      {
		//	stateGame.TurnsPassed = 0;
		//	stateGame.CurrentPlayer = Player.X;
		//	stateGame.SquareWho = new Player[3, 3];
		//	stateGame.GameOver = false;
		//}
		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public int MakeBotMove(GameState state)
		{
			
			int myNumber = GetCurrentPlayersNumber(state);
			var clonedState = Clone(state);

			var availableMoves = clonedState.Board.AvailablePositions;

			var scores = new Dictionary<int, int>();
			var topScore = int.MinValue;
			// go through all available moves and get a minmax score for each. Then chose the best move.
			foreach (var move in availableMoves)
			{
				var score = GetMinMaxScoreForMovesRecursive(myNumber, move, Clone(clonedState), 1);
				scores[move] = score;

				if (score > topScore)
				{
					topScore = score;
				}
			}

			// This isn't needed but to keep things interesting, if there are multiple spots with top scores, switch things
			// up by randomly choosing one rather than just the first each time.  
			var topMoves = scores
				.Where(x => x.Value == topScore)
				.Select(x => x.Key)
				.ToArray();

			if (topMoves.Length == 1)
			{
				return topMoves[0];
			}

			var rndIndex = new Random().Next(0, topMoves.Length - 1);
			return topMoves[rndIndex];
		}

		public int GetMinMaxScoreForMovesRecursive(int myNumber, int move, GameState state, int depth)
		{

			var availableSpots = state.Board.AvailablePositions;
			if (availableSpots.All(x => x != move))
			{
				throw new IllegalMoveException();
			}


			ApplyMove(move, state);

			var result = DidMoveWin(state);
			switch (result)
			{
				case ResultState.Draw:
					return DrawScore();
				case ResultState.Player1Win:
					return WinOrLoseScore(myNumber, PLAYER_1, depth);
				case ResultState.Player2Win:
					return WinOrLoseScore(myNumber, PLAYER_2, depth);
			}


			// The game is not done yet. Now we'll look at all the next available spots and check all the possible scores.
			// We'll continue to do so recursively until we get a result for each position

			depth++;
			var nextAvailableMoves = state.Board.AvailablePositions;
			var scores = new int[nextAvailableMoves.Count];

			for (var i = 0; i < nextAvailableMoves.Count; i++)
			{
				var score = GetMinMaxScoreForMovesRecursive(myNumber, nextAvailableMoves[i], Clone(state), depth);
				scores[i] = score;
			}

			// here we check if it's the maximizing player (minimax bot) turn. If so, pick the best score.
			// if it's not (i.e. the opponent's turn), assume that they will choose the best possible option (lowest score)
			var isMyTurn = GetCurrentPlayersNumber(state) == myNumber;
			return isMyTurn ? scores.Max() : scores.Min();
		}

		 //Scoring:
		 // - Draw is meh... 0 points
		 // - A Loss is bad: -10. But a loss much later in the game is better than an immediate loss, so we add depth
		 // - A Win is great: +10. An immediate win trumps a later win. So we subtract the depth from the win score
		private static int DrawScore() => 0;
		private static int LoseScore(int depth) => -100 + depth;
		private static int WinScore(int depth) => 100 - depth;

		public GameState Clone(GameState stateGameClone)
		{
			var clonedState = new GameState(stateGameClone, stateGameClone.Board);
			
			return clonedState;
		}

		private static int WinOrLoseScore(int myNumber, int winningPlayer, int depth)
		{
			return myNumber == winningPlayer
				? WinScore(depth)
				: LoseScore(depth);
		}


	}
}