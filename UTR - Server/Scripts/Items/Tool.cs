namespace item
{
	public class Tool : packets.Item
	{
		public int type;
		public int damage;
		public int cooldown;
		public int scale;
		public int range;
		public int aoe;
		public int aoeShape;

		public Tool(Buffer _buff) : base(_buff)
		{
		}

		public Tool(int _id, int _itemId, int _data = -1) : base(_id, _itemId, _data)
		{
		}
	}
}