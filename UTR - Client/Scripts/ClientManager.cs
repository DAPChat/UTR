using Godot;
using System;

using packets;

public partial class ClientManager : Node
{
	static Client client;

	static ClientManager sceneTree;

	public override void _Ready()
	{
		base._Ready();

		client = new();
		sceneTree = this;
	}

	public static void Print(string message)
	{
		GD.Print(message);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		Vector2I _inputVect = Vector2I.Zero;

		if (Input.IsActionPressed("left"))
			_inputVect.X -= 1;
		if (Input.IsActionPressed("right"))
			_inputVect.X += 1;
		if (Input.IsActionPressed("down"))
			_inputVect.Y -= 1;
		if (Input.IsActionPressed("up"))
			_inputVect.Y += 1;

		client.SendUDP(new InputPacket(_inputVect).Serialize());
	}

	public static void MovePlayer(MovePacket _move)
	{
		sceneTree.GetNode<Sprite2D>("Player").Position = new Vector2(_move.x, _move.y);
	}
}
