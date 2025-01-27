using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using packets;

public static class PacketManager
{
	private static Dictionary<int, Func<Buffer, Packet>> packetL = new();

	public static void CompileAll()
	{
		packetL[0] = CreateCreator<Buffer, Packet>();
		packetL[1] = CreateCreator<Buffer, MovePacket>();
		packetL[2] = CreateCreator<Buffer, InputPacket>();
		packetL[3] = CreateCreator<Buffer, RoomPacket>();
		packetL[4] = CreateCreator<Buffer, SlotPacket>();
		packetL[5] = CreateCreator<Buffer, StatsPacket>();
		packetL[6] = CreateCreator<Buffer, EnemyPacket>();
		packetL[7] = CreateCreator<Buffer, StatePacket>();
		packetL[8] = CreateCreator<Buffer, ItemPacket>();
	}

	static Func<TArg, T> CreateCreator<TArg, T>()
	{
		var constructor = typeof(T).GetConstructor([typeof(TArg)]);
		var parameter = Expression.Parameter(typeof(TArg));
		var creatorExpression = Expression.Lambda<Func<TArg, T>>(Expression.New(constructor, [parameter]), parameter);
		return creatorExpression.Compile();
	}

	public static Packet CreatePacket(byte[] buff)
	{
		Buffer _tempBuff = new(buff);

		try
		{
			return packetL[_tempBuff.ReadInt()](_tempBuff);
		}
		catch (Exception ex)
		{
			string s = "";
			foreach (var b in buff)
			{
				s += b;
			}
			ClientManager.Print(s);
		}
		return null;
	}
}