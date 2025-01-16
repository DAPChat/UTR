using Godot;
using items;
using System.Linq;

namespace packets
{
	public class SlotPacket : Packet
	{
		public Item item;
		public int slot;
		public int count;

		public int order;

		public SlotPacket(Buffer _buff) : base(_buff) {}

		public SlotPacket(int _id, Item _item, int _slot, int _count, int _data = -1) : base(_id, _data)
		{
			item = _item;
			slot = _slot;
			count = _count;
		}

		public override byte[] Serialize()
		{
			order = ServerManager.GetClient(playerId).player.slotOrder;
			ServerManager.GetClient(playerId).player.slotOrder++;

			return Concat(Serialize([4, playerId, data, slot, count, order]), item.Serialize());
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			slot = buff.ReadInt();
			count = buff.ReadInt();
			order = buff.ReadInt();
			item = new(buff);
		}

		public override string ToString()
		{
			return "{" + item + ": " + slot + ": " + count + "}";
		}
	}
}