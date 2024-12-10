using System.IO;

namespace packets
{
	public class MovePacket : Packet
	{
		public int order;
		public float x, y;

		public MovePacket(Buffer _buff) : base(_buff) { }

		public MovePacket(int _id, int _order, float _x, float _y, int _data = -1) : base(_id, _data)
		{
			order = _order;

			ClientManager.GetPlayer(_id).outOrder++;

			x = _x;
			y = _y;
		}

		public override byte[] Serialize()
		{
			return Serialize([1, playerId, data, order, x, y]);
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			order = buff.ReadInt();
			x = buff.ReadFloat();
			y = buff.ReadFloat();
		}

		public override void Run()
		{
			ClientManager.MovePlayer(this);
		}
	}
}