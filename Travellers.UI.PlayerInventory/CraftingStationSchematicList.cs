using System.Collections.Generic;
using System.Linq;
using Bossa.Travellers.Materials;
using Newtonsoft.Json.Linq;
using Travellers.UI.Framework;
using Travellers.UI.Models;
using Travellers.UI.Tutorial;
using Travellers.UI.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class CraftingStationSchematicList : SchematicList
{
	[SerializeField]
	private InputField _searchField;

	[SerializeField]
	private UIButtonController _filterButtonController;

	[SerializeField]
	private UIFilterRarityToggleController _commonRarityToggle;

	[SerializeField]
	private UIFilterRarityToggleController _uncommonRarityToggle;

	[SerializeField]
	private UIFilterRarityToggleController _rareRarityToggle;

	[SerializeField]
	private UIFilterRarityToggleController _exoticRarityToggle;

	[SerializeField]
	private GameObject _rarityDropDownGO;

	[SerializeField]
	private ScrollRect _schematicScroller;

	private List<SchematicCategorySlot> _allCategorySlots = new List<SchematicCategorySlot>();

	private List<SchematicCategorySlot> _activeCategorySlots = new List<SchematicCategorySlot>();

	private List<SchematicsRarity> _raritiesToFilterBy = new List<SchematicsRarity>();

	private string _stringToFilterBy = string.Empty;

	private InventorySystem _inventorySystem;

	private bool _localDataTemplate;

	private ISchematicSystem _schematicSystem;

	[InjectableMethod]
	public void InjectDependencies(InventorySystem inventorySystem, ISchematicSystem schematicSystem)
	{
		_schematicSystem = schematicSystem;
		_inventorySystem = inventorySystem;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInventoryEvents.InventoryItemPickedUp, OnInventoryItemPickedUp);
	}

	protected override void ProtectedInit()
	{
		_commonRarityToggle.SetButtonEvent(delegate
		{
			OnFilterSchematicByRarityPressed(SchematicsRarity.Tier1);
		});
		_commonRarityToggle.SetRarityColourSet(RarityHelper.GetRarityColoursForButtonStates(SchematicsRarity.Tier1));
		_uncommonRarityToggle.SetButtonEvent(delegate
		{
			OnFilterSchematicByRarityPressed(SchematicsRarity.Tier2);
		});
		_uncommonRarityToggle.SetRarityColourSet(RarityHelper.GetRarityColoursForButtonStates(SchematicsRarity.Tier2));
		_rareRarityToggle.SetButtonEvent(delegate
		{
			OnFilterSchematicByRarityPressed(SchematicsRarity.Tier3);
		});
		_rareRarityToggle.SetRarityColourSet(RarityHelper.GetRarityColoursForButtonStates(SchematicsRarity.Tier3));
		_exoticRarityToggle.SetButtonEvent(delegate
		{
			OnFilterSchematicByRarityPressed(SchematicsRarity.Tier4);
		});
		_exoticRarityToggle.SetRarityColourSet(RarityHelper.GetRarityColoursForButtonStates(SchematicsRarity.Tier4));
		_filterButtonController.SetButtonEvent(OnFilterSchematicButtonPressed);
		_searchField.onValueChanged.AddListener(OnSearchBarValueChanged);
	}

	protected override void Activate()
	{
		SetStringFilterActive(isEnabled: true);
	}

	private void OnFilterSchematicButtonPressed()
	{
		if (_allItemSchematicData.Count != 0)
		{
			SetRarityFilterActive(!_rarityDropDownGO.activeSelf);
		}
	}

	private void OnInventoryItemPickedUp(object[] obj)
	{
		if (_rarityDropDownGO.activeSelf)
		{
			SetRarityFilterActive(isEnabled: false);
		}
	}

	private void OnFilterSchematicByRarityPressed(SchematicsRarity rarity)
	{
		if (_raritiesToFilterBy.Contains(rarity))
		{
			_raritiesToFilterBy.Remove(rarity);
		}
		else
		{
			_raritiesToFilterBy.Add(rarity);
		}
		ApplyFilters();
	}

	private void OnSearchBarValueChanged(string newString)
	{
		_stringToFilterBy = newString;
		ApplyFilters();
	}

	public override void SetCraftingDataTemplate(CraftingStationData craftingDataTemplate, bool clientOnly = false)
	{
		_localDataTemplate = clientOnly;
		_craftingDataTemplate = craftingDataTemplate;
		TurnOffFilters();
		RefreshSchematicDisplay();
	}

	public override void RefreshSchematicDisplay()
	{
		if (_craftingDataTemplate != null)
		{
			BuildSchematicButtonHierarchy();
			CheckIfSchematicLoaded();
			ApplyFilters();
		}
	}

	protected override void BuildSchematicButtonHierarchy()
	{
		if (_craftingDataTemplate == null)
		{
			return;
		}
		List<SchematicCategoryData> schematicCategoryData = _craftingDataTemplate.SchematicCategoryData;
		_activeCategorySlots.Clear();
		_allItemSchematicData.Clear();
		for (int i = 0; i < schematicCategoryData.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < _allCategorySlots.Count; j++)
			{
				if (_allCategorySlots[j].CategoryData.CategoryTypeEnum == schematicCategoryData[i].CategoryTypeEnum)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				SchematicCategorySlot schematicCategorySlot = UIObjectFactory.Create<SchematicCategorySlot>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, _schematicParent, isObjectActive: true);
				SetupSchematicCategory(schematicCategorySlot, schematicCategoryData[i]);
				_allCategorySlots.Add(schematicCategorySlot);
			}
		}
		Dictionary<CraftingCategory, SchematicCategoryData> dictionary = schematicCategoryData.ToDictionary((SchematicCategoryData x) => x.CategoryTypeEnum, (SchematicCategoryData x) => x);
		for (int k = 0; k < _allCategorySlots.Count; k++)
		{
			SchematicCategoryData value = null;
			if (!dictionary.TryGetValue(_allCategorySlots[k].CategoryData.CategoryTypeEnum, out value))
			{
				_allCategorySlots[k].HideElement();
				continue;
			}
			SetupSchematicCategory(_allCategorySlots[k], value);
			_activeCategorySlots.Add(_allCategorySlots[k]);
		}
	}

	private void SetupSchematicCategory(SchematicCategorySlot categorySlot, SchematicCategoryData schematicCatData)
	{
		categorySlot.PopulateData(this, schematicCatData);
		categorySlot.BuildHierarchy();
		categorySlot.ShowElement();
		foreach (KeyValuePair<string, SchematicCategoryData> childSchematicCategory in schematicCatData.ChildSchematicCategories)
		{
			foreach (SchematicData childItemSchematic in childSchematicCategory.Value.ChildItemSchematics)
			{
				_allItemSchematicData.Add(childItemSchematic);
			}
		}
		foreach (SchematicData childShipSchematic in schematicCatData.ChildShipSchematics)
		{
			_allItemSchematicData.Add(childShipSchematic);
		}
		if (categorySlot.CategoryData.CategoryTypeEnum == CraftingCategory.Personal)
		{
			TutorialTransformAnchor componentInChildren = categorySlot.GetComponentInChildren<TutorialTransformAnchor>(includeInactive: true);
			componentInChildren.ReRegisterAnchor(TutorialHookType.Schematics_PersonalCraftingCategory);
		}
		if (categorySlot.CategoryData.CategoryTypeEnum == CraftingCategory.CraftingStation)
		{
			TutorialTransformAnchor componentInChildren2 = categorySlot.GetComponentInChildren<TutorialTransformAnchor>(includeInactive: true);
			componentInChildren2.ReRegisterAnchor(TutorialHookType.Schematics_AssemblyStationCraftingCategory);
		}
	}

	private void SetRarityFilterActive(bool isEnabled)
	{
		_rarityDropDownGO.SetActive(isEnabled);
	}

	private void SetStringFilterActive(bool isEnabled)
	{
		_searchField.interactable = isEnabled;
		if (!isEnabled)
		{
			_searchField.text = string.Empty;
		}
	}

	public override void CraftingStarted()
	{
		UpdateCraftingState();
	}

	public override void CraftingFinished()
	{
		UpdateCraftingState();
	}

	private void TurnOffFilters()
	{
		_commonRarityToggle.SetToggleSelected(isSelected: true);
		_uncommonRarityToggle.SetToggleSelected(isSelected: true);
		_rareRarityToggle.SetToggleSelected(isSelected: true);
		_exoticRarityToggle.SetToggleSelected(isSelected: true);
		_rarityDropDownGO.SetActive(value: false);
		_raritiesToFilterBy.Clear();
		_raritiesToFilterBy.AddRange(SchematicsReferenceStore.AllSchematicRarities);
		_stringToFilterBy = string.Empty;
	}

	public override void UpdateCraftingState()
	{
		if (IsCurrentlyCrafting())
		{
			ChangeState(CraftingState.CurrentlyCrafting);
		}
		else
		{
			ChangeState(CraftingState.FreeToUse);
		}
	}

	private void CheckIfSchematicLoaded()
	{
		if (_craftingDataTemplate.LoadedSchematic != null)
		{
			switch (_craftingDataTemplate.UITabType)
			{
			case CharacterSheetTabType.ItemCraft:
				if (_schematicSystem.PlayerHasLearnedSchematic(_craftingDataTemplate.LoadedSchematic.UniqueID))
				{
					SelectSchematic(_craftingDataTemplate.LoadedSchematic);
					break;
				}
				foreach (SchematicCategorySlot activeCategorySlot in _activeCategorySlots)
				{
					activeCategorySlot.DeselectElement();
				}
				break;
			case CharacterSheetTabType.MultitoolCraft:
				SelectSchematic(_craftingDataTemplate.LoadedSchematic);
				break;
			}
		}
		else
		{
			switch (_craftingDataTemplate.UITabType)
			{
			case CharacterSheetTabType.ItemCraft:
				ChangeState(CraftingState.NoSchematic);
				break;
			case CharacterSheetTabType.MultitoolCraft:
			case CharacterSheetTabType.Schematics:
				SelectFirstAvailableSlot();
				break;
			}
		}
		UpdateCraftingState();
	}

	protected override void ApplyCraftingState()
	{
		switch (_currentState)
		{
		case CraftingState.CurrentlyCrafting:
			_inputBlocker.SetActive(value: true);
			break;
		case CraftingState.EditingShip:
			_inputBlocker.SetActive(value: false);
			break;
		case CraftingState.FreeToUse:
			_inputBlocker.SetActive(value: false);
			break;
		case CraftingState.NoSchematic:
			_inputBlocker.SetActive(value: false);
			break;
		case CraftingState.ShipDocked:
			_inputBlocker.SetActive(value: true);
			break;
		}
	}

	public override SchematicCategorySlot CategoryPressed(CraftingCategory category, bool forceShow = false)
	{
		SchematicCategorySlot result = null;
		foreach (SchematicCategorySlot activeCategorySlot in _activeCategorySlots)
		{
			if (activeCategorySlot.CategoryData.CategoryTypeEnum == category)
			{
				result = activeCategorySlot;
				if (forceShow || !activeCategorySlot.IsSelectedCategory)
				{
					activeCategorySlot.SelectElement();
				}
				else
				{
					activeCategorySlot.DeselectElement();
				}
			}
			else
			{
				activeCategorySlot.DeselectElement();
			}
		}
		return result;
	}

	public override void SubcategoryPressed(CraftingCategory category, string subCategory, bool forceShow = false)
	{
		SchematicCategorySlot schematicCategorySlot = CategoryPressed(category, forceShow: true);
		schematicCategorySlot.SetSubcategoryActive(subCategory, forceShow);
	}

	public override void SchematicButtonPressed()
	{
		_rarityDropDownGO.SetActive(value: false);
		SelectSchematic(_craftingDataTemplate.LoadedSchematic);
	}

	private void SelectSchematic(SchematicData schematic)
	{
		if (schematic != null)
		{
			if (_currentSchematic != null && schematic.UniqueID != _currentSchematic.UniqueID)
			{
				DeselectSchematic(_currentSchematic);
			}
			SchematicCategorySlot schematicCategorySlot = CategoryPressed(schematic.CraftingCategoryEnum, forceShow: true);
			if (schematic.CraftingCategoryEnum == CraftingCategory.Shipyard)
			{
				schematicCategorySlot.SetShipSchematicActive(schematic.UniqueID);
			}
			else
			{
				SchematicSubCategorySlot schematicSubCategorySlot = schematicCategorySlot.SetSubcategoryActive(schematic.itemType, forceShow: true);
				schematicSubCategorySlot.SelectChildElement(schematic.UniqueID);
			}
			_currentSchematic = schematic;
		}
	}

	private void DeselectSchematic(SchematicData schematic)
	{
		SchematicCategorySlot category = GetCategory(schematic.CraftingCategoryEnum);
		category.DeselectSubcategoryAndHideChildren(schematic.itemType);
	}

	private SchematicCategorySlot GetCategory(CraftingCategory category)
	{
		foreach (SchematicCategorySlot allCategorySlot in _allCategorySlots)
		{
			if (allCategorySlot.CategoryData.CategoryTypeEnum == category)
			{
				return allCategorySlot;
			}
		}
		return null;
	}

	private void SelectFirstAvailableSlot()
	{
		SchematicData schematicData = null;
		for (int i = 0; i < _allItemSchematicData.Count; i++)
		{
			SchematicData schematicData2 = _allItemSchematicData[i];
			if (GetItemFitsFilters(schematicData2))
			{
				schematicData = schematicData2;
				break;
			}
		}
		SelectSchematic(schematicData);
		_craftingDataTemplate.LoadSchematic(schematicData, CharacterData.Instance.userName);
		if (!_localDataTemplate)
		{
			_inventorySystem.SendNewSchematicToServer();
		}
	}

	private void ApplyFilters()
	{
		foreach (SchematicCategorySlot activeCategorySlot in _activeCategorySlots)
		{
			if (activeCategorySlot.IsSelectedCategory)
			{
				activeCategorySlot.ApplyFilters();
			}
		}
		_schematicScroller.verticalNormalizedPosition = 1f;
	}

	public bool GetItemFitsFilters(SchematicData schematic)
	{
		if (schematic == null)
		{
			return false;
		}
		if (!_raritiesToFilterBy.Contains(schematic.rarityParsed))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(_stringToFilterBy) && !schematic.title.ToLower().Contains(_stringToFilterBy))
		{
			return false;
		}
		return true;
	}

	public JToken SerialiseList()
	{
		JArray jArray = new JArray();
		foreach (SchematicData allItemSchematicDatum in _allItemSchematicData)
		{
			JObject jObject = new JObject();
			jObject.Add("data", JObject.FromObject(allItemSchematicDatum));
			jArray.Add(jObject);
		}
		return jArray;
	}

	protected override void ProtectedDispose()
	{
	}
}
