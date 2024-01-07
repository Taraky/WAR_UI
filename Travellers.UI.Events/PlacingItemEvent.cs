namespace Travellers.UI.Events;

public class PlacingItemEvent : UIEvent
{
	private readonly int _placedItemId;

	public int PlacedItemId => _placedItemId;

	public PlacingItemEvent(int itemId)
	{
		_placedItemId = itemId;
	}
}
