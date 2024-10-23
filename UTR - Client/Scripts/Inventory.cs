using Godot;
using items;
using System;

public partial class Inventory : Panel
{
	Control slots;

	public override void _Ready()
	{
		slots = GetNode<Control>("Slots");
	}

	public void SetSlot(Item _item, int _slot, int _amt)
	{
		Panel _p = slots.GetChild<Panel>(_slot);
		_p.GetNode<TextureRect>("Sprite").Texture = ResourceLoader.Load<Texture2D>(_item.item.icon);
		_p.GetNode<Label>("Count").Text = _amt.ToString();

		(_p as Slot).Instance(_item);
	}

	public void SetTooltip(Item _item)
	{
		GetNode<RichTextLabel>("Tooltip").Text = "Tooltip\n" + _item.ToString();
	}
}
