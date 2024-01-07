namespace Travellers.UI.PlayerInventory;

public class HighlightSlotEvent
{
	public CharacterSlotType CharacterSlotType;

	public HighlightSlotEvent(CharacterSlotType characterSlotType)
	{
		CharacterSlotType = characterSlotType;
	}
}
