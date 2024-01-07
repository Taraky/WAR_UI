namespace Travellers.UI.Events;

public class ItemConsumedEvent : UIEvent
{
	public string ConsumedItemName;

	public ItemConsumedEvent(string itemName)
	{
		ConsumedItemName = itemName;
	}
}
