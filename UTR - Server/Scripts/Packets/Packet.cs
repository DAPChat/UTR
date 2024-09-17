namespace packets
{
	public abstract class Packet
	{
		public int playerId;

		public Packet(Buffer _buff)
		{
			Deserialize(_buff);
		}

		public Packet(int _id)
		{
			playerId = _id;
		}

		public abstract void Run(game.Game _game);

		public abstract byte[] Serialize();

		public abstract void Deserialize(Buffer buff);
	}
}