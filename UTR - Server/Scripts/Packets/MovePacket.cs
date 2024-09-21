using System.IO;

namespace packets
{
	public class MovePacket : Packet
	{
		public float x, y;

		public MovePacket(Buffer _buff) : base(_buff) { }

		public MovePacket(int _id, float _x, float _y, int _data = -1) : base(_id, _data)
		{
			x = _x;
			y = _y;
		}

		public override byte[] Serialize()
		{
			using (MemoryStream m = new())
			{
				using (BinaryWriter writer = new(m))
				{
					writer.Write(1);
					writer.Write(playerId);
					writer.Write(data);
					writer.Write(x);
					writer.Write(y);
				}
				return m.ToArray();
			}
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			x = buff.ReadFloat();
			y = buff.ReadFloat();
		}

		public override void Run(int _gId)
		{
			//ServerManager.Print("pain");
			//ServerManager.GetGame(_gId).SendAll(Serialize());
		}
	}
}