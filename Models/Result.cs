using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace ChessBoxing.Models
{
	public class Result
	{

		//[JsonConverter(typeof(JsonStringEnumConverter))]
		public Player Winner { get; set; }
		public WinInfo? Information { get; set; }
	}
}
