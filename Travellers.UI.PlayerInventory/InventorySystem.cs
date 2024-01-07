using System;
using System.Collections.Generic;
using System.Diagnostics;
using Improbable;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.Models;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

[InjectedSystem]
public class InventorySystem : UISystem
{
	private bool _updateHotbarSlots;

	private bool _updateKnowledgeSchematics;

	public int CurrentHealth;

	public int MaxHealth;

	private readonly Dictionary<EntityId, InventoryContents> _inventoryDataLookup = new Dictionary<EntityId, InventoryContents>();

	private readonly Dictionary<EntityId, CraftingStationData> _craftingDataLookup = new Dictionary<EntityId, CraftingStationData>();

	private readonly Dictionary<EntityId, HashSet<ICraftingDataListener>> _craftingDataInterestedObjectsLookup = new Dictionary<EntityId, HashSet<ICraftingDataListener>>();

	private readonly HashSet<EntityId> _pendingCraftingDataDeregister = new HashSet<EntityId>();

	private readonly List<Action<InventoryContents>> _onPlayerInventorySet = new List<Action<InventoryContents>>();

	private readonly HashSet<EntityId> _retrieveCraftingDataErrorRegister = new HashSet<EntityId>();

	private readonly ISchematicSystem _schematicSystem;

	public bool UpdateHotbarSlots
	{
		get
		{
			if (_updateHotbarSlots)
			{
				_updateHotbarSlots = false;
				return true;
			}
			return _updateHotbarSlots;
		}
	}

	public bool UpdateKnowledgeSchematics
	{
		get
		{
			if (_updateKnowledgeSchematics)
			{
				_updateKnowledgeSchematics = false;
				return true;
			}
			return _updateKnowledgeSchematics;
		}
	}

	public bool IsStorageObjectActive => CurrentOpenStorageInventoryEntityId != default(EntityId);

	public bool IsPlayerInventoryWaitingForServer => PlayerInventory.IsWaitingForServer;

	public bool IsStorageInventoryWaitingForServer
	{
		get
		{
			if (!IsStorageObjectActive)
			{
				return false;
			}
			return CurrentStorageInventory.IsWaitingForServer;
		}
	}

	public bool AreAnyInventoriesWaitingForServer => IsStorageInventoryWaitingForServer || IsPlayerInventoryWaitingForServer;

	public bool IsCurrentCraftingWaitingForServer
	{
		get
		{
			if (CurrentCraftingData == null)
			{
				return false;
			}
			return CurrentCraftingData.IsWaitingForServer;
		}
	}

	public InventoryContents CurrentStorageInventory => RetrieveInventoryDataTemplate(CurrentOpenStorageInventoryEntityId);

	public InventoryContents PlayerInventory { get; private set; }

	public bool HasCurrentSelectedSlot
	{
		get
		{
			if (OriginInventory != null && OriginInventory.CurrentSelectedSlotData != null)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsPlayerDraggingItem { get; set; }

	public InventoryContents LandingInventory => DestinationInventory;

	public bool IsCrossInventory => !DestinationInventory.Equals(OriginInventory);

	public bool AllReferenceAndPlayerDataLoaded { get; private set; }

	public InventoryContents DestinationInventory { get; private set; }

	public InventoryContents OriginInventory { get; private set; }

	public EntityId PlayerInventoryEntityId { get; private set; }

	public EntityId CurrentOpenStorageInventoryEntityId { get; private set; }

	public CraftingStationData CurrentCraftingData { get; private set; }

	public CraftingStationData PlayerCraftingStationData { get; private set; }

	public InventorySystem(ISchematicSystem schematicSystem)
	{
		_schematicSystem = schematicSystem;
	}

	public override void Init()
	{
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInventoryEvents.SchematicsUpdated, OnSchematicsUpdated);
		_eventList.AddEvent(WAUIInventoryEvents.RegisterInventory, OnAddInventoryData);
		_eventList.AddEvent(WAUIInventoryEvents.DeregisterInventory, OnRemoveInventoryData);
		_eventList.AddEvent(WAUIInventoryEvents.ChangeInWorldStorageEntityID, OnChangeInWorldStorageEntity);
		_eventList.AddEvent(WAUIInventoryEvents.PlayerInventoryUpdated, OnPlayerInventoryUpdated);
		_eventList.AddEvent(WAUIInventoryEvents.LockboxInventoryUpdated, OnLockboxInventoryUpdated);
		_eventList.AddEvent(WAUIInventoryEvents.InWorldInventoryUpdated, OnInWorldInventoryUpdated);
		_eventList.AddEvent(WAUIPlayerProfileEvents.UpdateHealth, OnUpdateHealth);
	}

	public override void ControlledUpdate()
	{
		foreach (KeyValuePair<EntityId, CraftingStationData> item in _craftingDataLookup)
		{
			if (item.Value != null)
			{
				item.Value.ControlledUpdate();
			}
		}
	}

	private void OnSchematicsUpdated(object[] obj)
	{
		_updateKnowledgeSchematics = true;
		if (CurrentCraftingData != null)
		{
			_schematicSystem.CheckIfSchematicHierarchyRebuildNeeded(CurrentCraftingData);
		}
	}

	private void OnAddInventoryData(object[] obj)
	{
		InventoryContents inventoryDataTemplate = (InventoryContents)obj[0];
		RegisterInventoryData(inventoryDataTemplate);
	}

	private void OnRemoveInventoryData(object[] obj)
	{
		InventoryContents inventoryDataTemplate = (InventoryContents)obj[0];
		DeregisterInventoryData(inventoryDataTemplate);
	}

	private void OnUpdateHealth(object[] obj)
	{
		UpdateHealthBarUIEvent updateHealthBarUIEvent = (UpdateHealthBarUIEvent)obj[0];
		MaxHealth = updateHealthBarUIEvent.MaxHealth;
		CurrentHealth = updateHealthBarUIEvent.CurrentHealth;
	}

	private void OnChangeInWorldStorageEntity(object[] obj)
	{
		EntityId currentOpenStorageInventoryEntityId = (EntityId)obj[0];
		SetCurrentOpenStorageInventoryEntityId(currentOpenStorageInventoryEntityId);
	}

	private void OnPlayerInventoryUpdated(object[] obj)
	{
		SetHotbarAsShouldUpdate();
		if (IsStorageObjectActive)
		{
			AreCurrentStorageInventoryAndPlayerInventoriesReady();
			return;
		}
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.UpdateInventoryUIGrid, new InventoryUpdateEvent(PlayerInventory));
		UpdateClothingSlots();
	}

	private void OnLockboxInventoryUpdated(object[] obj)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.UpdateLockbox, null);
	}

	private void OnInWorldInventoryUpdated(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (CurrentOpenStorageInventoryEntityId == inventoryUpdateEvent.InventoryToUpdate.inventoryEntityId)
		{
			AreCurrentStorageInventoryAndPlayerInventoriesReady();
			return;
		}
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.UpdateInventoryUIGrid, new InventoryUpdateEvent(inventoryUpdateEvent.InventoryToUpdate));
	}

	private void AreCurrentStorageInventoryAndPlayerInventoriesReady()
	{
		if (!AreAnyInventoriesWaitingForServer)
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.UpdateInventoryUIGrid, new InventoryUpdateEvent(PlayerInventory));
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.UpdateInventoryUIGrid, new InventoryUpdateEvent(CurrentStorageInventory));
		}
	}

	public void ClearAllReferences()
	{
		_craftingDataLookup.Clear();
		_inventoryDataLookup.Clear();
		_craftingDataInterestedObjectsLookup.Clear();
		_pendingCraftingDataDeregister.Clear();
		PlayerCraftingStationData = null;
		PlayerInventoryEntityId = default(EntityId);
		CurrentCraftingData = null;
		CurrentOpenStorageInventoryEntityId = default(EntityId);
	}

	public void RegisterCraftingData(CraftingStationData craftingDataTemplate)
	{
		if (_craftingDataLookup.ContainsKey(craftingDataTemplate.CraftingEntityId))
		{
			WALogger.Warn<InventorySystem>(LogChannel.UI, "Attempting to add duplicate crafting data template for Entity: {0}", new object[1] { craftingDataTemplate.CraftingEntityId });
			return;
		}
		_craftingDataLookup[craftingDataTemplate.CraftingEntityId] = craftingDataTemplate;
		if (!_craftingDataInterestedObjectsLookup.ContainsKey(craftingDataTemplate.CraftingEntityId))
		{
			return;
		}
		foreach (ICraftingDataListener item in _craftingDataInterestedObjectsLookup[craftingDataTemplate.CraftingEntityId])
		{
			item.OnCraftingDataReady();
		}
	}

	public void DeregisterCraftingData(CraftingStationData craftingDataTemplate)
	{
		if (_craftingDataLookup.ContainsKey(craftingDataTemplate.CraftingEntityId))
		{
			if (_craftingDataInterestedObjectsLookup.ContainsKey(craftingDataTemplate.CraftingEntityId) && _craftingDataInterestedObjectsLookup[craftingDataTemplate.CraftingEntityId].Count > 0)
			{
				_pendingCraftingDataDeregister.Add(craftingDataTemplate.CraftingEntityId);
			}
			else
			{
				_craftingDataLookup.Remove(craftingDataTemplate.CraftingEntityId);
			}
		}
		else
		{
			WALogger.Warn<InventorySystem>(LogChannel.UI, "Failed to unegister crafting data template for Entity: {0} of type {1}", new object[2] { craftingDataTemplate.CraftingEntityId, craftingDataTemplate.UITabType });
		}
	}

	public void RegisterInterestInCraftingData(ICraftingDataListener interestedObject, EntityId entityRelatedToCraftingData)
	{
		if (!_craftingDataInterestedObjectsLookup.ContainsKey(entityRelatedToCraftingData))
		{
			_craftingDataInterestedObjectsLookup.Add(entityRelatedToCraftingData, new HashSet<ICraftingDataListener>());
		}
		_craftingDataInterestedObjectsLookup[entityRelatedToCraftingData].Add(interestedObject);
		if (_craftingDataLookup.ContainsKey(entityRelatedToCraftingData))
		{
			interestedObject.OnCraftingDataReady();
		}
	}

	public void DeregisterInterestInCraftingData(ICraftingDataListener interestedObject, EntityId entityRelatedToCraftingData)
	{
		if (!_craftingDataInterestedObjectsLookup.ContainsKey(entityRelatedToCraftingData) || !_craftingDataInterestedObjectsLookup[entityRelatedToCraftingData].Contains(interestedObject))
		{
			return;
		}
		_craftingDataInterestedObjectsLookup[entityRelatedToCraftingData].Remove(interestedObject);
		if (_craftingDataInterestedObjectsLookup[entityRelatedToCraftingData].Count == 0)
		{
			_craftingDataInterestedObjectsLookup.Remove(entityRelatedToCraftingData);
			if (_pendingCraftingDataDeregister.Contains(entityRelatedToCraftingData))
			{
				_pendingCraftingDataDeregister.Remove(entityRelatedToCraftingData);
				DeregisterCraftingData(_craftingDataLookup[entityRelatedToCraftingData]);
			}
		}
	}

	public void RegisterInventoryData(InventoryContents inventoryDataTemplate)
	{
		if (_inventoryDataLookup.ContainsKey(inventoryDataTemplate.inventoryEntityId))
		{
			WALogger.Warn<InventorySystem>(LogChannel.UI, "Attempting to add duplicate inventory data template for Entity: {0}", new object[1] { inventoryDataTemplate.inventoryEntityId });
			return;
		}
		_inventoryDataLookup[inventoryDataTemplate.inventoryEntityId] = inventoryDataTemplate;
		if (_retrieveCraftingDataErrorRegister.Contains(inventoryDataTemplate.inventoryEntityId))
		{
			_retrieveCraftingDataErrorRegister.Remove(inventoryDataTemplate.inventoryEntityId);
			WALogger.Error<InventorySystem>(LogChannel.UI, "Failed to retrieve crafting data template for Entity: {0}\n follow up: data registered after retrieve", new object[1] { inventoryDataTemplate.inventoryEntityId });
		}
	}

	public void DeregisterInventoryData(InventoryContents inventoryDataTemplate)
	{
		if (_inventoryDataLookup.ContainsKey(inventoryDataTemplate.inventoryEntityId))
		{
			_inventoryDataLookup.Remove(inventoryDataTemplate.inventoryEntityId);
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.CloseOpenStorageInventory, new InventoryUpdateEvent(inventoryDataTemplate));
		}
		else
		{
			WALogger.Warn<InventorySystem>(LogChannel.UI, "Failed to unegister inventory data template for Entity: {0} of type {1}", new object[2] { inventoryDataTemplate.inventoryEntityId, inventoryDataTemplate.InventoryTypeEnum });
		}
	}

	public CraftingStationData RetrieveCraftingDataTemplate(EntityId entityId, int debugCallId)
	{
		if (!_craftingDataLookup.TryGetValue(entityId, out var value))
		{
			WALogger.Error<InventorySystem>(LogChannel.UI, "Failed to retrieve crafting data template for Entity: {0} and debugCallId {1}\n" + new StackTrace(), new object[2] { entityId, debugCallId });
			_retrieveCraftingDataErrorRegister.Add(entityId);
		}
		return value;
	}

	public InventoryContents RetrieveInventoryDataTemplate(EntityId entityId)
	{
		if (entityId.Id == 0)
		{
			return null;
		}
		if (!_inventoryDataLookup.TryGetValue(entityId, out var value))
		{
			WALogger.Error<InventorySystem>(LogChannel.UI, "Failed to retrieve inventory data template for Entity: {0}", new object[1] { entityId });
		}
		return value;
	}

	public void SetPlayerInventoryContents(InventoryContents inventoryContents)
	{
		if (inventoryContents == null)
		{
			return;
		}
		PlayerInventory = inventoryContents;
		foreach (Action<InventoryContents> item in _onPlayerInventorySet)
		{
			item(PlayerInventory);
		}
		_onPlayerInventorySet.Clear();
	}

	public void QueuePlayerInventoryAction(Action<InventoryContents> action)
	{
		if (PlayerInventory == null)
		{
			_onPlayerInventorySet.Add(action);
		}
		else
		{
			action(PlayerInventory);
		}
	}

	public void SetPlayerCraftingStationData(CraftingStationData craftingStationData)
	{
		PlayerCraftingStationData = craftingStationData;
	}

	public void SetCurrentCraftingData(CraftingStationData craftingStationData)
	{
		CurrentCraftingData = craftingStationData;
	}

	public void SetCurrentOpenStorageInventoryEntityId(EntityId value)
	{
		CurrentOpenStorageInventoryEntityId = value;
	}

	public void SendNewSchematicToServer()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUICraftingEvents.SetSchematicOnServer, CurrentCraftingData);
	}

	public void SetOriginInventory(InventoryContents inventoryToUpdate)
	{
		OriginInventory = inventoryToUpdate;
		DestinationInventory = inventoryToUpdate;
	}

	public void SetDestinationInventory(InventoryContents newDestinationInventory)
	{
		if (!DestinationInventory.Equals(newDestinationInventory))
		{
			if (OriginInventory.Equals(newDestinationInventory))
			{
				DestinationInventory.SetAsActiveInItemTransfer(isActive: false);
				OriginInventory.SetAsActiveInItemTransfer(isActive: true);
			}
			else
			{
				newDestinationInventory.SetAsActiveInItemTransfer(isActive: true);
				OriginInventory.SetAsActiveInItemTransfer(isActive: false);
			}
			DestinationInventory = newDestinationInventory;
		}
	}

	public void SetHotbarAsShouldUpdate()
	{
		_updateHotbarSlots = true;
	}

	public void SetAllInventoryServersAsWaiting()
	{
		if (PlayerInventory != null)
		{
			PlayerInventory.SetServerAsWaiting(isWaiting: true);
		}
		if (CurrentStorageInventory != null)
		{
			CurrentStorageInventory.SetServerAsWaiting(isWaiting: true);
		}
	}

	public void ClosingInventory(InventoryContents currentInventory)
	{
		if (currentInventory != null && HasCurrentSelectedSlot)
		{
			OriginInventory.ReturnSelectedItem();
			OriginInventory.SetCurrentSelectedSlotData(null);
		}
	}

	public bool TryGetEmptyHotBarSlotIndex(out int slotIndex)
	{
		if (PlayerInventory == null)
		{
			slotIndex = 0;
			return false;
		}
		return PlayerInventory.TryGetEmptyHotBarSlotIndex(out slotIndex);
	}

	public void UpdateClothingSlots()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.UpdateClothingSlots, null);
	}

	protected override void Dispose()
	{
	}
}
