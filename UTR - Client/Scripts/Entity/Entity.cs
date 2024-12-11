using Godot;
using System;

public partial class Entity : RigidBody2D
{
	public int id;
	public int order;
	public int health;
	public Vector2 position;

	public void Instantiate(int _id)
	{
		GetNode<AnimatedSprite2D>("Sprite").Play("idle");

		id = _id;
		order = -1;
	}

	public void Update(int _order, Vector2 _pos, int _health)
	{
		if (_order > order)
		{
			order = _order;
			position = _pos;
			health = _health;
		}
	}

	public override void _Process(double delta)
	{
		Position = position;
	}

	public void StateChange(int _s, int _data)
	{
		AnimatedSprite2D sprite = GetNode<AnimatedSprite2D>("Sprite");

		if (_s == 0)
		{
			sprite.Play("idle");
		}
		else if (_s == 1)
		{
			sprite.Play("run");
		}
	}
}