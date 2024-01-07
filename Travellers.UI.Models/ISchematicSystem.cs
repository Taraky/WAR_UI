using System;
using System.Collections.Generic;
using Bossa.Travellers.Items;
using Improbable.Collections;
using RSG;
using Travellers.UI.Framework;
using Travellers.UI.PlayerInventory;

namespace Travellers.UI.Models;

[InjectedInterface]
public interface ISchematicSystem
{
	bool IsPlayerAndReferenceDataLoaded { get; }

	void AllReferenceAndPlayerDataLoaded();

	void ReferenceDataReset();

	void DeserialiseJson(string json);

	SchematicData LookupSchematic(string uuId);

	IPromise<SchematicData> LookupSchematicAsync(string uuId);

	void CheckIfSchematicHierarchyRebuildNeeded(CraftingStationData craftingDataTemplate);

	void RebuildCraftingDataSchematicHierarchy(CraftingStationData craftingDataTemplate);

	void UpdateNonShipSchematics(Improbable.Collections.List<string> defaultSchematics, Improbable.Collections.List<string> learnedSchematics);

	void UpdateShipSchematics(Improbable.Collections.List<ShipHullSchematicData> shipData);

	System.Collections.Generic.List<SchematicData> GetShipSchematics();

	bool PlayerHasLearnedSchematic(string uuidOrJSON);

	int SchematicsCapPerCategory(CraftingCategory category);

	void SetUnlearnSchematicCallback(Action<string> unlearnSchematic);

	void UnlearnSchematic(string uid);
}
