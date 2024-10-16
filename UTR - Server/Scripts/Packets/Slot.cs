using items;

namespace packets
{
	public class Slot : Packet
	{
		Item item;
		int slot;

		public Slot(Buffer _buff) : base(_buff)
		{}

		public Slot(int _id, Item _item, int _slot, int _data = -1) : base(_id, _data)
		{
			item = _item;
			slot = _slot;
		}
	}
}