using System.IO;

namespace packets
{
	public class MovePacket : Packet
	{
		public int order;
		public float x, y;

		public float cX, cY;

		public MovePacket(Buffer _buff) : base(_buff) { }

		public MovePacket(int _id, int _order, float _x, float _y, float _cX, float _cY, int _data = -1) : base(_id, _data)
		{
			order = _order;

			if (ClientManager.GetPlayer(_id) != null)
				ClientManager.GetPlayer(_id).outOrder++;

			x = _x;
			y = _y;
			cX = _cX;
			cY = _cY;
		}

		public override byte[] Serialize()
		{
			return Serialize([1, playerId, data, order, x, y, cX, cY]);
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			order = buff.ReadInt();
			x = buff.ReadFloat();
			y = buff.ReadFloat();
			cX = buff.ReadFloat();
			cY = buff.ReadFloat();
		}

		public override void Run()
		{
			ClientManager.MovePlayer(this);
		}
	}
}