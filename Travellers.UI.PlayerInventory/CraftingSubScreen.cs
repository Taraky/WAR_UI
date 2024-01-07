using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.Models;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class CraftingSubScreen : UIScreenComponent
{
	private CraftingUI _currentCrafting;

	[SerializeField]
	private ShipCraftingUI _shipCraftingUI;

	[SerializeField]
	private CraftingStationCraftingUI _generalCraftingUI;

	private InventorySystem _inventorySystem;

	private ISchematicSystem _schematicSystem;

	public CraftingUI CurrentCrafting => _currentCrafting;

	[InjectableMethod]
	public void InjectDependencies(InventorySystem inventorySystem, ISchematicSystem schematicSystem)
	{
		_schematicSystem = schematicSystem;
		_inventorySystem = inventorySystem;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIButtonEvents.SchematicItemPressed, OnSchematicItemButtonPressed);
		_eventList.AddEvent(WAUIInventoryEvents.ShipSchematicsCancelAndReloadLastShipSchematic, OnReloadLastShipSchematic);
	}

	protected override void ProtectedInit()
	{
		SetObjectsActive(shipyardActive: false, generalCraftingActive: false);
	}

	protected override void ProtectedDispose()
	{
	}

	private void OnSchematicItemButtonPressed(object[] obj)
	{
		SchematicCategoryListItemPressed schematicCategoryListItemPressed = obj[0] as SchematicCategoryListItemPressed;
		_inventorySystem.CurrentCraftingData.LoadSchematicWithEvent(schematicCategoryListItemPressed.Schematic, CharacterData.Instance.userName);
		_inventorySystem.SendNewSchematicToServer();
		_currentCrafting.SelectNewSchematic();
	}

	private void OnReloadLastShipSchematic(object[] obj)
	{
		SchematicData data = obj[0] as SchematicData;
		_inventorySystem.CurrentCraftingData.LoadSchematicWithEvent(data, CharacterData.Instance.userName);
		_inventorySystem.SendNewSchematicToServer();
		_currentCrafting.SelectNewSchematic();
	}

	protected override void Deactivate()
	{
		SetObjectsActive(shipyardActive: false, generalCraftingActive: false);
	}

	public void ShowCraftingModule()
	{
		CraftingStationData currentCraftingData = _inventorySystem.CurrentCraftingData;
		CharacterSheetTabType uITabType = currentCraftingData.UITabType;
		switch (uITabType)
		{
		case CharacterSheetTabType.MultitoolCraft:
		case CharacterSheetTabType.ItemCraft:
		case CharacterSheetTabType.Cooking:
		case CharacterSheetTabType.Clothing:
			_currentCrafting = _generalCraftingUI;
			break;
		case CharacterSheetTabType.ShipCraft:
			_currentCrafting = _shipCraftingUI;
			break;
		default:
			WALogger.Error<SchematicsSubScreen>(LogChannel.UI, "Trying to show illogical crafting category - {0}", new object[1] { uITabType });
			break;
		}
		_schematicSystem.RebuildCraftingDataSchematicHierarchy(currentCraftingData);
		_currentCrafting.SetCraftingDataTemplate(currentCraftingData);
		SetSectionsActiveForCategory(uITabType);
	}

	private void SetSectionsActiveForCategory(CharacterSheetTabType category)
	{
		bool flag = category == CharacterSheetTabType.ShipCraft;
		bool flag2 = category == CharacterSheetTabType.ItemCraft || category == CharacterSheetTabType.MultitoolCraft || category == CharacterSheetTabType.Cooking || category == CharacterSheetTabType.Clothing;
		_shipCraftingUI.SetAsActiveCraftingSection(flag);
		_generalCraftingUI.SetAsActiveCraftingSection(flag2);
		SetObjectsActive(flag, flag2);
	}

	private void SetObjectsActive(bool shipyardActive, bool generalCraftingActive)
	{
		_shipCraftingUI.SetObjectActive(shipyardActive);
		_generalCraftingUI.SetObjectActive(generalCraftingActive);
	}
}
