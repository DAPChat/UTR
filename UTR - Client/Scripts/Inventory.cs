using Godot;
using items;
using System;

public partial class Inventory : Panel
{
	GridContainer slots;

	public override void _Ready()
	{
		slots = GetNode<GridContainer>("Slots");
	}

	public void SetSlot(Item _item, int _slot, int _amt)
	{
		Panel _p = slots.GetChild<Panel>(_slot);
		_p.GetNode<TextureRect>("Sprite").Texture = ResourceLoader.Load<Texture2D>(_item.item.icon);
		_p.GetNode<Label>("Count").Text = _amt.ToString();
	}
}
