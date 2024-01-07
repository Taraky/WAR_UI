namespace Travellers.UI.PlayerInventory;

public class CharacterSheetScreenStateFacade
{
	private CharacterSheetScreen _characterSheetScreen;

	public bool IsInventoryScreenActive => _characterSheetScreen.IsObjectActiveInScene;

	public bool IsLastOpenedTabPartOfCharacterSheetScreen => _characterSheetScreen.CurrentTabType == CharacterSheetTabType.Character || _characterSheetScreen.CurrentTabType == CharacterSheetTabType.Knowledge || _characterSheetScreen.CurrentTabType == CharacterSheetTabType.Schematics || _characterSheetScreen.CurrentTabType == CharacterSheetTabType.MultitoolCraft || _characterSheetScreen.CurrentTabType == CharacterSheetTabType.Logbook;

	public CharacterSheetTabType LastOpenedInventoryTab => _characterSheetScreen.CurrentTabType;

	public bool IsCurrentCraftingNull => _characterSheetScreen.CurrentCrafting == null;

	public bool IsHullEditorUINull
	{
		get
		{
			if (IsCurrentCraftingNull)
			{
				return true;
			}
			return _characterSheetScreen.CurrentCrafting.hullEditorUI == null;
		}
	}

	public ShipCraftingUIHelper GetHullEditorUI
	{
		get
		{
			if (IsHullEditorUINull)
			{
				return null;
			}
			return _characterSheetScreen.CurrentCrafting.hullEditorUI;
		}
	}

	public InventoryGridUI GetPlayerGrid => _characterSheetScreen._playerGrid;

	public CharacterSheetScreenStateFacade(CharacterSheetScreen characterSheetScreen)
	{
		_characterSheetScreen = characterSheetScreen;
	}
}
