namespace Travellers.UI.Events;

public class StartedCraftingShipPartEvent : UIEvent
{
	public readonly string ItemId;

	public StartedCraftingShipPartEvent(string itemId)
	{
		ItemId = itemId;
	}
}
