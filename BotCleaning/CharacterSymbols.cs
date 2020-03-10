using System;

namespace BotCleaning
{
	public enum CharacterSymbols
	{
		Target = 'd',
		Empty = '-',
		Bot = 'b'
	}

	public interface ISymbolActionStrategy
	{
		CharacterSymbols TargetCharacterSymbol { get; }
		CharacterSymbols BotCharacterSymbol { get; }
		CharacterSymbols EmptyCharacterSymbol { get; }

		Action<IPlayableLocation> TargetAction { get; }
		Action<IPlayableLocation> BotAction { get; }
		Func<IPlayableLocation,IPlayableLocation>  BotMoveStrategy { get; }
	}
}