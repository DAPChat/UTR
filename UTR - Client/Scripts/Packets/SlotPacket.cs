using items;
using System.Linq;

namespace packets
{
	public class SlotPacket : Packet
	{
		public Item item;
		public int slot;
		public int count;

		public SlotPacket(Buffer _buff) : base(_buff) {}

		public SlotPacket(int _id, Item _item, int _slot, int _count, int _data = -1) : base(_id, _data)
		{
			item = _item;
			slot = _slot;
			count = _count;
		}

		public override byte[] Serialize()
		{
			return Concat(Serialize([4, playerId, data, slot, count]), item.Serialize());
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			slot = buff.ReadInt();
			count = buff.ReadInt();
			item = new(buff);
		}

		public override void Run()
		{
			if (data == 2)
			{
				ClientManager.SetPlayerItem(playerId, item);
				return;
			}
			if (playerId == ClientManager.client.id)
			{
				ClientManager.SetSlot(item, slot, count, data);
			}
		}
	}
}