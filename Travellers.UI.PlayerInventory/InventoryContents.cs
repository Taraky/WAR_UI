using System.Collections.Generic;
using System.Linq;
using Bossa.Travellers.Inventory;
using Improbable;
using Improbable.Collections;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.Login;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

public class InventoryContents : MonoBehaviour
{
	public delegate void ItemCraftingStarted(string itemTypeId, int amount);

	public const int FirstEquippableSlotIndex = 4;

	public const int LastEquippableSlotIndex = 7;

	public static int LastServerItemId = 0;

	public Dictionary<InventoryItemKey, InventorySlotData> AllSlotDataLookup = new Dictionary<InventoryItemKey, InventorySlotData>();

	public Dictionary<InventoryItemKey, InventorySlotData> LockboxSlotDataLookup = new Dictionary<InventoryItemKey, InventorySlotData>();

	public InventoryItemData[] HotbarSlotContents = new InventoryItemData[8];

	private System.Collections.Generic.List<string> _allowedItems;

	public InventoryObjectType InventoryTypeEnum;

	public int InventoryWidth;

	public int InventoryHeight;

	private bool _hasBelt;

	private int _beltRow;

	private readonly LazyUISystem<InventorySystem> _inventorySystem = new LazyUISystem<InventorySystem>();

	public ItemCraftingStarted onItemCraftingStarted;

	public EntityId inventoryEntityId;

	private bool _isWaitingForServer;

	private static readonly Dictionary<string, string> _collectSfxLookUp = new Dictionary<string, string>
	{
		{ "Wood", "Wood" },
		{ "Metal", "Metal" },
		{ "Atlas", "Atlas" },
		{ "daccatBerries", "PlantsVegetation" },
		{ "Fuel", "Fuel" }
	};

	public InventorySpaceChecker InventorySpaceChecker { get; private set; }

	public InventorySlotData CurrentSelectedSlotData { get; private set; }

	public InventoryItemKey CurrentSelectedItemKey { get; private set; }

	public bool IsWaitingForServer => _isWaitingForServer;

	public bool IsInventoryOpen { get; set; }

	public static InventoryContents Create(InventoryObjectType inventoryType, GameObject parent)
	{
		InventoryContents inventoryContents = parent.AddComponent<InventoryContents>();
		inventoryContents.InventoryTypeEnum = inventoryType;
		return inventoryContents;
	}

	public void Setup(int width, int height, bool hasBelt, int beltRow)
	{
		_beltRow = beltRow;
		_hasBelt = hasBelt;
		InventoryWidth = width;
		InventoryHeight = height;
		InventorySpaceChecker = new InventorySpaceChecker(InventoryWidth, InventoryHeight, hasBelt, beltRow);
	}

	public void SetServerAsWaiting(bool isWaiting)
	{
		_isWaitingForServer = isWaiting;
	}

	public void RemoveSlot(InventorySlotData item)
	{
		InventoryItemKey key = new InventoryItemKey(inventoryEntityId, item.serverItemId, item.ItemTypeId, item.IsNewlySplitItem);
		AllSlotDataLookup.Remove(key);
	}

	public bool AddToInventory(string itemTypeId, int amount = 1, int timeToBuild = 0, Map<string, string> meta = null)
	{
		InventorySlotData inventorySlotData = new InventorySlotData(LastServerItemId + 1, itemTypeId, amount, 0, 0, Time.realtimeSinceStartup, timeToBuild, meta);
		Vector2 newLocation = Vector2.zero;
		bool isRotated = inventorySlotData.rotated;
		if (InventorySpaceChecker.FindSpace(inventorySlotData, out newLocation, out isRotated))
		{
			AddItemSlotDataToLookup(inventorySlotData);
			if (timeToBuild > 0 && onItemCraftingStarted != null)
			{
				onItemCraftingStarted(itemTypeId, amount);
			}
			return true;
		}
		return false;
	}

	public void AddAllowedItems(Improbable.Collections.List<string> allowedItems)
	{
		_allowedItems = allowedItems;
		foreach (string allowedItem in _allowedItems)
		{
		}
	}

	public bool IsItemAllowed(string idToCheck)
	{
		if (_allowedItems == null || _allowedItems.Count == 0)
		{
			return true;
		}
		return _allowedItems.Contains(idToCheck);
	}

	public void SetCurrentSelectedSlotData(InventorySlotData slotData)
	{
		if (slotData != null)
		{
			CurrentSelectedItemKey = new InventoryItemKey(inventoryEntityId, slotData.serverItemId, slotData.ItemTypeId, slotData.IsNewlySplitItem);
			CurrentSelectedSlotData = slotData;
		}
		else
		{
			CurrentSelectedItemKey = new InventoryItemKey(inventoryEntityId, 0, string.Empty);
			CurrentSelectedSlotData = null;
		}
	}

	public void ClearItemList()
	{
		AllSlotDataLookup.Clear();
	}

	public void AddServerSlotListToMainLookup(Improbable.Collections.List<ScalaSlottedInventoryItem> _serverSlotDataList)
	{
		Dictionary<InventoryItemKey, InventorySlotData> prev = new Dictionary<InventoryItemKey, InventorySlotData>(AllSlotDataLookup);
		AllSlotDataLookup.Clear();
		foreach (ScalaSlottedInventoryItem _serverSlotData in _serverSlotDataList)
		{
			InventorySlotData inventorySlotData = InventorySlotData.ParseFromServerInventoryItem(_serverSlotData);
			InventoryItemKey key = new InventoryItemKey(inventoryEntityId, inventorySlotData.serverItemId, inventorySlotData.ItemTypeId);
			AllSlotDataLookup[key] = inventorySlotData;
			if (LastServerItemId < inventorySlotData.serverItemId)
			{
				LastServerItemId = inventorySlotData.serverItemId;
			}
		}
		if (this == _inventorySystem.Value.PlayerInventory && LoadingScreen.CurrentVisibility == LoadingScreenVisibility.Deactivated && !IsInventoryOpen)
		{
			Dictionary<InventoryItemData, int> inventoryChangedList = GetInventoryChangedList(prev, AllSlotDataLookup);
			PlayItemCollectSfx(inventoryChangedList);
		}
	}

	public void AddServerSlotListToLockboxLookup(Improbable.Collections.List<ScalaSlottedInventoryItem> _serverSlotDataList)
	{
		LockboxSlotDataLookup.Clear();
		foreach (ScalaSlottedInventoryItem _serverSlotData in _serverSlotDataList)
		{
			InventorySlotData inventorySlotData = InventorySlotData.ParseFromServerInventoryItem(_serverSlotData);
			InventoryItemKey key = new InventoryItemKey(inventoryEntityId, inventorySlotData.serverItemId, inventorySlotData.ItemTypeId);
			LockboxSlotDataLookup[key] = inventorySlotData;
		}
	}

	public void UpdateHotbarSlots()
	{
		System.Collections.Generic.List<InventorySlotData> list = new System.Collections.Generic.List<InventorySlotData>();
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item in AllSlotDataLookup)
		{
			if (item.Value.hotBarSlotNum > -1)
			{
				list.Add(item.Value);
			}
		}
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item2 in LockboxSlotDataLookup)
		{
			if (item2.Value.hotBarSlotNum > -1)
			{
				list.Add(item2.Value);
			}
		}
		HotbarSlotContents = new InventoryItemData[8];
		foreach (InventorySlotData item3 in list)
		{
			if (item3.hotBarSlotNum >= 8)
			{
				WALogger.Error<InventoryContents>(LogChannel.UI, "Only {0} hotbar slots. An item {1} has slot number {2}", new object[3] { 8, item3.ItemTypeId, item3.hotBarSlotNum });
			}
			else
			{
				HotbarSlotContents[item3.hotBarSlotNum] = item3.InventoryItemData;
			}
		}
	}

	public bool TryGetEmptyHotBarSlotIndex(out int slotIndex)
	{
		for (int i = 4; i < HotbarSlotContents.Length; i++)
		{
			InventoryItemData inventoryItemData = HotbarSlotContents[i];
			if (inventoryItemData == null)
			{
				slotIndex = i;
				return true;
			}
		}
		slotIndex = 0;
		return false;
	}

	public void SplitItem(InventorySlotData newItemData)
	{
		AddItemSlotDataToLookup(newItemData, isSplitItem: true);
	}

	public InventoryItemKey AddItemSlotDataToLookup(InventorySlotData inventorySlotData, bool isSplitItem = false)
	{
		InventoryItemKey inventoryItemKey = new InventoryItemKey(inventoryEntityId, inventorySlotData.serverItemId, inventorySlotData.ItemTypeId, isSplitItem);
		if (inventorySlotData.lockBoxItem)
		{
			LockboxSlotDataLookup[inventoryItemKey] = inventorySlotData;
		}
		else
		{
			AllSlotDataLookup[inventoryItemKey] = inventorySlotData;
		}
		return inventoryItemKey;
	}

	public void ReturnSelectedItem()
	{
		if (CurrentSelectedSlotData != null)
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.ReturnSelectedItemToOriginInventory, new InventoryUpdateEvent(this));
			SetCurrentSelectedSlotData(null);
		}
	}

	public void RemoveSelectedItemFromInventory()
	{
		RemoveItemSlotDataFromLookup(CurrentSelectedSlotData);
	}

	public void RemoveItemSlotDataFromLookup(InventorySlotData inventorySlotData)
	{
		InventoryItemKey key = new InventoryItemKey(inventoryEntityId, inventorySlotData.serverItemId, inventorySlotData.ItemTypeId);
		if (inventorySlotData.lockBoxItem)
		{
			LockboxSlotDataLookup.Remove(key);
		}
		else
		{
			AllSlotDataLookup.Remove(key);
		}
		InventoryGridUI.PlayDeleteSfx();
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.ResetAndRefreshInventory, new InventoryUpdateEvent(this));
	}

	public int GetItemIdAtHotSlot(int n)
	{
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item in AllSlotDataLookup)
		{
			InventorySlotData value = item.Value;
			if (value.hotBarSlotNum == n && InventoryItemManager.Instance.LookupItem(value.ItemTypeId).equippable)
			{
				return value.serverItemId;
			}
		}
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item2 in LockboxSlotDataLookup)
		{
			InventorySlotData value2 = item2.Value;
			if (value2.hotBarSlotNum == n && InventoryItemManager.Instance.LookupItem(value2.ItemTypeId).equippable)
			{
				return value2.serverItemId;
			}
		}
		return -1;
	}

	public string GetItemTypeIdAtHotSlot(int n)
	{
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item in AllSlotDataLookup)
		{
			InventorySlotData value = item.Value;
			if (value.hotBarSlotNum == n)
			{
				InventoryItemData inventoryItemData = InventoryItemManager.Instance.LookupItem(value.ItemTypeId);
				if (inventoryItemData == null)
				{
					return string.Empty;
				}
				if (inventoryItemData.equippable)
				{
					return value.ItemTypeId;
				}
			}
		}
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item2 in LockboxSlotDataLookup)
		{
			InventorySlotData value2 = item2.Value;
			if (value2.hotBarSlotNum == n && InventoryItemManager.Instance.LookupItem(value2.ItemTypeId).equippable)
			{
				return value2.ItemTypeId;
			}
		}
		return string.Empty;
	}

	public void FillItemQuantitiesPerCategoryList(System.Collections.Generic.List<InventoryItemTypeIdQuantity> listToFill, string category)
	{
		System.Collections.Generic.List<InventorySlotData> list = AllSlotDataLookup.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			InventorySlotData inventorySlotData = list[i];
			if (inventorySlotData == null || inventorySlotData.InventoryItemData == null || inventorySlotData.InventoryItemData.category != category)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < listToFill.Count; j++)
			{
				InventoryItemTypeIdQuantity value = listToFill[j];
				if (!(value.ItemTypeId != inventorySlotData.InventoryItemData.itemTypeId))
				{
					flag = true;
					value.Quantity += inventorySlotData.amount;
					listToFill[j] = value;
					break;
				}
			}
			if (!flag)
			{
				InventoryItemTypeIdQuantity inventoryItemTypeIdQuantity = default(InventoryItemTypeIdQuantity);
				inventoryItemTypeIdQuantity.ItemTypeId = inventorySlotData.InventoryItemData.itemTypeId;
				inventoryItemTypeIdQuantity.Quantity = inventorySlotData.amount;
				InventoryItemTypeIdQuantity item = inventoryItemTypeIdQuantity;
				listToFill.Add(item);
			}
		}
	}

	public void FillOrderedItemQuantitiesPerCategoryList(System.Collections.Generic.List<InventoryItemTypeIdQuantity> listToFill, string category)
	{
		FillItemQuantitiesPerCategoryList(listToFill, category);
		if (listToFill.Count != 0)
		{
			listToFill.Sort(Comparers.InventoryItemTypeIdQuantityComparer);
		}
	}

	public InventoryItemTypeIdQuantity GetHighestQuantityItemByCategory(string category)
	{
		System.Collections.Generic.List<InventoryItemTypeIdQuantity> list = new System.Collections.Generic.List<InventoryItemTypeIdQuantity>();
		FillItemQuantitiesPerCategoryList(list, category);
		if (list.Count == 0)
		{
			return default(InventoryItemTypeIdQuantity);
		}
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < list.Count; i++)
		{
			InventoryItemTypeIdQuantity inventoryItemTypeIdQuantity = list[i];
			if (inventoryItemTypeIdQuantity.Quantity >= num2)
			{
				num2 = inventoryItemTypeIdQuantity.Quantity;
				num = i;
			}
		}
		if (num == -1)
		{
			return default(InventoryItemTypeIdQuantity);
		}
		return list[num];
	}

	public void RemoveByItemId(int i)
	{
		AllSlotDataLookup = AllSlotDataLookup.Where((KeyValuePair<InventoryItemKey, InventorySlotData> x) => x.Value.serverItemId != i).ToDictionary((KeyValuePair<InventoryItemKey, InventorySlotData> x) => x.Key, (KeyValuePair<InventoryItemKey, InventorySlotData> x) => x.Value);
		LockboxSlotDataLookup = LockboxSlotDataLookup.Where((KeyValuePair<InventoryItemKey, InventorySlotData> x) => x.Value.serverItemId != i).ToDictionary((KeyValuePair<InventoryItemKey, InventorySlotData> x) => x.Key, (KeyValuePair<InventoryItemKey, InventorySlotData> x) => x.Value);
	}

	public InventorySlotData GetItemById(int i)
	{
		InventorySlotData inventorySlotData = AllSlotDataLookup.Values.ToList().Find((InventorySlotData x) => x.serverItemId == i);
		if (inventorySlotData == null)
		{
			inventorySlotData = LockboxSlotDataLookup.Values.ToList().Find((InventorySlotData x) => x.serverItemId == i);
		}
		return inventorySlotData;
	}

	public InventorySlotData GetItemByItemTypeId(string itemTypeId)
	{
		foreach (InventorySlotData value in AllSlotDataLookup.Values)
		{
			if (value.ItemTypeId != itemTypeId)
			{
				continue;
			}
			return value;
		}
		return null;
	}

	public InventorySlotData GetItemInCharacterSlot(CharacterSlotType type, int slotId)
	{
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item in AllSlotDataLookup)
		{
			if (item.Value.slotType == type && item.Value.utilitySlotNum == slotId)
			{
				return item.Value;
			}
		}
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item2 in LockboxSlotDataLookup)
		{
			if (item2.Value.slotType == type && item2.Value.utilitySlotNum == slotId)
			{
				return item2.Value;
			}
		}
		return null;
	}

	public void SetAsActiveInItemTransfer(bool isActive)
	{
		if (isActive)
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.InventoryActiveInItemTransfer, new InventoryUpdateEvent(this));
		}
		else
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.InventoryInactiveInItemTransfer, new InventoryUpdateEvent(this));
		}
	}

	public void UnequipCurrentClothingItemAndPickUp()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.UnequipSelectedClothingItemAndPickUp, new InventoryUpdateEvent(this));
	}

	public void UnequipCurrentClothingItemAndAutoPosition()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.UnequipSelectedClothingItemAndAutoPosition, new InventoryUpdateEvent(this));
	}

	public void UnequipLockboxClothingItem()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.UnequipLockboxClothingItem, new InventoryUpdateEvent(this));
	}

	public void EquipSelectedClothingItem()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.EquipSelectedClothingItem, new InventoryUpdateEvent(this));
	}

	public void EquipCurrentSelectedHotbarSlottableItemInFirstFreeSlot()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.EquipCurrentSelectedInFirstHotbarSlot, null);
	}

	public void EquipCurrentSelectedHotbarSlottableItemInSlot(int slotToEquipIn)
	{
		CurrentSelectedSlotData.hotBarSlotNum = slotToEquipIn;
		ReturnSelectedItem();
	}

	public void CheckValidityOfInventoryMove()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.FinaliseInventoryMove, new InventoryUpdateEvent(this));
	}

	public void UpdateInventorySpaceChecker()
	{
		InventorySpaceChecker = new InventorySpaceChecker(InventoryWidth, InventoryHeight, _hasBelt, _beltRow);
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item in AllSlotDataLookup)
		{
			if (item.Value.slotType == CharacterSlotType.None)
			{
				InventorySpaceChecker.AddItem(item.Value);
			}
		}
	}

	private static Dictionary<InventoryItemData, int> GetItemCountPerItemKey(Dictionary<InventoryItemKey, InventorySlotData> dictionary)
	{
		Dictionary<InventoryItemData, int> dictionary2 = new Dictionary<InventoryItemData, int>();
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item in dictionary)
		{
			if (item.Value.InventoryItemData != null)
			{
				if (dictionary2.ContainsKey(item.Value.InventoryItemData))
				{
					dictionary2[item.Value.InventoryItemData] += item.Value.amount;
				}
				else
				{
					dictionary2.Add(item.Value.InventoryItemData, item.Value.amount);
				}
			}
		}
		return dictionary2;
	}

	private static Dictionary<InventoryItemData, int> GetInventoryChangedList(Dictionary<InventoryItemKey, InventorySlotData> prev, Dictionary<InventoryItemKey, InventorySlotData> current)
	{
		Dictionary<InventoryItemData, int> itemCountPerItemKey = GetItemCountPerItemKey(prev);
		Dictionary<InventoryItemData, int> itemCountPerItemKey2 = GetItemCountPerItemKey(current);
		Dictionary<InventoryItemData, int> dictionary = new Dictionary<InventoryItemData, int>();
		foreach (KeyValuePair<InventoryItemData, int> item in itemCountPerItemKey2)
		{
			if (itemCountPerItemKey.ContainsKey(item.Key))
			{
				if (itemCountPerItemKey[item.Key] != item.Value)
				{
					dictionary.Add(item.Key, item.Value - itemCountPerItemKey[item.Key]);
				}
			}
			else
			{
				dictionary.Add(item.Key, item.Value);
			}
		}
		return dictionary;
	}

	private void PlayItemCollectSfx(Dictionary<InventoryItemData, int> changedList)
	{
		System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
		foreach (KeyValuePair<InventoryItemData, int> changed in changedList)
		{
			if (changed.Value > 0)
			{
				string value = null;
				_collectSfxLookUp.TryGetValue(changed.Key.category, out value);
				if (changed.Key.category == "Wood" && changed.Value <= 5)
				{
					value = null;
				}
				else if (changed.Key.category.Contains("MeatRaw"))
				{
					value = "Organics";
				}
				if (value != null && !list.Contains(value))
				{
					list.Add(value);
				}
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			AudioPlayer.CreateOneShotSoundAt(LocalPlayer.Transform.position).SetSwitch("MaterialType_HarvestCraft", list[i]).PostEvent("Play_CollectHarvest")
				.Play();
		}
	}

	public string GetItemHotkey(string itemIdToLookFor)
	{
		string result = string.Empty;
		for (int i = 4; i <= 7; i++)
		{
			if (GetItemTypeIdAtHotSlot(i) == itemIdToLookFor)
			{
				result = (i + 1).ToString();
				break;
			}
		}
		return result;
	}

	private void OnDestroy()
	{
		onItemCraftingStarted = null;
	}

	protected bool Equals(InventoryContents other)
	{
		return base.Equals((object)other) && inventoryEntityId.Equals(other.inventoryEntityId);
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (object.ReferenceEquals(this, obj))
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((InventoryContents)obj);
	}

	public override int GetHashCode()
	{
		return (base.GetHashCode() * 397) ^ inventoryEntityId.GetHashCode();
	}
}
