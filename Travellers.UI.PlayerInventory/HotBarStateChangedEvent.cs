namespace Travellers.UI.PlayerInventory;

public class HotBarStateChangedEvent
{
	public bool Show;

	public HotBarStateChangedEvent(bool show)
	{
		Show = show;
	}
}
