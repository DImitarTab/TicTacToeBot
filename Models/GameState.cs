namespace ChessBoxing.Models
{
	public class GameState
	{
		public Board Board { get; set; }
		public Player CurrentPlayer { get; set; }
		public bool GameOver { get; set; }
		public int TurnsPassed { get; set; }
		public ResultState ResultEnd { get; set; }


		//private static GameState instance = new GameState();

		//public GameState()
		//{
		//	TurnsPassed = 0;
		//	CurrentPlayer = Player.X;
		//	GameOver = false;
		//	ResultEnd = ResultState.InProgress;
		//	Board = new Board();
		//}

		private GameState()
		{

			TurnsPassed = 0;
			CurrentPlayer = Player.X;
			GameOver = false;
			ResultEnd = ResultState.InProgress;
			Board = new Board();
		}
		private static GameState? instance = null;
		public static GameState Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new GameState();
				}
				return instance;
			}
			
		}


		public void ReleaseInstance()
		{
			instance = null; 
		}

		public GameState(GameState copyState, Board bboard)
		{
			TurnsPassed = copyState.TurnsPassed;
			CurrentPlayer = copyState.CurrentPlayer;
			GameOver = copyState.GameOver;
			ResultEnd = copyState.ResultEnd;
			Board = new Board(bboard);
		}

		
		//public static GameState Instance
		//{
		//	get { return instance; }
		//}


	}
}
