namespace Travellers.UI.PlayerInventory;

public static class CraftingMaterialSlotsExtension
{
	public static bool AllSlotsHaveRequiredMaterials(this CraftingMaterialSlot[] craftingMaterialSlots)
	{
		foreach (CraftingMaterialSlot craftingMaterialSlot in craftingMaterialSlots)
		{
			if (craftingMaterialSlot.IsUsedByCurrentSchematic && !craftingMaterialSlot.IsFull)
			{
				return false;
			}
		}
		return true;
	}

	public static bool AllSlotsAreEmpty(this CraftingMaterialSlot[] craftingMaterialSlots)
	{
		foreach (CraftingMaterialSlot craftingMaterialSlot in craftingMaterialSlots)
		{
			if (craftingMaterialSlot.IsUsedByCurrentSchematic && !craftingMaterialSlot.IsEmpty)
			{
				return false;
			}
		}
		return true;
	}
}
