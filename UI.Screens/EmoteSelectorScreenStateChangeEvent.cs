using Travellers.UI.Events;

namespace UI.Screens;

public class EmoteSelectorScreenStateChangeEvent : UIEvent
{
	public bool IsActive;

	public EmoteSelectorScreenStateChangeEvent(bool isActive)
	{
		IsActive = isActive;
	}
}
