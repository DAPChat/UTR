using Godot;
using System;

using items;

public partial class Player : CharacterBody2D
{


	public void Instantiate()
	{

	}

	public void SetActiveItem(Item _item)
	{
		GetNode<Sprite2D>("Item").Texture = ResourceLoader.Load<Texture2D>(_item.item.icon);
	}
}
