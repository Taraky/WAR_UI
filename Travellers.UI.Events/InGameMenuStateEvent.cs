using Travellers.UI.InGame;

namespace Travellers.UI.Events;

public class InGameMenuStateEvent : UIEvent
{
	public InGameMenuState NewState = InGameMenuState.Off;

	public bool ToggleState;

	public InGameMenuStateEvent(InGameMenuState newState = InGameMenuState.Off, bool toggleState = false)
	{
		NewState = newState;
		ToggleState = toggleState;
	}
}
