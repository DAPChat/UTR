using packets;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace items
{
	public abstract class Item
	{
		public static List<Item> Items = new List<Item>();

		public static Item CreateItem(int _id)
		{
			

			return null;
		}

		private static Dictionary<int, Func<Buffer, Item>> itemT = new();

		public static void CompileAll()
		{
			itemT[0] = CreateCreator<Buffer, Item>();
		}

		static Func<TArg, T> CreateCreator<TArg, T>()
		{
			var constructor = typeof(T).GetConstructor([typeof(TArg)]);
			var parameter = Expression.Parameter(typeof(TArg));
			var creatorExpression = Expression.Lambda<Func<TArg, T>>(Expression.New(constructor, [parameter]), parameter);
			return creatorExpression.Compile();
		}
	}
}