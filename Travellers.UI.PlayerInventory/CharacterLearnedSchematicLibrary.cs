using System;
using System.Collections.Generic;
using Bossa.Travellers.Items;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

public class CharacterLearnedSchematicLibrary
{
	private abstract class SchematicReloadMonitor
	{
		public readonly Dictionary<string, SchematicData> SchematicsById = new Dictionary<string, SchematicData>();

		private List<SchematicData> _schematics = new List<SchematicData>();

		public bool NeedsReload { get; protected set; }

		public List<SchematicData> Schematics => _schematics;

		public void MarkReloaded()
		{
			NeedsReload = false;
		}

		protected void RepopulateSchematicCollections(HashSet<SchematicData> schematicsToAdd)
		{
			_schematics = new List<SchematicData>();
			SchematicsById.Clear();
			foreach (SchematicData item in schematicsToAdd)
			{
				SchematicsById[item.UniqueID] = item;
				_schematics.Add(item);
			}
		}
	}

	private class NonShipSchematicReloadMonitor : SchematicReloadMonitor
	{
		private readonly SchematicsReferenceStore _schematicsReferenceStore;

		private HashSet<string> _cachedItemSchematicIds = new HashSet<string>();

		public NonShipSchematicReloadMonitor(SchematicsReferenceStore schematicsReferenceStore)
		{
			_schematicsReferenceStore = schematicsReferenceStore;
		}

		public void UpdateSchematicsCollection(HashSet<string> rawIds)
		{
			HashSet<SchematicData> hashSet = new HashSet<SchematicData>();
			HashSet<string> hashSet2 = new HashSet<string>();
			foreach (string rawId in rawIds)
			{
				if (!_cachedItemSchematicIds.Contains(rawId) && !base.NeedsReload)
				{
					base.NeedsReload = true;
				}
				SchematicData schematicData = _schematicsReferenceStore.LookupSchematic(rawId);
				if (schematicData != null && hashSet2.Add(schematicData.UniqueID))
				{
					hashSet.Add(schematicData);
				}
			}
			if (_cachedItemSchematicIds.Count != hashSet2.Count)
			{
				base.NeedsReload = true;
			}
			_cachedItemSchematicIds = hashSet2;
			if (base.NeedsReload)
			{
				RepopulateSchematicCollections(hashSet);
			}
		}
	}

	private class ShipSchematicReloadMonitor : SchematicReloadMonitor
	{
		private HashSet<ShipHullSchematicData> _cachedShipHullData = new HashSet<ShipHullSchematicData>();

		public void UpdateSchematicsCollection(List<ShipHullSchematicData> rawHullData)
		{
			HashSet<SchematicData> hashSet = new HashSet<SchematicData>();
			HashSet<ShipHullSchematicData> hashSet2 = new HashSet<ShipHullSchematicData>();
			foreach (ShipHullSchematicData rawHullDatum in rawHullData)
			{
				if (!_cachedShipHullData.Contains(rawHullDatum) && !base.NeedsReload)
				{
					base.NeedsReload = true;
				}
				SchematicData item = SchematicData.FromShipHullData(rawHullDatum);
				if (hashSet2.Add(rawHullDatum))
				{
					hashSet.Add(item);
				}
			}
			if (_cachedShipHullData.Count != hashSet2.Count)
			{
				base.NeedsReload = true;
			}
			_cachedShipHullData = hashSet2;
			if (base.NeedsReload)
			{
				RepopulateSchematicCollections(hashSet);
			}
		}
	}

	private static readonly Dictionary<CharacterSheetTabType, HashSet<CraftingCategory>> CraftingCategoryByCharacterTabType;

	private static readonly Dictionary<CraftingCategory, CharacterSheetTabType> CharacterTabByCraftingCategory;

	private static Dictionary<CraftingCategory, string> _craftingCategoryNameByTabDisplay;

	private static readonly HashSet<CraftingCategory> _nonShipCraftingCategories;

	private static readonly Dictionary<CraftingCategory, SchematicCategoryData> _schematicDataListByCraftingCategory;

	private NonShipSchematicReloadMonitor _nonShipSchematicMonitor;

	private ShipSchematicReloadMonitor _shipSchematicMonitor;

	private readonly SchematicsReferenceStore _schematicsReferenceStore;

	static CharacterLearnedSchematicLibrary()
	{
		CraftingCategoryByCharacterTabType = new Dictionary<CharacterSheetTabType, HashSet<CraftingCategory>>
		{
			{
				CharacterSheetTabType.MultitoolCraft,
				new HashSet<CraftingCategory> { CraftingCategory.Personal }
			},
			{
				CharacterSheetTabType.Schematics,
				new HashSet<CraftingCategory>
				{
					CraftingCategory.Personal,
					CraftingCategory.CraftingStation,
					CraftingCategory.Cooking,
					CraftingCategory.Clothing,
					CraftingCategory.Shipyard
				}
			},
			{
				CharacterSheetTabType.ItemCraft,
				new HashSet<CraftingCategory> { CraftingCategory.CraftingStation }
			},
			{
				CharacterSheetTabType.ShipCraft,
				new HashSet<CraftingCategory> { CraftingCategory.Shipyard }
			},
			{
				CharacterSheetTabType.Cooking,
				new HashSet<CraftingCategory> { CraftingCategory.Cooking }
			},
			{
				CharacterSheetTabType.Clothing,
				new HashSet<CraftingCategory> { CraftingCategory.Clothing }
			}
		};
		CharacterTabByCraftingCategory = new Dictionary<CraftingCategory, CharacterSheetTabType>
		{
			{
				CraftingCategory.Personal,
				CharacterSheetTabType.MultitoolCraft
			},
			{
				CraftingCategory.CraftingStation,
				CharacterSheetTabType.ItemCraft
			},
			{
				CraftingCategory.Shipyard,
				CharacterSheetTabType.ShipCraft
			},
			{
				CraftingCategory.Cooking,
				CharacterSheetTabType.Cooking
			},
			{
				CraftingCategory.Clothing,
				CharacterSheetTabType.Clothing
			}
		};
		_craftingCategoryNameByTabDisplay = new Dictionary<CraftingCategory, string>
		{
			{
				CraftingCategory.Personal,
				"ITEMS"
			},
			{
				CraftingCategory.CraftingStation,
				"SHIP PARTS"
			},
			{
				CraftingCategory.Shipyard,
				"SHIP FRAMES"
			},
			{
				CraftingCategory.Cooking,
				"COOKING"
			},
			{
				CraftingCategory.Clothing,
				"CLOTHING"
			}
		};
		_schematicDataListByCraftingCategory = new Dictionary<CraftingCategory, SchematicCategoryData>();
		_nonShipCraftingCategories = new HashSet<CraftingCategory>();
		foreach (CraftingCategory value in Enum.GetValues(typeof(CraftingCategory)))
		{
			if (value != 0 && value != CraftingCategory.None)
			{
				_nonShipCraftingCategories.Add(value);
			}
		}
	}

	public CharacterLearnedSchematicLibrary(SchematicsReferenceStore schematicsReferenceStore)
	{
		_schematicsReferenceStore = schematicsReferenceStore;
		ClearSchematics();
		foreach (KeyValuePair<CraftingCategory, string> item in _craftingCategoryNameByTabDisplay)
		{
			_schematicDataListByCraftingCategory[item.Key] = new SchematicCategoryData(item.Key, item.Value);
		}
	}

	public bool TryAddRawNonShipSchematics(HashSet<string> rawStrings)
	{
		if (!_schematicsReferenceStore.GsimReferenceDataLoaded)
		{
			WALogger.Warn<CharacterData>(LogChannel.Schematics, "Attempting to get schematic before SchematicManager has parsed reference data", new object[0]);
			return false;
		}
		_nonShipSchematicMonitor.UpdateSchematicsCollection(rawStrings);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.SchematicsUpdated, null);
		return true;
	}

	public void AddShipSchematics(List<ShipHullSchematicData> shipData)
	{
		_shipSchematicMonitor.UpdateSchematicsCollection(shipData);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.SchematicsUpdated, null);
	}

	public List<SchematicData> GetShipSchematics()
	{
		return _schematicDataListByCraftingCategory[CraftingCategory.Shipyard].ChildShipSchematics;
	}

	public bool PlayerHasLearnedSchematic(string uuidOrJSON)
	{
		if (string.IsNullOrEmpty(uuidOrJSON))
		{
			return false;
		}
		if (!_schematicsReferenceStore.GsimReferenceDataLoaded)
		{
			WALogger.Warn<CharacterData>(LogChannel.Schematics, "Attempting to get schematic before SchematicManager has parsed reference data", new object[0]);
			return false;
		}
		if (_nonShipSchematicMonitor.SchematicsById.ContainsKey(uuidOrJSON))
		{
			return true;
		}
		SchematicData schematicData = _schematicsReferenceStore.LookupSchematic(uuidOrJSON);
		if (schematicData != null && _nonShipSchematicMonitor.SchematicsById.ContainsKey(schematicData.UniqueID))
		{
			return true;
		}
		return false;
	}

	public static CharacterSheetTabType GetUITabFromCraftingCategory(CraftingCategory category)
	{
		if (!CharacterTabByCraftingCategory.TryGetValue(category, out var value))
		{
			WALogger.Error<CharacterLearnedSchematicLibrary>(LogChannel.UI, "Attempting to get a UI tab type for a crating category with no association - {0}", new object[1] { category });
		}
		return value;
	}

	public bool CheckIfSchematicHierarchyRebuildNeeded()
	{
		bool result = false;
		if (_nonShipSchematicMonitor.NeedsReload)
		{
			result = true;
			BuildItemSchematicHierarchy();
			_nonShipSchematicMonitor.MarkReloaded();
		}
		if (_shipSchematicMonitor.NeedsReload)
		{
			result = true;
			BuildShipSchematicHierarchy();
			_shipSchematicMonitor.MarkReloaded();
		}
		return result;
	}

	public List<SchematicCategoryData> RequestSchematicListForTabType(CharacterSheetTabType categoryToRetrieve)
	{
		List<SchematicCategoryData> list = new List<SchematicCategoryData>();
		HashSet<CraftingCategory> hashSet = CraftingCategoryByCharacterTabType[categoryToRetrieve];
		foreach (CraftingCategory item in hashSet)
		{
			list.Add(_schematicDataListByCraftingCategory[item]);
		}
		return list;
	}

	public bool DoesUITabTypeContainCategory(CharacterSheetTabType categoryToRetrieve, SchematicData schematicData)
	{
		if (!CraftingCategoryByCharacterTabType.TryGetValue(categoryToRetrieve, out var value))
		{
			WALogger.Error<CharacterLearnedSchematicLibrary>(LogChannel.UI, "Attempting to get a list of schematic categories for a UI tab type that is not stored - {0}", new object[1] { categoryToRetrieve });
			return false;
		}
		return value.Contains(schematicData.CraftingCategoryEnum);
	}

	private void CleanCategoryDataCollections(CraftingCategory categoryToClean)
	{
		if (!_schematicDataListByCraftingCategory.TryGetValue(categoryToClean, out var value))
		{
			WALogger.Error<CharacterLearnedSchematicLibrary>(LogChannel.UI, "Trying to clean up hierarchy for category that isn't stored - {0}", new object[1] { categoryToClean });
		}
		else
		{
			value.CleanLists();
		}
	}

	private void BuildItemSchematicHierarchy()
	{
		foreach (CraftingCategory nonShipCraftingCategory in _nonShipCraftingCategories)
		{
			CleanCategoryDataCollections(nonShipCraftingCategory);
		}
		List<SchematicData> schematics = _nonShipSchematicMonitor.Schematics;
		foreach (SchematicData item in schematics)
		{
			CraftingCategory craftingCategory = (CraftingCategory)Enum.Parse(typeof(CraftingCategory), item.category);
			string itemType = item.itemType;
			if (!_schematicDataListByCraftingCategory[craftingCategory].ChildSchematicCategories.ContainsKey(itemType))
			{
				_schematicDataListByCraftingCategory[craftingCategory].ChildSchematicCategories[itemType] = new SchematicCategoryData(craftingCategory, itemType, item.HumanReadableItemType, itemType);
			}
			_schematicDataListByCraftingCategory[craftingCategory].ChildSchematicCategories[itemType].ChildItemSchematics.Add(item);
		}
	}

	private void BuildShipSchematicHierarchy()
	{
		CleanCategoryDataCollections(CraftingCategory.Shipyard);
		CraftingCategory key = CraftingCategory.Shipyard;
		if (!_schematicDataListByCraftingCategory.ContainsKey(key))
		{
			WALogger.Error<CharacterLearnedSchematicLibrary>(LogChannel.UI, "Attempting to build ship data lists but no category exists for ship data", new object[0]);
		}
		_schematicDataListByCraftingCategory[key].ChildShipSchematics.Clear();
		foreach (SchematicData schematic in _shipSchematicMonitor.Schematics)
		{
			_schematicDataListByCraftingCategory[key].ChildShipSchematics.Add(schematic);
		}
	}

	public void ClearSchematics()
	{
		_nonShipSchematicMonitor = new NonShipSchematicReloadMonitor(_schematicsReferenceStore);
		_shipSchematicMonitor = new ShipSchematicReloadMonitor();
	}
}
