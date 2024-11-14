using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using packets;

namespace game
{
	public class Dungeon
	{
		public RoomPacket[] rooms;
		public Vector2I startRoom = new (-1, -1);
		int size;

		public Dungeon(int _size, int _sX, int _sY, TileMapLayer dungeon)
		{
			size = _size;
            rooms = GenRoom(_sX, _sY);

			int _rScale = 10;

			foreach (RoomPacket _room in rooms)
			{
				int x = _room.x * _rScale;
				int y = _room.y * _rScale;
				int h = _room.h * _rScale;
				int w = _room.w * _rScale;

				for (int i = x; i <= w + x; i++)
				{
					for (int j = y; j <= h + y; j++)
					{
						if (j == y || i == x || j == y + h || i == x + w)
							dungeon.SetCell(new Vector2I(i, j), 0, new (0 ,0), 1);
					}
				}
			}

			foreach (RoomPacket _room in rooms)
			{
				int x = _room.x * _rScale;
				int y = _room.y * _rScale;
				int h = _room.h * _rScale;
				int w = _room.w * _rScale;

				int dX = 0;
				int dY = 0;
				// l 1, u 2, r 3, d 4
				if (_room.r == 1)
				{
					dX = x;
					dY = y + 5;
				}
				if (_room.r == 2)
				{
					dY = y;
					dX = x + 5;
				}
				if (_room.r == 3)
				{
					dX = x + w;
					dY = y + 5;
				}
				if (_room.r == 4)
				{
					dY = y;
					dX = x + 5;
				}

				dungeon.SetCell(new(dX, dY));
			}
		}

		private RoomPacket[] GenRoom(int _x, int _y)
		{
			RandomNumberGenerator rng = new();
			rng.Randomize();

			int _rmsSpawned = 0;
			bool _boss = false;

			Vector2I[,] _rms = new Vector2I[_x+2,_y+2];

			for (int i = 0; i < _x; i++)
				for (int j = 0; j < _y; j++)
					_rms[i,j].X = 0;

			startRoom = new(rng.RandiRange(2, _x - 2), rng.RandiRange(2, _y - 2));

			_rms[startRoom.X, startRoom.Y].X = 2;

			while (_rmsSpawned < size)
			{
				int x = rng.RandiRange(1, _x - 2);
				int y = rng.RandiRange(1, _y - 2);

				int adj = CardinalAdjacents(_rms, x, y, 1);

				if (adj != -1 && _rms[x, y].X == 0)
				{
					_rms[x, y].X = 1;
					_rms[x, y].Y = adj;
					_rmsSpawned++;
				}
			}

			while (!_boss)
			{
				var x = rng.RandiRange(2, _x - 2);
				var y = rng.RandiRange(2, _y - 2);

				int adj = CardinalAdjacents(_rms, x, y, 2);

				if ((CardinalAdjacents(_rms, x, y, 1) == -1) && !DiagAdjacents(_rms, x, y, 1) && (adj != -1) && _rms[x, y].X == 0) {
					_rms[x-1,y-1].X = 3;
					_rms[x-1,y-1].Y = adj;
					_boss = true;
				}
			}

			List<RoomPacket> _fRooms = new();

			for (int i = 0; i < _x; i++)
				for (int j = 0; j < _y; j++)
				{
					if (_rms[i, j].X == 0) continue;
					if (_rms[i, j].X == 1 || _rms[i, j].X == 2) _fRooms.Add(new(-1, i, j, 1, 1, _rms[i,j].Y, _rms[i, j].X));
					else if (_rms[i, j].X == 3) _fRooms.Add(new(-1, i, j, 3, 3, _rms[i,j].Y, _rms[i, j].X));
				}

			return _fRooms.ToArray();
		}

		static int CardinalAdjacents(Vector2I[,] _rms, int _x, int _y, int _dist)
		{
			// l 1, u 2, r 3, d 4

			if (_rms[_x + _dist, _y].X > 0)
				return 3;
			if (_rms[_x - _dist, _y].X > 0)
				return 1;
			if (_rms[_x, _y + _dist].X > 0)
				return 2;
			if (_rms[_x, _y - _dist].X > 0)
				return 4;

			return -1;
		}

		static bool DiagAdjacents(Vector2I[,] _rms, int _x, int _y, int _dist)
		{
			if (_rms[_x + _dist, _y + _dist].X > 0)
				return true;
			if (_rms[_x - _dist, _y + _dist].X > 0)
				return true;
			if (_rms[_x + _dist, _y - _dist].X > 0)
				return true;
			if (_rms[_x - _dist, _y - _dist].X > 0)
				return true;

			return false;
		}
	}
}