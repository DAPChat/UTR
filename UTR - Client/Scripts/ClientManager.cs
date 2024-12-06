using Godot;
using System;

using packets;
using System.Collections.Generic;

public partial class ClientManager : Node
{
	public static Client client { get; private set; }

	static ClientManager sceneTree;

	static Dictionary<int, Player> players = new();

	static Dictionary<int, Entity> entities = new();

	public static bool active = false;
	public static int curId;
	public static List<Packet> packetQ = new();

	private static bool readingQueue = false;

	private static Camera2D camera;
	private static TileMapLayer dungeon;
	public static Inventory inventory;
	public static Hotbar hotbar;

	public override void _Ready()
	{
		base._Ready();

		dungeon = GetNode<TileMapLayer>("GameRoom");
		inventory = GetNode<Panel>("UI/Inventory") as Inventory;
		hotbar = GetNode<Panel>("UI/Hotbar") as Hotbar;

		curId = GD.RandRange(1,999);

		camera = new();
		camera.Zoom = new(4, 4);

		client = new();
		sceneTree = this;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (packetQ.Count != 0 && !readingQueue)
		{
			readingQueue = true;
			ReadQueue();
		}

		if (!active) return;

		Vector2I _inputVect = Vector2I.Zero;

		AnimatedSprite2D _sprite = players[client.id].GetNode<AnimatedSprite2D>("PlayerView");

		if (!_sprite.IsPlaying()) _sprite.Play("run_accel");

		if (Input.IsActionPressed("down"))
			_inputVect.Y += 1;
		if (Input.IsActionPressed("up"))
			_inputVect.Y -= 1;
		if (Input.IsActionPressed("left"))
		{
			_inputVect.X -= 1;
			players[client.id].Scale = new (1, 1);
		}
		if (Input.IsActionPressed("right"))
		{
			_inputVect.X += 1;
			players[client.id].Scale = new(-1, 1);
		}

		if (_inputVect == Vector2I.Zero || !_sprite.IsPlaying())
		{
			if (_sprite.Animation != "run_accel" || !_sprite.IsPlaying())
				_sprite.Play("run_accel");
		}
		else if (_sprite.Animation != "run_max")
		{
			_sprite.Play("run_max");
		}

		client.udp.Send(new MovePacket(client.id, _inputVect.X, _inputVect.Y).Serialize());

		players[client.id].Velocity = players[client.id].Velocity.MoveToward(((Vector2)_inputVect).Normalized() * 100, 1500 * (float)delta);
		try
		{
			//players[client.id].MoveAndSlide();
		}
		catch (Exception) { }
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("inventory"))
		{
			if (!inventory.Visible) inventory.Show();
			else inventory.Hide();
		}

		int _in = -1;

		if (@event.IsActionPressed("slot_1"))
			_in = 1;
		if (@event.IsActionPressed("slot_2"))
			_in = 2;
		if (@event.IsActionPressed("slot_3"))
			_in = 3;
		if (@event.IsActionPressed("slot_4"))
			_in = 4;
		if (@event.IsActionPressed("slot_5"))
			_in = 5;
		if (@event.IsActionPressed("use"))
			_in = 6;

		if (_in != -1)
			client.udp.Send(new InputPacket(client.id, _in).Serialize());
	}

	private static void ReadQueue()
	{
		while (packetQ.Count != 0)
		{
			if (packetQ[0] != null)
			{
				packetQ[0].Run();
				packetQ.RemoveAt(0);
			}
			else packetQ.RemoveAt(0);
		}
		readingQueue = false;
	}

	public static void MovePlayer(MovePacket _move)
	{
		if (!players.ContainsKey(_move.playerId))
		{
			CharacterBody2D _tempPlayer = (CharacterBody2D)ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn").Instantiate().Duplicate();
			sceneTree.GetNode<Node2D>("Players").AddChild(_tempPlayer);

			players[_move.playerId] = _tempPlayer as Player;
			players[_move.playerId].Instantiate();

			if (_move.playerId == client.id)
			{
				_tempPlayer.AddChild(camera);
			}
		}

		if (!active && client.id == _move.playerId && _move.data != -2)
		{
			active = true;
		}

		AnimatedSprite2D _pAnim = players[_move.playerId].GetNode<AnimatedSprite2D>("PlayerView");
		Sprite2D _wpn = players[_move.playerId].GetNode<Sprite2D>("Item");

		if (_move.data != 0 && _move.playerId != client.id)
		{
			if (_move.data == 1)
			{
				players[_move.playerId].Scale = new Vector2(-1, 1);
			}
			else
			{
				players[_move.playerId].Scale = new Vector2(1, 1);
			}

			if (_pAnim.Animation != "run_max")
				_pAnim.Play("run_max");
		}
		else if ((_pAnim.Animation != "run_accel" || !_pAnim.IsPlaying()) && _move.playerId != client.id)
		{
			_pAnim.Play("run_accel");
		}

		players[_move.playerId].SetDeferred(Node2D.PropertyName.Position, new Vector2(_move.x, _move.y));
	}

	public static void Print(string message)
	{
		GD.Print(message);
	}

	public static void CreateRoom(RoomPacket _room)
	{
		//Vector2I _rPos = new(_room.x, _room.y) * 16;

		//dungeon.Clear();

		int _rScale = 16;

		int x = _room.x * _rScale;
		int y = _room.y * _rScale;
		int h = _room.h * _rScale;
		int w = _room.w * _rScale;

		for (int i = x; i <= w + x; i++)
		{
			for (int j = y; j <= h + y; j++)
			{
				if (dungeon.GetCellSourceId(new(i, j)) == 1) continue;
				if (j == y || i == x || j == y + h || i == x + w)
					dungeon.SetCell(new Vector2I(i, j), 0, new(0, 2));
				else dungeon.SetCell(new Vector2I(i, j), 0, new(8, 3));
			}
		}

		int by = _room.data == 3 ? 24 : 8;

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

			dungeon.SetCell(new(dX, dY), 1, new(5, 3));
		}

		return;
		for (int i = _room.x; i <= _room.w*_rScale + _room.x + 1; i++)
		{
			for (int j = _room.y; j <= _room.h*_rScale + _room.y + 1; j++)
			{
				if (j == _room.y) dungeon.SetCell(new Vector2I(i, j), 0, new(0, 2));
				if (i == _room.x) dungeon.SetCell(new Vector2I(i, j), 0, new(0, 2));
				if (j == _room.y+_room.h*_rScale+1) dungeon.SetCell(new Vector2I(i, j), 0, new(0, 2));
				if (i == _room.x+_room.w*_rScale+1) dungeon.SetCell(new Vector2I(i, j), 0, new(0, 2));
			}
		}
	}

	public static void SetSlot(items.Item _item, int _slot, int _amt, int _loc)
	{
		if (_loc == 0)
			inventory.SetSlot(_item, _slot, _amt);
		else hotbar.SetSlot(_item, _slot, _amt);
	}

	public static void RemoveSlot(SlotPacket _slot)
	{
		if (_slot.data == 0)
			inventory.RemoveSlot(_slot.slot);
		else hotbar.RemoveSlot(_slot.slot);
	}

	public static void SetPlayerItem(SlotPacket _slot)
	{
		players[_slot.playerId].SetActiveItem(_slot);
	}

	public static void UpdateStats(StatsPacket _stats)
	{
		if (_stats.playerId != client.id) return;

		sceneTree.GetNode<Label>("UI/Health").Text = _stats.health.ToString();// + "/" + players[_stats.playerId].health;
		sceneTree.GetNode<HSlider>("UI/HealthSlider").Value = _stats.health;
	}

	public static void MoveEntity(EnemyPacket _enemy)
	{
		if (!entities.ContainsKey(_enemy.enemyId))
		{
			Entity body = (Entity)ResourceLoader.Load<PackedScene>("res://Scenes/enemy.tscn").Instantiate().Duplicate();

			sceneTree.GetNode<Node>("Enemies").AddChild(body);
			body.Instantiate();
			entities[_enemy.enemyId] = body;
		}
		entities[_enemy.enemyId].Update(new(_enemy.x, _enemy.y), _enemy.health);
	}

	public static void RemoveEntity(int _eId)
	{
		entities[_eId].QueueFree();
		entities.Remove(_eId);
	}

	public static void RemoveClient(int _id)
	{
		players[_id].QueueFree();
		players.Remove(_id);
	}

	public static void SetClient(Packet _packet)
	{
		client.id = _packet.playerId;

		_packet.data = TitleScene.reqId;

		MovePlayer(new(_packet.playerId, 0, 0, -2));

		client.udp.Send(_packet.Serialize());
	}
}
