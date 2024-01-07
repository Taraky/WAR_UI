using System.Collections.Generic;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class SchematicSubCategorySlot : UIScreenComponent, ICompositeHierarchyElement, IHierarchyElement
{
	public SchematicCategoryData SubCategoryData;

	[SerializeField]
	private TextStyler _categoryName;

	[SerializeField]
	private GameObject _listRoot;

	[SerializeField]
	private UIButtonController _uiButtonController;

	[SerializeField]
	private Image _rotatingButton;

	private List<SchematicSlot> _allChildSchematicSlots = new List<SchematicSlot>();

	private List<SchematicSlot> _activeChildSchematicSlots = new List<SchematicSlot>();

	private CraftingStationSchematicList _schematicList;

	public bool IsSelectedSchematicSubCat { get; private set; }

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	public void PopulateData(CraftingStationSchematicList schematicList, SchematicCategoryData catData)
	{
		_schematicList = schematicList;
		SubCategoryData = catData;
		_categoryName.SetText(catData.SubCategoryDisplayName);
		_uiButtonController.SetButtonEvent(WAUIButtonEvents.SchematicSubCategoryPressed, new SchematicCategoryListItemPressed(catData.CategoryTypeEnum, catData.SubCategoryId));
		if (IsSelectedSchematicSubCat)
		{
			SelectElement();
		}
		else
		{
			DeselectElement();
		}
	}

	public void BuildHierarchy()
	{
		List<SchematicData> childItemSchematics = SubCategoryData.ChildItemSchematics;
		_activeChildSchematicSlots.Clear();
		for (int i = 0; i < childItemSchematics.Count; i++)
		{
			SchematicSlot schematicSlot;
			if (_allChildSchematicSlots.Count <= i)
			{
				schematicSlot = UIObjectFactory.Create<SchematicSlot>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, _listRoot.transform, isObjectActive: true);
				_allChildSchematicSlots.Add(schematicSlot);
			}
			else
			{
				schematicSlot = _allChildSchematicSlots[i];
			}
			schematicSlot.PopulateData(childItemSchematics[i]);
			_activeChildSchematicSlots.Add(schematicSlot);
		}
		if (_allChildSchematicSlots.Count > childItemSchematics.Count)
		{
			for (int j = childItemSchematics.Count; j < _allChildSchematicSlots.Count; j++)
			{
				SchematicSlot schematicSlot2 = _allChildSchematicSlots[j];
				schematicSlot2.DeselectElement();
				schematicSlot2.HideElement();
			}
		}
	}

	public void SelectElement()
	{
		IsSelectedSchematicSubCat = true;
		_rotatingButton.rectTransform.eulerAngles = new Vector3(0f, 0f, 0f);
		SetObjectActive(isActive: true);
		_uiButtonController.SetButtonSelected(selected: true);
		_listRoot.gameObject.SetActive(value: true);
		foreach (SchematicSlot activeChildSchematicSlot in _activeChildSchematicSlots)
		{
			if (_schematicList.GetItemFitsFilters(activeChildSchematicSlot.SchemData))
			{
				activeChildSchematicSlot.ShowElement();
			}
			else
			{
				activeChildSchematicSlot.HideElement();
			}
		}
	}

	public void DeselectElement()
	{
		IsSelectedSchematicSubCat = false;
		_rotatingButton.rectTransform.eulerAngles = new Vector3(0f, 0f, 90f);
		SetObjectActive(isActive: true);
		_uiButtonController.SetButtonSelected(selected: false);
		foreach (SchematicSlot activeChildSchematicSlot in _activeChildSchematicSlots)
		{
			activeChildSchematicSlot.HideElement();
		}
		_listRoot.gameObject.SetActive(value: false);
	}

	public void DeselectChildren()
	{
		foreach (SchematicSlot activeChildSchematicSlot in _activeChildSchematicSlots)
		{
			activeChildSchematicSlot.DeselectElement();
		}
	}

	public void HideElement()
	{
		SetObjectActive(isActive: false);
	}

	public void ShowElement()
	{
		SetObjectActive(isActive: true);
		_uiButtonController.SetButtonSelected(IsSelectedSchematicSubCat);
	}

	public void SelectChildElement(string id)
	{
		foreach (SchematicSlot activeChildSchematicSlot in _activeChildSchematicSlots)
		{
			if (activeChildSchematicSlot.GetData().UniqueID == id)
			{
				activeChildSchematicSlot.SelectElement();
			}
			else
			{
				activeChildSchematicSlot.DeselectElement();
			}
		}
	}

	public int ApplyFilters(bool showChildElements = true)
	{
		int num = 0;
		for (int i = 0; i < _activeChildSchematicSlots.Count; i++)
		{
			SchematicSlot schematicSlot = _activeChildSchematicSlots[i];
			if (_schematicList.GetItemFitsFilters(schematicSlot.SchemData))
			{
				num++;
				if (showChildElements)
				{
					schematicSlot.ShowElement();
				}
			}
			else
			{
				schematicSlot.HideElement();
			}
		}
		return num;
	}

	protected override void ProtectedDispose()
	{
	}
}
