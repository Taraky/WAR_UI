using Travellers.UI.Framework;

namespace Travellers.UI.Events;

public class ChangeWindowStateEvent : UIEvent
{
	public WindowState NewState;

	public ChangeWindowStateEvent(WindowState newState)
	{
		NewState = newState;
	}
}
