using System.Collections.Generic;
using System.Linq;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.Models;
using Travellers.UI.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class SchematicCategorySlot : UIScreenComponent, ICompositeHierarchyElement, IHierarchyElement
{
	public SchematicCategoryData CategoryData;

	[SerializeField]
	private UIButtonController _uiButtonController;

	[SerializeField]
	private TextStyler _categoryName;

	[SerializeField]
	private TextStyler _learntSchematics;

	[SerializeField]
	private Transform _schematicsRoot;

	[SerializeField]
	private Image _rotatingButton;

	private List<SchematicSubCategorySlot> _allChildSubcategory = new List<SchematicSubCategorySlot>();

	private List<SchematicSubCategorySlot> _activeChildSubcategory = new List<SchematicSubCategorySlot>();

	private List<SchematicSlot> _craftingStationShipSchematicSlots = new List<SchematicSlot>();

	private CraftingStationSchematicList _schematicList;

	private ISchematicSystem _schematicSystem;

	public bool IsSelectedCategory { get; private set; }

	[InjectableMethod]
	public void InjectDependencies(ISchematicSystem schematicSystem)
	{
		_schematicSystem = schematicSystem;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	public void PopulateData(CraftingStationSchematicList schematicList, SchematicCategoryData catData)
	{
		_schematicList = schematicList;
		CategoryData = catData;
		_categoryName.SetText(catData.Description);
		_uiButtonController.SetButtonEvent(WAUIButtonEvents.SchematicCategoryPressed, new SchematicCategoryListItemPressed(catData.CategoryTypeEnum));
		SetLearntSchematicsLabel();
	}

	public void BuildHierarchy()
	{
		BuildItemHierarchy();
		BuildShipHierarchy();
	}

	private void BuildItemHierarchy()
	{
		List<SchematicCategoryData> list = CategoryData.ChildSchematicCategories.Values.ToList();
		_activeChildSubcategory.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			SchematicSubCategorySlot schematicSubCategorySlot;
			if (_allChildSubcategory.Count <= i)
			{
				schematicSubCategorySlot = UIObjectFactory.Create<SchematicSubCategorySlot>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, _schematicsRoot, isObjectActive: true);
				_allChildSubcategory.Add(schematicSubCategorySlot);
			}
			else
			{
				schematicSubCategorySlot = _allChildSubcategory[i];
			}
			schematicSubCategorySlot.PopulateData(_schematicList, list[i]);
			schematicSubCategorySlot.BuildHierarchy();
			_activeChildSubcategory.Add(schematicSubCategorySlot);
		}
		if (_allChildSubcategory.Count > list.Count)
		{
			for (int j = list.Count; j < _allChildSubcategory.Count; j++)
			{
				SchematicSubCategorySlot schematicSubCategorySlot2 = _allChildSubcategory[j];
				schematicSubCategorySlot2.DeselectElement();
				schematicSubCategorySlot2.HideElement();
			}
		}
	}

	private void BuildShipHierarchy()
	{
		List<SchematicData> childShipSchematics = CategoryData.ChildShipSchematics;
		for (int i = 0; i < childShipSchematics.Count; i++)
		{
			if (_craftingStationShipSchematicSlots.Count > i)
			{
				_craftingStationShipSchematicSlots[i].PopulateData(childShipSchematics[i]);
				continue;
			}
			SchematicSlot schematicSlot = UIObjectFactory.Create<SchematicSlot>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, _schematicsRoot, isObjectActive: true);
			schematicSlot.PopulateData(childShipSchematics[i]);
			_craftingStationShipSchematicSlots.Add(schematicSlot);
		}
		if (_craftingStationShipSchematicSlots.Count > childShipSchematics.Count)
		{
			for (int j = childShipSchematics.Count; j < _craftingStationShipSchematicSlots.Count; j++)
			{
				_craftingStationShipSchematicSlots[j].HideElement();
			}
		}
	}

	public void SelectElement()
	{
		IsSelectedCategory = true;
		_rotatingButton.rectTransform.eulerAngles = new Vector3(0f, 0f, 0f);
		_uiButtonController.SetButtonSelected(selected: true);
		_schematicsRoot.gameObject.SetActive(value: true);
		foreach (SchematicSubCategorySlot item in _activeChildSubcategory)
		{
			int num = item.ApplyFilters();
			if (num > 0)
			{
				item.ShowElement();
				if (!item.IsSelectedSchematicSubCat)
				{
					item.DeselectChildren();
					item.DeselectElement();
				}
			}
			else
			{
				item.HideElement();
			}
		}
		foreach (SchematicSlot craftingStationShipSchematicSlot in _craftingStationShipSchematicSlots)
		{
			craftingStationShipSchematicSlot.ShowElement();
		}
		TriggerTutorialPopups();
	}

	public void DeselectElement()
	{
		IsSelectedCategory = false;
		_rotatingButton.rectTransform.eulerAngles = new Vector3(0f, 0f, 90f);
		_uiButtonController.SetButtonSelected(selected: false);
		_schematicsRoot.gameObject.SetActive(value: false);
		foreach (SchematicSubCategorySlot item in _activeChildSubcategory)
		{
			item.HideElement();
		}
		foreach (SchematicSlot craftingStationShipSchematicSlot in _craftingStationShipSchematicSlots)
		{
			craftingStationShipSchematicSlot.HideElement();
		}
	}

	public void HideElement()
	{
		SetObjectActive(isActive: false);
	}

	public void ShowElement()
	{
		SetObjectActive(isActive: true);
	}

	private void TriggerTutorialPopups()
	{
		switch (CategoryData.CategoryTypeEnum)
		{
		case CraftingCategory.Personal:
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.SCHEMATICS_TAB_ACTIVE));
			break;
		case CraftingCategory.CraftingStation:
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.ASSEMBLY_STATION_TAB_CLICKED));
			break;
		case CraftingCategory.Shipyard:
			break;
		}
	}

	public SchematicSubCategorySlot SetSubcategoryActive(string subcategoryId, bool forceShow)
	{
		SchematicSubCategorySlot schematicSubCategorySlot = null;
		foreach (SchematicSubCategorySlot item in _activeChildSubcategory)
		{
			if (item.SubCategoryData.SubCategoryId == subcategoryId)
			{
				schematicSubCategorySlot = item;
				if (forceShow || !schematicSubCategorySlot.IsSelectedSchematicSubCat)
				{
					schematicSubCategorySlot.SelectElement();
				}
				else
				{
					schematicSubCategorySlot.DeselectElement();
				}
			}
			else
			{
				item.DeselectElement();
				if (item.ApplyFilters(showChildElements: false) == 0)
				{
					item.HideElement();
				}
			}
		}
		return schematicSubCategorySlot;
	}

	public void SetShipSchematicActive(string shipUUid)
	{
		foreach (SchematicSlot craftingStationShipSchematicSlot in _craftingStationShipSchematicSlots)
		{
			if (craftingStationShipSchematicSlot.GetData().UniqueID == shipUUid)
			{
				craftingStationShipSchematicSlot.SelectElement();
			}
			else
			{
				craftingStationShipSchematicSlot.DeselectElement();
			}
		}
	}

	public void DeselectSubcategoryAndHideChildren(string subcategoryId)
	{
		foreach (SchematicSubCategorySlot item in _activeChildSubcategory)
		{
			if (item.SubCategoryData.SubCategoryId == subcategoryId)
			{
				item.DeselectChildren();
				item.DeselectElement();
				break;
			}
		}
	}

	public void ApplyFilters()
	{
		for (int i = 0; i < _activeChildSubcategory.Count; i++)
		{
			SchematicSubCategorySlot schematicSubCategorySlot = _activeChildSubcategory[i];
			int num = schematicSubCategorySlot.ApplyFilters(schematicSubCategorySlot.IsSelectedSchematicSubCat);
			if (num > 0)
			{
				schematicSubCategorySlot.ShowElement();
			}
			else
			{
				schematicSubCategorySlot.HideElement();
			}
		}
	}

	private void SetLearntSchematicsLabel()
	{
		if (!(_learntSchematics != null))
		{
			return;
		}
		switch (CategoryData.CategoryTypeEnum)
		{
		case CraftingCategory.Personal:
		case CraftingCategory.CraftingStation:
		case CraftingCategory.Cooking:
		case CraftingCategory.Clothing:
			CategoryData.AmountOwned = 0f;
			foreach (KeyValuePair<string, SchematicCategoryData> childSchematicCategory in CategoryData.ChildSchematicCategories)
			{
				foreach (SchematicData childItemSchematic in childSchematicCategory.Value.ChildItemSchematics)
				{
					if (childItemSchematic.unlearnable)
					{
						CategoryData.AmountOwned += 1f;
					}
				}
			}
			CategoryData.TotalAmount = _schematicSystem.SchematicsCapPerCategory(CategoryData.CategoryTypeEnum);
			if (CategoryData.TotalAmount > 0f)
			{
				_learntSchematics.SetText($"{CategoryData.AmountOwned}/{CategoryData.TotalAmount}");
			}
			else
			{
				_learntSchematics.SetText($"{CategoryData.AmountOwned}");
			}
			break;
		case CraftingCategory.Shipyard:
			_learntSchematics.SetText($"{CategoryData.ChildShipSchematics.Count}");
			break;
		}
	}

	protected override void ProtectedDispose()
	{
	}
}
