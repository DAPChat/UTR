using System;
using System.Collections.Generic;
using System.Linq.Expressions;

public static class PacketManager
{
	private static Dictionary<int, Func<Buffer, object>> packets = new();

	public static void CompileAll()
	{
		packets[0] = CreateCreator<Buffer, ActionPacket>();
	}

	static Func<TArg, T> CreateCreator<TArg, T>()
	{
		var constructor = typeof(T).GetConstructor([typeof(TArg)]);
		var parameter = Expression.Parameter(typeof(TArg));
		var creatorExpression = Expression.Lambda<Func<TArg, T>>(Expression.New(constructor, [parameter]), parameter);
		return creatorExpression.Compile();
	}

	public static object CreatePacket(byte[] buff)
	{
		Buffer _tempBuff = new(buff);
		int _packetId = _tempBuff.ReadInt();

		ServerManager.Print(_packetId.ToString());

		return packets[_packetId](_tempBuff);
	}
}