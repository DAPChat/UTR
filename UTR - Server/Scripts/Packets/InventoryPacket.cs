using System.Collections.Generic;

namespace packets
{
	public class InventoryPacket : Packet
	{
		public Slot[] inventory;
		public Slot[] hotbar;

		public InventoryPacket(Buffer _buff) : base(_buff){}

		public InventoryPacket(int _id, Slot[] _inv, Slot[] _bar, int _data = -1) : base(_id, _data)
		{
			inventory = _inv;
			hotbar = _bar;
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);

			inventory = new Slot[8];
			hotbar = new Slot[5];

			for (int i = 0; i < buff.ReadInt(); i++)
			{
				inventory[i] = new(buff.ReadInt(), buff.ReadInt());
			}
			for (int i = 0; i < buff.ReadInt(); i++)
			{
				hotbar[i] = new(buff.ReadInt(), buff.ReadInt());
			}
		}

		public override byte[] Serialize()
		{
			//List<int>

			//return Serialize([4, playerId, data, inv, hot]);
			return null;
		}
	}
}