using System.Collections.Generic;
using System.Linq;
using Bossa.Travellers.Craftingstation;
using Bossa.Travellers.World;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.InfoPopups;
using Travellers.UI.Models;
using Travellers.UI.Sound;
using UnityEngine;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class CraftingStationCraftingUI : CraftingUI
{
	[SerializeField]
	protected RawImage _mainSchematicIconRawImage;

	[SerializeField]
	private GameObject _noSchematicWarning;

	[SerializeField]
	private GameObject _schematicCoolingFactorObject;

	[SerializeField]
	private TextStyler _schematicAmount;

	[SerializeField]
	private TextStyler _schematicDescription;

	[SerializeField]
	private TextStyler _schematicCoolingFactor;

	[SerializeField]
	private RectTransform _schematicMaterialsRoot;

	[SerializeField]
	public CraftingMaterialSlot[] schematicsOnlySlots;

	[SerializeField]
	protected ItemAttribute[] _schematicAttributeBars;

	[SerializeField]
	private CraftingStationSchematicList _craftingStationSchematicList;

	private Material _coloredIconMaterial;

	private AspectRatioFitter _mainIconAspectFitter;

	private ISchematicSystem _schematicSystem;

	private InventorySystem _inventorySystem;

	[InjectableMethod]
	public void InjectDependencies(ISchematicSystem schematicSystem, InventorySystem inventorySystem)
	{
		_schematicSystem = schematicSystem;
		_inventorySystem = inventorySystem;
	}

	private void Start()
	{
		ProcIconRenderer.InitialiseProcPartRenderSetup();
	}

	protected override void AddListeners()
	{
		base.AddListeners();
		_eventList.AddEvent(WAUIButtonEvents.SchematicCategoryPressed, OnCategoryButtonPressed);
		_eventList.AddEvent(WAUIButtonEvents.SchematicSubCategoryPressed, OnSubCategoryButtonPressed);
		_eventList.AddEvent(WAUIButtonEvents.UnlearnSchematic, OnUnlearnSchematicPressed);
	}

	protected override void Activate()
	{
		_craftingStationSchematicList.SetObjectActive(isActive: true);
	}

	protected override void Deactivate()
	{
		_craftingStationSchematicList.SetObjectActive(isActive: false);
		base.Deactivate();
	}

	public override void SetAsActiveCraftingSection(bool willBeActive)
	{
		_craftingStationSchematicList.SetObjectActive(willBeActive);
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
		if (base._craftingStationData.LoadedSchematic.unlearnable)
		{
			DialogPopupFacade.ShowConfirmationDialog("Unlearn Schematic", $"Are you sure you want to unlearn {base._craftingStationData.LoadedSchematic.title}?", delegate
			{
				string dataIdToUnlearn = base._craftingStationData.LoadedSchematic.UniqueID;
				string text = null;
				SchematicData schematicData = null;
				string itemType = base._craftingStationData.LoadedSchematic.itemType;
				CraftingCategory categoryType = base._craftingStationData.LoadedSchematic.CraftingCategoryEnum;
				SchematicCategoryData schematicCategoryData = base._craftingStationData.SchematicCategoryData.Find((SchematicCategoryData c) => c.CategoryTypeEnum == categoryType);
				SchematicCategoryData schematicCategoryData2 = schematicCategoryData.ChildSchematicCategories[itemType];
				if (schematicCategoryData2.ChildItemSchematics.Count > 1)
				{
					schematicData = schematicCategoryData2.ChildItemSchematics.Find((SchematicData i) => i.UniqueID != dataIdToUnlearn);
					text = schematicData.referenceData;
					if (text == null)
					{
						text = schematicData.UniqueID;
					}
				}
				if (schematicData != null)
				{
					SelectNewSchematic();
					_inventorySystem.CurrentCraftingData.LoadSchematicWithEvent(schematicData, CharacterData.Instance.userName);
					_inventorySystem.SendNewSchematicToServer();
				}
				base._craftingStationData.SetLastSchematic(text);
				_schematicSystem.UnlearnSchematic(dataIdToUnlearn);
				CheckUnlearnState();
			});
		}
		else
		{
			WALogger.Warn<CraftingUI>("Trying to unlearn non unlearnable schematic " + base._craftingStationData.LoadedSchematic.schematicId);
			OSDMessage.SendMessage("That schematic cannot be unlearnt.", MessageType.PlayerDeath);
		}
	}

	public override void SelectNewSchematic()
	{
		ReturnAnySlottedMaterials();
		LoadSchematicFromCraftingData();
		_craftingStationSchematicList.SchematicButtonPressed();
	}

	public override void UpdateLoadedSchematic()
	{
		CheckIfSchematicLoaded();
		_craftingStationSchematicList.RefreshSchematicDisplay();
	}

	public override void CheckIfSchematicLoaded()
	{
		if (base._craftingStationData.LoadedSchematic != null)
		{
			LoadSchematicFromCraftingData();
		}
		else
		{
			_noSchematicWarning.SetActive(value: true);
		}
	}

	protected override void UpdateSchematicListState()
	{
		_craftingStationSchematicList.UpdateCraftingState();
	}

	public override void LoadSchematicFromCraftingData()
	{
		SetForCrafting(base._craftingStationData.CurrentSchematicCanBeCraftedHere);
		SetupSlots();
		CheckIcon();
		SetWeight();
		SetCoolingFactor(base._craftingStationData.CachedPredictedStats);
		SetName();
		SetDescription();
		SetAmountToBeCrafted();
		UpdateAttributes(base._craftingStationData.CachedPredictedStats);
		CheckCraftButtonState();
		UpdateCraftProgressBarsAndTimers();
		CheckUnlearnState();
		CheckSoundState();
		_noSchematicWarning.SetActive(value: false);
	}

	public override void SetCraftingDataTemplate(CraftingStationData craftingStationData)
	{
		base.SetCraftingDataTemplate(craftingStationData);
		_craftingStationSchematicList.SetCraftingDataTemplate(craftingStationData);
	}

	protected override void SetForCrafting(bool craft)
	{
		_craftButton.SetObjectActive(craft);
		_craftingMaterialsRoot.gameObject.SetActive(craft);
		_schematicMaterialsRoot.gameObject.SetActive(!craft);
		_currentSlots = ((!craft) ? schematicsOnlySlots : craftingOnlySlots);
	}

	protected override void CheckIcon()
	{
		if (base._craftingStationData.CraftingSlotData != null && base._craftingStationData.CraftingSlotData.Count != 0)
		{
			UpdateIcon(base._craftingStationData.LoadedSchematic);
		}
	}

	protected override void OnCraftingSlotEmptyUpdated(object[] obj)
	{
		base.OnCraftingSlotEmptyUpdated(obj);
		SchematicData loadedSchematic = base._craftingStationData.LoadedSchematic;
		if (loadedSchematic != null)
		{
			UpdateIcon(loadedSchematic, initing: false);
			_craftingStationSchematicList.OnCraftingSlotsEmptyUpdated(base.AllSlotsAreEmpty);
		}
	}

	protected override void OnSlottedMaterialsUpdated(object[] obj)
	{
		base.OnSlottedMaterialsUpdated(obj);
		SchematicData loadedSchematic = base._craftingStationData.LoadedSchematic;
		if (loadedSchematic != null)
		{
			UpdateIcon(loadedSchematic, initing: false);
		}
	}

	protected void SetNonProceduralIcon(SchematicData schematic)
	{
		_mainSchematicIconRawImage.enabled = false;
		_mainSchematicIcon.enabled = true;
		string empty = string.Empty;
		int num = 0;
		int num2 = 0;
		if (base._craftingStationData.CraftingSlotData[0].inventoryData != null)
		{
			if (base._craftingStationData.CraftingSlotData[0].inventoryData.category == "Wood")
			{
				num2 += base._craftingStationData.CraftingSlotData[0].CurrentAmount;
			}
			else if (base._craftingStationData.CraftingSlotData[0].inventoryData.category == "Metal")
			{
				num += base._craftingStationData.CraftingSlotData[0].CurrentAmount;
			}
		}
		empty = ((num < num2) ? base._craftingStationData.LoadedSchematic.iconId.Replace("Metal", "Wood") : base._craftingStationData.LoadedSchematic.iconId);
		if (base._craftingStationData.LoadedSchematic.SchematicType == SchematicType.Ship)
		{
			empty = "shipyard_placeholder_icon";
		}
		if (!string.IsNullOrEmpty(empty))
		{
			Sprite iconSprite = InventoryIconManager.Instance.GetIconSprite(empty);
			_mainSchematicIcon.enabled = true;
			_mainSchematicIcon.sprite = iconSprite;
			if (ProcColoredIconHelper.ShouldUseProcColoredShader(schematic))
			{
				InventoryItemData itemData = InventoryItemManager.Instance.LookupItem(base._craftingStationData.LoadedSchematic.schematicId);
				ProcColoredIconHelper.SetProcColoredIconWithColor(base._craftingStationData.CachedSlotList, ref _coloredIconMaterial, ProcColoredIconHelper.IsClothingRelated(schematic));
				ProcColoredIconHelper.SetAspectRatio(_mainSchematicIcon.gameObject, ref _mainIconAspectFitter, itemData);
				_mainSchematicIcon.material = _coloredIconMaterial;
				_mainSchematicIcon.preserveAspect = false;
			}
			else
			{
				_mainSchematicIcon.color = Color.white;
				_mainSchematicIcon.preserveAspect = true;
				_mainSchematicIcon.material = null;
			}
		}
	}

	private void UpdateIcon(SchematicData schematic, bool initing = true)
	{
		if (schematic.IsProcedural)
		{
			SetProceduralIcon(schematic, initing);
		}
		else
		{
			SetNonProceduralIcon(schematic);
		}
	}

	protected override void SetupSlots()
	{
		base.SetupSlots();
		_craftingStationSchematicList.OnCraftingSlotsEmptyUpdated(base.AllSlotsAreEmpty);
	}

	protected void SetProceduralIcon(SchematicData schematic, bool isGenerate)
	{
		if (ProcIconRenderer.SetProcIcon(schematic.itemType, !isGenerate, schematic.modules, base._craftingStationData.CachedSlotList, _mainSchematicIconRawImage))
		{
			_mainSchematicIconRawImage.enabled = true;
			_mainSchematicIcon.enabled = false;
		}
		else
		{
			SetNonProceduralIcon(schematic);
		}
	}

	protected override void UpdateAttributes(PredictedStatDataExtra predictedStatsExtra)
	{
		int num = 0;
		if (predictedStatsExtra.predictedStats != null)
		{
			List<PredictedStatData> orderedStats = SchematicData.GetOrderedStats(predictedStatsExtra.predictedStats);
			foreach (PredictedStatData item in orderedStats)
			{
				_schematicAttributeBars[num].intValues = true;
				_schematicAttributeBars[num].AttributeId = item.statId;
				if (item.statId.Equals("hpStat"))
				{
					if (base._craftingStationData.LoadedSchematic.IsProcedural)
					{
						_schematicAttributeBars[num].SetValue(SchematicData.GetStatTitle(item.statId), item.baseNormalized, item.modifierNormalized, 100f);
					}
					else
					{
						float num2 = item.baseNormalized / SchematicData.baseHPValue;
						float v = item.modifierNormalized / SchematicData.baseHPValue - num2;
						_schematicAttributeBars[num].SetValue(SchematicData.GetStatTitle(item.statId), num2, v, 100f);
					}
				}
				else
				{
					_schematicAttributeBars[num].SetValue(SchematicData.GetStatTitle(item.statId), item.baseNormalized, item.modifierNormalized, 100f);
				}
				_schematicAttributeBars[num].gameObject.SetActive(value: true);
				num++;
			}
			SetCoolingFactor(predictedStatsExtra);
		}
		for (int i = num; i < _schematicAttributeBars.Length; i++)
		{
			_schematicAttributeBars[i].ShowValues(show: false);
		}
	}

	public override void HighlightAttributes(string subComponentType)
	{
		string[] affectedStatsBySlot = SchematicData.GetAffectedStatsBySlot(base._craftingStationData.LoadedSchematic.itemType, subComponentType);
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

	public override void LowLightAttributes()
	{
		for (int i = 0; i < _schematicAttributeBars.Length; i++)
		{
			_schematicAttributeBars[i].LowLight();
		}
	}

	public void SetAmountToBeCrafted()
	{
		_schematicAmount.SetText($"x{base._craftingStationData.LoadedSchematic.amountToCraft}");
	}

	public void SetDescription()
	{
		_schematicDescription.SetText(base._craftingStationData.LoadedSchematic.description);
	}

	public void SetCoolingFactor(PredictedStatDataExtra predictedStatsExtra)
	{
		float conductivityFinalStat = predictedStatsExtra.conductivityFinalStat;
		if (conductivityFinalStat > 0f)
		{
			_schematicCoolingFactorObject.SetActive(value: true);
			int num = Mathf.RoundToInt(conductivityFinalStat * 100f);
			_schematicCoolingFactor.SetText($"Cooling Factor: {num}");
		}
		else
		{
			_schematicCoolingFactorObject.SetActive(value: false);
		}
	}

	private void CheckSoundState()
	{
		if (base._craftingStationData.CraftingInProgress)
		{
			PlayStartCraftingSound();
		}
	}

	protected override void CraftingStarted()
	{
		CheckCraftButtonState();
		UpdateSlotStates();
		PlayStartCraftingSound();
		_craftingStationSchematicList.CraftingStarted();
	}

	protected override void CraftingFinished()
	{
		UpdateCraftProgressBarsAndTimers();
		SetupSlots();
		CheckCraftButtonState();
		PlayStopCraftingSound();
		_craftingStationSchematicList.CraftingFinished();
	}

	protected override void PlayStartCraftingSound()
	{
		if (base._craftingStationData.HasLoadedSchematic)
		{
			if (base._craftingStationData.LoadedSchematic.CraftingCategoryEnum == CraftingCategory.CraftingStation)
			{
				SoundScreen.PlayASound("Play_Inventory_Crafting_Loop");
			}
			else if (base._craftingStationData.LoadedSchematic.CraftingCategoryEnum == CraftingCategory.Personal)
			{
				SoundScreen.PlayASound("Play_Inventory_Crafting_Loop");
			}
		}
	}

	protected override void PlayStopCraftingSound()
	{
		if (base._craftingStationData.HasLoadedSchematic)
		{
			if (base._craftingStationData.LoadedSchematic.CraftingCategoryEnum == CraftingCategory.CraftingStation)
			{
				SoundScreen.PlayASound("Stop_Inventory_Crafting_Loop");
			}
			else if (base._craftingStationData.LoadedSchematic.CraftingCategoryEnum == CraftingCategory.Personal)
			{
				SoundScreen.PlayASound("Stop_Inventory_Crafting_Loop");
			}
		}
	}
}
