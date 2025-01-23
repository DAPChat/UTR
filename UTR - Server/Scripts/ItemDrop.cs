using Godot;
using System;

using items;

public partial class ItemDrop : Area2D
{
	public Item item;
	public int id;

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

				if (_p.AddItem(item, true))
				{
					ServerManager.GetGame(_p.gId).itemIds.Remove(id);
					ServerManager.GetGame(_p.gId).SendAll(new packets.ItemPacket(id, item, Position.X, Position.Y, 0).Serialize());
					QueueFree();
				}
				else accepting = true;
			}
		};
	}
}
