﻿namespace packets
{
	public class EnemyPacket : Packet
	{
		int enemyId;
		float x;
		float y;
		int health;

		public EnemyPacket(Buffer _buff) : base(_buff)
		{
		}

		public EnemyPacket(int _id, enemy.Enemy _en, int _data = -1) : base(_id, _data)
		{
			enemyId = _en.enemyId;
			x = _en.Position.X;
			y = _en.Position.Y;
			health = _en.health;
		}

		public override byte[] Serialize()
		{
			return Serialize([6, playerId, data, enemyId, x, y, health]);
		}

		public override void Deserialize(Buffer buff)
		{
			base.Deserialize(buff);
			enemyId = buff.ReadInt();
			x = buff.ReadFloat();
			y = buff.ReadFloat();
			health = buff.ReadInt();
		}
	}
}