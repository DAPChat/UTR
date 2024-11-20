using Godot;
using System;

public partial class Room : Area2D
{
	int gId;
	packets.RoomPacket rp;

	public void Instantiate(int _gId, packets.RoomPacket _rp)
	{
		gId = _gId;
		rp = _rp;

		BodyEntered += (body) => {
			if (body.GetType() == typeof(Player))
				ServerManager.GetGame(gId).ChangeRoom(((Player)body).cId, rp);
		};
	}
}
