using System;
using Godot;

using packets;

namespace game
{
	public class Dungeon
	{
		public RoomPacket[] rooms;
		int size;

		public Dungeon(int _size, TileMapLayer dungeon)
		{
			rooms = GenRoom();

			foreach (RoomPacket _room in rooms)
			{
				for (int i = _room.x; i <= _room.w * 16 + _room.x; i++)
				{
					for (int j = _room.y; j <= _room.h * 16 + _room.y; j++)
					{
						if (j == _room.y || i == _room.x || j == _room.y + _room.h * 16 || i == _room.x + _room.w * 16)
							dungeon.SetCell(new Vector2I(i, j), 0, new (0 ,0), 1);
					}
				}
			}
		}

		private RoomPacket[] GenRoom()
		{
			return [new RoomPacket(-1, 0, 0, 1, 1)];
		}
	}
}