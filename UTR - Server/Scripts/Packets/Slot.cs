using items;
using System.Linq;

namespace packets
{
	public class Slot : Packet
	{
		public Item item;
		public int slot;
		public int count;

		public Slot(Buffer _buff) : base(_buff) {}

		public Slot(int _id, Item _item, int _slot, int _count, int _data = -1) : base(_id, _data)
		{
			item = _item;
			slot = _slot;
			count = _count;
		}

		public override byte[] Serialize()
		{
			return Serialize([4, playerId, data, slot, count]).Concat(item.Serialize()).ToArray();
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			slot = buff.ReadInt();
			count = buff.ReadInt();
			item = new(buff);
		}
	}
}