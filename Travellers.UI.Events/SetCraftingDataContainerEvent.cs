using Travellers.UI.PlayerInventory;

namespace Travellers.UI.Events;

public class SetCraftingDataContainerEvent : UIEvent
{
	public readonly CraftingStationData CraftingDataContainer;

	public SetCraftingDataContainerEvent(CraftingStationData craftingDataContainer)
	{
		CraftingDataContainer = craftingDataContainer;
	}
}
