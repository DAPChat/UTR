namespace packets
{
	public abstract class Packet
	{
		public Packet(Buffer _buff)
		{
			Deserialize(_buff);
		}

		public abstract byte[] Serialize();

		public abstract void Deserialize(Buffer buff);
	}
}