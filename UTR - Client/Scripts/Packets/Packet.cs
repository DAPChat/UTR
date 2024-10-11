using System;
using System.IO;

namespace packets
{
	public class Packet
	{
		public int playerId;
		public int data = -1;

		public Packet(Buffer _buff)
		{
			Deserialize(_buff);
		}

		public Packet(int _id, int _data = -1)
		{
			playerId = _id;
			data = _data;
		}

		public virtual void Run()
		{
			ClientManager.SetClient(this);
		}

		public virtual byte[] Serialize()
		{
			return Serialize([0, playerId, data]);
		}

		public virtual void Deserialize(Buffer buff)
		{
			playerId = buff.ReadInt();
			data = buff.ReadInt();
		}

		public static byte[] Serialize(object[] _o)
		{
			using (MemoryStream m = new())
			{
				using (BinaryWriter writer = new(m))
				{
					foreach (object obj in _o)
					{
						Type t = obj.GetType();

						if (t == typeof(int)) writer.Write((int)obj);
						else if (t == typeof(float)) writer.Write((float)obj);
						else if (t == typeof(string)) writer.Write((string)obj);
					}
				}

				return m.ToArray();
			}
		}
	}
}