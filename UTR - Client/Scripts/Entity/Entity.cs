using Godot;
using System;

public partial class Entity : RigidBody2D
{
	public int health;

	public void Instantiate()
	{

	}

	public void Update(Vector2 _pos, int _health)
	{
		Position = _pos;
		health = _health;
	}
}