using System;
using System.Collections.Generic;
using Bossa.Travellers.Craftingstation;
using Improbable;
using Improbable.Collections;
using RSG;
using Travellers.UI.Framework;
using Travellers.UI.Models;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

public class CraftingStationData
{
	public EntityId CraftingEntityId;

	public System.Collections.Generic.List<CraftingSlotData> CraftingSlotData = new System.Collections.Generic.List<CraftingSlotData>();

	private SchematicData _loadedSchematic;

	private string _schematicID;

	[NonSerialized]
	public byte[] loadedHull;

	public string schematicOwner;

	public float CraftingProgressNormalised;

	public string DisplayableCraftingTimeRemaining;

	private float _localRemainingTime;

	private float _gsimRemainingTime;

	private bool isWaitingForServer;

	public float Weight;

	public System.Collections.Generic.List<SchematicCategoryData> SchematicCategoryData;

	private Improbable.Collections.List<SlottedMaterial> _cachedSlotList;

	private readonly LazyUIInterface<ISchematicSystem> _schematicSystem = new LazyUIInterface<ISchematicSystem>();

	public CharacterSheetTabType UITabType { get; private set; }

	public ShipHullEditorVisualizer ShipHullEditorVisualiser { get; private set; }

	public ShipyardVisualizer ShipyardVisualizer { get; private set; }

	public bool HasLoadedSchematic => LoadedSchematic != null;

	public SchematicData LoadedSchematic
	{
		get
		{
			if (_loadedSchematic == null)
			{
				GetSchematicFromID();
			}
			return _loadedSchematic;
		}
	}

	public bool CurrentSchematicCanBeCraftedHere
	{
		get
		{
			if (!HasLoadedSchematic)
			{
				return false;
			}
			return UITabType == CharacterLearnedSchematicLibrary.GetUITabFromCraftingCategory(LoadedSchematic.CraftingCategoryEnum);
		}
	}

	public bool CraftingInProgress { get; private set; }

	public bool AllSlotsAreEmptyRemotely { get; private set; }

	public bool AllSlotsAreFullRemotely { get; private set; }

	public bool HasCurrentlyEditedHullBeenModified { get; private set; }

	public bool IsWaitingForServer
	{
		get
		{
			return isWaitingForServer;
		}
		set
		{
			isWaitingForServer = value;
		}
	}

	public Improbable.Collections.List<SlottedMaterial> CachedSlotList => _cachedSlotList;

	public PredictedStatDataExtra CachedPredictedStats { get; private set; }

	public CraftingStationData()
	{
		AllSlotsAreEmptyRemotely = true;
	}

	public CraftingStationData(CharacterSheetTabType uiTabType)
		: this()
	{
		UITabType = uiTabType;
	}

	public void SetLastSchematic(string id)
	{
		_schematicID = id;
		TryLoadSchematic();
	}

	public bool IsDisplayedShipHullSameAsSchematic()
	{
		if (LoadedSchematic == null || ShipHullEditorVisualiser == null || ShipHullEditorVisualiser.MeshEditor == null)
		{
			return false;
		}
		byte[] hullDataBytes = LoadedSchematic.HullDataBytes;
		return ShipHullEditorVisualiser.MeshEditor.IsDisplayedShipHullSameAs(hullDataBytes);
	}

	private void TryLoadSchematic()
	{
		GetSchematicFromID().Then(delegate(bool wasLoaded)
		{
			if (wasLoaded)
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.CraftingDataSchematicLoaded, this);
			}
		});
	}

	private IPromise<bool> GetSchematicFromID()
	{
		if (!string.IsNullOrEmpty(_schematicID))
		{
			return _schematicSystem.Value.LookupSchematicAsync(_schematicID).Then(delegate(SchematicData schematicToLoad)
			{
				if (schematicToLoad == null)
				{
					WALogger.Error<CraftingStationData>(LogChannel.UI, "Schematic is null", new object[0]);
					return Promise<bool>.Resolved(promisedValue: false);
				}
				LoadSchematic(schematicToLoad, CharacterData.Instance.userName);
				return Promise<bool>.Resolved(promisedValue: true);
			});
		}
		LoadSchematic(null, CharacterData.Instance.userName);
		return Promise<bool>.Resolved(promisedValue: true);
	}

	public void SetShipyardVisualizer(ShipyardVisualizer shipyardVisualizer)
	{
		ShipyardVisualizer = shipyardVisualizer;
	}

	public void SetShipHullEditorVisualiser(ShipHullEditorVisualizer shipHullEditorVisualizer)
	{
		ShipHullEditorVisualiser = shipHullEditorVisualizer;
	}

	public void LoadSchematicWithEvent(SchematicData data, string userName)
	{
		LoadSchematic(data, userName);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.CraftingDataSchematicLoaded, this);
	}

	public void LoadSchematic(SchematicData data, string userName)
	{
		if (CraftingInProgress)
		{
			return;
		}
		_loadedSchematic = data;
		schematicOwner = userName;
		CraftingSlotData = new System.Collections.Generic.List<CraftingSlotData>();
		if (data == null)
		{
			_schematicID = null;
			return;
		}
		for (int i = 0; i < LoadedSchematic.craftingRequirements.Length; i++)
		{
			CraftingSlotData item = new CraftingSlotData(this, LoadedSchematic.craftingRequirements[i], i);
			CraftingSlotData.Add(item);
		}
		SetupCraftingTimerAndDisplay();
	}

	public void UpdateCraftingTimeRemaining(int seconds)
	{
		_gsimRemainingTime = seconds;
	}

	public void DestroyItems()
	{
		if (CraftingSlotData == null)
		{
			return;
		}
		for (int i = 0; i < CraftingSlotData.Count; i++)
		{
			if (CraftingSlotData[i] != null)
			{
				CraftingSlotData[i].CurrentAmount = 0;
			}
		}
	}

	public void StartCrafting()
	{
		SetupCraftingTimerAndDisplay();
		CraftingInProgress = true;
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUICraftingEvents.CraftingStarted, this);
	}

	public void FinishCrafting()
	{
		CraftingProgressNormalised = 1f;
		SetupCraftingTimerAndDisplay();
		CraftingInProgress = false;
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUICraftingEvents.CraftingCompleted, this);
	}

	private void SetupCraftingTimerAndDisplay()
	{
		_localRemainingTime = LoadedSchematic.timeToCraft;
		SetCraftingDisplayTime();
	}

	private void SetCraftingDisplayTime()
	{
		int num = (int)_localRemainingTime % 60;
		int num2 = (int)_localRemainingTime / 60;
		DisplayableCraftingTimeRemaining = num2 + ":" + ((num >= 10) ? num.ToString() : ("0" + num));
	}

	public void ControlledUpdate()
	{
		if (CraftingInProgress)
		{
			UpdateCraftProgress();
		}
	}

	public void UpdateCraftProgress()
	{
		if (_gsimRemainingTime < 0f)
		{
			CraftingProgressNormalised = 0f;
			SetupCraftingTimerAndDisplay();
		}
		else
		{
			_localRemainingTime -= Time.deltaTime;
			CraftingProgressNormalised = ((float)LoadedSchematic.timeToCraft - _localRemainingTime) / (float)LoadedSchematic.timeToCraft;
			SetCraftingDisplayTime();
		}
	}

	public void SyncPredictedStats(PredictedStatDataExtra predictedStats)
	{
		CachedPredictedStats = predictedStats;
	}

	public void SyncCraftingItems(Improbable.Collections.List<SlottedMaterial> gsimSlottedMaterials)
	{
		_cachedSlotList = gsimSlottedMaterials;
		bool allSlotsAreEmptyRemotely = true;
		bool allSlotsAreFullRemotely = true;
		if (!HasLoadedSchematic)
		{
			return;
		}
		for (int i = 0; i < CraftingSlotData.Count; i++)
		{
			SlottedMaterial slottedMaterial = gsimSlottedMaterials[i];
			CraftingSlotData craftingSlotData = CraftingSlotData[i];
			if (slottedMaterial.rawMaterial.materialTypeId != string.Empty)
			{
				InventoryItemData inventoryData = InventoryItemManager.Instance.LookupItem(slottedMaterial.rawMaterial.materialTypeId);
				craftingSlotData.inventoryData = inventoryData;
				craftingSlotData.CurrentAmount = slottedMaterial.amount;
				if (slottedMaterial.amount > 0)
				{
					allSlotsAreEmptyRemotely = false;
				}
				if (!craftingSlotData.IsFull)
				{
					allSlotsAreFullRemotely = false;
				}
				craftingSlotData.quality = slottedMaterial.rawMaterial.quality;
			}
			else if (craftingSlotData != null)
			{
				craftingSlotData.CurrentAmount = slottedMaterial.amount;
			}
			if (craftingSlotData != null && slottedMaterial.customizationMaterial.HasValue)
			{
				allSlotsAreEmptyRemotely = false;
				InventoryItemData customizationInventoryData = InventoryItemManager.Instance.LookupItem(slottedMaterial.customizationMaterial.Value.materialTypeId);
				craftingSlotData.CustomizationInventoryData = customizationInventoryData;
				craftingSlotData.CustomizationInventoryMeta = slottedMaterial.customizationMaterial.Value.meta;
			}
			else
			{
				craftingSlotData.CustomizationInventoryData = null;
				craftingSlotData.CustomizationInventoryMeta = new Map<string, string>();
			}
			if (craftingSlotData != null && !craftingSlotData.IsFull)
			{
				allSlotsAreFullRemotely = false;
			}
		}
		AllSlotsAreEmptyRemotely = allSlotsAreEmptyRemotely;
		AllSlotsAreFullRemotely = allSlotsAreFullRemotely;
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUICraftingEvents.SlottedMaterialsUpdated, this);
	}

	public void SetModifiedHull(byte[] newHull)
	{
		if (newHull == null)
		{
			loadedHull = null;
			return;
		}
		loadedHull = new byte[newHull.Length];
		Array.Copy(newHull, loadedHull, newHull.Length);
		SetCurrentHullModifiedStatus(isModified: true);
	}

	public void OnCraftValidationFailed(string reason)
	{
		WALogger.Warn<CraftingStationData>("CraftingUI::OnCraftValidationFailed {0}", new object[1] { reason });
	}

	public void SetWeight(float currentWeight)
	{
		Weight = Mathf.Max(currentWeight, 0f);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUICraftingEvents.CurrentWeightUpdated, this);
	}

	public void SetCurrentHullModifiedStatus(bool isModified)
	{
		HasCurrentlyEditedHullBeenModified = isModified;
		if (isModified)
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUICraftingEvents.ShipHullIsBeingModified, null);
		}
		else
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUICraftingEvents.ShipHullIsNotBeingModified, null);
		}
	}
}
