using System.IO;

namespace packets
{
	public class Packet
	{
		public int playerId;
		public int gameId;

		public Packet(Buffer _buff)
		{
			Deserialize(_buff);
		}

		public Packet(int _id) 
		{
			playerId = _id;
		}

		public virtual void Run()
		{
			ClientManager.SetClient(this);
		}

		public virtual byte[] Serialize()
		{
			using (MemoryStream m = new())
			{
				using (BinaryWriter writer = new(m))
				{
					writer.Write(0);
					writer.Write(playerId);
					writer.Write(gameId);
				}
				return m.ToArray();
			}
		}

		public virtual void Deserialize(Buffer buff)
		{
			playerId = buff.ReadInt();
			gameId = buff.ReadInt();
		}
	}
}