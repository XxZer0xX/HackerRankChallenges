using System;
using System.Collections.Generic;

namespace BotCleaning
{
	using static Math;

	public class PlayableLocation : IPlayableLocation
	{
		private readonly Dictionary<Tuple<int, int>, double> _temporalTransactions = new Dictionary<Tuple<int, int>, double>();
		private double _locationScore;

		private readonly Func<Tuple<int, int>, int, double, double> _seed = (location, bounds, threshold) =>
			(Abs(location.Item1 - bounds) + Abs(location.Item2 - bounds) * threshold) * .1;

		public event EventHandler<LocationScoreEventArgs> LocationScoreUpdatedLinearly;
		public event EventHandler<LocationScoreEventArgs> LocationScoreUpdatedDiagonally;

		public Board Parent { get; }
		public Tuple<int, int> Location { get; }
		public ICharacter Tenant { get; set; }

		public double Score 
		{
			get => _locationScore;
			set
			{
				var exponentialValue = Abs((_locationScore + value) - _locationScore);
				_locationScore = value;
				var args = new LocationScoreEventArgs(exponentialValue);
				HandleLocationScoreUpdatedLinearly(this, args);
				HandleLocationScoreUpdatedDiagonally(this, args);
			}
		}

		public bool IsTargetCell() => Tenant.CharacterSymbol == CharacterSymbols.Target;

		public PlayableLocation(Tuple<int, int> location, Board parent, ICharacter tenant)
		{
			Location = location;
			Parent = parent;
			Tenant = tenant;

			_locationScore = Tenant != null && Tenant.CharacterSymbol == CharacterSymbols.Target ? parent.CalculatedDefaultCellScore : 0;
		}

		public void HandleLocationScoreUpdatedLinearly(object sender, LocationScoreEventArgs e)
		{
			if (e.RadiatedValue < 0)
				throw new ArithmeticException("Radiated value is negative.");

			var s = ((PlayableLocation)sender);

			if (_temporalTransactions.ContainsKey(s.Location) || s._temporalTransactions.ContainsKey(Location) || e.RadiatedValue < Parent.ScoreRadiationThreshold || Equals(sender, this))
				return;

			LocationScoreUpdatedLinearly?.Invoke(this, new LocationScoreEventArgs(e.RadiatedValue * _seed(Location, Parent.Bounds, Parent.ScoreRadiationThreshold)));

			if (!_temporalTransactions.ContainsKey(s.Location))
				_temporalTransactions.Add(s.Location, 0);

			_temporalTransactions[s.Location] += e.RadiatedValue;

			Score += e.RadiatedValue;
			RegisterRadiatedLocation(s, e.RadiatedValue);
		}

		private void RegisterRadiatedLocation(PlayableLocation piece, double radiatedValue)
		{
			piece._temporalTransactions.TryAdd(Location, radiatedValue);
		}

		public void HandleLocationScoreUpdatedDiagonally(object sender, LocationScoreEventArgs e)
		{
			if (e.RadiatedValue < 0)
				throw new ArithmeticException("Radiated value is negative.");
			
			var s = ((PlayableLocation)sender);

			if (_temporalTransactions.ContainsKey(s.Location) || s._temporalTransactions.ContainsKey(Location) || Abs(e.RadiatedValue) < Parent.ScoreRadiationThreshold || Equals(sender, this))
				return;

			LocationScoreUpdatedDiagonally?.Invoke(this, new LocationScoreEventArgs(e.RadiatedValue * _seed(Location, Parent.Bounds, Parent.ScoreRadiationThreshold)));

			if (!_temporalTransactions.ContainsKey(s.Location))
				_temporalTransactions.Add(s.Location, 0);

			_temporalTransactions[s.Location] += e.RadiatedValue;

			Score += e.RadiatedValue;
			RegisterRadiatedLocation(s, e.RadiatedValue);
		}

		public void Radiate()
		{
			LocationScoreUpdatedLinearly?.Invoke(this, new LocationScoreEventArgs(_locationScore * Parent.ScoreRadiationThreshold));
			LocationScoreUpdatedDiagonally?.Invoke(this, new LocationScoreEventArgs(_locationScore * Parent.ScoreRadiationThreshold));
		}

		public IEnumerable<IPlayableLocation> GetConnectedNeighbors()
		{
			if (Location.Item1 > 0)
				yield return Parent[new Tuple<int, int>(Location.Item1 - 1, Location.Item2)];
			if (Location.Item1 < Parent.Bounds)
				yield return Parent[new Tuple<int, int>(Location.Item1 + 1, Location.Item2)];
			if (Location.Item2 > 0)
				yield return Parent[new Tuple<int, int>(Location.Item1, Location.Item2 - 1)];
			if (Location.Item2 > Parent.Bounds)
				yield return Parent[new Tuple<int, int>(Location.Item1, Location.Item2 + 1)];
		}
	}

	public interface IPlayableLocation
	{
		event EventHandler<LocationScoreEventArgs> LocationScoreUpdatedLinearly;
		event EventHandler<LocationScoreEventArgs> LocationScoreUpdatedDiagonally;
		void HandleLocationScoreUpdatedLinearly(object sender, LocationScoreEventArgs e);
		void HandleLocationScoreUpdatedDiagonally(object sender, LocationScoreEventArgs e);

		Board Parent { get; }
		Tuple<int, int> Location { get; }
		ICharacter Tenant { get; set; }
		double Score { get; set; }	
		bool IsTargetCell();
		void Radiate();
		IEnumerable<IPlayableLocation> GetConnectedNeighbors();
	}
}