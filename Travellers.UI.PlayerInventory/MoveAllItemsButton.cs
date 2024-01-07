using Improbable;
using Travellers.UI.Framework;
using Travellers.UI.Sound;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Travellers.UI.PlayerInventory;

public class MoveAllItemsButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public InventoryGridUI from;

	public InventoryGridUI to;

	private readonly LazyUISystem<InventorySystem> _inventorySystem = new LazyUISystem<InventorySystem>();

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!from.CurrentInventory.IsWaitingForServer && !to.CurrentInventory.IsWaitingForServer)
		{
			EntityId inventoryEntityId = from.CurrentInventory.inventoryEntityId;
			EntityId inventoryEntityId2 = to.CurrentInventory.inventoryEntityId;
			InventoryGridUI.RequestInventoryMoveAll(from, to, inventoryEntityId, inventoryEntityId2);
			bool flag = from.CurrentInventory.gameObject.name.Contains("crate") || from.CurrentInventory.gameObject.name.Contains("Crate");
			bool flag2 = to.CurrentInventory.gameObject.name.Contains("crate") || to.CurrentInventory.gameObject.name.Contains("Crate");
			if (flag && !flag2)
			{
				SoundScreen.PlayASound("Play_StorageCrate_RemoveItem");
			}
			else if (!flag && flag2)
			{
				SoundScreen.PlayASound("Play_StorageCrate_AddItem");
			}
			else
			{
				SoundScreen.PlayASound((!(to.CurrentInventory == _inventorySystem.Value.PlayerInventory)) ? "Play_Inventory_DropItem" : "Play_Inventory_ItemMove_Drop_generic");
			}
		}
	}
}
