using Bossa.Travellers.Craftingstation;
using Improbable.Collections;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.InfoPopups;
using Travellers.UI.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class CipherUiSlot : UIScreenComponent, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	[SerializeField]
	private Image _background;

	[SerializeField]
	private Sprite _lockedSprite;

	[SerializeField]
	private Sprite _unlockedSprite;

	[SerializeField]
	private RawImage _rawImage;

	[SerializeField]
	private float lockedAlpha = 0.1f;

	private Option<Cipher> _currentCipherData;

	private SchematicData _loadedSchematicData;

	private int _index;

	private bool _isUnlocked;

	private InventorySystem _inventorySystem;

	public bool IsUnlocked => _isUnlocked;

	public bool HasCipher => _currentCipherData.HasValue;

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

	public void Setup(SchematicData schematic, bool isUnlocked, Option<Cipher> cipher, int index)
	{
		_loadedSchematicData = schematic;
		_isUnlocked = isUnlocked;
		_index = index;
		_currentCipherData = cipher;
		OnCipherUpdated();
	}

	public bool AttemptInstallCipher(InventorySlotData inventorySlot)
	{
		string val = null;
		if (!inventorySlot.Meta.TryGetValue("cipherShipPartType", out val))
		{
			WALogger.ErrorOnce<CipherSlot>("Cipher doesn't have cipherShipPartType for " + inventorySlot.ItemTypeId, new object[0]);
			return false;
		}
		if (_loadedSchematicData.itemType.ToLower() != val.ToLower())
		{
			OSDMessage.SendMessage("Can't install " + val + " cipher into " + _loadedSchematicData.itemType + " schematic");
			return false;
		}
		string val2 = null;
		if (!inventorySlot.Meta.TryGetValue("cipherRarity", out val2))
		{
			WALogger.ErrorOnce<CipherSlot>("Cipher doesn't have cipherRarity for " + inventorySlot.ItemTypeId, new object[0]);
			return false;
		}
		string val3 = null;
		if (!inventorySlot.Meta.TryGetValue("cipherStats", out val3))
		{
			WALogger.ErrorOnce<CipherSlot>("Cipher doesn't have cipherStat for " + inventorySlot.ItemTypeId, new object[0]);
			return false;
		}
		LocalPlayer.Instance.inventoryModificationBehaviour.TryInstallCipher(_index, inventorySlot.serverItemId, inventorySlot.lockBoxItem, _loadedSchematicData.referenceData);
		return true;
	}

	private void OnCipherUpdated()
	{
		if (_currentCipherData == null)
		{
			_rawImage.enabled = false;
		}
		else
		{
			InventoryItemData inventoryItemData = InventoryItemManager.Instance.LookupItem(_currentCipherData.Value.itemTypeId);
			if (inventoryItemData != null)
			{
				string iconName = inventoryItemData.iconName;
				if (_currentCipherData.HasValue)
				{
					_rawImage.enabled = true;
					string mainStat = string.Empty;
					int currentMax = 0;
					_currentCipherData.Value.stats.ForEach(delegate(CipherStat stat)
					{
						if (stat.statModification > currentMax)
						{
							currentMax = stat.statModification;
							mainStat = stat.statName;
						}
					});
					string part = _currentCipherData.Value.shipPartType.ToString();
					string colorStringFromStatAndPart = CipherIconUtil.GetColorStringFromStatAndPart(mainStat, part);
					iconName = "slot_" + colorStringFromStatAndPart;
					Texture iconTexture = InventoryIconManager.Instance.GetIconTexture(iconName);
					_rawImage.texture = iconTexture;
					_rawImage.CrossFadeAlpha(1f, 0f, ignoreTimeScale: true);
				}
				else
				{
					WALogger.Error<CipherUiSlot>("The cipher option is 'None', that should never happen, CHECK IT!");
					_rawImage.enabled = true;
				}
			}
		}
		if ((bool)_background)
		{
			Color color = _background.color;
			color.a = ((!_isUnlocked) ? lockedAlpha : 1f);
			_background.color = color;
			_background.sprite = ((!_isUnlocked) ? _lockedSprite : _unlockedSprite);
		}
	}

	private ScannableData CreateScannableData()
	{
		ScannableData scannableData = new ScannableData();
		if (_currentCipherData.HasValue)
		{
			InventoryItemData inventoryItemData = InventoryItemManager.Instance.LookupItem(_currentCipherData.Value.itemTypeId);
			if (inventoryItemData != null)
			{
				scannableData.title = inventoryItemData.name;
				scannableData.description = string.Concat(RarityHelper.FormatRarity(_currentCipherData.Value.rarity), " ", _currentCipherData.Value.shipPartType, " Cipher\n", ScannableData.GenerateCipherStatDescription(_currentCipherData.Value.stats));
			}
		}
		else
		{
			scannableData.title = "Cipher Slot";
			if (_isUnlocked)
			{
				scannableData.description = "Place a cipher for this ship part here to alter this schematic's stats.";
			}
			else
			{
				scannableData.description = "Cipher slots can be unlocked in the knowledge tree, allowing you to plug ciphers into schematics to alter their stats.";
			}
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
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		UIWindowController.PopState<ScannerToolPopupState>();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button != 0 || !_isUnlocked)
		{
			return;
		}
		if (_inventorySystem.HasCurrentSelectedSlot)
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.DraggedInventoryItemDropped, eventData);
		}
		else if (_currentCipherData.HasValue)
		{
			DialogPopupFacade.ShowConfirmationDialog("Destroy cipher", "Are you sure you want to destroy the cipher? It will remove the effect on the schematic.", delegate
			{
				LocalPlayer.Instance.inventoryModificationBehaviour.TryDestroyCipher(_index, _loadedSchematicData.referenceData);
			}, "CONFIRM");
		}
	}
}
