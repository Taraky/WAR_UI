using System;
using Improbable;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class InventoryChestScreen : UIScreenComponent
{
	[SerializeField]
	private InventoryGridUI _containerInventoryGrid;

	[SerializeField]
	private MoveAllItemsButton _moveAllItemsToChest;

	[SerializeField]
	private MoveAllItemsButton _moveAllItemsFromChest;

	private InventorySystem _inventorySystem;

	private Action<string> _setTabNameFunc;

	[InjectableMethod]
	public void InjectDependencies(InventorySystem inventorySystem)
	{
		_inventorySystem = inventorySystem;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInventoryEvents.CloseOpenStorageInventory, OnStorageCloseRequested);
	}

	protected override void ProtectedInit()
	{
	}

	protected override void Activate()
	{
		ShowStorageScreenForType();
	}

	public void PrepareToClose()
	{
		_containerInventoryGrid.PrepareToClose();
	}

	protected override void Deactivate()
	{
		_containerInventoryGrid.PlayCloseSound();
	}

	private void OnStorageCloseRequested(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(_containerInventoryGrid.CurrentInventory))
		{
			SetObjectActive(isActive: false);
			_containerInventoryGrid.PrepareToClose();
			_inventorySystem.SetCurrentOpenStorageInventoryEntityId(default(EntityId));
		}
	}

	public void ShowStorageScreenForType()
	{
		_containerInventoryGrid.SetInventory(_inventorySystem.CurrentStorageInventory);
		_moveAllItemsToChest.from = CharacterSheetScreen.Facade.GetPlayerGrid;
		_moveAllItemsFromChest.to = CharacterSheetScreen.Facade.GetPlayerGrid;
		_containerInventoryGrid.PlayOpenSound();
		string empty = string.Empty;
		empty = _containerInventoryGrid.CurrentInventoryType switch
		{
			InventoryObjectType.Ammo => "AMMO BOX", 
			InventoryObjectType.Chest => "CHEST", 
			InventoryObjectType.Bag => "BAG", 
			InventoryObjectType.Container => "CONTAINER", 
			InventoryObjectType.Crate => "CRATE", 
			InventoryObjectType.Ruin => "RUIN", 
			_ => "CONTAINER", 
		};
		_containerInventoryGrid.Resize();
		_containerInventoryGrid.RefreshInventory();
		if (_setTabNameFunc != null)
		{
			_setTabNameFunc(empty);
		}
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetNameUpdateFunc(Action<string> setTabName)
	{
		_setTabNameFunc = setTabName;
	}
}
