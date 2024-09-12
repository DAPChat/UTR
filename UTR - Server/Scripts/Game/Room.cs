using System.Numerics;

namespace game
{
	public class Room
	{
		public Vector2 doorPos;

		public Room(int _seed, int _roomNum, Vector2? _prevDoor)
		{
			if (_prevDoor == null)
			{
				// First Room

				return;
			}

			// Do what you must
		}
	}
}