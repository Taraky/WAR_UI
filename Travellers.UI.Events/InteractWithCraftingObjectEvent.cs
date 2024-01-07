using Travellers.UI.PlayerInventory;

namespace Travellers.UI.Events;

public class InteractWithCraftingObjectEvent : UIEvent
{
	public CraftingCategory CraftingCategory;

	public InteractWithCraftingObjectEvent(CraftingCategory craftingCategory)
	{
		CraftingCategory = craftingCategory;
	}
}
