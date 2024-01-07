using System.Collections.Generic;
using System.Linq;
using Bossa.Travellers.Craftingstation;
using Bossa.Travellers.Materials;
using Bossa.Travellers.Utils.ErrorHandling;
using Bossa.Travellers.World;
using GameDBLocalization;
using Improbable.Collections;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.InfoPopups;
using Travellers.UI.Models;
using Travellers.UI.Utility;
using UnityEngine;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class SchematicsSubScreen : UIScreenComponent
{
	public const int NumGSimMaterialSlots = 6;

	[SerializeField]
	protected RawImage _mainSchematicIconRawImage;

	[SerializeField]
	protected GameObject _noSchematicWarning;

	[SerializeField]
	private GameObject _schematicsPresentParent;

	[SerializeField]
	protected TextStyler _schematicDescription;

	[SerializeField]
	protected CraftingMaterialSlot[] _schematicsOnlySlots;

	[SerializeField]
	protected ItemAttribute[] _schematicAttributeBars;

	[SerializeField]
	private CraftingStationSchematicList _craftingStationSchematicList;

	[SerializeField]
	protected Image _mainSchematicIcon;

	[SerializeField]
	private TextStylerTextMeshPro _schematicTitle;

	[SerializeField]
	private UIButtonController _unlearnButton;

	[SerializeField]
	private TextStylerTextMeshPro _instructionText;

	[SerializeField]
	private CipherUiSlot[] _cipherUiSlots;

	[SerializeField]
	private GameObject _cipherPanel;

	private Material _coloredIconMaterial;

	private AspectRatioFitter _mainIconAspectFitter;

	private string _tabName;

	private CraftingStationData _craftingStationData;

	private ISchematicSystem _schematicSystem;

	private SchematicSlot _currentSelectedSlot;

	[InjectableMethod]
	public void InjectDependencies(ISchematicSystem schematicSystem)
	{
		_schematicSystem = schematicSystem;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInventoryEvents.SchematicHierarchyChanged, OnSchematicHierarchyChanged);
		_eventList.AddEvent(WAUIButtonEvents.SchematicItemPressed, OnSchematicItemButtonPressed);
		_eventList.AddEvent(WAUIButtonEvents.SchematicCategoryPressed, OnCategoryButtonPressed);
		_eventList.AddEvent(WAUIButtonEvents.SchematicSubCategoryPressed, OnSubCategoryButtonPressed);
		_eventList.AddEvent(WAUIButtonEvents.UnlearnSchematic, OnUnlearnSchematicPressed);
	}

	protected override void ProtectedInit()
	{
		_muteListenersWhenInactive = true;
		_unlearnButton.SetButtonEvent(UnlearnSchematic);
		_unlearnButton.SetButtonEnabled(isShowing: true);
	}

	protected override void ProtectedDispose()
	{
	}

	protected override void Activate()
	{
		_craftingStationData = new CraftingStationData(CharacterSheetTabType.Schematics);
		_schematicSystem.RebuildCraftingDataSchematicHierarchy(_craftingStationData);
		_craftingStationSchematicList.SetCraftingDataTemplate(_craftingStationData);
		LoadSchematicFromCraftingData();
	}

	private void OnSchematicHierarchyChanged(object[] obj)
	{
		UpdateLoadedSchematic();
	}

	private void OnSchematicItemButtonPressed(object[] obj)
	{
		SchematicCategoryListItemPressed schematicCategoryListItemPressed = obj[0] as SchematicCategoryListItemPressed;
		_currentSelectedSlot = schematicCategoryListItemPressed.SchematicSlot;
		_craftingStationData.LoadSchematic(schematicCategoryListItemPressed.Schematic, CharacterData.Instance.userName);
		LoadSchematicFromCraftingData();
		_craftingStationSchematicList.SchematicButtonPressed();
	}

	private void OnCategoryButtonPressed(object[] obj)
	{
		SchematicCategoryListItemPressed schematicCategoryListItemPressed = obj[0] as SchematicCategoryListItemPressed;
		_craftingStationSchematicList.CategoryPressed(schematicCategoryListItemPressed.CategoryTypeEnum);
	}

	private void OnSubCategoryButtonPressed(object[] obj)
	{
		SchematicCategoryListItemPressed schematicCategoryListItemPressed = obj[0] as SchematicCategoryListItemPressed;
		_craftingStationSchematicList.SubcategoryPressed(schematicCategoryListItemPressed.CategoryTypeEnum, schematicCategoryListItemPressed.SubCategoryId);
	}

	private void OnUnlearnSchematicPressed(object[] obj)
	{
		UnlearnSchematic();
	}

	public void UnlearnSchematic()
	{
		if (_craftingStationData.LoadedSchematic.unlearnable)
		{
			DialogPopupFacade.ShowConfirmationDialog("Unlearn Schematic", $"Are you sure you want to unlearn {_craftingStationData.LoadedSchematic.title}?", delegate
			{
				string dataIdToUnlearn = _craftingStationData.LoadedSchematic.UniqueID;
				string text = null;
				string itemType = _craftingStationData.LoadedSchematic.itemType;
				CraftingCategory categoryType = _craftingStationData.LoadedSchematic.CraftingCategoryEnum;
				SchematicCategoryData schematicCategoryData = _craftingStationData.SchematicCategoryData.Find((SchematicCategoryData c) => c.CategoryTypeEnum == categoryType);
				SchematicCategoryData schematicCategoryData2 = schematicCategoryData.ChildSchematicCategories[itemType];
				if (schematicCategoryData2.ChildItemSchematics.Count > 1)
				{
					SchematicData schematicData = schematicCategoryData2.ChildItemSchematics.Find((SchematicData i) => i.UniqueID != dataIdToUnlearn);
					text = schematicData.referenceData;
					if (text == null)
					{
						text = schematicData.UniqueID;
					}
				}
				_craftingStationData.SetLastSchematic(text);
				_schematicSystem.UnlearnSchematic(dataIdToUnlearn);
				CheckUnlearnState();
			});
		}
		else
		{
			WALogger.Warn<CraftingUI>("Trying to unlearn non unlearnable schematic " + _craftingStationData.LoadedSchematic.schematicId);
			OSDMessage.SendMessage("That schematic cannot be unlearnt.", MessageType.PlayerDeath);
		}
	}

	public void UpdateLoadedSchematic()
	{
		_craftingStationSchematicList.RefreshSchematicDisplay();
		CheckIfSchematicLoaded();
	}

	public void CheckIfSchematicLoaded()
	{
		if (_craftingStationData.LoadedSchematic != null)
		{
			_craftingStationData.LoadSchematic(_currentSelectedSlot.SchemData, CharacterData.Instance.userName);
			LoadSchematicFromCraftingData();
		}
		else
		{
			_noSchematicWarning.SetActive(value: true);
		}
	}

	public void LoadSchematicFromCraftingData()
	{
		SetupSlots();
		CheckIcon();
		SetName();
		SetDescription();
		SetAttributes();
		SetInstruction();
		CheckUnlearnState();
		SetCipherSlot();
		_noSchematicWarning.SetActive(value: false);
	}

	protected virtual void SetupSlots()
	{
		for (int i = 0; i < _schematicsOnlySlots.Length; i++)
		{
			_schematicsOnlySlots[i].SlotIndex = i;
			if (i < _craftingStationData.CraftingSlotData.Count)
			{
				_schematicsOnlySlots[i].SetNewCraftingData(_craftingStationData, _craftingStationData.CraftingSlotData[i], HighlightAttributes, LowLightAttributes);
			}
			else
			{
				_schematicsOnlySlots[i].SetNewCraftingData(null, null, null, null);
			}
		}
	}

	private void CheckIcon()
	{
		if (_craftingStationData.CraftingSlotData != null && _craftingStationData.CraftingSlotData.Count != 0)
		{
			SchematicData loadedSchematic = _craftingStationData.LoadedSchematic;
			if (loadedSchematic.IsProcedural)
			{
				SetProceduralIcon(loadedSchematic);
			}
			else
			{
				SetNonProceduralIcon(loadedSchematic);
			}
		}
	}

	protected void SetName()
	{
		SchematicsRarity rarityParsed = _craftingStationData.LoadedSchematic.rarityParsed;
		RarityColourSet rarityColoursForButtonStates = RarityHelper.GetRarityColoursForButtonStates(rarityParsed);
		_schematicTitle.SetRarityColourSet(rarityColoursForButtonStates);
		_schematicTitle.SetText(_craftingStationData.LoadedSchematic.GetFormattedTitle());
	}

	protected void SetNonProceduralIcon(SchematicData schematic)
	{
		_mainSchematicIconRawImage.enabled = false;
		_mainSchematicIcon.enabled = true;
		string empty = string.Empty;
		int num = 0;
		int num2 = 0;
		if (_craftingStationData.CraftingSlotData[0].inventoryData != null)
		{
			if (_craftingStationData.CraftingSlotData[0].inventoryData.category == "Wood")
			{
				num2 += _craftingStationData.CraftingSlotData[0].CurrentAmount;
			}
			else if (_craftingStationData.CraftingSlotData[0].inventoryData.category == "Metal")
			{
				num += _craftingStationData.CraftingSlotData[0].CurrentAmount;
			}
		}
		empty = ((num < num2) ? _craftingStationData.LoadedSchematic.iconId.Replace("Metal", "Wood") : _craftingStationData.LoadedSchematic.iconId);
		if (_craftingStationData.LoadedSchematic.SchematicType == SchematicType.Ship)
		{
			empty = "shipyard_placeholder_icon";
		}
		if (!string.IsNullOrEmpty(empty))
		{
			Sprite iconSprite = InventoryIconManager.Instance.GetIconSprite(empty);
			_mainSchematicIcon.enabled = true;
			_mainSchematicIcon.sprite = iconSprite;
			if (ProcColoredIconHelper.ShouldUseProcColoredShader(_craftingStationData.LoadedSchematic))
			{
				InventoryItemData itemData = InventoryItemManager.Instance.LookupItem(_craftingStationData.LoadedSchematic.schematicId);
				ProcColoredIconHelper.SetProcColoredIconWithoutColor(ref _coloredIconMaterial);
				ProcColoredIconHelper.SetAspectRatio(_mainSchematicIcon.gameObject, ref _mainIconAspectFitter, itemData);
				_mainSchematicIcon.material = _coloredIconMaterial;
				_mainSchematicIcon.preserveAspect = false;
			}
			else
			{
				_mainSchematicIcon.preserveAspect = true;
				_mainSchematicIcon.material = null;
			}
		}
	}

	protected void SetProceduralIcon(SchematicData schematic)
	{
		Improbable.Collections.List<SlottedMaterial> list = new Improbable.Collections.List<SlottedMaterial>();
		for (int i = 0; i < 6; i++)
		{
			list.Add(new SlottedMaterial(i, new RawMaterial(string.Empty, 1, string.Empty, new Map<string, string>()), 1, null));
		}
		if (ProcIconRenderer.SetProcIcon(schematic.itemType, isUpdate: false, schematic.modules, list, _mainSchematicIconRawImage))
		{
			_mainSchematicIconRawImage.enabled = true;
			_mainSchematicIcon.enabled = false;
		}
		else
		{
			SetNonProceduralIcon(schematic);
		}
	}

	private void SetAttributes()
	{
		int num = 0;
		if (_craftingStationData.LoadedSchematic.baseHp > 0f)
		{
			_schematicAttributeBars[0].AttributeId = "hpStat";
			string statTitle = SchematicData.GetStatTitle("hpStat");
			if (_craftingStationData.LoadedSchematic.IsProcedural)
			{
				_schematicAttributeBars[0].SetValue(statTitle, _craftingStationData.LoadedSchematic.baseHp, 100f, showMax: false);
			}
			else
			{
				_schematicAttributeBars[0].SetValue(statTitle, _craftingStationData.LoadedSchematic.baseHp / SchematicData.baseHPValue, 100f, showMax: false);
			}
			_schematicAttributeBars[0].gameObject.SetActive(value: true);
			num = 1;
		}
		if (_craftingStationData.LoadedSchematic.baseStats != null)
		{
			foreach (KeyValuePair<string, float> orderedStat in _craftingStationData.LoadedSchematic.OrderedStats)
			{
				_schematicAttributeBars[num].AttributeId = orderedStat.Key;
				_schematicAttributeBars[num].SetValue(SchematicData.GetStatTitle(orderedStat.Key), orderedStat.Value, 100f);
				_schematicAttributeBars[num].gameObject.SetActive(value: true);
				num++;
			}
		}
		for (int i = num; i < _schematicAttributeBars.Length; i++)
		{
			_schematicAttributeBars[i].ShowValues(show: false);
		}
	}

	public void HighlightAttributes(string subComponentType)
	{
		string[] affectedStatsBySlot = SchematicData.GetAffectedStatsBySlot(_craftingStationData.LoadedSchematic.itemType, subComponentType);
		if (affectedStatsBySlot == null)
		{
			return;
		}
		for (int i = 0; i < _schematicAttributeBars.Length; i++)
		{
			if (affectedStatsBySlot.Contains(_schematicAttributeBars[i].AttributeId))
			{
				_schematicAttributeBars[i].Highlight();
			}
		}
	}

	public void LowLightAttributes()
	{
		for (int i = 0; i < _schematicAttributeBars.Length; i++)
		{
			_schematicAttributeBars[i].LowLight();
		}
	}

	public void SetDescription()
	{
		_schematicDescription.SetText(_craftingStationData.LoadedSchematic.description);
	}

	protected void CheckUnlearnState()
	{
		bool flag = !LocalPlayer.Instance.NewPlayerVisualiser.IsNew;
		_unlearnButton.SetObjectActive(flag && _craftingStationData.HasLoadedSchematic && _craftingStationData.LoadedSchematic.unlearnable && _schematicSystem.PlayerHasLearnedSchematic(_craftingStationData.LoadedSchematic.UniqueID));
	}

	private void SetInstruction()
	{
		switch (_craftingStationData.LoadedSchematic.CraftingCategoryEnum)
		{
		case CraftingCategory.Personal:
			_instructionText.SetText(Localizer.LocalizeString(LocalizationSchema.KeySCHEMATICS_TAB_INSTRUCTION_PERSONAL));
			break;
		case CraftingCategory.Shipyard:
			_instructionText.SetText(Localizer.LocalizeString(LocalizationSchema.KeySCHEMATICS_TAB_INSTRUCTION_SHIPYARD));
			break;
		case CraftingCategory.CraftingStation:
			_instructionText.SetText(Localizer.LocalizeString(LocalizationSchema.KeySCHEMATICS_TAB_INSTRUCTION_CRAFTING_STATION));
			break;
		case CraftingCategory.Cooking:
			_instructionText.SetText(Localizer.LocalizeString(LocalizationSchema.KeySCHEMATICS_TAB_INSTRUCTION_COOKING));
			break;
		case CraftingCategory.Clothing:
			_instructionText.SetText(Localizer.LocalizeString(LocalizationSchema.KeySCHEMATICS_TAB_INSTRUCTION_CLOTHING));
			break;
		}
	}

	private void SetCipherSlot()
	{
		if (_craftingStationData.LoadedSchematic.cipherSlots == null)
		{
			_cipherPanel.SetActive(value: false);
			return;
		}
		_cipherPanel.SetActive(value: true);
		if (_craftingStationData.LoadedSchematic.cipherSlots.Count > _cipherUiSlots.Length)
		{
			ReleaseError.Raise<SchematicsSubScreen>(() => "cipherSlots in schematic has more slots than UI, update your 'SchematicsSubScreen' prefab!");
			return;
		}
		System.Collections.Generic.List<Option<Cipher>> cipherSlotParsed = _craftingStationData.LoadedSchematic.cipherSlotParsed;
		for (int i = 0; i < _cipherUiSlots.Length; i++)
		{
			CipherUiSlot cipherUiSlot = _cipherUiSlots[i];
			if (i < cipherSlotParsed.Count)
			{
				cipherUiSlot.Setup(_craftingStationData.LoadedSchematic, isUnlocked: true, cipherSlotParsed[i], i);
			}
			else
			{
				cipherUiSlot.Setup(_craftingStationData.LoadedSchematic, isUnlocked: false, null, i);
			}
		}
	}
}
