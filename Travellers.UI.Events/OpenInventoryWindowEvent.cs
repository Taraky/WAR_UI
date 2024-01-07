using Travellers.UI.PlayerInventory;

namespace Travellers.UI.Events;

public class OpenInventoryWindowEvent : UIEvent
{
	public CharacterSheetTab CharacterSheetTab;

	public OpenInventoryWindowEvent(CharacterSheetTab characterSheetTab)
	{
		CharacterSheetTab = characterSheetTab;
	}
}
