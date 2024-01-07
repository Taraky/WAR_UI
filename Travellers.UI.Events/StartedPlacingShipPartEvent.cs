namespace Travellers.UI.Events;

public class StartedPlacingShipPartEvent : UIEvent
{
	public readonly string ItemId;

	public StartedPlacingShipPartEvent(string itemId)
	{
		ItemId = itemId;
	}
}
