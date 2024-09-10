using System.IO;

namespace packets
{
	public class ActionPacket(Buffer _buff) : Packet(_buff)
	{
		public float x, y;

		public override byte[] Serialize()
		{
			using (MemoryStream m = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(m))
				{
					writer.Write(0);
					writer.Write(x);
					writer.Write(y);
				}
				return m.ToArray();
			}
		}

		public override void Deserialize(Buffer buff)
		{
			x = (float)buff.ReadFloat();
			y = (float)buff.ReadFloat();
		}
	}
}