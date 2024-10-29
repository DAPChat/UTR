using Godot;
using System;

using packets;

public partial class Player : CharacterBody2D
{
	public int health;

	public void Instantiate()
	{

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
}
