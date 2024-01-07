using System;

namespace Travellers.UI.InfoPopups;

public class SplitItemData
{
	public readonly InventorySlotData InventorySlotData;

	public readonly Action<InventorySlotData> Callback;

	public SplitItemData(InventorySlotData inventorySlotData, Action<InventorySlotData> callback)
	{
		InventorySlotData = inventorySlotData;
		Callback = callback;
	}
}
