using Godot;
using items;
using System;

public partial class Inventory : Panel
{
	Control slots;

	public Slot activeHover = null;

	public override void _Ready()
	{
		slots = GetNode<Control>("Slots");
	}

	public void SetSlot(Item _item, int _slot, int _amt)
	{
		Panel _p = slots.GetChild<Panel>(_slot);
		_p.GetNode<TextureRect>("Sprite").Texture = ResourceLoader.Load<Texture2D>(_item.item.icon);
		_p.GetNode<Label>("Count").Text = _amt.ToString();

		(_p as Slot).Instance(_item, 1, _slot);
	}

	public void RemoveSlot(int _slot)
	{
		Panel _p = slots.GetChild<Panel>(_slot);
		_p.GetNode<TextureRect>("Sprite").Texture = null;
		_p.GetNode<Label>("Count").Text = "";

		(_p as Slot).item = null;
	}

	public void SetTooltip(Slot _slot)
	{
		if (_slot == null || _slot.item == null) return;

		activeHover = _slot;

		GetNode<RichTextLabel>("TooltipBg/Tooltip").Text = "Tooltip:\n" + _slot.item.ToString();
	}
}
