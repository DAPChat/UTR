using Godot;
using System;

public partial class Slot : Panel
{
	public items.Item item;

	public void Instance(items.Item _item)
	{
		item = _item;

		GD.Print("Here");

		MouseEntered += () =>
		{
			GD.Print("Hereee");
			ClientManager.inventory.SetTooltip(item);
		};
	}
}
