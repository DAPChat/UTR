﻿using Godot;

namespace packets
{
	public class StatePacket : Packet
	{
		public int entityType;
		public int state;

		public StatePacket(Buffer _buff) : base(_buff)
		{

		}

		public StatePacket(int _id, int _eT, int _s, int _data = -1) : base(_id, _data)
		{
			entityType = _eT;
			state = _s;
		}

		public override byte[] Serialize()
		{
			return Serialize([7, playerId, data, entityType, state]);
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			entityType = buff.ReadInt();
			state = buff.ReadInt();
		}

		public override void Run()
		{
			// 0 -> player
			// 1 -> enemy
			if (entityType == 0 && ClientManager.GetPlayer(playerId) != null)
				ClientManager.GetPlayer(playerId).StateUpdate(state, data);
			else if (entityType == 1 && ClientManager.GetEntity(playerId) != null)
				ClientManager.GetEntity(playerId).StateChange(state, data);
		}
	}
}