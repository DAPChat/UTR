using Godot;
using System;

using packets;

public partial class Player : CharacterBody2D
{
	public int outOrder;
	public int inOrder;
	public int health;

	public void Instantiate()
	{
		outOrder = 0;
		inOrder = -1;
	}

	public void SetActiveItem(SlotPacket _slot)
	{
		if (_slot.count == 0)
		{
			GetNode<Sprite2D>("Item").Texture = null;
			return;
		}

		GetNode<Sprite2D>("Item").Texture = ResourceLoader.Load<Texture2D>(_slot.item.item.icon);
	}

	// Update States
	/*
	-1 -> dead
	0 -> use item (anim)
	1 -> damage (take -> kb)
	 */

	public void StateUpdate(int _s, int _data)
	{
		if (_s == 0)
		{

		}
		else if (_s == 1)
		{

		}
	}
}
