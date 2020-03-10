using System;
using System.Collections.Generic;
using System.Linq;

namespace BotCleaning
{
	public class Level
	{
		private readonly int _mapSize;
		private readonly string[] _mapMatrix;
		private readonly char _bot;
		private readonly char _target;

		private Tuple<int, int> _currentLocation;
		private Tuple<int, int> _targetLocation;

		private Queue<string> movesList;

		public bool IsAtTarget => Equals(_currentLocation, _targetLocation);

		public Level(int mapSize, string[] mapMatrix, char bot, char target, Tuple<int,int> botPosition = null)
		{
			_currentLocation = botPosition;
			_mapSize = mapSize;
			_mapMatrix = mapMatrix;
			_bot = bot;
			_target = target;
			ParseMatrixForData();
		}

		private void ParseMatrixForData()
		{
			var bounds = _mapSize - 1;

			for (var i = 0; i < _mapSize; i++)
			{
				if (_targetLocation != null && _currentLocation != null)
					break;

				var rowArray = _mapMatrix[i].ToCharArray();
				var reverseLoop = false;


				for (var j = 0; j < rowArray.Length; j++)
				{
					if (_targetLocation != null && _currentLocation != null)
						break;

					if (_targetLocation == null && rowArray[j] == _target)
						_targetLocation = new Tuple<int, int>(i, j);

					if (_currentLocation == null && rowArray[j] == _bot)
						_currentLocation = new Tuple<int, int>(i, j);
				}

				if(_targetLocation?.Item1 != _currentLocation.Item1)
				{
					reverseLoop = !reverseLoop;
				}
			}

			if (_targetLocation == null)
				throw new Exception("target is not in this map");

			if (_currentLocation == null)
				throw new Exception("current position is not in this map");

			CalcualteOptimalPath();
		}
		
		private void CalcualteOptimalPath()
		{
			var xTargetMoveCount = _currentLocation.Item2 - _targetLocation.Item2;
			var yTargetMoveCount = _currentLocation.Item1 - _targetLocation.Item1;

			var xMoveDirection = xTargetMoveCount == 0 ? null : xTargetMoveCount > 0 ? (bool?)true : false;
			var yMoveDirection = yTargetMoveCount == 0 ? null : yTargetMoveCount > 0 ? (bool?)true : false;

			var moves = Enumerable.Repeat(MapResponseMove(true, xMoveDirection), Math.Abs(xTargetMoveCount)).ToList();
			moves.AddRange(Enumerable.Repeat(MapResponseMove(false, yMoveDirection), Math.Abs(yTargetMoveCount)));
			movesList = new Queue<string>(moves);
		}

		private static string MapResponseMove(bool isXOrientation, bool? moveDirection)
		{
			var reverseLinearly = isXOrientation ? "LEFT" : "UP";
			var forwardLinearly = isXOrientation ? "RIGHT" : "DOWN";

			return !moveDirection.HasValue ? null : moveDirection.Value ? reverseLinearly : forwardLinearly;
		}

		public string GetNextMove()
		{
			return movesList?.Dequeue();
		}
	}
}