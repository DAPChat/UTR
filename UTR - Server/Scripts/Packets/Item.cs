using System.IO;

namespace packets
{
	public class Item : Packet
	{
		int itemId;

		public Item(Buffer _buff) : base(_buff)
		{
		}

		public Item(int _id, int _itemId, int _data = -1) : base(_id, _data)
		{
			itemId = _id;
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			itemId = buff.ReadInt();
		}

		public override byte[] Serialize()
		{
			return Serialize([4, playerId, data, itemId]);
		}
	}
}