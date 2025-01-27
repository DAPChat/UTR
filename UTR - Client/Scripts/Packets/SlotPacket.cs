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

		public SlotPacket(int _id, Item _item, int _slot, int _count, int _order, int _data = -1) : base(_id, _data)
		{
			if (ClientManager.GetPlayer(_id) != null)
				ClientManager.GetPlayer(_id).slotOrder++;

			item = _item;
			slot = _slot;
			count = _count;
			order = _order;
		}

		public override byte[] Serialize()
		{
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

		public override void Run()
		{

			if (data == 2)
			{
				//if (order <= ClientManager.GetPlayer(playerId).slotOrder) return;
				ClientManager.SetPlayerItem(this);
				return;
			}
			if (playerId == ClientManager.client.id)
			{
				if (count <= 0) ClientManager.RemoveSlot(this);
				else ClientManager.SetSlot(item, slot, count, data);
			}
		}
	}
}