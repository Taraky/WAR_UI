using System.Collections.Generic;
using System.Linq;
using Bossa.Travellers.World;
using GameDBLocalization;
using Improbable;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.InfoPopups;
using Travellers.UI.Sound;
using Travellers.UI.Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class InventoryGridUI : UIScreenComponent, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	[SerializeField]
	private LayoutElement _containerLayout;

	public static int cellSize = 30;

	public InventoryObjectType CurrentInventoryType;

	[SerializeField]
	private InventorySlotRectController DropTarget;

	[SerializeField]
	private InventorySlotRectController OriginalLocationPlaceholder;

	private bool _originalLocationSlotRotation;

	private readonly Dictionary<InventoryItemKey, InventoryItemSlot> _activeInventoryItemSlotLookup = new Dictionary<InventoryItemKey, InventoryItemSlot>();

	private bool hasBeenInitialised;

	private bool _mouseOverUIOutOfBounds;

	private TypeOfMouseDrag _currentTypeOfMouseDrag = TypeOfMouseDrag.None;

	public InventoryContents CurrentInventory { get; private set; }

	private InventorySystem _inventorySystem { get; set; }

	[InjectableMethod]
	public void InjectDependencies(InventorySystem inventorySystem)
	{
		_inventorySystem = inventorySystem;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInGameEvents.UIFloorReceiverEnter, OnOutsideOfUIEntered);
		_eventList.AddEvent(WAUIInGameEvents.UIFloorReceiverExit, OnOutsideOfUIExited);
		_eventList.AddEvent(WAUIInGameEvents.UIFloorReceiverClicked, OnOutsideOfUIClicked);
		_eventList.AddEvent(WAUIInventoryEvents.DraggedInventoryItemDropped, OnInventoryItemDragFinished);
		_eventList.AddEvent(WAUIInventoryEvents.InventoryItemPickedUp, OnInventoryItemPickedUp);
		_eventList.AddEvent(WAUIInventoryEvents.EquipSelectedClothingItem, OnSelectedClothingItemEquipped);
		_eventList.AddEvent(WAUIInventoryEvents.UnequipSelectedClothingItemAndPickUp, OnSelectedClothingItemUnequippedAndPickedUp);
		_eventList.AddEvent(WAUIInventoryEvents.UnequipSelectedClothingItemAndAutoPosition, OnSelectedClothingItemUnequippedAndAutoPositioned);
		_eventList.AddEvent(WAUIInventoryEvents.UnequipLockboxClothingItem, OnSelectedLockboxItemUnequipped);
		_eventList.AddEvent(WAUIInventoryEvents.ResetAndRefreshInventory, OnResetAndRefreshInventory);
		_eventList.AddEvent(WAUIInventoryEvents.ReturnSelectedItemToOriginInventory, OnReturnSelectedItemToOriginInventory);
		_eventList.AddEvent(WAUIInventoryEvents.FinaliseInventoryMove, OnFinaliseInventoryMove);
		_eventList.AddEvent(WAUIInventoryEvents.InventoryInactiveInItemTransfer, OnInventorySetInactiveInItemTransfer);
		_eventList.AddEvent(WAUIInventoryEvents.InventoryActiveInItemTransfer, OnInventorySetActiveInItemTransfer);
		_eventList.AddEvent(WAUIInventoryEvents.UpdateInventoryUIGrid, OnInventoryUpdated);
		_eventList.AddEvent(WAUIInventoryEvents.ChangeMouseDragType, OnChangeMouseDragType);
		_eventList.AddEvent(WAUIInventoryEvents.ItemSlotTooltipOpened, OnItemSlotTooltipOpened);
	}

	protected override void ProtectedInit()
	{
		_muteListenersWhenInactive = false;
	}

	protected override void ProtectedDispose()
	{
	}

	protected override void Activate()
	{
	}

	public void PrepareToClose()
	{
		_inventorySystem.ClosingInventory(CurrentInventory);
	}

	protected override void Deactivate()
	{
	}

	private void Update()
	{
		if (DropTarget.IsObjectActiveInScene)
		{
			DropTarget.FollowMouseUsingInventoryGrid();
		}
		if (_currentTypeOfMouseDrag != TypeOfMouseDrag.None && Input.GetMouseButtonDown(1))
		{
			Rotate();
		}
	}

	public void CheckifFirstTimeOpen()
	{
		if (!hasBeenInitialised && InventoryItemManager.Instance.HasDeserialised)
		{
			hasBeenInitialised = true;
			Resize();
			RefreshInventory();
		}
	}

	public void SetInventory(InventoryContents inventory)
	{
		if (inventory != null)
		{
			CurrentInventory = inventory;
			CurrentInventoryType = inventory.InventoryTypeEnum;
		}
	}

	private void Rotate()
	{
		if (!(CurrentInventory == null) && CurrentInventory.Equals(_inventorySystem.LandingInventory))
		{
			DropTarget.TryRotateItem();
		}
	}

	private void OnInventoryItemPickedUp(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(CurrentInventory))
		{
			SetDropzone(isActive: true);
			SetPlaceholder(isActive: true);
		}
	}

	private void OnSelectedClothingItemEquipped(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(CurrentInventory))
		{
			CurrentInventory.CurrentSelectedSlotData.slotType = CurrentInventory.CurrentSelectedSlotData.InventoryItemData.wearable;
			CurrentInventory.CurrentSelectedSlotData.utilitySlotNum = -1;
			InventorySlotData currentSelectedSlotData = CurrentInventory.CurrentSelectedSlotData;
			if (_activeInventoryItemSlotLookup.TryGetValue(CurrentInventory.CurrentSelectedItemKey, out var value))
			{
				_activeInventoryItemSlotLookup.Remove(CurrentInventory.CurrentSelectedItemKey);
				Object.Destroy(value.gameObject);
				SetDropzone(isActive: false);
				SetPlaceholder(isActive: false);
			}
			_inventorySystem.PlayerInventory.SetCurrentSelectedSlotData(null);
			_inventorySystem.PlayerInventory.SetServerAsWaiting(isWaiting: true);
			if (currentSelectedSlotData.InventoryItemData.wearable == CharacterSlotType.Tool)
			{
				LocalPlayer.Instance.inventoryModificationBehaviour.EquipTool(currentSelectedSlotData.serverItemId);
			}
			else
			{
				LocalPlayer.Instance.inventoryModificationBehaviour.EquipWearable(currentSelectedSlotData.serverItemId, currentSelectedSlotData.utilitySlotNum, currentSelectedSlotData.lockBoxItem);
			}
		}
	}

	private void OnSelectedClothingItemUnequippedAndPickedUp(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(CurrentInventory))
		{
			if (CurrentInventory.CurrentSelectedSlotData.lockBoxItem)
			{
				WALogger.Warn<InventoryGridUI>(LogChannel.UI, "Attempting to unequip a lockbox item using non-lockbox path {0}", new object[1] { CurrentInventory.CurrentSelectedSlotData.ItemTypeId });
			}
			if (!GetFreeSpaceForNewItem(CurrentInventory.CurrentSelectedSlotData))
			{
				CurrentInventory.SetCurrentSelectedSlotData(null);
				return;
			}
			CurrentInventory.CurrentSelectedSlotData.slotType = CharacterSlotType.None;
			SetDropzone(isActive: true);
			SetPlaceholder(isActive: true);
			InventoryItemKey key = new InventoryItemKey(CurrentInventory.inventoryEntityId, CurrentInventory.CurrentSelectedSlotData.serverItemId, CurrentInventory.CurrentSelectedSlotData.ItemTypeId);
			InventoryItemSlot inventoryItemSlot = CreateItemSlotType(CurrentInventory.CurrentSelectedSlotData);
			inventoryItemSlot.SetData(_inventorySystem.CurrentCraftingData, CurrentInventory, CurrentInventory.CurrentSelectedSlotData, (RectTransform)base.transform);
			_activeInventoryItemSlotLookup[key] = inventoryItemSlot;
			inventoryItemSlot.SetForClickedItemStayingOnMouseAfterServerRefresh();
			LocalPlayer.Instance.inventoryModificationBehaviour.UnequipWearable(CurrentInventory.CurrentSelectedSlotData.InventoryItemData.wearable.ToString(), CurrentInventory.CurrentSelectedSlotData.utilitySlotNum, isLockboxItem: false, CurrentInventory.CurrentSelectedSlotData.serverItemId);
		}
	}

	private void OnSelectedClothingItemUnequippedAndAutoPositioned(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(CurrentInventory))
		{
			if (CurrentInventory.CurrentSelectedSlotData.lockBoxItem)
			{
				WALogger.Warn<InventoryGridUI>(LogChannel.UI, "Attempting to unequip a lockbox item using non-lockbox path {0}", new object[1] { CurrentInventory.CurrentSelectedSlotData.ItemTypeId });
			}
			InventorySlotData currentSelectedSlotData = CurrentInventory.CurrentSelectedSlotData;
			if (!GetFreeSpaceForNewItem(currentSelectedSlotData))
			{
				CurrentInventory.SetCurrentSelectedSlotData(null);
				return;
			}
			CurrentInventory.CurrentSelectedSlotData.slotType = CharacterSlotType.None;
			CurrentInventory.SetCurrentSelectedSlotData(null);
			RefreshClientSideInventory();
			_inventorySystem.PlayerInventory.SetServerAsWaiting(isWaiting: true);
			LocalPlayer.Instance.inventoryModificationBehaviour.UnequipWearable(currentSelectedSlotData.InventoryItemData.wearable.ToString(), currentSelectedSlotData.utilitySlotNum, isLockboxItem: false, currentSelectedSlotData.serverItemId);
		}
	}

	private void OnSelectedLockboxItemUnequipped(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(CurrentInventory))
		{
			InventorySlotData currentSelectedSlotData = CurrentInventory.CurrentSelectedSlotData;
			CurrentInventory.CurrentSelectedSlotData.slotType = CharacterSlotType.None;
			CurrentInventory.SetCurrentSelectedSlotData(null);
			RefreshClientSideInventory();
			_inventorySystem.PlayerInventory.SetServerAsWaiting(isWaiting: true);
			LocalPlayer.Instance.inventoryModificationBehaviour.UnequipWearable(currentSelectedSlotData.InventoryItemData.wearable.ToString(), currentSelectedSlotData.utilitySlotNum, isLockboxItem: true, currentSelectedSlotData.serverItemId);
		}
	}

	private bool GetFreeSpaceForNewItem(InventorySlotData inventorySlotData)
	{
		Vector2 newLocation = Vector2.zero;
		bool isRotated = inventorySlotData.rotated;
		if (!inventorySlotData.lockBoxItem && !CurrentInventory.InventorySpaceChecker.FindSpace(inventorySlotData, out newLocation, out isRotated))
		{
			return false;
		}
		inventorySlotData.xPosition = (int)newLocation.x;
		inventorySlotData.yPosition = (int)newLocation.y;
		inventorySlotData.rotated = isRotated;
		return true;
	}

	private void OnResetAndRefreshInventory(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(CurrentInventory))
		{
			ResetAndRefreshInventory();
		}
	}

	private void OnReturnSelectedItemToOriginInventory(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(CurrentInventory))
		{
			ReturnCurrentSelectedItemToInventory();
		}
	}

	private void OnFinaliseInventoryMove(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(CurrentInventory))
		{
			FinaliseInventoryItemMove();
		}
	}

	private void OnInventorySetInactiveInItemTransfer(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(CurrentInventory))
		{
			SetDropzone(isActive: false);
		}
	}

	private void OnInventorySetActiveInItemTransfer(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(CurrentInventory))
		{
			SetDropzone(isActive: true);
		}
	}

	private void OnInventoryUpdated(object[] obj)
	{
		InventoryUpdateEvent inventoryUpdateEvent = (InventoryUpdateEvent)obj[0];
		if (inventoryUpdateEvent.InventoryToUpdate.Equals(CurrentInventory))
		{
			RefreshInventory();
		}
	}

	private void OnChangeMouseDragType(object[] obj)
	{
		UpdateMouseDragTypeEvent updateMouseDragTypeEvent = (UpdateMouseDragTypeEvent)obj[0];
		_currentTypeOfMouseDrag = updateMouseDragTypeEvent.TypeOfMouseDrag;
	}

	private void OnOutsideOfUIExited(object[] obj)
	{
		_mouseOverUIOutOfBounds = false;
	}

	private void OnOutsideOfUIEntered(object[] obj)
	{
		_mouseOverUIOutOfBounds = true;
	}

	private void OnOutsideOfUIClicked(object[] obj)
	{
		if (CurrentInventory != null && _inventorySystem.HasCurrentSelectedSlot && CurrentInventory.Equals(_inventorySystem.LandingInventory))
		{
			HideDraggingRelatedElements();
		}
	}

	private void OnItemSlotTooltipOpened(object[] obj)
	{
		if (!(CurrentInventory == null) && _inventorySystem.HasCurrentSelectedSlot)
		{
			InventoryContents inventoryContents = ((ItemSlotTooltipOpenedEvent)obj[0]).InventoryContents;
			if (inventoryContents == CurrentInventory)
			{
				HideDraggingRelatedElements();
			}
			else
			{
				HideDragGizmos();
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!(CurrentInventory == null) && _inventorySystem.HasCurrentSelectedSlot && CanThisInventoryStoreItem())
		{
			if (CurrentInventory.Equals(_inventorySystem.OriginInventory))
			{
				_inventorySystem.SetDestinationInventory(_inventorySystem.OriginInventory);
			}
			else if (!CurrentInventory.Equals(_inventorySystem.DestinationInventory))
			{
				_inventorySystem.SetDestinationInventory(CurrentInventory);
			}
		}
	}

	private bool CanThisInventoryStoreItem()
	{
		if (CurrentInventoryType == InventoryObjectType.Ammo && CurrentInventory.IsItemAllowed(_inventorySystem.OriginInventory.CurrentSelectedSlotData.ItemTypeId))
		{
			return true;
		}
		return true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (CurrentInventory.Equals(_inventorySystem.LandingInventory) && _inventorySystem.HasCurrentSelectedSlot && eventData.button == PointerEventData.InputButton.Left && _currentTypeOfMouseDrag == TypeOfMouseDrag.ClickOnceAndFollow)
		{
			if (CurrentInventory.CurrentSelectedSlotData != null && CurrentInventory.CurrentSelectedSlotData.InventoryItemData != null)
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.LowlightSlot, new HighlightSlotEvent(CurrentInventory.CurrentSelectedSlotData.InventoryItemData.wearable));
			}
			FinaliseInventoryItemMove();
		}
	}

	private void OnInventoryItemDragFinished(object[] obj)
	{
		if (CurrentInventory == null)
		{
			return;
		}
		PointerEventData pointerEventData = (PointerEventData)obj[0];
		if (!_inventorySystem.HasCurrentSelectedSlot || !CurrentInventory.Equals(_inventorySystem.LandingInventory))
		{
			return;
		}
		GameObject obj2 = pointerEventData.pointerCurrentRaycast.gameObject;
		if (TryInteractingWithToolSlot(obj2) || TryInteractingWithCharacterSlot(obj2) || TryInteractingWithHotBarSlot(obj2) || TryInteractWithCraftingMaterialSlot(obj2) || TryInteractingWithCipherSlot(obj2))
		{
			return;
		}
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.LowlightSlot, new HighlightSlotEvent(_inventorySystem.OriginInventory.CurrentSelectedSlotData.InventoryItemData.wearable));
		if (!TryInteractingWithGridUI(obj2) && !TryInteractingWithInventoryItemSlot(obj2))
		{
			if (_mouseOverUIOutOfBounds)
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInGameEvents.UIFloorReceiverClicked, null);
				return;
			}
			HideDraggingRelatedElements();
			_inventorySystem.OriginInventory.ReturnSelectedItem();
		}
	}

	private bool TryInteractingWithToolSlot(GameObject obj)
	{
		CharacterToolSlot component = obj.GetComponent<CharacterToolSlot>();
		if (component == null)
		{
			return false;
		}
		if (!component.TryEquip())
		{
			HideDraggingRelatedElements();
			_inventorySystem.OriginInventory.ReturnSelectedItem();
			OSDMessage.SendMessage("This item cannot be equipped in this slot.", MessageType.ClientError);
		}
		return true;
	}

	private bool TryInteractingWithCharacterSlot(GameObject obj)
	{
		CharacterSlot component = obj.GetComponent<CharacterSlot>();
		if (component == null)
		{
			return false;
		}
		if (CurrentInventory != null)
		{
			InventorySlotData currentSelectedSlotData = CurrentInventory.CurrentSelectedSlotData;
			if (currentSelectedSlotData != null && currentSelectedSlotData.Meta.ContainsKey("health") && float.TryParse(currentSelectedSlotData.Meta["health"], out var result) && result <= 0f)
			{
				OSDMessage.SendMessage(Localizer.LocalizeString(LocalizationSchema.KeyEQUIP_GEAR_DAMAGED), MessageType.ClientError);
				return false;
			}
		}
		if (!component.TryEquip())
		{
			HideDraggingRelatedElements();
			_inventorySystem.OriginInventory.ReturnSelectedItem();
			OSDMessage.SendMessage("This item cannot be equipped in this slot.", MessageType.ClientError);
		}
		return true;
	}

	private bool TryInteractingWithHotBarSlot(GameObject obj)
	{
		HotbarSlot component = obj.GetComponent<HotbarSlot>();
		if (component == null)
		{
			return false;
		}
		if (!component.AttemptEquip())
		{
			HideDraggingRelatedElements();
			_inventorySystem.OriginInventory.ReturnSelectedItem();
			OSDMessage.SendMessage("This item cannot be equipped in this slot.", MessageType.ClientError);
		}
		return true;
	}

	private bool TryInteractWithCraftingMaterialSlot(GameObject obj)
	{
		CraftingMaterialSlot component = obj.GetComponent<CraftingMaterialSlot>();
		CraftingCustomizationSlot component2 = obj.GetComponent<CraftingCustomizationSlot>();
		ShipBlueprintMaterialUI component3 = obj.GetComponent<ShipBlueprintMaterialUI>();
		if (component == null && component2 == null && component3 == null)
		{
			return false;
		}
		TypeOfMouseDrag currentTypeOfMouseDrag = _currentTypeOfMouseDrag;
		HideDraggingRelatedElements();
		if (component != null)
		{
			if (component.IsUsedByCurrentSchematic)
			{
				component.TryDropCurrentHeldItemOnSlot();
			}
			else if (currentTypeOfMouseDrag == TypeOfMouseDrag.HoldLeftButtonDown)
			{
				CurrentInventory.ReturnSelectedItem();
			}
		}
		else if (component2 != null)
		{
			if (component2.IsUsedByCurrentSchematic)
			{
				component2.TryDropCurrentHeldItemOnSlot();
			}
			else if (currentTypeOfMouseDrag == TypeOfMouseDrag.HoldLeftButtonDown)
			{
				CurrentInventory.ReturnSelectedItem();
			}
		}
		else if (component3 != null)
		{
			component3.TryDropCurrentHeldItemOnSlot();
		}
		return true;
	}

	private bool TryInteractingWithGridUI(GameObject obj)
	{
		InventoryGridUI component = obj.GetComponent<InventoryGridUI>();
		if (component == null)
		{
			return false;
		}
		if (!CanThisInventoryStoreItem())
		{
			return true;
		}
		if (_currentTypeOfMouseDrag == TypeOfMouseDrag.HoldLeftButtonDown)
		{
			FinaliseInventoryItemMove();
		}
		return true;
	}

	private bool TryInteractingWithInventoryItemSlot(GameObject obj)
	{
		InventoryItemSlot component = obj.GetComponent<InventoryItemSlot>();
		if (component == null)
		{
			return false;
		}
		if (_currentTypeOfMouseDrag == TypeOfMouseDrag.HoldLeftButtonDown)
		{
			CurrentInventory.CheckValidityOfInventoryMove();
		}
		return true;
	}

	private bool TryInteractingWithCipherSlot(GameObject obj)
	{
		CipherUiSlot cipherSlot = obj.GetComponentInParent<CipherUiSlot>();
		if (cipherSlot == null)
		{
			return false;
		}
		InventorySlotData item = CurrentInventory.CurrentSelectedSlotData;
		HideDraggingRelatedElements();
		CurrentInventory.ReturnSelectedItem();
		if (cipherSlot.IsUnlocked)
		{
			string text = "Are you sure you want to install the cipher?";
			if (cipherSlot.HasCipher)
			{
				text += " It will override the one that's already in the slot.";
			}
			DialogPopupFacade.ShowConfirmationDialog("Install cipher", text, delegate
			{
				if (cipherSlot.AttemptInstallCipher(item))
				{
					_inventorySystem.PlayerInventory.SetServerAsWaiting(isWaiting: true);
					SoundScreen.PlayASound("Play_Inventory_Item_Equip");
				}
			}, "CONFIRM");
		}
		return true;
	}

	private void ResetAndRefreshInventory()
	{
		if (_inventorySystem.HasCurrentSelectedSlot)
		{
			_inventorySystem.OriginInventory.SetCurrentSelectedSlotData(null);
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.RemoveStepFromSystem, new TutorialRemoveStepFromSystemEvent(TutorialStep.INVENTORY_PICKED_ITEM));
		}
		RefreshInventory();
	}

	public void RefreshInventory()
	{
		RefreshClientSideInventory();
	}

	private void RefreshClientSideInventory()
	{
		List<InventoryItemSlot> list = _activeInventoryItemSlotLookup.Values.ToList();
		_activeInventoryItemSlotLookup.Clear();
		InventoryItemSlot inventoryItemSlot = null;
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item in CurrentInventory.AllSlotDataLookup)
		{
			InventorySlotData value = item.Value;
			if (value.slotType == CharacterSlotType.None)
			{
				InventoryItemKey key = item.Key;
				InventoryItemSlot inventoryItemSlot2;
				if (list.Count > 0)
				{
					inventoryItemSlot2 = list[0];
					list.RemoveAt(0);
				}
				else
				{
					inventoryItemSlot2 = CreateItemSlotType(value);
				}
				inventoryItemSlot2.SetData(_inventorySystem.CurrentCraftingData, CurrentInventory, value, (RectTransform)base.transform);
				if (CurrentInventory.CurrentSelectedItemKey.Equals(key))
				{
					inventoryItemSlot = inventoryItemSlot2;
					CurrentInventory.SetCurrentSelectedSlotData(value);
				}
				_activeInventoryItemSlotLookup[key] = inventoryItemSlot2;
				CurrentInventory.InventorySpaceChecker.AddItem(value);
				_inventorySystem.SetHotbarAsShouldUpdate();
			}
		}
		if (list.Count > 0)
		{
			foreach (InventoryItemSlot item2 in list)
			{
				Object.Destroy(item2.gameObject);
			}
		}
		CurrentInventory.UpdateInventorySpaceChecker();
		if (inventoryItemSlot != null)
		{
			inventoryItemSlot.SetForClickedItemStayingOnMouseAfterServerRefresh();
			SetDropzone(isActive: true);
			SetPlaceholder(isActive: true);
		}
		else
		{
			SetDropzone(isActive: false);
			SetPlaceholder(isActive: false);
		}
	}

	private void SetDropzone(bool isActive)
	{
		if (isActive)
		{
			DropTarget.SetupAsPlaceholder(_inventorySystem.OriginInventory.CurrentSelectedSlotData, (RectTransform)base.transform, cellSize, CurrentInventory.InventorySpaceChecker);
			DropTarget.SetObjectActive(isActive: true);
			_inventorySystem.IsPlayerDraggingItem = true;
		}
		else
		{
			DropTarget.SetObjectActive(isActive: false);
			_inventorySystem.IsPlayerDraggingItem = false;
		}
	}

	private void SetPlaceholder(bool isActive)
	{
		if (isActive)
		{
			_originalLocationSlotRotation = _inventorySystem.OriginInventory.CurrentSelectedSlotData.rotated;
			OriginalLocationPlaceholder.SetupAsPlaceholder(_inventorySystem.OriginInventory.CurrentSelectedSlotData, (RectTransform)base.transform, cellSize, CurrentInventory.InventorySpaceChecker);
			OriginalLocationPlaceholder.SetObjectActive(isActive: true);
		}
		else
		{
			OriginalLocationPlaceholder.SetObjectActive(isActive: false);
		}
	}

	public void Resize()
	{
		_containerLayout.preferredHeight = CurrentInventory.InventoryHeight * cellSize;
		_containerLayout.preferredWidth = CurrentInventory.InventoryWidth * cellSize;
	}

	private InventoryItemSlot CreateItemSlotType(InventorySlotData inventorySlotData)
	{
		InventoryItemSlot inventoryItemSlot = UIObjectFactory.Create<InventoryItemSlot>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, base.transform, isObjectActive: true);
		if (inventorySlotData.InventoryItemData.wearable != 0)
		{
			inventoryItemSlot.SetSlotType(InventoryItemSlot.ItemSlotType.Clothing);
		}
		else if (inventorySlotData.InventoryItemData.equippable)
		{
			inventoryItemSlot.SetSlotType(InventoryItemSlot.ItemSlotType.Hotbar);
		}
		else if (inventorySlotData.InventoryItemData.category != string.Empty)
		{
			inventoryItemSlot.SetSlotType(InventoryItemSlot.ItemSlotType.Craftable);
		}
		return inventoryItemSlot;
	}

	private void FinaliseInventoryItemMove()
	{
		InventorySlotData currentSelectedSlotData = _inventorySystem.OriginInventory.CurrentSelectedSlotData;
		CurrentInventory.SetCurrentSelectedSlotData(null);
		if (_inventorySystem.IsCrossInventory)
		{
			_inventorySystem.OriginInventory.RemoveSelectedItemFromInventory();
		}
		currentSelectedSlotData.xPosition = (int)DropTarget.ServerAdjustedCoords.x;
		currentSelectedSlotData.yPosition = (int)DropTarget.ServerAdjustedCoords.y;
		currentSelectedSlotData.rotated = DropTarget.IsRotated;
		CurrentInventory.AddItemSlotDataToLookup(currentSelectedSlotData);
		RefreshClientSideInventory();
		if (currentSelectedSlotData.IsNewlySplitItem)
		{
			LocalPlayer.Instance.inventoryModificationBehaviour.RequestSplitItemStack(_inventorySystem.OriginInventory.inventoryEntityId, _inventorySystem.DestinationInventory.inventoryEntityId, currentSelectedSlotData.serverItemId, currentSelectedSlotData.xPosition, currentSelectedSlotData.yPosition, currentSelectedSlotData.rotated, currentSelectedSlotData.amount);
		}
		else if (_inventorySystem.IsCrossInventory)
		{
			_inventorySystem.SetAllInventoryServersAsWaiting();
			LocalPlayer.Instance.inventoryModificationBehaviour.RequestCrossInventoryMoveItem(_inventorySystem.OriginInventory.inventoryEntityId, _inventorySystem.DestinationInventory.inventoryEntityId, currentSelectedSlotData.serverItemId, currentSelectedSlotData.xPosition, currentSelectedSlotData.yPosition, currentSelectedSlotData.rotated, currentSelectedSlotData.lockBoxItem);
		}
		else
		{
			CurrentInventory.SetServerAsWaiting(isWaiting: true);
			LocalPlayer.Instance.inventoryModificationBehaviour.RequestInventoryMoveItem(CurrentInventory.inventoryEntityId, currentSelectedSlotData.serverItemId, currentSelectedSlotData.xPosition, currentSelectedSlotData.yPosition, currentSelectedSlotData.rotated, currentSelectedSlotData.lockBoxItem);
		}
		PlayDropItemSound(currentSelectedSlotData);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.RemoveStepFromSystem, new TutorialRemoveStepFromSystemEvent(TutorialStep.INVENTORY_PICKED_ITEM));
	}

	private void HideDraggingRelatedElements()
	{
		if (_activeInventoryItemSlotLookup.ContainsKey(CurrentInventory.CurrentSelectedItemKey))
		{
			_activeInventoryItemSlotLookup[CurrentInventory.CurrentSelectedItemKey].SetObjectActive(isActive: false);
		}
		else
		{
			WALogger.Error<InventoryGridUI>("Item not found in the inventory {0}", new object[1] { CurrentInventory.CurrentSelectedItemKey });
		}
		HideDragGizmos();
	}

	private void HideDragGizmos()
	{
		SetDropzone(isActive: false);
		SetPlaceholder(isActive: false);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.ChangeMouseDragType, new UpdateMouseDragTypeEvent(TypeOfMouseDrag.None));
	}

	public static void RequestInventoryMoveAll(InventoryGridUI fromGrid, InventoryGridUI toGrid, EntityId srcInvEntityId, EntityId destInvEntityId)
	{
		fromGrid.RefreshClientSideInventory();
		fromGrid.CurrentInventory.SetServerAsWaiting(isWaiting: true);
		toGrid.RefreshClientSideInventory();
		toGrid.CurrentInventory.SetServerAsWaiting(isWaiting: true);
		LocalPlayer.Instance.inventoryModificationBehaviour.MoveAll(srcInvEntityId, destInvEntityId);
	}

	private void ReturnCurrentSelectedItemToInventory()
	{
		if (_inventorySystem.HasCurrentSelectedSlot && CurrentInventory.Equals(_inventorySystem.OriginInventory))
		{
			InventorySlotData currentSelectedSlotData = _inventorySystem.OriginInventory.CurrentSelectedSlotData;
			if (currentSelectedSlotData.IsNewlySplitItem)
			{
				ReturnCurrentSplitItem(currentSelectedSlotData);
			}
			else
			{
				ReturnCurrentNonSplitItem();
			}
		}
	}

	private void ReturnCurrentSplitItem(InventorySlotData slotToReturn)
	{
		InventoryItemSlot inventoryItemSlot = _activeInventoryItemSlotLookup[_inventorySystem.OriginInventory.CurrentSelectedItemKey];
		_activeInventoryItemSlotLookup.Remove(_inventorySystem.OriginInventory.CurrentSelectedItemKey);
		foreach (KeyValuePair<InventoryItemKey, InventoryItemSlot> item in _activeInventoryItemSlotLookup)
		{
			if (item.Key.ItemId == slotToReturn.serverItemId)
			{
				item.Value.InventorySlotSourceData.amount += slotToReturn.amount;
				item.Value.SetData(_inventorySystem.CurrentCraftingData, CurrentInventory, item.Value.InventorySlotSourceData, (RectTransform)base.transform);
			}
		}
		Object.Destroy(inventoryItemSlot.gameObject);
		CurrentInventory.RemoveSlot(slotToReturn);
		CurrentInventory.SetCurrentSelectedSlotData(null);
	}

	private void ReturnCurrentNonSplitItem()
	{
		InventoryItemSlot inventoryItemSlot = _activeInventoryItemSlotLookup[_inventorySystem.OriginInventory.CurrentSelectedItemKey];
		inventoryItemSlot.InventorySlotSourceData.rotated = _originalLocationSlotRotation;
		inventoryItemSlot.SetData(_inventorySystem.CurrentCraftingData, CurrentInventory, inventoryItemSlot.InventorySlotSourceData, (RectTransform)base.transform);
	}

	public void PlayCloseSound()
	{
		switch (CurrentInventoryType)
		{
		case InventoryObjectType.Chest:
			SoundScreen.PlayASound("Play_Chest_Close");
			break;
		case InventoryObjectType.Crate:
			SoundScreen.PlayASound("Play_StorageCrate_Close");
			break;
		default:
			SoundScreen.PlayASound("Play_Chest_Close");
			break;
		}
	}

	public void PlayOpenSound()
	{
		InventoryObjectType currentInventoryType = CurrentInventoryType;
		if (currentInventoryType == InventoryObjectType.Crate)
		{
			SoundScreen.PlayASound("Play_StorageCrate_Open");
		}
		else
		{
			SoundScreen.PlayASound("Play_Chest_Open");
		}
	}

	public void PlayDropItemSound(InventorySlotData inventorySlotData)
	{
		bool isCrate = CurrentInventoryType == InventoryObjectType.Crate;
		string category = string.Empty;
		if (inventorySlotData != null && InventoryItemManager.Instance.IsRawMaterial(inventorySlotData.ItemTypeId))
		{
			category = inventorySlotData.InventoryItemData.category;
		}
		PlayDropSfxByItemCategory(category, isCrate);
	}

	public static void PlayPickSfxByItemCategory(string category, bool isCrate)
	{
		switch (category)
		{
		case "Metal":
			SoundScreen.PlayASound("Play_Inventory_ItemMove_Pick_metal");
			break;
		case "Wood":
			SoundScreen.PlayASound("Play_Inventory_ItemMove_Pick_wood");
			break;
		case "Fuel":
			SoundScreen.PlayASound("Play_Inventory_ItemMove_Pick_Fuel");
			break;
		default:
			SoundScreen.PlayASound((!isCrate) ? "Play_Inventory_ItemMove_Pick_Generic" : "Play_StorageCrate_RemoveItem");
			break;
		}
	}

	public static void PlayDropSfxByItemCategory(string category, bool isCrate)
	{
		switch (category)
		{
		case "Metal":
			SoundScreen.PlayASound("Play_Inventory_ItemMove_Drop_metal");
			return;
		case "Wood":
			SoundScreen.PlayASound("Play_Inventory_ItemMove_Drop_wood");
			return;
		case "Stone":
			SoundScreen.PlayASound("Play_Inventory_ItemMove_Drop_stone");
			return;
		case "Fuel":
			SoundScreen.PlayASound("Play_Inventory_ItemMove_Drop_Fuel");
			return;
		}
		SoundScreen.PlayASound("Play_StorageCrate_AddItem");
		if (!isCrate)
		{
			SoundScreen.PlayASound("Play_Inventory_ItemMove_Drop_generic");
		}
	}

	public static void PlayDeleteSfx()
	{
		SoundScreen.PlayASound("Play_Inventory_ItemMove_Delete_Item");
	}
}
