using Bossa.Travellers.Materials;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class SchematicSlot : UIScreenComponent, IPrimitiveHierarchyElement, IPointerEnterHandler, IPointerExitHandler, IHierarchyElement, IEventSystemHandler
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	protected UIButtonController _uiButtonController;

	protected int _index = -1;

	protected bool _isSelected;

	private Material _coloredIconMaterial;

	private AspectRatioFitter _iconAspectRatioFitter;

	public SchematicData SchemData { get; protected set; }

	public int Index
	{
		get
		{
			return _index;
		}
		set
		{
			_index = value;
		}
	}

	public string CurrentTitle { get; private set; }

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void SelectElement()
	{
		_uiButtonController.SetButtonSelected(selected: true);
		_isSelected = true;
		SetObjectActive(isActive: true);
	}

	public void DeselectElement()
	{
		_isSelected = false;
		_uiButtonController.SetButtonSelected(selected: false);
	}

	public void HideElement()
	{
		SetObjectActive(isActive: false);
	}

	public void ShowElement()
	{
		SetObjectActive(isActive: true);
		_uiButtonController.SetButtonSelected(_isSelected);
	}

	public virtual void ShowSelectedOverlay(bool show)
	{
	}

	public void OnPointerEnter(PointerEventData data)
	{
		if (!SchemData.IsShip)
		{
			ScannableData scannableData = ScannableData.FromSchematicData(GetData());
			scannableData.FollowMouse = true;
			UIWindowController.PushState(new ScannerToolPopupState(scannableData));
		}
	}

	public void OnPointerExit(PointerEventData data)
	{
		if (!SchemData.IsShip)
		{
			UIWindowController.PopState<ScannerToolPopupState>();
		}
	}

	public void PopulateData(SchematicData Data)
	{
		SetItemData(Data);
		if (_isSelected)
		{
			SelectElement();
		}
		else
		{
			DeselectElement();
		}
	}

	public virtual void SetItemData(SchematicData data)
	{
		SchemData = data;
		_uiButtonController.SetButtonEvent(WAUIButtonEvents.SchematicItemPressed, new SchematicCategoryListItemPressed(data, this));
		_uiButtonController.SetButtonSelected(selected: true);
		if (data == null)
		{
			return;
		}
		if (_icon != null)
		{
			Sprite sprite;
			switch (data.CraftingCategoryEnum)
			{
			case CraftingCategory.Personal:
			case CraftingCategory.CraftingStation:
			case CraftingCategory.Cooking:
			case CraftingCategory.Clothing:
				sprite = InventoryIconManager.Instance.GetIconSprite(data.iconId);
				if (ProcColoredIconHelper.ShouldUseProcColoredShader(data))
				{
					InventoryItemData itemData = InventoryItemManager.Instance.LookupItem(data.schematicId);
					ProcColoredIconHelper.SetProcColoredIconWithoutColor(ref _coloredIconMaterial);
					ProcColoredIconHelper.SetAspectRatio(_icon.gameObject, ref _iconAspectRatioFitter, itemData);
					_icon.material = _coloredIconMaterial;
					_icon.preserveAspect = false;
				}
				else
				{
					_icon.preserveAspect = true;
					_icon.material = null;
				}
				break;
			case CraftingCategory.Shipyard:
				sprite = InventoryIconManager.Instance.GetIconSprite("shipyard_placeholder_icon");
				_icon.material = null;
				break;
			default:
				sprite = null;
				break;
			}
			_icon.sprite = sprite;
			_icon.enabled = true;
		}
		SetName(data.GetFormattedTitle());
		SchematicsRarity rarityParsed = data.rarityParsed;
		_uiButtonController.SetRarityColourSet(RarityHelper.GetRarityColoursForButtonStates(rarityParsed));
	}

	public virtual void RefreshName()
	{
		SetName(SchemData.GetFormattedTitle());
	}

	protected virtual void SetName(string name)
	{
		CurrentTitle = name;
		_uiButtonController.SetText(name);
	}

	public SchematicData GetData()
	{
		return SchemData;
	}
}
