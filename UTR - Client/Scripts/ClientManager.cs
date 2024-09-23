using Godot;
using System;

using packets;
using System.Collections.Generic;

public partial class ClientManager : Node
{
	static Client client;

	static ClientManager sceneTree;

	static Dictionary<int, Player> players = new();

	public static bool active = false;
	public static int curId;
	public static List<Packet> packetQ = new();

	private static bool readingQueue = false;

	public override void _Ready()
	{
		base._Ready();

		curId = GD.RandRange(1,999);

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
			_sprite.FlipH = false;
		}
		if (Input.IsActionPressed("right"))
		{
			_inputVect.X += 1;
			_sprite.FlipH = true;
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

		client.udp.Send(new InputPacket(client.id, _inputVect).Serialize());

		players[client.id].Velocity = ((Vector2)_inputVect).Normalized() * (float)delta * 30000;
		try
		{
			players[client.id].MoveAndSlide();
		}
		catch (Exception){ }
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
			sceneTree.GetNode<Node>("Players").AddChild(_tempPlayer);

			players[_move.playerId] = _tempPlayer as Player;

			if (_move.playerId == client.id) active = true;
		}

		AnimatedSprite2D _pAnim = players[_move.playerId].GetNode<AnimatedSprite2D>("PlayerView");

		if (_move.data == 1 && _move.playerId != client.id)
		{
			float _pLocX = players[_move.playerId].Position.X;
			float _nLocX = _move.x;

			if (_pLocX != _nLocX)
			{
				if (_pLocX < _nLocX)
					_pAnim.FlipH = true;
				else if (_pLocX > _nLocX)
					_pAnim.FlipH = false;
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

	public static void SetClient(Packet _packet)
	{
		client.id = _packet.playerId;

		_packet.data = TitleScene.reqId;

		client.udp.Send(_packet.Serialize());
	}
}
