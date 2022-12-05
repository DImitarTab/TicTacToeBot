namespace ChessBoxing.Models
{
	public class Board
	{
		public int[] Positions { get; set; } 
		public List<int> AvailablePositions { get; set; } 

		public Board() {
			Positions =	new int[9];
			AvailablePositions = Enumerable.Range(0, Positions.Length).ToList();
		}

		public Board(Board copyBoard)
		{
			Positions = copyBoard.Positions.ToArray();
			AvailablePositions = new List<int>(copyBoard.AvailablePositions);
		}


	}
}
