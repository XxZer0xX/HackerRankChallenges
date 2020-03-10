using System;

namespace BotCleaning
{
	public interface ICharacter
	{
		IPlayableLocation CurrentLocation { get; set; }
		CharacterSymbols CharacterSymbol { get; }
		void PerformActionAtCurrentLocation();
		IPlayableLocation GetNextMove();
	}

	public abstract class Character : ICharacter
	{
		public IPlayableLocation CurrentLocation { get; set; }
		public CharacterSymbols CharacterSymbol { get; }

		protected Character(CharacterSymbols characterSymbol)
		{
			CharacterSymbol = characterSymbol;
		}

		public abstract void PerformActionAtCurrentLocation();

		public abstract IPlayableLocation GetNextMove();
	}

	public class PlayableCharacter : Character
	{
		private readonly Func<IPlayableLocation,IPlayableLocation> _moveStrategy;
		private readonly Action<IPlayableLocation> _action;

		public PlayableCharacter(CharacterSymbols characterSymbol, Action<IPlayableLocation> action, Func<IPlayableLocation,IPlayableLocation> moveStrategy) : base(characterSymbol)
		{
			_action = action;
			_moveStrategy = moveStrategy;
		}

		public void MoveToLocation(PlayableLocation location)
		{
			CurrentLocation = location;
			location.Tenant = this;
		}

		public override void PerformActionAtCurrentLocation()
		{
			_action(CurrentLocation);
		}

		public override IPlayableLocation GetNextMove()
		{
			return _moveStrategy(CurrentLocation);
		}
	}

	public class NpcCharacter : Character
	{
		private readonly Action<IPlayableLocation> _action;

		public NpcCharacter(CharacterSymbols characterSymbol, Action<IPlayableLocation> action) : base(characterSymbol)
		{
			_action = action;
		}

		public override void PerformActionAtCurrentLocation()
		{
			_action(CurrentLocation);
		}

		public override IPlayableLocation GetNextMove() => default;
	}
}