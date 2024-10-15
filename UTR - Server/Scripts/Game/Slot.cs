public class Slot
{
	public int number;
	public Item item;

	public Slot(int _num, int _item)
	{
		number = _num;
		item = new (_item);
	}
}