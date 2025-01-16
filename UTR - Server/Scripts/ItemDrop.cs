using Godot;
using System;

using items;

public partial class ItemDrop : Area2D
{
	public Item item;

	bool accepting = true;
	
	public void Instantiate(Item _item)
	{
		item = _item;

		AreaEntered += (body) =>
		{
			if (body.GetParent().GetType() == typeof(Player))
			{
				if (!accepting) return;
				accepting = false;
				Player _p = (Player)body.GetParent();

				if (_p.AddItem(item, true)) QueueFree();
				else accepting = true;
			}
		};
	}
}
