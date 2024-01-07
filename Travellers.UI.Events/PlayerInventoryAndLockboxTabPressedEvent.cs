namespace Travellers.UI.Events;

public class PlayerInventoryAndLockboxTabPressedEvent : UIEvent
{
	public LockboxTabType TabType;

	public PlayerInventoryAndLockboxTabPressedEvent(LockboxTabType tabType)
	{
		TabType = tabType;
	}
}
