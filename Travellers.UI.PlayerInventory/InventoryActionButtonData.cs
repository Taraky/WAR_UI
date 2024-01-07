using System;

namespace Travellers.UI.PlayerInventory;

public struct InventoryActionButtonData
{
	public InventoryTooltipPopup.ItemAction ActionType;

	public readonly string Title;

	public readonly Action Action;

	public InventoryActionButtonData(InventoryTooltipPopup.ItemAction actionType, string title, Action action)
	{
		ActionType = actionType;
		Title = title;
		Action = action;
	}
}
