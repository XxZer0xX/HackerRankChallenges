using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotCleaning
{
	public class Board
	{
		public readonly CharacterSymbols CharacterPiece;
		public readonly CharacterSymbols TargetPiece;
		private readonly ISymbolActionStrategy _actionStrategy;

		public IPlayableLocation? this[Tuple<int, int> location] =>
			Metadata.TryGetValue(location, out var cell) ? cell : null;

		public Dictionary<Tuple<int, int>, IPlayableLocation> Metadata { get; }

		public ICharacter PlayableCharacter { get; private set; }
		public int Bounds => Details.Length - 1; 
		public double ScoreRadiationThreshold { get; set; }

		public CharacterSymbols[][] Details { get; }
		
		public bool CharacterShouldPerformAction => PlayableCharacter?.CurrentLocation?.IsTargetCell() ?? false;
		public double CalculatedDefaultCellScore { get; set; }

		public Board(string[] board, CharacterSymbols characterPiece, CharacterSymbols targetPiece, ISymbolActionStrategy actionStrategy)
		{
			CharacterPiece = characterPiece;
			TargetPiece = targetPiece;
			_actionStrategy = actionStrategy;

			//this is null
			Details = new CharacterSymbols[board.Length][];
			var targetCount = 0;
			for (var r = 0; r < board.Length; r++)
			{
				for (var c = 0; c < board.Length; c++)
				{
					switch ((int)board[r][c])
					{
						case (int)CharacterSymbols.Target:
							Details[r][c] = CharacterSymbols.Target;
							break;
						case (int)CharacterSymbols.Bot:
							Details[r][c] = CharacterSymbols.Bot;
							break;
						case (int)CharacterSymbols.Empty:
							Details[r][c] = CharacterSymbols.Empty;
							break;
						default:
							throw new Exception("Invalid character on board.");
					}

					if (Details[r][c] == targetPiece)
						targetCount++;
				}
			}

			CalculatedDefaultCellScore = (100 * targetCount) *.1;
			ScoreRadiationThreshold = Math.Sqrt(CalculatedDefaultCellScore)*.01;
			Metadata = BuildMetadata();
			RegisterCellRelationshipEvents();
		}

		public Dictionary<Tuple<int, int>, IPlayableLocation> BuildMetadata()
		{
			var result = new Dictionary<Tuple<int, int>, IPlayableLocation>();

			for (var r = 0; r < Bounds+1; r++)
			for (var c = 0; c < Details[r].Length; c++)
			{
				var location = new Tuple<int, int>(r, c);
				var symbol = Details[r][c];
				var character = symbol == CharacterSymbols.Bot
					? new PlayableCharacter(_actionStrategy.BotCharacterSymbol, _actionStrategy.BotAction, _actionStrategy.BotMoveStrategy )
					: (ICharacter) new NpcCharacter(symbol, _actionStrategy.TargetAction);

				if (PlayableCharacter == default && character is PlayableCharacter playableCharacter)
					PlayableCharacter = playableCharacter;

				result.Add(location, new PlayableLocation(location, this, character));
			}

			return result;
		}

		public void RegisterCellRelationshipEvents()
		{
			for (var r = 0; r < Bounds+1; r++)
			{
				for (var c = 0; c < Details[r].Length; c++)
				{
					var cell = Metadata[new Tuple<int, int>(r, c)];

					RegisterLinearValueTransformationEvents(cell);
					RegisterDiagonalValueTransformationEvents(cell);
				}
			}
		}

		private void RegisterLinearValueTransformationEvents(IPlayableLocation cell)
		{
			ApplyUp(cell);
			ApplyRight(cell);
			ApplyDown(cell);
			ApplyLeft(cell);
		}

		private void RegisterDiagonalValueTransformationEvents(IPlayableLocation cell)
		{
			ApplyUpRight(cell);
			ApplyUpLeft(cell);
			ApplyDownLeft(cell);
			ApplyDownRight(cell);
		}

		private void ApplyLeft(IPlayableLocation cell)
		{
			if (cell.Location.Item2 == 0 ||
			    !cell.Parent.Metadata.TryGetValue(new Tuple<int, int>(cell.Location.Item1, cell.Location.Item2 - 1),
				    out var neighbor))
				return;

			cell.LocationScoreUpdatedLinearly += neighbor.HandleLocationScoreUpdatedLinearly;
		}

		private void ApplyDown(IPlayableLocation cell)
		{
			if (cell.Location.Item1 == cell.Parent.Bounds ||
			    !cell.Parent.Metadata.TryGetValue(new Tuple<int, int>(cell.Location.Item1 + 1, cell.Location.Item2),
				    out var neighbor))
				return;

			cell.LocationScoreUpdatedLinearly += neighbor.HandleLocationScoreUpdatedLinearly;
		}

		private void ApplyRight(IPlayableLocation cell)
		{
			if (cell.Location.Item2 == cell.Parent.Bounds ||
			    !cell.Parent.Metadata.TryGetValue(new Tuple<int, int>(cell.Location.Item1, cell.Location.Item2 + 1),
				    out var neighbor))
				return;

			cell.LocationScoreUpdatedLinearly += neighbor.HandleLocationScoreUpdatedLinearly;
		}

		private void ApplyUp(IPlayableLocation cell)
		{
			if (cell.Location.Item1 == 0 ||
			    !cell.Parent.Metadata.TryGetValue(new Tuple<int, int>(cell.Location.Item1 - 1, cell.Location.Item2),
				    out var neighbor))
				return;

			cell.LocationScoreUpdatedLinearly += neighbor.HandleLocationScoreUpdatedLinearly;
		}

		private void ApplyUpLeft(IPlayableLocation cell)
		{
			if (cell.Location.Item1 == 0 || cell.Location.Item2 == 0 ||
			    !cell.Parent.Metadata.TryGetValue(new Tuple<int, int>(cell.Location.Item1 - 1, cell.Location.Item2 - 1),
				    out var neighbor))
				return;

			cell.LocationScoreUpdatedDiagonally += neighbor.HandleLocationScoreUpdatedDiagonally;
		}

		private void ApplyDownLeft(IPlayableLocation cell)
		{
			if (cell.Location.Item1 == cell.Parent.Bounds || cell.Location.Item2 == 0 ||
			    !cell.Parent.Metadata.TryGetValue(new Tuple<int, int>(cell.Location.Item1 + 1, cell.Location.Item2 - 1),
				    out var neighbor))
				return;

			cell.LocationScoreUpdatedDiagonally += neighbor.HandleLocationScoreUpdatedDiagonally;
		}

		private void ApplyDownRight(IPlayableLocation cell)
		{
			if (cell.Location.Item1 == cell.Parent.Bounds || cell.Location.Item2 == cell.Parent.Bounds ||
			    !cell.Parent.Metadata.TryGetValue(new Tuple<int, int>(cell.Location.Item1 + 1, cell.Location.Item2 + 1),
				    out var neighbor))
				return;

			cell.LocationScoreUpdatedDiagonally += neighbor.HandleLocationScoreUpdatedDiagonally;
		}

		private void ApplyUpRight(IPlayableLocation cell)
		{
			if (cell.Location.Item1 == 0 || cell.Location.Item2 == cell.Parent.Bounds ||
			    !cell.Parent.Metadata.TryGetValue(new Tuple<int, int>(cell.Location.Item1 - 1, cell.Location.Item2 + 1),
				    out var neighbor))
				return;

			cell.LocationScoreUpdatedDiagonally += neighbor.HandleLocationScoreUpdatedDiagonally;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();

			for (int r = 0; r < Bounds +1; r++)
			{
				builder.Append("|");
				var line = Enumerable.Range(0, Bounds+1).Select(c =>
				{
					var entry = Metadata[new Tuple<int, int>(r, c)];
					//var temp = $" {entry.Score:0##.0000000} ";
					var temp = $" {(entry.Tenant.CharacterSymbol == CharacterSymbols.Target ? $"{entry.Score:0##.0000000}" : $"     {entry.Tenant}     " )} ";
					return temp;
				});
				builder.AppendLine($"{string.Join(" | ",line)}|");
			}

			return builder.ToString();
		}
	}
}

