using System;
using System.Collections.Generic;
using Bossa.Travellers.World;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.Sound;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class InventoryTooltipPopup : UIPopup
{
	[Serializable]
	public enum ItemAction
	{
		USE,
		EQUIP,
		SALVAGE,
		COLLECT,
		DESTROY,
		UNEQUIP,
		LEARN,
		SPLIT
	}

	[SerializeField]
	private InventoryActionList _actionListContainer;

	[SerializeField]
	private ItemActionDeleteConfirm _deleteConfirmBox;

	[SerializeField]
	private RectTransform _movementHandleObject;

	private Vector3 _movementReferencePoint;

	[SerializeField]
	private Vector2 _offset;

	private InventorySystem _inventorySystem;

	private IInventoryActionSource _inventoryActionSource;

	[InjectableMethod]
	public void InjectDependencies(InventorySystem inventorySystem)
	{
		_inventorySystem = inventorySystem;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		Vector3[] array = new Vector3[4];
		_movementHandleObject.GetWorldCorners(array);
		RectTransform rectTransform = (RectTransform)base.transform;
		rectTransform.GetWorldCorners(array);
		_movementReferencePoint = array[1];
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetDataForPopup(IInventoryActionSource actionSource, bool onlyShowDestroy)
	{
		if (actionSource != null)
		{
			_inventoryActionSource = actionSource;
			if (onlyShowDestroy)
			{
				Destroy(actionSource);
			}
			else
			{
				SetUpActionList(_inventoryActionSource);
			}
		}
	}

	private void SetUpActionList(IInventoryActionSource inventoryActionSource)
	{
		List<InventoryActionButtonData> list = new List<InventoryActionButtonData>();
		if (_inventoryActionSource.InventorySlotSourceData.InventoryItemData == null)
		{
			return;
		}
		CheckIfCanLearn(_inventoryActionSource, list);
		CheckIfCanUse(_inventoryActionSource, list);
		CheckIfCanSplit(_inventoryActionSource, list);
		bool flag = inventoryActionSource is CharacterSlot;
		bool flag2 = inventoryActionSource is LockboxItem;
		CheckIfCanEquip(_inventoryActionSource, list, flag);
		if (!flag && !flag2)
		{
			list.Add(new InventoryActionButtonData(ItemAction.DESTROY, "DESTROY", delegate
			{
				Destroy(inventoryActionSource);
			}));
		}
		_actionListContainer.SetListData(list);
		_actionListContainer.SetObjectActive(isActive: true);
		_deleteConfirmBox.SetObjectActive(isActive: false);
		SetPosition();
	}

	private void CheckIfCanUse(IInventoryActionSource inventoryActionSource, List<InventoryActionButtonData> actions)
	{
		InventorySlotData inventorySlotSourceData = inventoryActionSource.InventorySlotSourceData;
		if (inventorySlotSourceData.ItemTypeId.StartsWith("scrapItem-") || inventorySlotSourceData.ItemTypeId == "schematics")
		{
			actions.Add(new InventoryActionButtonData(ItemAction.SALVAGE, "SALVAGE", delegate
			{
				Salvage(inventoryActionSource);
			}));
		}
		else if (inventorySlotSourceData.ItemTypeId == "lore")
		{
			actions.Add(new InventoryActionButtonData(ItemAction.COLLECT, "COLLECT", delegate
			{
				Use(inventoryActionSource);
			}));
		}
		else if (inventorySlotSourceData.ItemTypeId.StartsWith("steamInvBundle-"))
		{
			actions.Add(new InventoryActionButtonData(ItemAction.USE, "OPEN", delegate
			{
				Use(inventoryActionSource);
			}));
		}
	}

	private void CheckIfCanEquip(IInventoryActionSource inventoryActionSource, List<InventoryActionButtonData> actions, bool isCharacterSlot)
	{
		InventorySlotData inventorySlotSourceData = inventoryActionSource.InventorySlotSourceData;
		bool flag = inventorySlotSourceData.InventoryItemData.equippable || inventorySlotSourceData.InventoryItemData.wearable != CharacterSlotType.None;
		bool flag2 = flag;
		if (flag)
		{
			flag = inventorySlotSourceData.hotBarSlotNum < 0 && _inventorySystem.TryGetEmptyHotBarSlotIndex(out var _);
		}
		if (inventorySlotSourceData.InventoryItemData.equippable && inventorySlotSourceData.hotBarSlotNum != -1)
		{
			flag = false;
			flag2 = true;
		}
		else if (inventorySlotSourceData.InventoryItemData.wearable != 0 && inventorySlotSourceData.slotType == inventorySlotSourceData.InventoryItemData.wearable)
		{
			flag = false;
			flag2 = true;
		}
		else
		{
			flag2 = false;
		}
		if (!LocalPlayer.Instance.playerMove.CanEquipThings)
		{
			flag = false;
		}
		if (flag && !isCharacterSlot)
		{
			actions.Add(new InventoryActionButtonData(ItemAction.EQUIP, "EQUIP", delegate
			{
				Equip(inventoryActionSource);
			}));
		}
		if (flag2)
		{
			actions.Add(new InventoryActionButtonData(ItemAction.UNEQUIP, "UNEQUIP", delegate
			{
				Unequip(inventoryActionSource);
			}));
		}
	}

	private void CheckIfCanLearn(IInventoryActionSource inventoryActionSource, List<InventoryActionButtonData> actions)
	{
		InventorySlotData inventorySlotSourceData = inventoryActionSource.InventorySlotSourceData;
		if (inventorySlotSourceData.InventoryItemData.itemTypeId == "schematics")
		{
			actions.Add(new InventoryActionButtonData(ItemAction.LEARN, "LEARN", delegate
			{
				Learn(inventoryActionSource);
			}));
		}
	}

	private void CheckIfCanSplit(IInventoryActionSource inventoryActionSource, List<InventoryActionButtonData> actions)
	{
		InventorySlotData inventorySlotSourceData = inventoryActionSource.InventorySlotSourceData;
		if (inventorySlotSourceData.amount > 1)
		{
			actions.Add(new InventoryActionButtonData(ItemAction.SPLIT, "SPLIT", delegate
			{
				Split(inventoryActionSource);
			}));
		}
	}

	private void Use(IInventoryActionSource actionSource)
	{
		if (actionSource != null)
		{
			actionSource.Use();
			_inventorySystem.SetHotbarAsShouldUpdate();
		}
		UIWindowController.PopState<InventoryTooltipPopupState>();
	}

	private void Learn(IInventoryActionSource actionSource)
	{
		if (_inventorySystem.CurrentCraftingData != null && !_inventorySystem.CurrentCraftingData.AllSlotsAreEmptyRemotely)
		{
			OSDMessage.SendMessage("Cannot learn schematic while crafting", MessageType.ClientError);
		}
		else
		{
			actionSource.Learn();
			_inventorySystem.SetHotbarAsShouldUpdate();
		}
		UIWindowController.PopState<InventoryTooltipPopupState>();
	}

	private void Equip(IInventoryActionSource actionSource)
	{
		actionSource.TryEquip();
		SoundScreen.PlayASound("Play_Inventory_Item_Equip");
		UIWindowController.PopState<InventoryTooltipPopupState>();
	}

	private void Unequip(IInventoryActionSource actionSource)
	{
		actionSource.Unequip(letUserPositionInInventory: false);
		SoundScreen.PlayASound("Play_Inventory_Item_UnEquip");
		UIWindowController.PopState<InventoryTooltipPopupState>();
	}

	private void Salvage(IInventoryActionSource actionSource)
	{
		if (actionSource.ParentInventoryContents != _inventorySystem.PlayerInventory)
		{
			OSDMessage.SendMessage("Can only salvage from your inventory", MessageType.ClientError);
		}
		else
		{
			DoSalvage(actionSource);
		}
		UIWindowController.PopState<InventoryTooltipPopupState>();
	}

	private void DoSalvage(IInventoryActionSource actionSource)
	{
		Use(actionSource);
		SoundScreen.PlayASound("Play_Inventory_ItemMove_Salvage_Item");
	}

	private void Destroy(IInventoryActionSource actionSource)
	{
		_actionListContainer.SetObjectActive(isActive: false);
		_deleteConfirmBox.SetActionSource(actionSource, DoDestroy, ReturnItemToInventory);
		SetPosition();
	}

	private void Split(IInventoryActionSource actionSource)
	{
		actionSource.Split();
		UIWindowController.PopState<InventoryTooltipPopupState>();
	}

	private void DoDestroy(IInventoryActionSource actionSource)
	{
		actionSource.Destroy();
		UIWindowController.PopState<InventoryTooltipPopupState>();
	}

	private void ReturnItemToInventory()
	{
		if (_inventorySystem.OriginInventory != null)
		{
			_inventorySystem.OriginInventory.ReturnSelectedItem();
		}
		UIWindowController.PopState<InventoryTooltipPopupState>();
	}

	private void SetPosition()
	{
		Vector2 vector = new Vector2(Input.mousePosition.x - _movementReferencePoint.x, Input.mousePosition.y - _movementReferencePoint.y);
		Vector2 vector2 = vector / UIStructure.RootCanvas.scaleFactor + _offset;
		float x = vector2.x;
		if (vector2.x + _movementHandleObject.rect.width > (float)Screen.width)
		{
			x = (float)Screen.width - _movementHandleObject.rect.width - 20f;
		}
		float y = vector2.y;
		if (vector2.y - _movementHandleObject.rect.height < (float)Screen.height * -1f)
		{
			y = (float)Screen.height * -1f + _movementHandleObject.rect.height + 20f;
		}
		Vector2 anchoredPosition = new Vector2(x, y);
		_movementHandleObject.anchoredPosition = anchoredPosition;
	}

	public void DismissAreaClicked()
	{
		if (_deleteConfirmBox.isActiveAndEnabled)
		{
			ReturnItemToInventory();
		}
		UIWindowController.PopState<InventoryTooltipPopupState>();
	}
}
