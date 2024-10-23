using Godot;
using System;

using packets;
using items;

public partial class Player : CharacterBody2D
{
	int cId;
	int gId;

	public SlotPacket[] inventory = new SlotPacket[8];
	public SlotPacket[] hotbar = new SlotPacket[5];

	public void Instantiate(int _cId, int _gId)
	{
		cId = _cId;
		gId = _gId;

		hotbar[0] = new(cId, ItemManager.GetItem(0), 0, 1);

		ServerManager.GetGame(gId).SendTo(cId, hotbar[0].Serialize());
	}
}
