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
		public RoomPacket startRoom = null;

		int gId;
		int size;

		RoomClass[,] _rms;

		public Dungeon(int _size, int _sX, int _sY, TileMapLayer dungeon, int _gId)
		{
			PackedScene _roomScene = ResourceLoader.Load<PackedScene>("res://Scenes/room.tscn");

			gId = _gId;
			size = _size;
            rooms = GenRoom(_sX, _sY);

			int _rScale = 16;

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

				Room _rmArea = _roomScene.Instantiate<Area2D>().Duplicate() as Room;
				_rmArea.Instantiate(_gId, _room);
				_rmArea.GetChild<CollisionShape2D>(0).Shape = (RectangleShape2D)_rmArea.GetChild<CollisionShape2D>(0).Shape.Duplicate();
				((RectangleShape2D)_rmArea.GetChild<CollisionShape2D>(0).Shape).Size = new ((16*w)-16, (16*h)-16);
				_rmArea.Position = new((16 * x) + (w * 16)/2 + 8, (16 * y) + (h *16)/2 + 8);
				ServerManager.GetGame(gId).GetNode<Node>("Rooms").AddChild(_rmArea);
			}

			foreach (RoomPacket _room in rooms)
			{
				int x = _room.x * _rScale;
				int y = _room.y * _rScale;
				int h = _room.h * _rScale;
				int w = _room.w * _rScale;

				
				// l 1, u 2, r 3, d 4

				int by = _room.data == 3? 24 : 8;

				for (int i = 0; i < _room.r.Length; i++)
				{
					int dX = 0;
					int dY = 0;

					if (!_room.r[i]) continue;

					if (i == 0)
					{
						dX = x;
						dY = y + by;
					}
					if (i == 1)
					{
						dY = y;
						dX = x + by;
					}
					if (i == 2)
					{
						dX = x + w;
						dY = y + by;
					}
					if (i == 3)
					{
						dY = y + h;
						dX = x + by;
					}

					dungeon.SetCell(new(dX, dY));
				}
			}
		}

		public class RoomClass
		{
			public int X;
			public bool[] Y = [false, false, false, false];

			public override string ToString()
			{
				string s = "";

				foreach (bool b in Y) s += b + ", ";

				return s;
			}
		}

		private RoomPacket[] GenRoom(int _x, int _y)
		{
			RandomNumberGenerator rng = new();
			rng.Randomize();

			int _rmsSpawned = 0;
			bool _boss = false;

			_rms = new RoomClass[_x+2,_y+2];

			for (int i = 0; i < _x+2; i++)
				for (int j = 0; j < _y+2; j++)
					_rms[i, j] = new() { X = 0 };

			Vector2I _startRoom = new(rng.RandiRange(2, _x - 2), rng.RandiRange(2, _y - 2));

			_rms[_startRoom.X, _startRoom.Y].X = 2;

			while (_rmsSpawned < size)
			{
				int x = rng.RandiRange(1, _x - 2);
				int y = rng.RandiRange(1, _y - 2);

				Vector2I adj = CardinalAdjacents(x, y, 1, true);

				if (adj.X != -1 && _rms[x, y].X == 0)
				{
					_rms[x, y].X = 1;
					_rms[x, y].Y[adj.X] = true;
					_rmsSpawned++;
				}
			}

			while (!_boss)
			{
				var x = rng.RandiRange(2, _x - 2);
				var y = rng.RandiRange(2, _y - 2);

				if (_rms[x, y].X != 0) continue;

				Vector2I adj = CardinalAdjacents(x, y, 2, false);

				if ((CardinalAdjacents(x, y, 1, false).X == -1) && !DiagAdjacents(x, y, 1) && (adj.X != -1) && _rms[x, y].X == 0) {
					_rms[x-1,y-1].X = 3;
					_rms[x-1,y-1].Y[adj.X] = true;
					CardinalAdjacents(x, y, 2, true);
					_boss = true;
				}
			}

			List<RoomPacket> _fRooms = new();

			for (int i = 0; i < _x; i++)
				for (int j = 0; j < _y; j++)
				{
					if (_rms[i, j].X == 0) continue;

					if (_rms[i, j].X == 1) _fRooms.Add(new(_fRooms.Count, i, j, 1, 1, _rms[i, j].Y, _rms[i, j].X));
					else if (_rms[i, j].X == 2)
					{
						RoomPacket _temp = new(_fRooms.Count, i, j, 1, 1, _rms[i, j].Y, _rms[i, j].X);
						_fRooms.Add(_temp);
						startRoom = _temp;
						
					}
					else if (_rms[i, j].X == 3) _fRooms.Add(new(_fRooms.Count, i, j, 3, 3, _rms[i, j].Y, _rms[i, j].X));
				}

			return _fRooms.ToArray();
		}

		private Vector2I CardinalAdjacents(int _x, int _y, int _dist, bool _door)
		{
			// l 1, u 2, r 3, d 4

			if (_rms[_x + _dist, _y].X > 0)
			{
				if (_door) _rms[_x + _dist, _y].Y[0] = true;
				//GD.Print(_x, " ", _y, " ", _rms[_x, _y], " ", (_x+_dist), " ", _y, " ", _rms[_x + _dist, _y]);
				return new(2, _rms[_x + _dist, _y].X);
			}
			if (_rms[_x - _dist, _y].X > 0)
			{
				if (_door) _rms[_x - _dist, _y].Y[2] = true;
				return new(0, _rms[_x - _dist, _y].X);
			}
			if (_rms[_x, _y + _dist].X > 0)
			{
				if (_door) _rms[_x, _y + _dist].Y[1] = true;
				return new(3, _rms[_x, _y + _dist].X);
			}
			if (_rms[_x, _y - _dist].X > 0)
			{
				if (_door) _rms[_x, _y - _dist].Y[3] = true;
				return new(1, _rms[_x, _y - _dist].X);
			}

			return new(-1, -1);
		}

		private bool DiagAdjacents(int _x, int _y, int _dist)
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