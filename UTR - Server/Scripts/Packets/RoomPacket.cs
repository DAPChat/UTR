using System.IO;
using System.Numerics;

namespace packets
{
	public class RoomPacket : Packet
	{
		public int x;
		public int y;
		public int w;
		public int h;

		public RoomPacket(Buffer _buff) : base(_buff)
		{
		}

		public RoomPacket(int _id, int _x, int _y, int _w, int _h, int _data = -1) : base(_id, _data)
		{
			x = _x;
			y = _y;
			w = _w; 
			h = _h;
		}

		public override byte[] Serialize()
		{
			return Serialize([3, playerId, data, x, y, w, h]);
		}

		public override void Deserialize(Buffer _buff)
		{
			base.Deserialize(_buff);
			x = _buff.ReadInt();
			y = _buff.ReadInt();
			w = _buff.ReadInt();
			h = _buff.ReadInt();
		}
	}
}