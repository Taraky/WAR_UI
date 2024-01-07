using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.Sound;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class HotbarSlot : UIScreenComponent, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public static HashSet<HotbarSlotType> EquippableSlotGroup = new HashSet<HotbarSlotType>
	{
		HotbarSlotType.EquippableSlot_1,
		HotbarSlotType.EquippableSlot_2,
		HotbarSlotType.EquippableSlot_3,
		HotbarSlotType.EquippableSlot_4
	};

	public static HashSet<HotbarSlotType> GauntletSlotGroup = new HashSet<HotbarSlotType>
	{
		HotbarSlotType.Salvage,
		HotbarSlotType.Repair,
		HotbarSlotType.Lifter,
		HotbarSlotType.Scanner
	};

	public Image background;

	public RawImage rawImage;

	public AspectRatioFitter aspectRatioFitter;

	private InventorySlotData _currentSlotData;

	public Image highlighted;

	public InventoryItemData equippedItem;

	public bool allowInteractions = true;

	private float baseAlpha = 0.7f;

	[SerializeField]
	public HotbarSlotType SlotType;

	public static HotbarSlotType currentSelected = HotbarSlotType.Salvage;

	private InventorySystem _inventorySystem;

	public bool IsEmpty => _currentSlotData == null;

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
	}

	protected override void ProtectedDispose()
	{
	}

	public bool IsInGroup(HotbarSlotType typeToCheck)
	{
		return EquippableSlotGroup.Contains(typeToCheck) || GauntletSlotGroup.Contains(typeToCheck);
	}

	private void Start()
	{
		if (allowInteractions && equippedItem == null)
		{
			rawImage.enabled = false;
		}
		if (!allowInteractions && background != null)
		{
			Color color = background.color;
			color.a = baseAlpha;
			background.color = color;
		}
		highlighted.enabled = false;
	}

	public void Setup(InventorySlotData slotData)
	{
		_currentSlotData = slotData;
		InventoryItemData inventoryItemData = slotData.InventoryItemData;
		rawImage.enabled = true;
		equippedItem = inventoryItemData;
		if (inventoryItemData != null)
		{
			Texture iconTexture = InventoryIconManager.Instance.GetIconTexture(inventoryItemData.iconName);
			float aspectRatio = (float)iconTexture.width / (float)iconTexture.height;
			rawImage.texture = iconTexture;
			aspectRatioFitter.aspectRatio = aspectRatio;
			rawImage.CrossFadeAlpha(1f, 0f, ignoreTimeScale: true);
			if ((bool)background)
			{
				Color color = background.color;
				color.a = baseAlpha;
				background.color = color;
			}
		}
	}

	public void SetToolState(bool activated)
	{
		if (SlotType != HotbarSlotType.Social && !EquippableSlotGroup.Contains(SlotType) && SlotType != HotbarSlotType.Inventory)
		{
			Color color = rawImage.color;
			color.a = ((!activated) ? 0.2f : 1f);
			Color color2 = background.color;
			color2.a = ((!activated) ? 0.2f : 0.7f);
			background.color = color2;
			rawImage.color = color;
		}
	}

	public bool AttemptEquip()
	{
		if (_inventorySystem.OriginInventory.InventoryTypeEnum == InventoryObjectType.Personal && _inventorySystem.OriginInventory.CurrentSelectedSlotData.InventoryItemData.equippable && _inventorySystem.OriginInventory.CurrentSelectedSlotData.slotType == CharacterSlotType.None && EquippableSlotGroup.Contains(SlotType) && !_inventorySystem.OriginInventory.CurrentSelectedSlotData.IsNewlySplitItem)
		{
			Equip();
			return true;
		}
		return false;
	}

	public void Equip()
	{
		InventorySlotData currentSelectedSlotData = _inventorySystem.OriginInventory.CurrentSelectedSlotData;
		_inventorySystem.OriginInventory.EquipCurrentSelectedHotbarSlottableItemInSlot((int)SlotType);
		_inventorySystem.SetHotbarAsShouldUpdate();
		_inventorySystem.PlayerInventory.SetServerAsWaiting(isWaiting: true);
		LocalPlayer.Instance.inventoryModificationBehaviour.AssignItemToHotbar(currentSelectedSlotData.serverItemId, currentSelectedSlotData.hotBarSlotNum, currentSelectedSlotData.lockBoxItem);
		SoundScreen.PlayASound("Play_Inventory_Item_Equip");
	}

	public void Unequip()
	{
		UIWindowController.PopState<ScannerToolPopupState>();
		if (_currentSlotData != null && _currentSlotData.hotBarSlotNum != -1)
		{
			int hotBarSlotNum = _currentSlotData.hotBarSlotNum;
			_currentSlotData.hotBarSlotNum = -1;
			_inventorySystem.SetHotbarAsShouldUpdate();
			_inventorySystem.PlayerInventory.SetServerAsWaiting(isWaiting: true);
			LocalPlayer.Instance.inventoryModificationBehaviour.RemoveItemFromHotbar(hotBarSlotNum, _currentSlotData.lockBoxItem);
			SoundScreen.PlayASound("Play_Inventory_Item_UnEquip");
		}
	}

	public void ClearItem()
	{
		rawImage.CrossFadeAlpha(0f, 0f, ignoreTimeScale: true);
		rawImage.enabled = false;
		rawImage.texture = null;
		_currentSlotData = null;
		if ((bool)background)
		{
			Color color = background.color;
			color.a = 0.2f;
			background.color = color;
		}
	}

	private ScannableData CreateScannableData()
	{
		ScannableData scannableData = null;
		switch (SlotType)
		{
		case HotbarSlotType.Salvage:
			scannableData = ScannableData.FromToolType(ToolType.Salvage);
			break;
		case HotbarSlotType.Repair:
			scannableData = ScannableData.FromToolType(ToolType.Repair);
			break;
		case HotbarSlotType.Lifter:
			scannableData = ScannableData.FromToolType(ToolType.Build);
			break;
		case HotbarSlotType.Scanner:
			scannableData = ScannableData.FromToolType(ToolType.Scan);
			break;
		case HotbarSlotType.Inventory:
			scannableData = new ScannableData();
			scannableData.Title = "Menu";
			scannableData.description = "Access your inventory and use other tabs to craft inventory items, manage your schematics and spend your knowledge.";
			break;
		case HotbarSlotType.Social:
			scannableData = new ScannableData();
			scannableData.Title = "Social";
			scannableData.description = "Create and join a crew or an alliance, allowing you to more easily stick together and play with like minded travellers.";
			break;
		default:
			if (_currentSlotData != null)
			{
				scannableData = ScannableData.FromInventorySlotData(_currentSlotData);
			}
			break;
		}
		return scannableData;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ScannableData scannableData = CreateScannableData();
		if (scannableData != null)
		{
			scannableData.FollowMouse = true;
			UIWindowController.PushState(new ScannerToolPopupState(scannableData));
		}
		if (allowInteractions)
		{
			if (_currentSlotData != null && SlotType != currentSelected)
			{
				highlighted.enabled = true;
				highlighted.CrossFadeAlpha(0.5f, 0.1f, ignoreTimeScale: true);
			}
			if (_inventorySystem.HasCurrentSelectedSlot && _inventorySystem.OriginInventory.InventoryTypeEnum == InventoryObjectType.Personal && _inventorySystem.OriginInventory.CurrentSelectedSlotData != null && _inventorySystem.OriginInventory.CurrentSelectedSlotData.InventoryItemData.equippable && _inventorySystem.OriginInventory.CurrentSelectedSlotData.slotType == CharacterSlotType.None && EquippableSlotGroup.Contains(SlotType))
			{
				highlighted.enabled = true;
				highlighted.CrossFadeAlpha(0.5f, 0.1f, ignoreTimeScale: true);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		UIWindowController.PopState<ScannerToolPopupState>();
		if (allowInteractions && SlotType != currentSelected && highlighted.color.a > 0f)
		{
			highlighted.CrossFadeAlpha(0f, 0.1f, ignoreTimeScale: true);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (allowInteractions && eventData.button == PointerEventData.InputButton.Left)
		{
			if (_inventorySystem.HasCurrentSelectedSlot)
			{
				AttemptEquip();
			}
			else if (_currentSlotData != null)
			{
				Unequip();
			}
		}
	}

	public void SetSelected(bool enabled)
	{
		highlighted.enabled = enabled;
		if (enabled)
		{
			highlighted.CrossFadeAlpha(0.5f, 0.1f, ignoreTimeScale: true);
		}
	}
}
