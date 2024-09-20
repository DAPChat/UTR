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

		if (Input.IsActionPressed("left"))
			_inputVect.X -= 1;
		if (Input.IsActionPressed("right"))
			_inputVect.X += 1;
		if (Input.IsActionPressed("down"))
			_inputVect.Y += 1;
		if (Input.IsActionPressed("up"))
			_inputVect.Y -= 1;

		client.udp.Send(new InputPacket(client.id, _inputVect).Serialize());

		players[client.id].Velocity = ((Vector2)_inputVect).Normalized() * (float)delta * 10000;
		try
		{
			players[client.id].MoveAndSlide();
		}catch (Exception){ }
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

		players[_move.playerId].SetDeferred(Node2D.PropertyName.Position, new Vector2(_move.x, _move.y));
	}

	public static void Print(string message)
	{
		GD.Print(message);
	}

	public static void SetClient(Packet _packet)
	{
		client.id = _packet.playerId;

		client.udp.Send(_packet.Serialize());
	}
}
