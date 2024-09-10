using System.IO;
using System.Xml.Linq;

public class ActionPacket : Packet
{
	public float x, y;
	public ActionPacket(Buffer buff) : base(buff)
	{
		x = buff.ReadFloat();
		y = buff.ReadFloat();

		ServerManager.Print(x + " : " + y);
	}

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
}