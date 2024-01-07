namespace Travellers.UI.Events;

public class ShipDockedUpdatedEvent : UIEvent
{
	public readonly bool IsDocked;

	public ShipDockedUpdatedEvent(bool isDocked)
	{
		IsDocked = isDocked;
	}
}
