using System;

namespace game
{
	public class Dungeon
	{
		int size;
		int seed;
		Room[] rooms;

		public Dungeon(int _size)
		{
			size = _size;
			rooms = new Room[size];

			Random rand = new(Environment.TickCount);

			seed = rand.Next();

			GenRoom();
		}

		private void GenRoom()
		{
			for (int i = 0; i < size; i++)
			{
				rooms[i] = new Room(seed, i, i == 0 ? null : rooms[i - 1].doorPos);
				// Create room and paste new room into world at correct pos
			}
		}
	}
}