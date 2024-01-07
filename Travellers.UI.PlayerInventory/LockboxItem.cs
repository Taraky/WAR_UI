using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class LockboxItem : UIScreenComponent, IInventoryActionSource, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	[SerializeField]
	private UILockboxItemImage _imageController;

	public InventorySlotData InventorySlotSourceData { get; private set; }

	public InventoryContents ParentInventoryContents { get; private set; }

	[InjectableMethod]
	public void InjectDependencies(InventorySystem inventorySystem)
	{
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

	public void SetData(InventorySlotData data)
	{
		if (data == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		InventorySlotSourceData = data;
		_imageController.SetItemDataAndParentRect(data, (RectTransform)base.transform);
		_imageController.UpdateItemIcon();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			UIWindowController.PushState(new InventoryTooltipPopupState(this));
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ScannableData scannableData = ScannableData.FromInventorySlotData(InventorySlotSourceData);
		UIWindowController.PushState(new ScannerToolPopupState(scannableData));
		_imageController.SetHighlight(isHighlighted: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		UIWindowController.PopState<ScannerToolPopupState>();
		_imageController.SetHighlight(isHighlighted: false);
	}

	public void Use()
	{
		WALogger.Error<CharacterSlot>(LogChannel.UI, "Attempting to [USE] an item in a lockbox slot. This shouldn't be possible. Inform your local UI rep as soon as possible", new object[0]);
	}

	public void Learn()
	{
		WALogger.Error<CharacterSlot>(LogChannel.UI, "Attempting to [LEARN] an item in a lockbox slot. This shouldn't be possible. Inform your local UI rep as soon as possible", new object[0]);
	}

	public bool TryEquip()
	{
		LocalPlayer.Instance.inventoryModificationBehaviour.EquipWearable(InventorySlotSourceData.serverItemId, InventorySlotSourceData.utilitySlotNum, InventorySlotSourceData.lockBoxItem);
		return true;
	}

	public void Unequip(bool letUserPositionInInventory)
	{
		LocalPlayer.Instance.inventoryModificationBehaviour.UnequipWearable(InventorySlotSourceData.slotType.ToString(), InventorySlotSourceData.utilitySlotNum, InventorySlotSourceData.lockBoxItem, InventorySlotSourceData.serverItemId);
	}

	public void Split()
	{
		WALogger.Error<CharacterSlot>(LogChannel.UI, "Attempting to [SPLIT] an item in a lockbox slot. This shouldn't be possible. Inform your local UI rep as soon as possible", new object[0]);
	}

	public void Destroy()
	{
		WALogger.Error<CharacterSlot>(LogChannel.UI, "Attempting to [DESTROY] an item in a lockbox slot. This shouldn't be possible. Inform your local UI rep as soon as possible", new object[0]);
	}
}
