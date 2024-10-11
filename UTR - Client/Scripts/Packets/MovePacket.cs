using System.IO;

namespace packets
{
	public class MovePacket : Packet
	{
		public float x, y;

		public MovePacket(Buffer _buff) : base(_buff) { }

		public MovePacket(int _id, float _x, float _y, int _data = 0) : base(_id, _data)
		{
			x = _x;
			y = _y;
		}

		public override byte[] Serialize()
		{
			return Serialize([1, playerId, data, x, y]);
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			x = buff.ReadFloat();
			y = buff.ReadFloat();
		}

		public override void Run()
		{
			ClientManager.MovePlayer(this);
		}
	}
}