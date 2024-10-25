using Godot;
using System;

using packets;
using items;

public partial class Player : CharacterBody2D
{
	int cId;
	int gId;
	int activeSlot = 0;

	public SlotPacket[] inventory = new SlotPacket[8];
	public SlotPacket[] hotbar = new SlotPacket[5];

	public void Instantiate(int _cId, int _gId)
	{
		cId = _cId;
		gId = _gId;

		hotbar[0] = new(cId, ItemManager.GetItem(0), 0, 1, 1);
		SlotPacket _sp = new(cId, hotbar[activeSlot].item, activeSlot, hotbar[activeSlot].count, 2);

		ServerManager.GetGame(gId).SendTo(cId, hotbar[0].Serialize());
		ServerManager.GetGame(gId).SendAll(_sp.Serialize());
	}

	public void SetActiveSlot(int _slot)
	{
		if (activeSlot == _slot - 1) return;
		if (hotbar[_slot - 1] == null) return;

		activeSlot = _slot-1;

		SlotPacket _sp = new(cId, hotbar[activeSlot].item, activeSlot, hotbar[activeSlot].count, 2);

		ServerManager.GetGame(gId).SendAll(_sp.Serialize());
	}

	public void UseItem()
	{
		if (hotbar[activeSlot] == null) return;
		
		Item item = hotbar[activeSlot].item;

		if (item.type == 1)
		{
			
		}
	}
}
