using System.IO;
using System.Xml.Linq;

public abstract class Packet
{
	Buffer buffer;

	public Packet(Buffer _buff)
	{
		buffer = _buff;
	}

	public abstract byte[] Serialize();
}