using System.IO;
using System.Linq;
using System.Numerics;

namespace packets
{
	public class RoomPacket : Packet
	{
		public int x;
		public int y;
		public int w;
		public int h;
		public bool[] r;

		public RoomPacket(Buffer _buff) : base(_buff)
		{
		}

		public RoomPacket(int _id, int _x, int _y, int _w, int _h, bool[] _r, int _data = -1) : base(_id, _data)
		{
			x = _x;
			y = _y;
			w = _w; 
			h = _h;
			r = _r;
		}

		public override byte[] Serialize()
		{
			object[] _r = new object[4];
			r.CopyTo(_r, 0);

			return Concat(Serialize([3, playerId, data, x, y, w, h]), Serialize(_r));
		}

		public override void Deserialize(Buffer _buff)
		{
			base.Deserialize(_buff);
			x = _buff.ReadInt();
			y = _buff.ReadInt();
			w = _buff.ReadInt();
			h = _buff.ReadInt();

			r = new bool [4]; 

			for (int i = 0; i < 4; i++)
			{
				r[i] = _buff.ReadBool();
			}
		}

		public override string ToString()
		{
			string s = "";

			foreach (bool b in r) s += b + ", ";

			return x + " " + y + " " + s;
		}
	}
}