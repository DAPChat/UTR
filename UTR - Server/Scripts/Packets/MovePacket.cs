using System.IO;

namespace packets
{
	public class MovePacket(Buffer _buff) : Packet(_buff)
	{
		public float x, y;

		public override byte[] Serialize()
		{
			using (MemoryStream m = new())
			{
				using (BinaryWriter writer = new(m))
				{
					writer.Write(0);
					writer.Write(playerId);
					writer.Write(x);
					writer.Write(y);
				}
				return m.ToArray();
			}
		}

		public override void Deserialize(Buffer buff)
		{
			playerId = (int)buff.ReadInt();
			x = (float)buff.ReadFloat();
			y = (float)buff.ReadFloat();
		}
	}
}