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

	public override void _Ready()
	{
		base._Ready();

		client = new();
		sceneTree = this;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

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
	}

	public static void MovePlayer(MovePacket _move)
	{
		if (!players.ContainsKey(_move.playerId))
		{
			CharacterBody2D _tempPlayer = (CharacterBody2D)ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn").Instantiate().Duplicate();
			sceneTree.GetNode<Node>("Players").AddChild(_tempPlayer);

			players[_move.playerId] = _tempPlayer as Player;
		}

		players[_move.playerId].Position = new Vector2(_move.x, _move.y);
	}

	public static void Print(string message)
	{
		GD.Print(message);
	}

	public static void SetClient(Packet _packet)
	{
		client.id = _packet.playerId;
		active = true;
	}
}
