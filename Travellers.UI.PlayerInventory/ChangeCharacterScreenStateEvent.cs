namespace Travellers.UI.PlayerInventory;

public class ChangeCharacterScreenStateEvent
{
	public bool ShowState { get; private set; }

	public CharacterSheetTabType TypeToShow { get; private set; }

	public ChangeCharacterScreenStateEvent(bool showState, CharacterSheetTabType typeToShow)
	{
		ShowState = showState;
		TypeToShow = typeToShow;
	}
}
