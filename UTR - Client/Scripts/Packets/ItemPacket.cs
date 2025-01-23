using items;

namespace packets
{
	public class ItemPacket : Packet
	{
		Item item;
		float x, y;

		public ItemPacket(Buffer _buff) : base(_buff)
		{
		}

		public ItemPacket(int _id, Item _item, float _x, float _y, int _data = -1) : base(_id, _data)
		{
			item = _item;
			x = _x;
			y = _y;
		}

		public override byte[] Serialize()
		{
			return Concat(Serialize([8, playerId, data, x, y]), item.Serialize());
		}

		public override void Deserialize(Buffer _buff)
		{
			base.Deserialize(_buff);
			x = _buff.ReadFloat();
			y = _buff.ReadFloat();
			item = new(_buff);
		}

		public override void Run()
		{
			if (data == -1)
				ClientManager.AddDrop(playerId, item, x, y);
			if (data == 0)
				ClientManager.RemoveDrop(playerId);
		}
	}
}