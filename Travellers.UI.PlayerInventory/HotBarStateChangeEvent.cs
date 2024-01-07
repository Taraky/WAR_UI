namespace Travellers.UI.PlayerInventory;

public class HotBarStateChangeEvent
{
	public HotbarSlotType SlotType { get; private set; }

	public bool Enabled { get; private set; }

	public HotBarStateChangeEvent(HotbarSlotType slotType, bool enabled)
	{
		SlotType = slotType;
		Enabled = enabled;
	}
}
