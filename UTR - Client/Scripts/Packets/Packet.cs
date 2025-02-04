﻿using System;
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
			if (data == -2) ClientManager.RemoveClient(playerId);
			else if (data == -1) ClientManager.SetClient(this);
			else if (data == 0)
			{
				ClientManager.SetRoomId(playerId);
			}
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
						else if (t == typeof(char)) writer.Write((char)obj);
						else if (t == typeof(bool)) writer.Write((bool)obj);
					}
				}

				return m.ToArray();
			}
		}

		public static byte[] Concat(byte[] _a, byte[] _b)
		{
			byte[] c = new byte[_a.Length + _b.Length];
			_a.CopyTo(c, 0);
			_b.CopyTo(c, _a.Length);
			return c;
		}
	}
}