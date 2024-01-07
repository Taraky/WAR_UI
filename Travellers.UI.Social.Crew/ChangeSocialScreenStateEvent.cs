using Travellers.UI.Events;

namespace Travellers.UI.Social.Crew;

public class ChangeSocialScreenStateEvent : UIEvent
{
	public bool IsEnabled;

	public ChangeSocialScreenStateEvent(bool isEnabled)
	{
		IsEnabled = isEnabled;
	}
}
