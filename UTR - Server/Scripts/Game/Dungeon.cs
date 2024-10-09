using System;
using Godot;

using packets;

namespace game
{
	public class Dungeon
	{
		public RoomPacket[] rooms;
		int size;

		public Dungeon(int _size, Node2D dungeon)
		{
			rooms = GenRoom();

			RigidBody2D _rb = ResourceLoader.Load<PackedScene>("res://Scenes/wall_tile.tscn").Instantiate<RigidBody2D>();

			foreach (RoomPacket _room in rooms)
			{
				RigidBody2D _irb = _rb.Duplicate() as RigidBody2D;
				_irb.Position = new Vector2(_room.x, _room.y) * 16;
				_irb.Scale = new Vector2(_room.w*16, 1);
				dungeon.AddChild(_irb);
				_irb = _rb.Duplicate() as RigidBody2D;
				_irb.Position = new Vector2(_room.x, _room.y+_room.h*16) * 16;
				_irb.Scale = new Vector2(_room.w*16, 1);
				dungeon.AddChild(_irb);
				_irb = _rb.Duplicate() as RigidBody2D;
				_irb.Position = new Vector2(_room.x, _room.y) * 16;
				_irb.Scale = new Vector2(1, _room.h*16);
				dungeon.AddChild(_irb);
				_irb = _rb.Duplicate() as RigidBody2D;
				_irb.Position = new Vector2(_room.x + _room.w*16, _room.y) * 16;
				_irb.Scale = new Vector2(1, _room.h*16);
				dungeon.AddChild(_irb);
			}
		}

		private RoomPacket[] GenRoom()
		{
			return [new RoomPacket(-1, 0, 0, 1, 1)];
		}
	}
}