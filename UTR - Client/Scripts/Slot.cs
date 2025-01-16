using Godot;
using System;

public partial class Slot : Panel
{
	public items.Item item;

	// 1 -> inventory
	// 2 -> hotbar
	public int loc;
	public int slot;

	bool begun = false;

	public void Instance(items.Item _item, int _loc, int _slot)
	{
		if (item != null && _item.item == item.item) return;

		item = _item;
		loc = _loc;
		slot = _slot;

		if (!begun)
		{
			begun = true;
			try
			{
				MouseEntered += () => ClientManager.inventory.SetTooltip(this);
			}
			catch (Exception) { }
		}
	}
}
