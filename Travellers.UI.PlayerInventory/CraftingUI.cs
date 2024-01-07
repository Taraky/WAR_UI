using System;
using System.Collections.Generic;
using Bossa.Travellers.Analytics;
using Bossa.Travellers.Craftingstation;
using Bossa.Travellers.CraftingStation;
using Bossa.Travellers.DebugServerFramework;
using Bossa.Travellers.Materials;
using Improbable;
using Newtonsoft.Json.Linq;
using Travellers.UI.Framework;
using Travellers.UI.Utility;
using UnityEngine;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

public abstract class CraftingUI : UIScreenComponent, ICallbackDebugServerRequestHandler
{
	[SerializeField]
	protected Image _mainSchematicIcon;

	[SerializeField]
	private TextStyler _schematicTitle;

	[SerializeField]
	private TextStyler _schematicWeight;

	[SerializeField]
	protected RectTransform _craftingMaterialsRoot;

	[SerializeField]
	public UIButtonController _craftButton;

	[SerializeField]
	private TextStyler _craftingTimeRemainingText;

	[SerializeField]
	public CraftingMaterialSlot[] craftingOnlySlots;

	[SerializeField]
	public CraftingCustomizationSlot[] customizationSlots;

	[SerializeField]
	public Image[] craftingProgressBars;

	[SerializeField]
	public ShipCraftingUIHelper hullEditorUI;

	protected CraftingMaterialSlot[] _currentSlots;

	private bool _craftingDebugMode;

	protected CraftingStationData _craftingStationData { get; private set; }

	protected bool AllSlotsHaveRequiredMaterials => _currentSlots.AllSlotsHaveRequiredMaterials();

	protected bool AllSlotsAreEmpty => _currentSlots.AllSlotsAreEmpty();

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInventoryEvents.SchematicHierarchyChanged, OnSchematicHierarchyChanged);
		_eventList.AddEvent(WAUIInventoryEvents.CraftingDataSchematicLoaded, OnUpdateLoadedSchematic);
		_eventList.AddEvent(WAUIButtonEvents.CraftButtonPressed, OnCraftingButtonPressed);
		_eventList.AddEvent(WAUICraftingEvents.CraftingStarted, OnCraftStarted);
		_eventList.AddEvent(WAUICraftingEvents.CraftingCompleted, OnCraftingFinished);
		_eventList.AddEvent(WAUICraftingEvents.SlottedMaterialsUpdated, OnSlottedMaterialsUpdated);
		_eventList.AddEvent(WAUICraftingEvents.CurrentWeightUpdated, OnWeightUpdated);
		_eventList.AddEvent(WAUICraftingEvents.CraftingSlotEmptyUpdated, OnCraftingSlotEmptyUpdated);
		_eventList.AddEvent(WAUICraftingEvents.PredictedStatsUpdated, OnPredictedStatsUpdated);
	}

	protected virtual void OnCraftingSlotEmptyUpdated(object[] obj)
	{
	}

	protected virtual void OnSlottedMaterialsUpdated(object[] obj)
	{
		CraftingStationData craftingStationData = (CraftingStationData)obj[0];
		if (craftingStationData != null && CheckIfSameEntity(craftingStationData.CraftingEntityId))
		{
			SlottedMaterialsUpdated();
		}
	}

	protected virtual void OnPredictedStatsUpdated(object[] obj)
	{
		CraftingStationData craftingStationData = (CraftingStationData)obj[0];
		PredictedStatDataExtra predictedStats = (PredictedStatDataExtra)obj[1];
		if (craftingStationData != null && CheckIfSameEntity(craftingStationData.CraftingEntityId) && predictedStats.predictedStats != null)
		{
			UpdateAttributes(predictedStats);
		}
	}

	protected override void ProtectedInit()
	{
		DebugServer.AddCallbackHandler("craftingui", this);
		_currentSlots = new CraftingMaterialSlot[0];
		_craftButton.SetButtonEvent(WAUIButtonEvents.CraftButtonPressed);
	}

	protected override void Deactivate()
	{
		if (LocalPlayer.Exists)
		{
			LocalPlayer.Instance.playerCraftingInteraction.LeaveCraftingStation();
		}
		PlayStopCraftingSound();
	}

	public abstract void SetAsActiveCraftingSection(bool willBeActive);

	protected void Update()
	{
		if (_craftingStationData.CraftingInProgress)
		{
			UpdateCraftProgressBarsAndTimers();
		}
		if (!LocalPlayer.Instance.playerProperties.IsSuperUser)
		{
			return;
		}
		if (!_craftingDebugMode && Input.GetKeyDown(KeyCode.LeftControl))
		{
			_craftingDebugMode = true;
			if (LocalPlayer.Instance != null)
			{
				LocalPlayer.Instance.playerCraftingInteraction.SetDebugMode(debugMode: true);
			}
		}
		if (_craftingDebugMode && Input.GetKeyUp(KeyCode.LeftControl))
		{
			_craftingDebugMode = false;
			if (LocalPlayer.Instance != null)
			{
				LocalPlayer.Instance.playerCraftingInteraction.SetDebugMode(debugMode: false);
			}
		}
		CheckCraftButtonState();
	}

	private bool CheckIfSameEntity(EntityId entityId)
	{
		return _craftingStationData != null && entityId == _craftingStationData.CraftingEntityId;
	}

	private void OnSchematicHierarchyChanged(object[] obj)
	{
		if (_craftingStationData != null)
		{
			UpdateLoadedSchematic();
		}
	}

	private void OnUpdateLoadedSchematic(object[] obj)
	{
		CraftingStationData craftingStationData = (CraftingStationData)obj[0];
		if (craftingStationData != null && CheckIfSameEntity(craftingStationData.CraftingEntityId))
		{
			UpdateLoadedSchematic();
		}
	}

	private void OnCraftingButtonPressed(object[] obj)
	{
		if (_craftingStationData != null)
		{
			PerformCraft();
		}
	}

	private void OnWeightUpdated(object[] obj)
	{
		CraftingStationData craftingStationData = (CraftingStationData)obj[0];
		if (craftingStationData != null && CheckIfSameEntity(craftingStationData.CraftingEntityId))
		{
			SetWeight();
		}
	}

	private void OnCraftingFinished(object[] obj)
	{
		CraftingStationData craftingStationData = (CraftingStationData)obj[0];
		if (craftingStationData != null && CheckIfSameEntity(craftingStationData.CraftingEntityId))
		{
			CraftingFinished();
		}
	}

	private void OnCraftStarted(object[] obj)
	{
		CraftingStationData craftingStationData = (CraftingStationData)obj[0];
		if (craftingStationData != null && CheckIfSameEntity(craftingStationData.CraftingEntityId))
		{
			CraftingStarted();
		}
	}

	public virtual void SetCraftingDataTemplate(CraftingStationData craftingStationData)
	{
		_craftingStationData = craftingStationData;
		CheckIfSchematicLoaded();
	}

	public abstract void CheckIfSchematicLoaded();

	public abstract void SelectNewSchematic();

	public abstract void UpdateLoadedSchematic();

	public abstract void LoadSchematicFromCraftingData();

	protected virtual void CheckUnlearnState()
	{
	}

	protected void ReturnAnySlottedMaterials()
	{
		LocalPlayer.Instance.playerCraftingInteraction.ReturnItemToPlayerInventory(LocalPlayer.Instance.playerCraftingInteraction.CraftingStationEntityId, -1, CraftingSlotType.Main);
		for (int i = 0; i < craftingOnlySlots.Length; i++)
		{
			craftingOnlySlots[i].ReturnItemToInventory();
		}
		for (int j = 0; j < customizationSlots.Length; j++)
		{
			customizationSlots[j].ReturnItemToInventory();
		}
	}

	protected abstract void SetForCrafting(bool craft);

	protected void SetName()
	{
		if (_craftingStationData != null && _craftingStationData.HasLoadedSchematic)
		{
			SchematicsRarity rarityParsed = _craftingStationData.LoadedSchematic.rarityParsed;
			RarityColourSet rarityColoursForButtonStates = RarityHelper.GetRarityColoursForButtonStates(rarityParsed);
			_schematicTitle.SetRarityColourSet(rarityColoursForButtonStates);
			_schematicTitle.SetText(_craftingStationData.LoadedSchematic.GetFormattedTitle());
		}
	}

	protected void SetWeight()
	{
		string text = $"{Mathf.Round(_craftingStationData.Weight * 100f) / 100f}kg";
		_schematicWeight.SetText(text);
	}

	protected abstract void CheckIcon();

	protected virtual void SetupSlots()
	{
		for (int i = 0; i < _currentSlots.Length; i++)
		{
			_currentSlots[i].SlotIndex = i;
			customizationSlots[i].SlotIndex = i;
			if (i < _craftingStationData.CraftingSlotData.Count)
			{
				_currentSlots[i].SetNewCraftingData(_craftingStationData, _craftingStationData.CraftingSlotData[i], HighlightAttributes, LowLightAttributes);
				_currentSlots[i].SetReturnedToInventoryCallback(CheckCraftButtonState);
				customizationSlots[i].SetNewCraftingData(_craftingStationData, _craftingStationData.CraftingSlotData[i]);
			}
			else
			{
				_currentSlots[i].SetNewCraftingData(null, null, null, null);
				customizationSlots[i].SetNewCraftingData(null, null);
			}
		}
	}

	protected void UpdateSlotStates()
	{
		for (int i = 0; i < _currentSlots.Length; i++)
		{
			_currentSlots[i].UpdateState();
		}
		for (int j = 0; j < customizationSlots.Length; j++)
		{
			customizationSlots[j].UpdateState();
		}
	}

	protected virtual void CheckCraftButtonState()
	{
		_craftButton.SetButtonState(IsCraftingPossible() ? UIButtonState.Default : UIButtonState.Disabled);
	}

	protected virtual void UpdateCraftProgressBarsAndTimers()
	{
		float craftingProgressNormalised = _craftingStationData.CraftingProgressNormalised;
		for (int i = 0; i < craftingProgressBars.Length; i++)
		{
			if (!craftingProgressBars[i].gameObject.activeInHierarchy)
			{
				craftingProgressBars[i].gameObject.SetActive(value: true);
			}
			craftingProgressBars[i].fillAmount = 1f - craftingProgressNormalised;
		}
		for (int j = 0; j < _currentSlots.Length; j++)
		{
			if (_currentSlots[j].IsUsedByCurrentSchematic)
			{
				_currentSlots[j].UpdateCraftingProgressAmount();
			}
		}
		_craftingTimeRemainingText.SetText(_craftingStationData.DisplayableCraftingTimeRemaining);
	}

	public void PerformCraft()
	{
		if (!IsCraftingPossible())
		{
			WALogger.Warn<CraftingUI>("Craft halted. Is UI intact?");
			return;
		}
		if (_craftingStationData.LoadedSchematic == null)
		{
			WALogger.Error<CraftingUI>("Trying to craft but current Loaded Schematic is null! _craftingStationData: [{0}] | _craftingStationData.CraftingEntityId: [{1}]", new object[2] { _craftingStationData, _craftingStationData.CraftingEntityId });
			return;
		}
		PlayerCraftingInteractionBehaviour playerCraftingInteraction = LocalPlayer.Instance.playerCraftingInteraction;
		switch (_craftingStationData.LoadedSchematic.CraftingCategoryEnum)
		{
		case CraftingCategory.Personal:
			PlayerAnalytics.SendAnalyticsEvent("craftItem", AnalPayload(_craftingDebugMode));
			playerCraftingInteraction.StartMultiToolCrafting(_craftingStationData.LoadedSchematic.schematicId);
			break;
		case CraftingCategory.CraftingStation:
			PlayerAnalytics.SendAnalyticsEvent("craftShipPart", AnalPayload(_craftingDebugMode));
			if (playerCraftingInteraction.CraftingStationEntityId.IsValid())
			{
				playerCraftingInteraction.TriggerCrafting(playerCraftingInteraction.CraftingStationEntityId);
			}
			break;
		case CraftingCategory.Shipyard:
			if (playerCraftingInteraction.CraftingStationEntityId.IsValid())
			{
				playerCraftingInteraction.TriggerCrafting(playerCraftingInteraction.CraftingStationEntityId);
			}
			break;
		case CraftingCategory.Cooking:
		case CraftingCategory.Clothing:
			if (playerCraftingInteraction.CraftingStationEntityId.IsValid())
			{
				playerCraftingInteraction.TriggerCrafting(playerCraftingInteraction.CraftingStationEntityId);
			}
			break;
		}
	}

	protected abstract void CraftingStarted();

	public abstract void LowLightAttributes();

	public abstract void HighlightAttributes(string text);

	protected abstract void CraftingFinished();

	protected virtual void UpdateAttributes(PredictedStatDataExtra predictedStats)
	{
	}

	protected void SlottedMaterialsUpdated()
	{
		UpdateSlotStates();
		UpdateSchematicListState();
		CheckCraftButtonState();
	}

	protected abstract void UpdateSchematicListState();

	public void ResetCraftingState()
	{
		_craftingStationData.DestroyItems();
		SetupSlots();
	}

	protected abstract void PlayStartCraftingSound();

	protected abstract void PlayStopCraftingSound();

	protected override void ProtectedDispose()
	{
	}

	private Dictionary<string, object> AnalPayload(bool debugMode)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["itemType"] = ((!_craftingStationData.LoadedSchematic.IsProcedural) ? _craftingStationData.LoadedSchematic.schematicId : _craftingStationData.LoadedSchematic.GetFormattedTitle());
		dictionary["itemAmount"] = _craftingStationData.LoadedSchematic.amountToCraft;
		if (debugMode)
		{
			dictionary["debugMode"] = true;
		}
		return dictionary;
	}

	public void HandleRequest(DebugServerRequest request)
	{
		JObject jObject = new JObject();
		if (_craftingStationData.HasLoadedSchematic)
		{
			try
			{
				jObject.Add("selected", JObject.FromObject(_craftingStationData.LoadedSchematic));
			}
			catch (Exception)
			{
				WALogger.Warn<CraftingUI>("Could not serialise selected: " + _craftingStationData.LoadedSchematic.title);
			}
		}
		request.callback(jObject);
	}

	private bool IsCraftingPossible()
	{
		if (_craftingDebugMode)
		{
			return true;
		}
		if (!_craftingStationData.CraftingInProgress && !_craftingStationData.IsWaitingForServer && _craftingStationData.CurrentSchematicCanBeCraftedHere && AllSlotsHaveRequiredMaterials && (_craftingStationData.UITabType != CharacterSheetTabType.ShipCraft || _craftingStationData.IsDisplayedShipHullSameAsSchematic()))
		{
			return true;
		}
		return false;
	}
}
