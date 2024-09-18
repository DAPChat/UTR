using Godot;
using System;

using packets;

public partial class ClientManager : Node
{
	static Client client;

	static ClientManager sceneTree;

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
