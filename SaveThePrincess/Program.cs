using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BotCleaning;

namespace SaveThePrincess
{
	public class Program
	{
		static void next_move(int posr, int posc, String[] level)
		{
			var sw = new Stopwatch();
			sw.Start();
			var board = new Board(level, CharacterSymbols.Bot, CharacterSymbols.Target, new SymbolActionStrategy());
			//var targets = board.Metadata.Where(record => record.Value.Tenant == board.TargetPiece).ToList();
			//targets.ForEach(item => item.Value.Radiate());
			sw.Stop();
			Console.WriteLine($"Elapsed Time: {sw.ElapsedMilliseconds} ms");
			Console.WriteLine(board.ToString());
			Console.ReadKey();
		}

		static void Main(String[] args)
		{
			//var board = new[] {"--", "-d"};
			//var board = new[] {"---", "---","--d"};
			//var board = new[] {"----", "----", "----", "--dd"};
			//var board = new[] {"-b--d", "-d--d", "--dd-", "--d--", "----d"};
			//var board = new[] {"-------", "-------", "-------", "-------", "-------","-------", "------d"};
			//var board = new[] {"-------", "-------", "-------", "---d---", "-------","-------", "-------"};
			//var board = new[] {"-d-----", "-----d-", "-------", "---d---", "-------","-d-----", "-----d-"};
			var board = new[] { "d-----d", "---b---", "-------", "---d---", "-------", "-------", "d-----d" };
			//var board = new[] {"d-------d", "---------", "--d------", "---------", "----d----","---------", "-d-------", "---------","d---d---d"};
			//var board = new[] {"---------", "---------", "---------", "---------", "----d----","---------", "---------", "---------","---------"};
			var temp = "0 1";

			//String temp = Console.ReadLine();
			String[] position = temp.Split(' ');
			int[] pos = new int[2];
			//String[] board = new String[5];
			//for(int i=0;i<5;i++) {
			//	board[i] = Console.ReadLine();
			//}
			for (int i = 0; i < 2; i++) pos[i] = Convert.ToInt32(position[i]);

			next_move(pos[0], pos[1], board);
		}
	}

	public class SymbolActionStrategy : ISymbolActionStrategy
	{
		public CharacterSymbols TargetCharacterSymbol => CharacterSymbols.Target;
		public CharacterSymbols BotCharacterSymbol => CharacterSymbols.Bot;
		public CharacterSymbols EmptyCharacterSymbol => CharacterSymbols.Empty;
		public Action<IPlayableLocation> TargetAction => location => location.Radiate();
		public Action<IPlayableLocation> BotAction => location => Console.WriteLine("CLEAN");
		public Func<IPlayableLocation, IPlayableLocation> BotMoveStrategy =>
			location =>
			{
				return location.GetConnectedNeighbors().OrderByDescending(l => l.Score).First();
			};
	}

}






