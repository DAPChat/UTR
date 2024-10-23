using Godot;
using System;

public partial class Slot : Panel
{
	public items.Item item;

	public void Instance(items.Item _item)
	{
		item = _item;

		MouseEntered += () =>
		{
			ClientManager.inventory.SetTooltip(item);
		};
	}
}
