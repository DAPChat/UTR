using Godot;
using items;
using System;

public partial class Hotbar : Panel
{
	public void SetSlot(Item _item, int _slot, int _amt)
	{
		Panel _p = GetChild<Panel>(_slot);
		_p.GetNode<TextureRect>("Sprite").Texture = ResourceLoader.Load<Texture2D>(_item.item.icon);
		_p.GetNode<Label>("Count").Text = _amt.ToString();

		(_p as Slot).Instance(_item);
	}
}
