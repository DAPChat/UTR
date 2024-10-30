using Godot;
using System;

public partial class Slot : Panel
{
	public items.Item item;

	public void Instance(items.Item _item)
	{
		if (item != null && _item.item == item.item) return;

		item = _item;

		try
		{
			MouseEntered += () => ClientManager.inventory.SetTooltip(item);
		}
		catch (Exception) { }
	}
}
