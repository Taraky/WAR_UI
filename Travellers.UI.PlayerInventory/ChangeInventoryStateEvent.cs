namespace Travellers.UI.PlayerInventory;

public class ChangeInventoryStateEvent
{
	public CharacterSheetTabType CharacterSheetTabType;

	public bool Enable;

	public ChangeInventoryStateEvent(CharacterSheetTabType tabTypeToUse = CharacterSheetTabType.MultitoolCraft, bool enabled = true)
	{
		CharacterSheetTabType = tabTypeToUse;
		Enable = enabled;
	}
}
