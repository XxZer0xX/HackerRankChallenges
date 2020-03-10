using System;

namespace BotCleaning
{
	public class LocationScoreEventArgs : EventArgs
	{
		internal double RadiatedValue;

		public LocationScoreEventArgs(double radiatedValue)
		{
			RadiatedValue = radiatedValue;
		}
	}
}