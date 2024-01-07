using System;
using System.Collections.Generic;
using Bossa.Travellers.Items;
using Improbable.Collections;
using RSG;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.PlayerInventory;
using WAUtilities.Logging;

namespace Travellers.UI.Models;

[InjectedSystem(InjectionType.Real)]
public class SchematicSystem : ISchematicSystem
{
	private CharacterLearnedSchematicLibrary _characterLearnedSchematicLibrary;

	private SchematicsReferenceStore _schematicsReferenceStore;

	private readonly HashSet<string> _rawNonShipSchematicsBuffer = new HashSet<string>();

	private readonly System.Collections.Generic.List<CraftingStationData> _craftingStationDataBuffer = new System.Collections.Generic.List<CraftingStationData>();

	private Action<string> _unlearnSchematic;

	private readonly Queue<Promise> _schematicRequestBuffer = new Queue<Promise>();

	private bool _characterSchematicsReceived;

	public bool IsPlayerAndReferenceDataLoaded => _characterSchematicsReceived && _schematicsReferenceStore.GsimReferenceDataLoaded;

	public SchematicSystem()
	{
		CreateNewStorageFacilities();
	}

	public void AllReferenceAndPlayerDataLoaded()
	{
		_characterSchematicsReceived = true;
		_characterLearnedSchematicLibrary.TryAddRawNonShipSchematics(_rawNonShipSchematicsBuffer);
		_rawNonShipSchematicsBuffer.Clear();
		foreach (CraftingStationData item in _craftingStationDataBuffer)
		{
			if (item != null)
			{
				RebuildCraftingDataSchematicHierarchy(item);
			}
		}
		while (_schematicRequestBuffer.Count > 0)
		{
			_schematicRequestBuffer.Dequeue().Resolve();
		}
		OSDMessage.SendMessage("All reference data has arrived");
		_craftingStationDataBuffer.Clear();
	}

	public void ReferenceDataReset()
	{
		OSDMessage.SendMessage("Resetting all reference data");
		CreateNewStorageFacilities();
	}

	private void CreateNewStorageFacilities()
	{
		_characterSchematicsReceived = false;
		_schematicRequestBuffer.Clear();
		_schematicsReferenceStore = new SchematicsReferenceStore();
		_characterLearnedSchematicLibrary = new CharacterLearnedSchematicLibrary(_schematicsReferenceStore);
	}

	public void DeserialiseJson(string json)
	{
		_schematicsReferenceStore.DeserialiseJson(json);
		OSDMessage.SendMessage("Deserialising schematic reference data");
	}

	public IPromise<SchematicData> LookupSchematicAsync(string uuId)
	{
		Promise<SchematicData> promise = new Promise<SchematicData>();
		if (!IsPlayerAndReferenceDataLoaded)
		{
			Promise promise2 = new Promise();
			_schematicRequestBuffer.Enqueue(promise2);
			promise2.Then(delegate
			{
				promise.Resolve(_schematicsReferenceStore.LookupSchematic(uuId));
			});
		}
		else
		{
			promise.Resolve(_schematicsReferenceStore.LookupSchematic(uuId));
		}
		return promise;
	}

	public SchematicData LookupSchematic(string uuId)
	{
		return _schematicsReferenceStore.LookupSchematic(uuId);
	}

	public void CheckIfSchematicHierarchyRebuildNeeded(CraftingStationData craftingDataTemplate)
	{
		if (_characterLearnedSchematicLibrary.CheckIfSchematicHierarchyRebuildNeeded())
		{
			craftingDataTemplate.SchematicCategoryData = _characterLearnedSchematicLibrary.RequestSchematicListForTabType(craftingDataTemplate.UITabType);
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.SchematicHierarchyChanged, null);
		}
	}

	public void RebuildCraftingDataSchematicHierarchy(CraftingStationData craftingDataTemplate)
	{
		if (!IsPlayerAndReferenceDataLoaded)
		{
			_craftingStationDataBuffer.Add(craftingDataTemplate);
			WALogger.Warn<InventorySystem>(LogChannel.UI, "Attempting to rebuild schematic hierarchy before data deserialised and player schematic data recieved", new object[0]);
		}
		else
		{
			_characterLearnedSchematicLibrary.CheckIfSchematicHierarchyRebuildNeeded();
			craftingDataTemplate.SchematicCategoryData = _characterLearnedSchematicLibrary.RequestSchematicListForTabType(craftingDataTemplate.UITabType);
		}
	}

	public void UpdateNonShipSchematics(Improbable.Collections.List<string> defaultSchematics, Improbable.Collections.List<string> learnedSchematics)
	{
		defaultSchematics.ForEach(delegate(string schematicString)
		{
			_rawNonShipSchematicsBuffer.Add(schematicString);
		});
		learnedSchematics.ForEach(delegate(string schematicString)
		{
			_rawNonShipSchematicsBuffer.Add(schematicString);
		});
		if (_schematicsReferenceStore.GsimReferenceDataLoaded && _characterLearnedSchematicLibrary.TryAddRawNonShipSchematics(_rawNonShipSchematicsBuffer))
		{
			_rawNonShipSchematicsBuffer.Clear();
		}
	}

	public void UpdateShipSchematics(Improbable.Collections.List<ShipHullSchematicData> shipData)
	{
		_characterLearnedSchematicLibrary.AddShipSchematics(shipData);
	}

	public System.Collections.Generic.List<SchematicData> GetShipSchematics()
	{
		return _characterLearnedSchematicLibrary.GetShipSchematics();
	}

	public bool PlayerHasLearnedSchematic(string uuidOrJSON)
	{
		return _characterLearnedSchematicLibrary.PlayerHasLearnedSchematic(uuidOrJSON);
	}

	public int SchematicsCapPerCategory(CraftingCategory category)
	{
		return category switch
		{
			CraftingCategory.Personal => LocalPlayer.Instance.inventoryVisualiser.MaxMultitoolSchematics, 
			CraftingCategory.CraftingStation => LocalPlayer.Instance.inventoryVisualiser.MaxCraftingStationSchematics, 
			CraftingCategory.Cooking => LocalPlayer.Instance.inventoryVisualiser.MaxCookingSchematics, 
			CraftingCategory.Clothing => LocalPlayer.Instance.inventoryVisualiser.MaxClothingSchematics, 
			CraftingCategory.Shipyard => 5, 
			_ => 0, 
		};
	}

	public void SetUnlearnSchematicCallback(Action<string> unlearnSchematic)
	{
		_unlearnSchematic = unlearnSchematic;
	}

	public void UnlearnSchematic(string uid)
	{
		if (_unlearnSchematic == null)
		{
			WALogger.Error<SchematicSystem>(LogChannel.UI, "Attempting to unlearn a schematic without a registered InventoryVisualiser callback for it. SchematicId is {0}", new object[1] { uid });
		}
		else
		{
			_unlearnSchematic(uid);
		}
	}
}
