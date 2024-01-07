using Travellers.UI.PlayerInventory;

namespace Travellers.UI.Events;

public class InventoryUpdateEvent : UIEvent
{
	public InventoryContents InventoryToUpdate;

	public InventoryUpdateEvent(InventoryContents inventoryToUpdate)
	{
		InventoryToUpdate = inventoryToUpdate;
	}
}
