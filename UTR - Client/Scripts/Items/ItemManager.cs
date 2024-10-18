using Godot;

namespace items
{
	public static class ItemManager
	{
		public static Item GetItem(int id)
		{
			return new Item(GetItemBase(id));
		}

		public static ItemBase GetItemBase(int id)
		{
			return ResourceLoader.Load<ItemBase>("res://Items/" + id + ".tres");
		}
	}
}