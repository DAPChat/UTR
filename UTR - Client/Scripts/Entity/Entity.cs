using Godot;
using System;

public partial class Entity : RigidBody2D
{
	public int id;
	public int order;
	public int health;

	public void Instantiate(int _id)
	{
		id = _id;
		order = -1;
	}

	public void Update(int _order, Vector2 _pos, int _health)
	{
		if (_order > order)
		{
			order = _order;
			Position = _pos;
			GD.Print(order, id, Position);
			health = _health;
		}
		else GD.Print("Nope");
	}

	public override void _PhysicsProcess(double delta)
	{
		GD.Print(id, Position);
	}
}